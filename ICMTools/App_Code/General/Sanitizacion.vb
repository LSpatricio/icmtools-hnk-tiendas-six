Imports System.IO

Public Class Sanitizacion

#Region " Propiedades Privadas "

    ''' <summary>
    ''' Módulos permitidos
    ''' </summary>
    Private ReadOnly _allowedModulos As String() = {
        "Categoria\EmpleadosActivos\Final",
        "Categoria\EmpleadosActivos\Inicial",
        "Categoria\MetaCategoria",
        "Categoria\MontoDistribuible",
        "Categoria\PorcentajeVenta",
        "Categoria\VentaMonto",
        "Categoria\VentaUnidades",
        "Clasificaciones",
        "EmpleadosLideres",
        "Excepciones",
        "ExcepcionesTiendas",
        "IncentivoCerveza\CategoriaMontosMetas",
        "IncentivoCerveza\CategoriaMontosVentas",
        "IncentivoCerveza\ConfiguracionPorcentajeVentas",
        "IncentivoCerveza\ConfiguracionDistribuciones",
        "IncentivoCerveza\HistEmpleadosActivos\Inicial",
        "IncentivoCerveza\HistEmpleadosActivos\Final",
        "MultiTiendaVariable\Entrada",
        "MultiTiendaVariable\EntradaEnfoque",
        "MultiTiendaVariable\EntradaVentas",
        "Multi_Tienda_Fijo_Entrada",
        "PagosManuales",
        "Tiendas",
        "TiendasGanadoras",
        "Servicios\CuotaServicio",
        "Servicios\VentaServicios",
        "VentaSugerida\Metas",
        "VentaSugerida\ImportVentas"
    }

    Private ReadOnly mLog As New Log()
    Private ReadOnly AllowedExtension As String() = {".xlsx", ".csv", ".xls"}

#End Region

#Region " Métodos Públicos "

    ''' <summary>
    ''' Sanitiza el texto a mostrar en un excel
    ''' </summary>
    ''' <param name="value">Valor</param>
    ''' <returns>Regresa valor sanitizado</returns>
    Public Function ExcelTexto(value As String) As String
        If String.IsNullOrEmpty(value) Then Return String.Empty

        Dim trimmed = value.Trim()

        If trimmed.StartsWith("=") OrElse
       trimmed.StartsWith("+") OrElse
       trimmed.StartsWith("-") OrElse
       trimmed.StartsWith("@") Then
            Return "'" & trimmed
        End If

        Return trimmed
    End Function

    ''' <summary>
    ''' Obtiene el módulo seguro
    ''' </summary>
    ''' <param name="input">Entrada</param>
    ''' <returns>Regresa el modulo</returns>
    Friend Function GetSafeModulo(input As String) As String

        For Each modulo As String In _allowedModulos
            If String.Equals(modulo, input, StringComparison.OrdinalIgnoreCase) Then
                Return modulo
            End If
        Next

        Return "INVALID"
    End Function

    ''' <summary>
    ''' Obtiene la extensión segura
    ''' </summary>
    ''' <param name="input">Entrada</param>
    ''' <returns>Regresa la extensión</returns>
    Public Function GetSafeExtension(input As String) As String
        Select Case input.ToLower().Trim()
            Case ".csv" : Return ".csv"
            Case ".xlsx" : Return ".xlsx"
            Case ".xls" : Return ".xls"
            Case Else : Return "INVALID"
        End Select
    End Function

    ''' <summary>
    ''' Sanitizar Texto
    ''' </summary>
    ''' <param name="valor">Valor</param>
    ''' <returns>Regresa valor sanitizado</returns>
    Public Function Texto(valor As String) As String

        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Dim result As String = valor.Trim()

        ' Normalizar espacios múltiples
        result = Regex.Replace(result, "\s+", " ")

        ' Quitar caracteres no deseados
        ' Permite letras, números, espacio, guión, punto y guión bajo
        result = Regex.Replace(result, "[^a-zA-Z0-9\s\.\-_]", String.Empty)

        Return result

    End Function

    ''' <summary>
    ''' Sanitiza Texto a DateTime
    ''' </summary>
    ''' <param name="valor">Valor</param>
    ''' <param name="formato">Formato</param>
    ''' <returns>Regresa valor</returns>
    Public Function TextoADateTime(valor As String, formato As String) As DateTime
        If String.IsNullOrWhiteSpace(valor) Then
            Return String.Empty
        End If

        Dim result As DateTime
        Dim cleanedValue As String = valor.Trim()

        If DateTime.TryParseExact(cleanedValue, formato, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, result) Then
            Return result
        End If

        Return String.Empty
    End Function

    ''' <summary>
    ''' Sanitiza Texto a Double
    ''' </summary>
    ''' <param name="valor">Valor</param>
    ''' <returns>Regresa valor</returns>
    Public Function TextoADouble(valor As String) As Double
        Dim result As Double = If(Double.TryParse(valor, result), result, 0)
        Return result
    End Function

    ''' <summary>
    ''' Sanitiza FileType y Extension obtenidos del Body de una peticion POST
    ''' </summary>
    ''' <param name="FileType">Pantalla Correspondiente (request.FileType)</param>
    ''' <param name="Extension">Extension del archivo (requst.Extension)</param>
    ''' <returns></returns>
    Public Function SanitizePathComponents(FileType As String, Extension As String) As String()
        Try
            Dim safeExtension As String = GetSafeExtension(Extension)
            If safeExtension = "INVALID" Then
                mLog.insertLog("Sanitize", "Excepcion Controlada", "Extension de archivo Incorrecta: " & Extension)
                Throw New Exception("Extensión no Permitida")
            End If

            Dim SafeFileType As String = GetSafeModulo(FileType)
            If SafeFileType = "INVALID" Then
                Throw New Exception("FileType no Permitido")
            End If

            Return {safeFileType, safeExtension}
        Catch ex As Exception
            If ex.Message.Contains("no Permit") Then
                Throw
            Else
                mLog.insertLog("SanitizeController", "Error critico", String.Format("Ocurrio un error al sanitizar los parametros para construir el Path: {0}", ex.Message))
                Throw New Exception("Error interno al procesar la ruta del archivo.")
            End If
        End Try
    End Function

#End Region

End Class
