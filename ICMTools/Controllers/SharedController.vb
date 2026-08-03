Imports System.Data.SqlClient
Imports System.Threading
Imports System.Web
Imports System.Web.Http
Imports DocumentFormat.OpenXml.Drawing.Diagrams
Imports DocumentFormat.OpenXml.Wordprocessing

Public Class SharedController
    Inherits ApiController
#Region "Variables Locales"
    Private ReadOnly mUser As User
    Private ReadOnly mLog As Log
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    Dim serviceData As New CustomServiceClass
#End Region
    Public Sub New()
        Me.mUser = CType(HttpContext.Current.Session.Item("User"), User)
        Me.mLog = New Log()
    End Sub
#Region "Metodos GET"
    '''<summary>
    '''Obtiene las Divisiones de Personal por Sociedad
    '''</summary>
    '''<returns>Una Lista de Divisiones de Personal.</returns>
    '''<remarks>
    '''Esta funcion suele ser usada para llenar listas de seleccion en el FrontEnd.
    '''</remarks>
    <HttpGet>
    <Route("api/shared/personneldivisions")>
    Public Function GetPersonnelDivision(Society As String) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")

        Try
            Thread.Sleep(1000)

            Dim myList As List(Of PersonnelDivisions)
            myList = serviceData.GetPersonnelDivisions(mUser.Model, mUser.Email, Society)

            Return Ok(New With {.l = myList})

        Catch ex As Exception
            Dim myList As ArrayList = Nothing
            Return Ok(New With {.l = myList})
        End Try
    End Function

    '''<summary>
    '''Obtiene las Divisiones de Personal por Sociedad
    '''</summary>
    '''<returns>Una Lista de Divisiones de Personal.</returns>
    '''<remarks>
    '''Esta funcion suele ser usada para llenar listas de seleccion en el FrontEnd.
    '''</remarks>
    <HttpGet>
    <Route("api/shared/personneldivisionsex")>
    Public Function GetPersonnelDivisionex(Society As String) As IHttpActionResult
        If Me.mUser Is Nothing Then Return BadRequest("Session Expired or User Not Authenticated")

        Try
            Thread.Sleep(1000)

            Dim myList As List(Of PersonnelDivisions)
            myList = serviceData.GetPersonnelDivisionsex(mUser.Model, mUser.Email, Society)

            Return Ok(New With {.l = myList})

        Catch ex As Exception
            Dim myList As ArrayList = Nothing
            Return Ok(New With {.l = myList})
        End Try
    End Function
    '''<summary>
    '''Obtiene Sociedades
    '''</summary>
    '''<returns>Una lista de Sociedades.</returns>
    '''<remarks>
    '''Esta funcion suele usarse para llenar listas de seleccion en el FrontEnd
    '''</remarks>
    <HttpGet>
    <Route("api/shared/societies")>
    Public Function GetSocieties() As IHttpActionResult
        Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)

        If mUser Is Nothing Then Return BadRequest("Session Expired")

        Dim result = New List(Of Societies)

        result = serviceData.GetSocieties(mUser.Model, mUser.Email)
        Return Ok(result)
    End Function

    <HttpGet>
    <Route("api/shared/successmessage")>
    Public Function GetSuccessMessage(pantalla As String, cantidadTotal As Integer) As IHttpActionResult
        Dim mensaje As String = GetMessage(pantalla, "CargaCompleta", cantidadTotal)
        Return Ok(New With {.d = 1, .r = mensaje})
    End Function

#End Region
#Region "Funciones"

    ''' <summary>
    ''' Método que genera el mensaje del controller.
    ''' </summary>
    ''' <param name="pantalla">Nombre de la Pantalla</param>
    ''' <param name="tipo">Tipo de Mensaje: CargaCompleta, CargaParcial, SinImportacion, NominaInvalida, Duplicados, SinRegistros, RegistrosInválidos, Error.</param>
    ''' <returns>Regresa el mensaje.</returns>
    Public Function GetMessage(pantalla As String, tipo As String)
        Return GetMessage(pantalla, tipo, Nothing, Nothing, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Método que genera el mensaje del controller.
    ''' </summary>
    ''' <param name="pantalla">Nombre de la Pantalla</param>
    ''' <param name="tipo">Tipo de Mensaje: CargaCompleta, CargaParcial, SinImportacion, NominaInvalida, Duplicados, SinRegistros, RegistrosInvalidos, Error.</param>
    ''' <param name="cantidadRegistros">Cantidad de registros por importar.</param>
    ''' <returns>Regresa el mensaje.</returns>
    Public Function GetMessage(pantalla As String, tipo As String, cantidadRegistros As Integer)
        Return GetMessage(pantalla, tipo, Nothing, Nothing, cantidadRegistros, Nothing)
    End Function

    ''' <summary>
    ''' Método que genera el mensaje del controller.
    ''' </summary>
    ''' <param name="pantalla">Nombre de la Pantalla</param>
    ''' <param name="tipo">Tipo de Mensaje: CargaCompleta, CargaParcial, SinImportacion, NominaInvalida, Duplicados, SinRegistros, RegistrosInvalidos, Error.</param>
    ''' <param name="cantidadRegistros">Cantidad de registros por importar.</param>
    ''' <param name="cantidadErroneos">Cantidad de registros erróneos.</param>
    ''' <returns>Regresa el mensaje.</returns>
    Public Function GetMessage(pantalla As String, tipo As String, cantidadRegistros As Integer, cantidadErroneos As Integer)
        Return GetMessage(pantalla, tipo, Nothing, Nothing, cantidadRegistros, cantidadErroneos)
    End Function

    ''' <summary>
    ''' Método que genera el mensaje del controller.
    ''' </summary>
    ''' <param name="pantalla">Nombre de la Pantalla</param>
    ''' <param name="tipo">Tipo de Mensaje: CargaCompleta, CargaParcial, SinImportacion, NominaInvalida, Duplicados, SinRegistros, RegistrosInvalidos, Error.</param>
    ''' <param name="columnas">Nombre de las columnas.</param>
    ''' <param name="valores">Valores de cada columna.</param>
    ''' <returns>Regresa el mensaje.</returns>
    Public Function GetMessage(pantalla As String, tipo As String, columnas As List(Of String), valores As List(Of String))
        Return GetMessage(pantalla, tipo, columnas, valores, Nothing, Nothing)
    End Function

    ''' <summary>
    ''' Método que genera el mensaje del controller.
    ''' </summary>
    ''' <param name="pantalla">Nombre de la Pantalla</param>
    ''' <param name="tipo">Tipo de Mensaje: CargaCompleta, CargaParcial, SinImportacion, NominaInvalida, Duplicados, SinRegistros, RegistrosInvalidos, Error.</param>
    ''' <param name="columnas">Nombre de las columnas.</param>
    ''' <param name="valores">Valores de cada columna.</param>
    ''' <param name="cantidadRegistros">Cantidad de registros por importar.</param>
    ''' <returns>Regresa el mensaje.</returns>
    Public Function GetMessage(pantalla As String, tipo As String, columnas As List(Of String), valores As List(Of String), cantidadRegistros As Integer)
        Return GetMessage(pantalla, tipo, columnas, valores, cantidadRegistros, Nothing)
    End Function

    ''' <summary>
    ''' Método que genera el mensaje del controller.
    ''' </summary>
    ''' <param name="pantalla">Nombre de la Pantalla</param>
    ''' <param name="tipo">Tipo de Mensaje: CargaCompleta, CargaParcial, ProcesoIncompleto, SinImportacion, NominaInvalida, Duplicados, SinRegistros, RegistrosInvalidos, Error.</param>
    ''' <param name="columnas">Nombre de las columnas.</param>
    ''' <param name="valores">Valores de cada columna.</param>
    ''' <returns>Regresa el mensaje.</returns>
    Public Function GetMessage(pantalla As String, tipo As String, columnas As List(Of String), valores As List(Of String), cantidadRegistros As Integer, cantidadErroneos As Integer)
        Dim mensaje As New StringBuilder("")
        Dim seccion As Integer = 0

        If (tipo.ToLower().Equals("cargacompleta")) Then
            seccion = 3
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width:50%;'>Ejecución Completada Exitosamente</td>")
            mensaje.Append("    <td style='width:50%;'>Se ejecutó correctamente el proceso externo<br><strong>Carga de " + pantalla + "</strong>.</td>")
            mensaje.Append("</tr>")
            If (cantidadRegistros > 0) Then
                mensaje.Append("<tr>")
                mensaje.Append("    <td style='width: 50%;'>Cantidad de registros</td>")
                mensaje.Append("    <td style='width:50%;'>Enviados: " & cantidadRegistros.ToString("N0") & ", Insertados: " & cantidadRegistros.ToString("N0") & ".</td>")
                mensaje.Append("</tr>")
            End If
        ElseIf (tipo.ToLower().Equals("cargaparcial")) Then
            seccion = 3
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width:50%;'>Ejecución Parcial Completada</td>")
            mensaje.Append("    <td style='width:50%;'>Se ejecutó parcialmente el proceso.<br><strong>Carga de " + pantalla + "</strong>.<br>Por favor revisa el archivo anexo de errores.</td>")
            mensaje.Append("</tr>")
            If (cantidadRegistros > 0) Then
                mensaje.Append("<tr>")
                mensaje.Append("    <td style='width: 50%;'>Cantidad de registros</td>")
                mensaje.Append("    <td style='width:50%;'>Enviados: " & cantidadRegistros.ToString("N0") & ", Insertados: " & (cantidadRegistros - cantidadErroneos).ToString("N0") & ", Inválidos: " & cantidadErroneos.ToString("N0") & ".</td>")
                mensaje.Append("</tr>")
            End If
        ElseIf (tipo.ToLower().Equals("procesoincompleto")) Then
            seccion = 3
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width:50%;'>Proceso Incompleto</td>")
            mensaje.Append("    <td style='width:50%;'>No fue posible concluir la <strong>Carga de " + pantalla + "</strong> debido a la detección de registros inválidos.<br/>Por favor revisa el archivo anexo de errores.</td>")
            mensaje.Append("</tr>")
            If (cantidadRegistros > 0) Then
                mensaje.Append("<tr>")
                mensaje.Append("    <td style='width: 50%;'>Cantidad de registros</td>")
                mensaje.Append("    <td style='width:50%;'>Enviados: " & cantidadRegistros.ToString("N0") & ", Inválidos: " & cantidadErroneos.ToString("N0") & ".</td>")
                mensaje.Append("</tr>")
            End If
        ElseIf (tipo.ToLower().Equals("sinimportacion")) Then
            seccion = 1
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width: 50%;'>Error al ejecutar el proceso de importación.</td>")
            mensaje.Append("    <td style='width:50%;'>No se encontró información válida en la <strong>Carga de " + pantalla + ".</strong><br>Por favor verifique la información del archivo.</td>")
            mensaje.Append("</tr>")
            If (cantidadRegistros > 0) Then
                mensaje.Append("<tr>")
                mensaje.Append("    <td style='width: 50%;'>Cantidad de registros</td>")
                mensaje.Append("    <td style='width:50%;'>Enviados: " & cantidadRegistros.ToString("N0") & ", Insertados: " & (cantidadRegistros - cantidadErroneos).ToString("N0") & ", Inválidos: " & cantidadErroneos.ToString("N0") & ".</td>")
                mensaje.Append("</tr>")
            End If
        ElseIf (tipo.ToLower().Equals("registrosinvalidos")) Then
            seccion = 1
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width: 30%;'>Error al ejecutar el proceso de importación.</td>")
            mensaje.Append("    <td style='width:70%;'>Se encontró información inválida en la <strong>Carga de " + pantalla + ".</strong><br>Por favor verifique la información del archivo.</td>")
            mensaje.Append("</tr>")
            If (cantidadRegistros > 0) Then
                mensaje.Append("<tr>")
                mensaje.Append("    <td style='width: 30%;'>Cantidad de registros</td>")
                mensaje.Append("    <td style='width:70%;'> " & cantidadRegistros.ToString("N0") & "</td>")
                mensaje.Append("</tr>")
            End If
            If (columnas.Count > 0 And valores.Count > 0) Then
                mensaje.Append("<tr>")
                mensaje.Append("    <td style='width: 30%;'>Registros inválidos</td>")
                mensaje.Append("    <td style='width:70%;'>")
                mensaje.Append("        <table id='Table' class='table table-sm table-hover'>")
                mensaje.Append("            <thead>")
                mensaje.Append("            </thead>")
                mensaje.Append("                <tr>")
                For Each columna As String In columnas
                    mensaje.Append("                <th>" + columna + "</th>")
                Next
                mensaje.Append("                </tr>")
                mensaje.Append("            <tbody>")
                For Each fila As String In valores
                    mensaje.Append("                <tr>")
                    For Each valor As String In fila.Split("|")
                        mensaje.Append("                <td>" + valor + "</td>")
                    Next
                    mensaje.Append("                </tr>")
                Next
                mensaje.Append("            </tbody>")
                mensaje.Append("        </table>")
                mensaje.Append("    </td>")
                mensaje.Append("</tr>")
            End If
        ElseIf (tipo.ToLower().Equals("duplicados")) Then
            seccion = 1
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width: 50%;'>Error al ejecutar el proceso de importación.</td>")
            mensaje.Append("    <td style='width:50%;'>Se encontraron registros duplicados en la <strong>Carga de " + pantalla + ".</strong><br>Por favor verifique la información del archivo.</td>")
            mensaje.Append("</tr>")
            If (cantidadRegistros > 0) Then
                mensaje.Append("<tr>")
                mensaje.Append("    <td style='width: 50%;'>Cantidad de registros</td>")
                mensaje.Append("    <td style='width:50%;'>Enviados: " & cantidadRegistros.ToString("N0") & ", Insertados: " & (cantidadRegistros - cantidadErroneos).ToString("N0") & ", Duplicados: " & cantidadErroneos.ToString("N0") & ".</td>")
                mensaje.Append("</tr>")
            End If
        ElseIf (tipo.ToLower().Equals("sinregistros")) Then
            seccion = 1
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width: 50%;'>Error al ejecutar el proceso de importación.</td>")
            mensaje.Append("    <td style='width:50%;'>No se encontraron registros en la <strong>Carga de " + pantalla + ".</strong><br>Por favor verifique el archivo.</td>")
            mensaje.Append("</tr>")
        ElseIf (tipo.ToLower().Equals("nominainvalida")) Then
            seccion = 1
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width: 50%;'>Error al ejecutar el proceso de importación.</td>")
            mensaje.Append("    <td style='width:50%;'>El nombre del archivo del proceso de <br><strong>Carga de " + pantalla + "</strong> debe contener un CCNominas válido.<br>El nombre del archivo debe finalizar con un guion bajo y una clave de CCNomina (Ejemplo: NombreArchivo_118A.xlsx)</td>")
            mensaje.Append("</tr>")
        ElseIf (tipo.ToLower().Equals("error")) Then
            seccion = 1
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width: 30%;'>Error al ejecutar el proceso de validación del archivo de " + pantalla + ".</td>")
            mensaje.Append("    <td style='width: 70%;'>")
            mensaje.Append("        Se han presentado inconvenientes al ejecutar el proceso que carga el archivo.")
            mensaje.Append("        <br><br>Por favor verifique la integridad de la información.")
            mensaje.Append("        <ul>")
            mensaje.Append("            <li>El archivo debe contener solo " + columnas.Count.ToString() + " columnas.</li>")
            mensaje.Append("            <li>El archivo debe contener solo " & columnas.Count.ToString() & " columnas.</li>")
            mensaje.Append("            <li>Las columnas deben estar en el orden mencionado.</li>")
            mensaje.Append("            <li>Ejemplo de archivo:</li>")
            mensaje.Append("            <table style=""width:400px;"">")
            mensaje.Append("                <tr>")
            For Each columna As String In columnas
                mensaje.Append("                    <th>" + columna + "</th>")
            Next
            mensaje.Append("                </tr>")
            mensaje.Append("                <tr>")
            For Each valor As String In valores
                mensaje.Append("                    <th>" + valor + "</th>")
            Next
            mensaje.Append("                </tr>")
            mensaje.Append("            </table>")
            mensaje.Append("        </ul>")
            mensaje.Append("    </td>")
            mensaje.Append("</tr>")
        Else
            seccion = 1
            mensaje.Append("<tr>")
            mensaje.Append("    <td style='width: 50%;'>Error al ejecutar el proceso de importación.</td>")
            mensaje.Append("    <td style='width:50%;'>Ocurrió un problema con la carga del archivo de <strong>" + pantalla + "<strong>.</td>")
            mensaje.Append("</tr>")
        End If

        Dim html As String = TableBuilder(mensaje.ToString(), seccion)
        Return html

    End Function

    Public Function GetListaExcel(ExcelArray As Object)
        Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
        Dim usedColumns As Integer = ExcelArray.GetUpperBound(1)
        Dim listaexcel = New List(Of String)
        For row As Integer = 2 To usedRows
            Dim excel As String = ExcelArray(row, 1)
            listaexcel.Add(excel)
        Next

        Return listaexcel
    End Function
    '''<summary>
    '''Construye una tabla para mostrar informacion en el FrontEnd
    '''</summary>
    '''<returns>Un elemento HTML.</returns>
    '''<remarks>
    '''Esta funcion Retorna la tabla que se retornara como respuesta a un proceso.
    '''</remarks>
    Public Function TableBuilder(elementList As String, Section As Integer)
        Dim elementTable As String = Nothing
        Select Case Section
            Case 1
                elementTable = "<table id='Table' class='table table-sm table-hover'>" +
                               "<thead>" + "<tr>" + "<th>Problema</th>" + "<th>Detalles</th>" +
                               "</tr>" + "</thead> " + "<tbody> " + "@LISTA@" + "</tbody> " + "</table>"
                elementTable = elementTable.Replace("@LISTA@", String.Join("", elementList))
                Return elementTable
            Case 2
                elementTable = "<table id='Table' class='table table-sm table-hover'>" + "<thead>" + "<tr>" +
                              "<th>Problema</th>" + "<th>IDStore</th>" + "<th>Date</th>" + "<th>Value</th>" +
                              "</tr>" + "</thead> " + "<tbody> " + "@LISTA@" + "</tbody> " + "</table>"
                elementTable = elementTable.Replace("@LISTA@", String.Join("", elementList))
                Return elementTable
            Case 3
                elementTable = "<table id='Table' class='table table-sm table-hover'>" + "<thead>" + "<tr>" +
                               "<th>Estatus</th>" + "<th>Mensaje</th>" + "</tr>" + "</thead> " + "<tbody> " +
                               "@LISTA@" + "</tbody> " + "</table>"
                elementTable = elementTable.Replace("@LISTA@", String.Join("", elementList))
                Return elementTable
        End Select
        Return Nothing
    End Function
#End Region
End Class