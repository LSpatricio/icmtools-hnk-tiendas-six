Imports System.Data
Imports System.Data.SqlClient
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports System.Threading.Tasks
Imports Dapper
Imports Serilog

Public Class SA132Service
    Private ReadOnly _excelReader As ExcelReader
    Private ReadOnly _excelService As ExcelService
    Private ReadOnly _repository As Repository
    Private ReadOnly _configuration As IAppConfiguration
    Private ReadOnly _sftpClient As SftpClient

    Public Sub New()
        _excelReader = New ExcelReader()
        _excelService = New ExcelService()
        _configuration = New AppConfiguration()
        _repository = New Repository(_configuration.ConnectionString)
        _sftpClient = New SftpClient()
    End Sub

    Public Async Function ProcesarSA132(
        request As ValidateFileRequest,
        idCarga As Guid,
        logger As ILogger) As Task(Of CargaResponse)

        Const tablaStaging As String = "STG_INGRESOSSIX"
        Const tablaDestino As String = "BDIINGRESOSSIX"
        Const sp As String = "SP_VALIDATE_INGRESOSSIX"

        Dim errores = Await ValidacionesSA132(request)

        errores.AddRange(Await ValidarDuplicadosSA132Async(
            tablaStaging,
            tablaDestino,
            logger))

        If errores.Any() Then
            Return New CargaResponse With {
                .Exitoso = False,
                .IdCarga = idCarga,
                .Errores = errores
            }
        End If

        logger.Information("No se encontraron errores de validacion en el archivo de SA132")

        Await _repository.EjecutarSPAsync($"dbo.{sp}", idCarga)
        logger.Information("Procedimiento almacenado {sp} ejecutado correctamente", sp)

        Return New CargaResponse With {
            .Exitoso = True,
            .IdCarga = idCarga,
            .Errores = New List(Of ExcelValidationError)()
        }
    End Function

    Public Async Function ValidacionesSA132(request As ValidateFileRequest) As Task(Of List(Of ExcelValidationError))
        Dim tableName As String = "STG_INGRESOSSIX"
        Dim tipo As Type = Type.GetType(request.FileClass)

        Dim valoresErrores As New List(Of ExcelValidationError)()
        Dim cantidadHojas As Integer = _excelReader.ContarHojas(request.Path)
        Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) =
            _excelService.CrearMepeoAtributos(tipo)

        Await _repository.LimpiarStaging(tableName)

        For i As Integer = 0 To cantidadHojas - 1
            valoresErrores.AddRange(
                Await _excelReader.CargaAsync(
                    request.Path,
                    request.HeaderRow,
                    i.ToString(),
                    mapeoColumnas,
                    tableName,
                    Nothing,
                    Nothing,
                    AddressOf ValidarFilaSA132))
        Next

        Return valoresErrores
    End Function

    Private Function ValidarFilaSA132(
        fila As DataRow,
        Optional regionSelector As String = Nothing,
        Optional catalogos As CatalogosDto = Nothing) As String

        Dim errores As New List(Of String)()

        ' Columnas trabajadas del reporte:
        ' Fecha, CeBeCategoria, CeBe, Categoria y SumaML.
        ' Fecha se genera internamente con el primer dia del mes actual.
        ' SumaML se normaliza a 0 cuando viene vacio y se redondea a 2 decimales.

        fila(NameOf(SA132ExcelDto.Fecha)) =
            New DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)

        If fila.IsNull(NameOf(SA132ExcelDto.Categoria)) OrElse String.IsNullOrWhiteSpace(fila(NameOf(SA132ExcelDto.Categoria)).ToString()) Then
            fila(NameOf(SA132ExcelDto.Categoria)) = String.Empty
        End If

        Dim sumaTexto As String = If(fila.IsNull(NameOf(SA132ExcelDto.SumaML)), String.Empty, fila(NameOf(SA132ExcelDto.SumaML)).ToString().Trim())

        If String.IsNullOrWhiteSpace(sumaTexto) Then
            fila(NameOf(SA132ExcelDto.SumaML)) = 0D.ToString("0.00", CultureInfo.InvariantCulture)
        Else
            Dim suma As Double
            If Double.TryParse(sumaTexto, NumberStyles.Any, CultureInfo.CurrentCulture, suma) OrElse
               Double.TryParse(sumaTexto, NumberStyles.Any, CultureInfo.InvariantCulture, suma) Then
                fila(NameOf(SA132ExcelDto.SumaML)) = Math.Round(CDec(suma), 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture)
            Else
                errores.Add("La columna 'Sum of ML' requiere un valor numerico valido.")
            End If
        End If

        If errores.Count = 0 Then Return Nothing
        Return String.Join("<br/>", errores)
    End Function

    Private Sub NormalizarTextoOpcionalParaStaging(fila As DataRow, columna As String)
        If fila.IsNull(columna) Then
            fila(columna) = String.Empty
        End If
    End Sub

    Private Async Function ValidarDuplicadosSA132Async(
        tablaStaging As String,
        tablaDestino As String,
        logger As ILogger) As Task(Of List(Of ExcelValidationError))
        Dim helperTables = Await PrepararTablasValidacionDuplicadosAsync(
            tablaStaging,
            tablaDestino)

        Try
            Dim errores = Await _repository.ValidarDuplicadosAsync(
                helperTables.TablaStaging,
                helperTables.TablaDestino)
            LimpiarTablasValidacionDuplicados(helperTables)
            Return errores
        Catch
            LimpiarTablasValidacionDuplicados(helperTables)
            Throw
        End Try
    End Function

    Private Async Function PrepararTablasValidacionDuplicadosAsync(
        tablaStaging As String,
        tablaDestino As String) As Task(Of HelperDuplicadosSa132)

        Dim sufijo As String = Guid.NewGuid().ToString("N")
        Dim helper As New HelperDuplicadosSa132 With {
            .TablaStaging = $"SA132_DUPCHK_STG_{sufijo}",
            .TablaDestino = $"SA132_DUPCHK_DST_{sufijo}"
        }

        Dim sql As String = $"
            IF OBJECT_ID('dbo.[{helper.TablaStaging}]', 'U') IS NOT NULL DROP TABLE dbo.[{helper.TablaStaging}];
            IF OBJECT_ID('dbo.[{helper.TablaDestino}]', 'U') IS NOT NULL DROP TABLE dbo.[{helper.TablaDestino}];

            CREATE TABLE dbo.[{helper.TablaStaging}](
                Fecha nvarchar(30) NOT NULL,
                CeBeCategoria nvarchar(255) NOT NULL,
                CeBe nvarchar(255) NOT NULL,
                Categoria nvarchar(255) NOT NULL,
                SumaML nvarchar(50) NOT NULL,
                IdCarga nvarchar(36) NULL,
                FechaInsercion nvarchar(30) NULL
            );

            CREATE TABLE dbo.[{helper.TablaDestino}](
                Fecha nvarchar(30) NOT NULL,
                CeBeCategoria nvarchar(255) NOT NULL,
                CeBe nvarchar(255) NOT NULL,
                Categoria nvarchar(255) NOT NULL,
                SumaML nvarchar(50) NOT NULL,
                IdCarga nvarchar(36) NULL,
                FechaInsercion nvarchar(30) NULL,
                CONSTRAINT [PK_{helper.TablaDestino}] PRIMARY KEY CLUSTERED (
                    Fecha,
                    CeBeCategoria,
                    CeBe,
                    Categoria,
                    SumaML
                )
            );

            INSERT INTO dbo.[{helper.TablaStaging}] (
                Fecha,
                CeBeCategoria,
                CeBe,
                Categoria,
                SumaML,
                IdCarga,
                FechaInsercion
            )
            SELECT
                COALESCE(CONVERT(nvarchar(30), Fecha, 126), ''),
                COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(255), CeBeCategoria))), ''), ''),
                COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(255), CeBe))), ''), ''),
                COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(255), Categoria))), ''), ''),
                COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(50), SumaML))), ''), '0'),
                NULL,
                NULL
            FROM dbo.{tablaStaging};

            INSERT INTO dbo.[{helper.TablaDestino}] (
                Fecha,
                CeBeCategoria,
                CeBe,
                Categoria,
                SumaML,
                IdCarga,
                FechaInsercion
            )
            SELECT
                COALESCE(CONVERT(nvarchar(30), Fecha, 126), ''),
                COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(255), CeBeCategoria))), ''), ''),
                COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(255), CeBe))), ''), ''),
                COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(255), Categoria))), ''), ''),
                COALESCE(NULLIF(LTRIM(RTRIM(CONVERT(nvarchar(50), SumaML))), ''), '0'),
                CONVERT(nvarchar(36), IdCarga),
                CONVERT(nvarchar(30), FechaInsercion, 126)
            FROM dbo.{tablaDestino};"

        Using connection As New SqlConnection(_configuration.ConnectionString)
            Await connection.OpenAsync()
            Await connection.ExecuteAsync(sql, commandTimeout:=600)
        End Using

        Return helper
    End Function

    Private Sub LimpiarTablasValidacionDuplicados(helper As HelperDuplicadosSa132)
        Dim sql As String = $"
            IF OBJECT_ID('dbo.[{helper.TablaStaging}]', 'U') IS NOT NULL DROP TABLE dbo.[{helper.TablaStaging}];
            IF OBJECT_ID('dbo.[{helper.TablaDestino}]', 'U') IS NOT NULL DROP TABLE dbo.[{helper.TablaDestino}];"

        Using connection As New SqlConnection(_configuration.ConnectionString)
            connection.Open()
            connection.Execute(sql, commandTimeout:=600)
        End Using
    End Sub

    Private Class HelperDuplicadosSa132
        Public Property TablaStaging As String
        Public Property TablaDestino As String
    End Class

    Public Async Function EnvioSA132(request As SendInfoRequest, logger As ILogger) As Task
        If Not Directory.Exists(request.PathSalida) Then
            Directory.CreateDirectory(request.PathSalida)
        End If

        Dim nombreArchivo As String = "BDIINGRESOSSIX.csv"
        Dim rutaArchivo As String = Path.Combine(request.PathSalida, nombreArchivo)

        Dim sql As String = "
            SELECT
                 FORMAT(Fecha, 'yyyy-MM-dd') AS Fecha
                ,CeBeCategoria
                ,CeBe
                ,Categoria
                ,SumaML
            FROM dbo.BDIINGRESOSSIX
            WHERE IdCarga = @IdCarga
            ORDER BY Fecha, CeBeCategoria, CeBe, Categoria"

        Await _repository.GenerarCsvAsync(
            sql,
            rutaArchivo,
            request.IdGui)

        logger.Information("Archivo CSV de SA132 generado correctamente {rutaArchivo}", rutaArchivo)

        Await _sftpClient.SubirArchivoAsync(rutaArchivo)

        logger.Information("Archivo CSV de SA132 enviado al SFTP")
    End Function
End Class
