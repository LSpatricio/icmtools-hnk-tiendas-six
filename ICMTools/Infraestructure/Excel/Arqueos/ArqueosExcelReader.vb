Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Reflection
Imports ExcelDataReader

Public Class ArqueosExcelReader

    Public ReadOnly Property _excelReader As ExcelReader

    Public Sub New()
        _excelReader = New ExcelReader()
    End Sub

    Public Function ValidacionesArqueos(
        rutaArchivo As String,
        filaEncabezado As Integer,
        nombreHoja As String,
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim errores As New List(Of ExcelValidationError)

        Using stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                If Not MoverAHoja(reader, nombreHoja) Then
                    errores.Add(New ExcelValidationError With {
                        .Problema = $"La hoja <strong>{nombreHoja}</strong> no existe en el archivo Excel.",
                        .Detalle = $"Hojas encontradas en el archivo Excel: {reader.Name}"
                    })
                    Return errores
                End If

                For i As Integer = 1 To filaEncabezado
                    If Not reader.Read() Then
                        Return errores
                    End If
                Next

                Dim encabezados As List(Of String) = LeerEncabezados(reader)

                Dim idxCodigoListado = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CodigoListado))
                Dim idxNumeroSAP = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.NumeroSAP))
                Dim idxAlmacen = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Almacen))
                Dim idxTipoListado = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.TipoListado))
                Dim idxEstatus = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Estatus))
                Dim idxFechaCreacion = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaCreacion))
                Dim idxNombre = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Nombre))
                Dim idxUsuarioCreador = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioCreador))
                Dim idxUsuarioCreadorPerfil = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioCreadorPerfil))
                Dim idxUsuarioAutorizador = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioAutorizador))
                Dim idxUsuarioAutorizadorPerfil = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioAutorizadorPerfil))
                Dim idxTipoCierre = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.TipoCierre))
                Dim idxFechaInicioConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaInicioConteo))
                Dim idxFechaFinConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaFinConteo))
                Dim idxFechaCierreConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaCierreConteo))
                Dim idxFechaTerminoConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaTerminoConteo))
                Dim idxSubinventario = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Subinventario))
                Dim idxIdProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.IdProducto))
                Dim idxCodigoProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CodigoProducto))
                Dim idxNombreProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.NombreProducto))
                Dim idxUnidad = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Unidad))
                Dim idxCantidadSistema = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CantidadSistema))
                Dim idxDiferencia = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Diferencia))
                Dim idxFaltante = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Faltante))
                Dim idxSobrante = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Sobrante))
                Dim idxFaltantePrecioCons = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FaltantePrecioCons))
                Dim idxSobrantePrecioCons = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.SobrantePrecioCons))
                Dim idxComentario = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Comentario))

                Dim indices As Integer() = {
                    idxCodigoListado, idxNumeroSAP, idxAlmacen, idxTipoListado, idxEstatus, idxFechaCreacion,
                    idxNombre, idxUsuarioCreador, idxUsuarioCreadorPerfil, idxUsuarioAutorizador,
                    idxUsuarioAutorizadorPerfil, idxTipoCierre, idxFechaInicioConteo, idxFechaFinConteo,
                    idxFechaCierreConteo, idxFechaTerminoConteo, idxSubinventario, idxIdProducto,
                    idxCodigoProducto, idxNombreProducto, idxUnidad, idxCantidadSistema, idxDiferencia,
                    idxFaltante, idxSobrante, idxFaltantePrecioCons, idxSobrantePrecioCons, idxComentario
                }

                Dim headersEncontrados As String = String.Join(", ", encabezados)

                For Each mapeo In mapeoColumnas
                    Dim indiceEncontrado As Integer = ObtenerIndiceColumna(encabezados, mapeoColumnas, mapeo.Key.Name)

                    If indiceEncontrado < 0 Then
                        errores.Add(New ExcelValidationError With {
                            .Problema = $"La columna '{mapeo.Value.ColumnName}' no se encuentra en la hoja <strong>{nombreHoja}</strong>.",
                            .Detalle = $"Columnas encontradas en el archivo Excel: {headersEncontrados}"
                        })
                    End If
                Next

                If errores.Count > 0 Then
                    Return errores
                End If

                Dim mapeoConIndicesReales As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = ClonarMapeoConIndicesReales(
                    mapeoColumnas,
                    encabezados)

                Using streamValidacion = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                    Using readerValidacion = ExcelReaderFactory.CreateReader(streamValidacion)
                        errores.AddRange(_excelReader.ValidacionesInformacion(readerValidacion, filaEncabezado, nombreHoja, mapeoConIndicesReales))
                    End Using
                End Using

                If errores.Count > 0 Then
                    Return errores
                End If

                If Not reader.Read() Then
                    Return errores
                End If

                Dim filaActual As Integer = filaEncabezado + 1

                Do
                    If FilaVaciaPorIndices(reader, indices) Then
                        Exit Do
                    End If

                    ValidarFilaArqueos(
                        errores,
                        reader,
                        nombreHoja,
                        filaActual,
                        idxCodigoListado,
                        idxNumeroSAP,
                        idxAlmacen,
                        idxTipoListado,
                        idxEstatus,
                        idxFechaCreacion,
                        idxNombre,
                        idxUsuarioCreador,
                        idxUsuarioCreadorPerfil,
                        idxUsuarioAutorizador,
                        idxUsuarioAutorizadorPerfil,
                        idxTipoCierre,
                        idxFechaInicioConteo,
                        idxFechaFinConteo,
                        idxFechaCierreConteo,
                        idxFechaTerminoConteo,
                        idxSubinventario,
                        idxIdProducto,
                        idxCodigoProducto,
                        idxNombreProducto,
                        idxUnidad,
                        idxCantidadSistema,
                        idxDiferencia,
                        idxFaltante,
                        idxSobrante,
                        idxFaltantePrecioCons,
                        idxSobrantePrecioCons,
                        idxComentario)

                    filaActual += 1
                Loop While reader.Read()
            End Using
        End Using

        Return errores
    End Function

    Public Function ObtenerDataTableStgArqueos(rutaArchivo As String) As DataTable
        Dim tabla As DataTable = CrearTablaStgArqueos()
        Dim excelService As New ExcelService()
        Dim tipoHoja As Type = GetType(ArqueosDetalleExcelDto)
        Dim mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute) = excelService.CrearMepeoAtributos(tipoHoja)
        Dim filaEncabezado As Integer = 1

        Using stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                Do
                    Dim nombreHojaActual As String = reader.Name

                    If HojaTieneEstructura(reader, filaEncabezado, mapeoColumnas) Then
                        CargarHojaEnTabla(rutaArchivo, filaEncabezado, nombreHojaActual, mapeoColumnas, tabla)
                    End If
                Loop While reader.NextResult()
            End Using
        End Using

        Return tabla
    End Function

    Public Function ValidacionesArqueosTodasLasHojas(
        rutaArchivo As String,
        filaEncabezado As Integer,
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim errores As New List(Of ExcelValidationError)
        Dim hojasValidas As Integer = 0

        Using stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                Do
                    Dim nombreHojaActual As String = reader.Name

                    If HojaTieneEstructura(reader, filaEncabezado, mapeoColumnas) Then
                        hojasValidas += 1
                        errores.AddRange(ValidacionesArqueos(rutaArchivo, filaEncabezado, nombreHojaActual, mapeoColumnas))
                    End If
                Loop While reader.NextResult()
            End Using
        End Using

        If hojasValidas = 0 Then
            errores.Add(New ExcelValidationError With {
                .Problema = "El archivo no contiene ninguna hoja con la estructura esperada para el reporte de Arqueos.",
                .Detalle = "Se revisaron todas las hojas del archivo y ninguna contiene todos los encabezados requeridos."
            })
        End If

        Return errores
    End Function

    Private Function HojaTieneEstructura(
        reader As IExcelDataReader,
        filaEncabezado As Integer,
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As Boolean

        For i As Integer = 1 To filaEncabezado
            If Not reader.Read() Then
                Return False
            End If
        Next

        Dim encabezados As List(Of String) = LeerEncabezados(reader)

        For Each mapeo In mapeoColumnas
            If ObtenerIndiceColumna(encabezados, mapeoColumnas, mapeo.Key.Name) < 0 Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Function CrearTablaStgArqueos() As DataTable
        Dim tabla As New DataTable()

        tabla.Columns.Add("NumeroSAP", GetType(Long))
        tabla.Columns.Add("Almacen", GetType(String))
        tabla.Columns.Add("TipoListado", GetType(String))
        tabla.Columns.Add("Estatus", GetType(String))
        tabla.Columns.Add("FechaCreacion", GetType(DateTime))
        tabla.Columns.Add("Nombre", GetType(String))
        tabla.Columns.Add("UsuarioCreador", GetType(String))
        tabla.Columns.Add("UsuarioCreadorPerfil", GetType(String))
        tabla.Columns.Add("UsuarioAutorizador", GetType(String))
        tabla.Columns.Add("UsuarioAutorizadorPerfil", GetType(String))
        tabla.Columns.Add("TipoCierre", GetType(String))
        tabla.Columns.Add("FechaInicioConteo", GetType(DateTime))
        tabla.Columns.Add("FechaFinConteo", GetType(DateTime))
        tabla.Columns.Add("FechaCierreConteo", GetType(DateTime))
        tabla.Columns.Add("FechaTerminoConteo", GetType(DateTime))
        tabla.Columns.Add("Subinventario", GetType(String))
        tabla.Columns.Add("IdProducto", GetType(String))
        tabla.Columns.Add("CodigoProducto", GetType(Long))
        tabla.Columns.Add("NombreProducto", GetType(String))
        tabla.Columns.Add("Unidad", GetType(String))
        tabla.Columns.Add("CantidadSistema", GetType(Decimal))
        tabla.Columns.Add("Diferencia", GetType(Decimal))
        tabla.Columns.Add("Faltante", GetType(Decimal))
        tabla.Columns.Add("Sobrante", GetType(Decimal))
        tabla.Columns.Add("FaltantePrecioCons", GetType(Decimal))
        tabla.Columns.Add("SobrantePrecioCons", GetType(Decimal))
        tabla.Columns.Add("Comentario", GetType(String))

        Return tabla
    End Function

    Private Sub CargarHojaEnTabla(
        rutaArchivo As String,
        filaEncabezado As Integer,
        nombreHoja As String,
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute),
        tabla As DataTable)

        Using stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                If Not MoverAHoja(reader, nombreHoja) Then
                    Return
                End If

                For i As Integer = 1 To filaEncabezado
                    If Not reader.Read() Then
                        Return
                    End If
                Next

                Dim encabezados As List(Of String) = LeerEncabezados(reader)

                Dim idxCodigoListado = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CodigoListado))
                Dim idxNumeroSAP = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.NumeroSAP))
                Dim idxAlmacen = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Almacen))
                Dim idxTipoListado = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.TipoListado))
                Dim idxEstatus = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Estatus))
                Dim idxFechaCreacion = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaCreacion))
                Dim idxNombre = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Nombre))
                Dim idxUsuarioCreador = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioCreador))
                Dim idxUsuarioCreadorPerfil = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioCreadorPerfil))
                Dim idxUsuarioAutorizador = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioAutorizador))
                Dim idxUsuarioAutorizadorPerfil = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioAutorizadorPerfil))
                Dim idxTipoCierre = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.TipoCierre))
                Dim idxFechaInicioConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaInicioConteo))
                Dim idxFechaFinConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaFinConteo))
                Dim idxFechaCierreConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaCierreConteo))
                Dim idxFechaTerminoConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaTerminoConteo))
                Dim idxSubinventario = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Subinventario))
                Dim idxIdProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.IdProducto))
                Dim idxCodigoProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CodigoProducto))
                Dim idxNombreProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.NombreProducto))
                Dim idxUnidad = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Unidad))
                Dim idxCantidadSistema = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CantidadSistema))
                Dim idxDiferencia = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Diferencia))
                Dim idxFaltante = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Faltante))
                Dim idxSobrante = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Sobrante))
                Dim idxFaltantePrecioCons = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FaltantePrecioCons))
                Dim idxSobrantePrecioCons = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.SobrantePrecioCons))
                Dim idxComentario = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Comentario))

                If Not reader.Read() Then
                    Return
                End If

                Do
                    If FilaVaciaPorIndices(reader, {idxCodigoListado, idxNumeroSAP, idxAlmacen, idxTipoListado, idxEstatus, idxFechaCreacion, idxNombre, idxUsuarioCreador, idxUsuarioCreadorPerfil, idxUsuarioAutorizador, idxUsuarioAutorizadorPerfil, idxTipoCierre, idxFechaInicioConteo, idxFechaFinConteo, idxFechaCierreConteo, idxFechaTerminoConteo, idxSubinventario, idxIdProducto, idxCodigoProducto, idxNombreProducto, idxUnidad, idxCantidadSistema, idxDiferencia, idxFaltante, idxSobrante, idxFaltantePrecioCons, idxSobrantePrecioCons, idxComentario}) Then
                        Exit Do
                    End If

                    Dim fila As DataRow = tabla.NewRow()

                    Dim _codigoListado = ObtenerTexto(reader.GetValue(idxCodigoListado))
                    fila("NumeroSAP") = ValorDataTable(ObtenerLong(reader.GetValue(idxNumeroSAP)))
                    fila("Almacen") = ValorDataTable(ObtenerTexto(reader.GetValue(idxAlmacen)))
                    fila("TipoListado") = ValorDataTable(ObtenerTexto(reader.GetValue(idxTipoListado)))
                    fila("Estatus") = ValorDataTable(ObtenerTexto(reader.GetValue(idxEstatus)))
                    fila("FechaCreacion") = ValorDataTable(ObtenerFecha(reader.GetValue(idxFechaCreacion)))
                    fila("Nombre") = ValorDataTable(ObtenerTexto(reader.GetValue(idxNombre)))
                    fila("UsuarioCreador") = ValorDataTable(ObtenerTexto(reader.GetValue(idxUsuarioCreador)))
                    fila("UsuarioCreadorPerfil") = ValorDataTable(ObtenerTexto(reader.GetValue(idxUsuarioCreadorPerfil)))
                    fila("UsuarioAutorizador") = ValorCadena(ObtenerTexto(reader.GetValue(idxUsuarioAutorizador)))
                    fila("UsuarioAutorizadorPerfil") = ValorCadena(ObtenerTexto(reader.GetValue(idxUsuarioAutorizadorPerfil)))
                    fila("TipoCierre") = If(String.IsNullOrWhiteSpace(ObtenerTexto(reader.GetValue(idxTipoCierre))), "", ObtenerTexto(reader.GetValue(idxTipoCierre)))
                    fila("FechaInicioConteo") = ValorDataTable(ObtenerFecha(reader.GetValue(idxFechaInicioConteo)))
                    fila("FechaFinConteo") = ValorDataTable(ObtenerFecha(reader.GetValue(idxFechaFinConteo)))
                    fila("FechaCierreConteo") = ValorDataTable(ObtenerFecha(reader.GetValue(idxFechaCierreConteo)))
                    fila("FechaTerminoConteo") = ValorFecha(ObtenerFecha(reader.GetValue(idxFechaTerminoConteo)))
                    fila("Subinventario") = ValorDataTable(ObtenerTexto(reader.GetValue(idxSubinventario)))
                    fila("IdProducto") = ValorDataTable(ObtenerTexto(reader.GetValue(idxIdProducto)))
                    fila("CodigoProducto") = ValorDataTable(ObtenerLong(reader.GetValue(idxCodigoProducto)))
                    fila("NombreProducto") = ValorDataTable(ObtenerTexto(reader.GetValue(idxNombreProducto)))
                    fila("Unidad") = ValorDataTable(ObtenerTexto(reader.GetValue(idxUnidad)))
                    fila("CantidadSistema") = ValorDataTable(ObtenerDecimal(reader.GetValue(idxCantidadSistema)))
                    fila("Diferencia") = ValorDataTable(ObtenerDecimal(reader.GetValue(idxDiferencia)))
                    fila("Faltante") = ValorDataTable(ObtenerDecimal(reader.GetValue(idxFaltante)))
                    fila("Sobrante") = ValorDataTable(ObtenerDecimal(reader.GetValue(idxSobrante)))
                    fila("FaltantePrecioCons") = ValorDataTable(ObtenerDecimal(reader.GetValue(idxFaltantePrecioCons)))
                    fila("SobrantePrecioCons") = ValorDataTable(ObtenerDecimal(reader.GetValue(idxSobrantePrecioCons)))
                    fila("Comentario") = If(String.IsNullOrWhiteSpace(ObtenerTexto(reader.GetValue(idxComentario))), "", ObtenerTexto(reader.GetValue(idxComentario)))

                    tabla.Rows.Add(fila)
                Loop While reader.Read()
            End Using
        End Using
    End Sub

    Private Function ValidacionesReglasArqueos(
        rutaArchivo As String,
        filaEncabezado As Integer,
        nombreHoja As String,
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Dim errores As New List(Of ExcelValidationError)

        Using stream = File.Open(rutaArchivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
            Using reader = ExcelReaderFactory.CreateReader(stream)
                If Not MoverAHoja(reader, nombreHoja) Then
                    Return errores
                End If

                For i As Integer = 1 To filaEncabezado
                    reader.Read()
                Next

                Dim encabezados As List(Of String) = LeerEncabezados(reader)

                If Not reader.Read() Then
                    Return errores
                End If

                Dim idxCodigoListado = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CodigoListado))
                Dim idxNumeroSAP = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.NumeroSAP))
                Dim idxAlmacen = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Almacen))
                Dim idxTipoListado = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.TipoListado))
                Dim idxEstatus = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Estatus))
                Dim idxFechaCreacion = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaCreacion))
                Dim idxNombre = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Nombre))
                Dim idxUsuarioCreador = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioCreador))
                Dim idxUsuarioCreadorPerfil = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioCreadorPerfil))
                Dim idxUsuarioAutorizador = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioAutorizador))
                Dim idxUsuarioAutorizadorPerfil = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.UsuarioAutorizadorPerfil))
                Dim idxTipoCierre = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.TipoCierre))
                Dim idxFechaInicioConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaInicioConteo))
                Dim idxFechaFinConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaFinConteo))
                Dim idxFechaCierreConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaCierreConteo))
                Dim idxFechaTerminoConteo = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FechaTerminoConteo))
                Dim idxSubinventario = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Subinventario))
                Dim idxIdProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.IdProducto))
                Dim idxCodigoProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CodigoProducto))
                Dim idxNombreProducto = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.NombreProducto))
                Dim idxUnidad = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Unidad))
                Dim idxCantidadSistema = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.CantidadSistema))
                Dim idxDiferencia = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Diferencia))
                Dim idxFaltante = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Faltante))
                Dim idxSobrante = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Sobrante))
                Dim idxFaltantePrecioCons = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.FaltantePrecioCons))
                Dim idxSobrantePrecioCons = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.SobrantePrecioCons))
                Dim idxComentario = ObtenerIndiceColumna(encabezados, mapeoColumnas, NameOf(ArqueosDetalleExcelDto.Comentario))

                Dim filaActual As Integer = filaEncabezado + 1

                Do
                    If FilaVaciaPorIndices(reader, {
                        idxCodigoListado, idxNumeroSAP, idxAlmacen, idxTipoListado, idxEstatus, idxFechaCreacion,
                        idxNombre, idxUsuarioCreador, idxUsuarioCreadorPerfil, idxUsuarioAutorizador,
                        idxUsuarioAutorizadorPerfil, idxTipoCierre, idxFechaInicioConteo, idxFechaFinConteo,
                        idxFechaCierreConteo, idxFechaTerminoConteo, idxSubinventario, idxIdProducto,
                        idxCodigoProducto, idxNombreProducto, idxUnidad, idxCantidadSistema, idxDiferencia,
                        idxFaltante, idxSobrante, idxFaltantePrecioCons, idxSobrantePrecioCons, idxComentario
                    }) Then
                        Exit Do
                    End If

                    ValidarFilaArqueos(
                        errores,
                        reader,
                        nombreHoja,
                        filaActual,
                        idxCodigoListado,
                        idxNumeroSAP,
                        idxAlmacen,
                        idxTipoListado,
                        idxEstatus,
                        idxFechaCreacion,
                        idxNombre,
                        idxUsuarioCreador,
                        idxUsuarioCreadorPerfil,
                        idxUsuarioAutorizador,
                        idxUsuarioAutorizadorPerfil,
                        idxTipoCierre,
                        idxFechaInicioConteo,
                        idxFechaFinConteo,
                        idxFechaCierreConteo,
                        idxFechaTerminoConteo,
                        idxSubinventario,
                        idxIdProducto,
                        idxCodigoProducto,
                        idxNombreProducto,
                        idxUnidad,
                        idxCantidadSistema,
                        idxDiferencia,
                        idxFaltante,
                        idxSobrante,
                        idxFaltantePrecioCons,
                        idxSobrantePrecioCons,
                        idxComentario)

                    filaActual += 1
                Loop While reader.Read()
            End Using
        End Using

        Return errores
    End Function

    Private Sub ValidarFilaArqueos(
        errores As List(Of ExcelValidationError),
        reader As IExcelDataReader,
        nombreHoja As String,
        filaActual As Integer,
        idxCodigoListado As Integer,
        idxNumeroSAP As Integer,
        idxAlmacen As Integer,
        idxTipoListado As Integer,
        idxEstatus As Integer,
        idxFechaCreacion As Integer,
        idxNombre As Integer,
        idxUsuarioCreador As Integer,
        idxUsuarioCreadorPerfil As Integer,
        idxUsuarioAutorizador As Integer,
        idxUsuarioAutorizadorPerfil As Integer,
        idxTipoCierre As Integer,
        idxFechaInicioConteo As Integer,
        idxFechaFinConteo As Integer,
        idxFechaCierreConteo As Integer,
        idxFechaTerminoConteo As Integer,
        idxSubinventario As Integer,
        idxIdProducto As Integer,
        idxCodigoProducto As Integer,
        idxNombreProducto As Integer,
        idxUnidad As Integer,
        idxCantidadSistema As Integer,
        idxDiferencia As Integer,
        idxFaltante As Integer,
        idxSobrante As Integer,
        idxFaltantePrecioCons As Integer,
        idxSobrantePrecioCons As Integer,
        idxComentario As Integer)

        Dim codigoListado = ObtenerTexto(reader.GetValue(idxCodigoListado))
        Dim numeroSAP = ObtenerLong(reader.GetValue(idxNumeroSAP))
        Dim almacen = ObtenerTexto(reader.GetValue(idxAlmacen))
        Dim tipoListado = ObtenerTexto(reader.GetValue(idxTipoListado))
        Dim estatus = ObtenerTexto(reader.GetValue(idxEstatus))
        Dim fechaCreacion = ObtenerFecha(reader.GetValue(idxFechaCreacion))
        Dim nombre = ObtenerTexto(reader.GetValue(idxNombre))
        Dim usuarioCreador = ObtenerTexto(reader.GetValue(idxUsuarioCreador))
        Dim usuarioCreadorPerfil = ObtenerTexto(reader.GetValue(idxUsuarioCreadorPerfil))
        Dim usuarioAutorizador = ObtenerTexto(reader.GetValue(idxUsuarioAutorizador))
        Dim usuarioAutorizadorPerfil = ObtenerTexto(reader.GetValue(idxUsuarioAutorizadorPerfil))
        Dim tipoCierre = ObtenerTexto(reader.GetValue(idxTipoCierre))
        Dim fechaInicioConteo = ObtenerFecha(reader.GetValue(idxFechaInicioConteo))
        Dim fechaFinConteo = ObtenerFecha(reader.GetValue(idxFechaFinConteo))
        Dim fechaCierreConteo = ObtenerFecha(reader.GetValue(idxFechaCierreConteo))
        Dim fechaTerminoConteo = ObtenerFecha(reader.GetValue(idxFechaTerminoConteo))
        Dim subinventario = ObtenerTexto(reader.GetValue(idxSubinventario))
        Dim idProducto = ObtenerTexto(reader.GetValue(idxIdProducto))
        Dim codigoProductoTexto = ObtenerTexto(reader.GetValue(idxCodigoProducto))
        Dim nombreProducto = ObtenerTexto(reader.GetValue(idxNombreProducto))
        Dim unidad = ObtenerTexto(reader.GetValue(idxUnidad))
        Dim cantidadSistema = ObtenerDecimal(reader.GetValue(idxCantidadSistema))
        Dim diferencia = ObtenerDecimal(reader.GetValue(idxDiferencia))
        Dim faltante = ObtenerDecimal(reader.GetValue(idxFaltante))
        Dim sobrante = ObtenerDecimal(reader.GetValue(idxSobrante))
        Dim faltantePrecioCons = ObtenerDecimal(reader.GetValue(idxFaltantePrecioCons))
        Dim sobrantePrecioCons = ObtenerDecimal(reader.GetValue(idxSobrantePrecioCons))
        Dim comentario = ObtenerTexto(reader.GetValue(idxComentario))

        If fechaCreacion.HasValue AndAlso fechaCreacion.Value > DateTime.Now Then
            AgregarError(errores, "La fecha de creación no puede ser posterior a la fecha de carga.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If String.IsNullOrWhiteSpace(codigoListado) Then
            AgregarError(errores, "Código de Listado no admite valores vacíos.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If fechaInicioConteo.HasValue AndAlso fechaFinConteo.HasValue AndAlso fechaInicioConteo.Value > fechaFinConteo.Value Then
            AgregarError(errores, "Inicio de Conteo debe ser menor o igual a Fin de Conteo.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If fechaFinConteo.HasValue AndAlso fechaCierreConteo.HasValue AndAlso fechaFinConteo.Value > fechaCierreConteo.Value Then
            AgregarError(errores, "Fin de Conteo debe ser menor o igual a Cierre de Conteo.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If fechaCierreConteo.HasValue AndAlso fechaTerminoConteo.HasValue AndAlso fechaCierreConteo.Value > fechaTerminoConteo.Value Then
            AgregarError(errores, "Cierre de Conteo debe ser menor o igual a Término de Conteo.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If faltante.HasValue AndAlso sobrante.HasValue AndAlso faltante.Value > 0D AndAlso sobrante.Value > 0D Then
            AgregarError(errores, "Faltante y Sobrante no pueden ser positivos al mismo tiempo.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If

        If diferencia.HasValue Then
            If diferencia.Value > 0D Then
                If Not sobrante.HasValue OrElse sobrante.Value <= 0D Then
                    AgregarError(errores, "Si la diferencia es positiva, Sobrante debe ser mayor que cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
                If faltante.HasValue AndAlso faltante.Value > 0D Then
                    AgregarError(errores, "Si la diferencia es positiva, Faltante debe ser cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
            ElseIf diferencia.Value < 0D Then
                If Not faltante.HasValue OrElse faltante.Value <= 0D Then
                    AgregarError(errores, "Si la diferencia es negativa, Faltante debe ser mayor que cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
                If sobrante.HasValue AndAlso sobrante.Value > 0D Then
                    AgregarError(errores, "Si la diferencia es negativa, Sobrante debe ser cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
            Else
                If (faltante.HasValue AndAlso faltante.Value > 0D) OrElse (sobrante.HasValue AndAlso sobrante.Value > 0D) Then
                    AgregarError(errores, "Si la diferencia es cero, Faltante y Sobrante deben ser cero.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
                End If
            End If
        End If

        If Not String.IsNullOrWhiteSpace(nombreProducto) AndAlso Not String.IsNullOrWhiteSpace(codigoProductoTexto) Then
            ' La consistencia exacta producto/código depende del catálogo maestro.
        End If

        If Not String.IsNullOrWhiteSpace(comentario) AndAlso comentario.Length > 500 Then
            AgregarError(errores, "Comentarios no puede exceder 500 caracteres.", $"Fila {filaActual}. Hoja <strong>{nombreHoja}</strong>.")
        End If
    End Sub

    Private Function LeerEncabezados(reader As IExcelDataReader) As List(Of String)
        Dim encabezados As New List(Of String)

        For i As Integer = 0 To reader.FieldCount - 1
            Dim valorEncabezado As String = If(reader.GetValue(i)?.ToString(), String.Empty).Trim()
            encabezados.Add(valorEncabezado)
        Next

        Return encabezados
    End Function

    Private Function ClonarMapeoConIndicesReales(
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute),
        encabezados As IList(Of String)) As Dictionary(Of PropertyInfo, ExcelColumnAttribute)

        Dim resultado As New Dictionary(Of PropertyInfo, ExcelColumnAttribute)

        For Each mapeo In mapeoColumnas
            Dim indiceReal As Integer = ObtenerIndiceColumna(encabezados, mapeoColumnas, mapeo.Key.Name)
            Dim atributoOriginal = mapeo.Value
            Dim atributoClonado As New ExcelColumnAttribute(indiceReal, atributoOriginal.ColumnName) With {
                .Requerido = atributoOriginal.Requerido,
                .ValoresIgnorados = atributoOriginal.ValoresIgnorados,
                .ColumnAliases = atributoOriginal.ColumnAliases
            }

            resultado(mapeo.Key) = atributoClonado
        Next

        Return resultado
    End Function

    Private Function ObtenerIndiceColumna(
        encabezados As IList(Of String),
        mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute),
        nombrePropiedad As String) As Integer

        Dim propiedad = mapeoColumnas.Keys.FirstOrDefault(Function(p) String.Equals(p.Name, nombrePropiedad, StringComparison.OrdinalIgnoreCase))

        If propiedad Is Nothing Then
            Return -1
        End If

        Dim atributo = mapeoColumnas(propiedad)
        Dim nombresEsperados As New List(Of String)

        If Not String.IsNullOrWhiteSpace(atributo.ColumnName) Then
            nombresEsperados.Add(atributo.ColumnName)
        End If

        If atributo.ColumnAliases IsNot Nothing AndAlso atributo.ColumnAliases.Length > 0 Then
            nombresEsperados.AddRange(atributo.ColumnAliases.Where(Function(x) Not String.IsNullOrWhiteSpace(x)))
        End If

        nombresEsperados.Add(nombrePropiedad)

        For i As Integer = 0 To encabezados.Count - 1
            Dim encabezadoActual = NormalizarTextoComparacionArqueos(encabezados(i))

            If nombresEsperados.Any(Function(nombreEsperado) String.Equals(encabezadoActual, NormalizarTextoComparacionArqueos(nombreEsperado), StringComparison.OrdinalIgnoreCase)) Then
                Return i
            End If
        Next

        Return -1
    End Function

    Private Function NormalizarTextoComparacionArqueos(valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Dim textoNormalizado = valor.Trim().Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        textoNormalizado = textoNormalizado.Normalize(System.Text.NormalizationForm.FormD)

        Dim caracteres = textoNormalizado.
            Where(Function(c) System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) <> System.Globalization.UnicodeCategory.NonSpacingMark).
            ToArray()

        Return New String(caracteres).Normalize(System.Text.NormalizationForm.FormC).ToLowerInvariant()
    End Function

    Private Function FilaVaciaPorIndices(reader As IExcelDataReader, indices As IEnumerable(Of Integer)) As Boolean
        For Each indiceColumna In indices
            If indiceColumna < 0 Then
                Continue For
            End If

            Dim valor = reader.GetValue(indiceColumna)

            If Not EsVacio(valor) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Function MoverAHoja(reader As IExcelDataReader, nombreHoja As String) As Boolean
        Do
            If String.Equals(reader.Name, nombreHoja, StringComparison.OrdinalIgnoreCase) Then
                Return True
            End If
        Loop While reader.NextResult()

        Return False
    End Function

    Private Function FilaVacia(reader As IExcelDataReader, mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As Boolean
        For Each mapeo In mapeoColumnas
            Dim indiceColumna As Integer = mapeo.Value.ColumnIndex
            Dim valor = reader.GetValue(indiceColumna)

            If Not EsVacio(valor) Then
                Return False
            End If
        Next

        Return True
    End Function

    Private Function ObtenerIndiceColumna(mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute), nombrePropiedad As String) As Integer
        Dim propiedad = mapeoColumnas.Keys.FirstOrDefault(Function(p) String.Equals(p.Name, nombrePropiedad, StringComparison.OrdinalIgnoreCase))

        If propiedad Is Nothing Then
            Return -1
        End If

        Return mapeoColumnas(propiedad).ColumnIndex
    End Function

    Private Function ObtenerTexto(valor As Object) As String
        If EsVacio(valor) Then
            Return Nothing
        End If

        Return valor.ToString().Trim()
    End Function

    Private Function ObtenerLong(valor As Object) As Nullable(Of Long)
        If EsVacio(valor) Then
            Return Nothing
        End If

        If TypeOf valor Is Long Then
            Return DirectCast(valor, Long)
        End If

        If TypeOf valor Is Integer OrElse TypeOf valor Is Short OrElse TypeOf valor Is Decimal OrElse TypeOf valor Is Double OrElse TypeOf valor Is Single Then
            Return Convert.ToInt64(valor, CultureInfo.InvariantCulture)
        End If

        Dim texto = valor.ToString().Trim()
        Dim resultado As Long

        If Long.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, resultado) Then
            Return resultado
        End If

        If Long.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, resultado) Then
            Return resultado
        End If

        Return Nothing
    End Function

    Private Function ObtenerDecimal(valor As Object) As Nullable(Of Decimal)
        If EsVacio(valor) Then
            Return Nothing
        End If

        If TypeOf valor Is Decimal Then
            Return DirectCast(valor, Decimal)
        End If

        If TypeOf valor Is Integer OrElse TypeOf valor Is Long OrElse TypeOf valor Is Double OrElse TypeOf valor Is Single Then
            Return Convert.ToDecimal(valor, CultureInfo.InvariantCulture)
        End If

        Dim texto = valor.ToString().Trim()
        Dim resultado As Decimal

        If Decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, resultado) Then
            Return resultado
        End If

        If Decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, resultado) Then
            Return resultado
        End If

        Return Nothing
    End Function

    Private Function ObtenerFecha(valor As Object) As Nullable(Of DateTime)
        If EsVacio(valor) Then
            Return Nothing
        End If

        If TypeOf valor Is DateTime Then
            Return DirectCast(valor, DateTime)
        End If

        Dim texto = NormalizarTextoFecha(valor.ToString())
        Dim resultado As DateTime
        Dim formatos As String() = {
            "dd/MM/yyyy",
            "d/MM/yyyy",
            "dd-MM-yyyy",
            "d-MM-yyyy",
            "dd/MM/yyyy HH:mm",
            "dd/MM/yyyy HH:mm:ss",
            "dd-MM-yyyy HH:mm",
            "dd-MM-yyyy HH:mm:ss",
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm",
            "yyyy-MM-dd HH:mm:ss",
            "MM/dd/yyyy",
            "M/d/yyyy",
            "MM/dd/yyyy HH:mm",
            "M/d/yyyy HH:mm",
            "MM/dd/yyyy HH:mm:ss",
            "M/d/yyyy HH:mm:ss"
        }

        If DateTime.TryParseExact(texto, formatos, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, resultado) Then
            Return resultado
        End If

        If DateTime.TryParse(texto, CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, resultado) Then
            Return resultado
        End If

        If DateTime.TryParse(texto, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, resultado) Then
            Return resultado
        End If

        Return Nothing
    End Function

    Private Function NormalizarTextoFecha(valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Dim texto = valor.Trim()
        texto = System.Text.RegularExpressions.Regex.Replace(
            texto,
            "\s*(a|p)\.?\s*m\.?",
            String.Empty,
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        )

        Return texto.Trim()
    End Function

    Private Function EsVacio(valor As Object) As Boolean
        Return valor Is Nothing OrElse valor Is DBNull.Value OrElse String.IsNullOrWhiteSpace(valor.ToString())
    End Function

    Private Function ValorDataTable(valor As Object) As Object
        If valor Is Nothing Then
            Return DBNull.Value
        End If

        Return valor
    End Function

    Private Function ValorFecha(valor As Nullable(Of DateTime)) As Object
        If valor.HasValue Then
            Return valor.Value
        End If

        Return DBNull.Value
    End Function

    Private Function ValorCadena(valor As String) As String
        If String.IsNullOrWhiteSpace(valor) Then
            Return ""
        End If

        Return valor
    End Function

    Private Sub AgregarError(errores As List(Of ExcelValidationError), problema As String, detalle As String)
        errores.Add(New ExcelValidationError With {
            .Problema = problema,
            .Detalle = detalle
        })
    End Sub

End Class
