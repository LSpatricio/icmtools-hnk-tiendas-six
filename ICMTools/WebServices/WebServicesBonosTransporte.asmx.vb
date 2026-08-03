Imports System.ComponentModel
Imports System.Web.Services
Imports System.Web.Script.Services

' Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente.
<System.Web.Script.Services.ScriptService()>
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ToolboxItem(False)>
Public Class WebServicesBonosTransporte
    Inherits System.Web.Services.WebService

    Private mLog As Log
    Dim ws As WebServiceICMGeneral

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function getDataInit(_type As String) As List(Of BonoTransporteLote)
        If Session.Item("User") IsNot Nothing Then
            Dim mUser As User = CType(Session.Item("User"), User)
            Return BonoTransporteLote.GetBonosByUser(mUser.Email, _type)
        Else
            HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
        Return New List(Of BonoTransporteLote)

    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function saveFile(extension As String, idSociedad As String, idDivision As String, idPeriod As String, comment As String) As BonoTransporteLote
        Dim bonos = New BonosUpload()
        Dim mUser As User
        If Session.Item("User") IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
        Else
            HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            Return Nothing
        End If

        Dim ExcelArray(,) As Object = bonos.GetDocumentArray(mUser.Email, extension)

        If ExcelArray IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
            Dim usedRows As Integer = ExcelArray.GetUpperBound(0)
            Dim usedColumns As Integer = ExcelArray.GetUpperBound(1)
            Dim bonoEnvio As New BonoTransporteLote()
            bonoEnvio.SocietyId = idSociedad
            bonoEnvio.DivisionID = idDivision
            bonoEnvio.PeriodoId = idPeriod
            bonoEnvio.CreationComment = comment

            Dim bono = BonoTransporteLote.UpsertBonos(mUser.Email, "I", bonoEnvio)
            Dim dt = bonos.createDatatable()
            'Validaciones'
            For row As Integer = 2 To usedRows
                Dim dataRow As DataRow = dt.NewRow()
                Dim ErrorValidacion = String.Empty
                Dim payee = ExcelArray(row, 1)
                Dim dateVal = ExcelArray(row, 2)
                Dim ccNom = ExcelArray(row, 3)
                Dim Amount = ExcelArray(row, 4)

                If (
                    Not String.IsNullOrWhiteSpace(payee) And
                    Not String.IsNullOrWhiteSpace(dateVal) And
                    Not String.IsNullOrWhiteSpace(ccNom) And
                    Not String.IsNullOrWhiteSpace(Amount)) Then

                    Dim dtval As Date = Nothing
                    Dim amounVal As Decimal = -1

                    Date.TryParse(dateVal, dtval)
                    Decimal.TryParse(Amount, amounVal)

                    If dtval = Date.MinValue Then
                        ErrorValidacion = "El formato de la fecha es incorrecta"
                        dateVal = "01/01/1900"
                    End If

                    If amounVal <= 0 Then
                        ErrorValidacion = "El monto no es valido"
                        Amount = "-1"
                    End If

                    dataRow("IDBono") = bono.IDBono
                    dataRow("Payee") = payee
                    dataRow("Date") = dateVal
                    dataRow("CCNom") = ccNom
                    dataRow("Amount") = Amount
                    dataRow("Reason") = ExcelArray(row, 5)
                    dataRow("Status") = "I"

                    If Not String.IsNullOrEmpty(ErrorValidacion) Then
                        dataRow("Status") = "E"
                    End If
                    dataRow("MessageResponse") = ErrorValidacion
                    If (bonos.ValidateDataRow(dataRow)) Then
                        dt.Rows.Add(dataRow)
                    End If
                End If
            Next
            ExcelArray = Nothing 'para liberar memoria
            Dim result = BonoTransporteLote.setBulkBonosTransporte(dt)
            BonoTransporteLote.UpsertBonos(mUser.Email, "V", New BonoTransporteLote(bono.IDBono))
            ' System.IO.File.Delete(Server.MapPath("~\UploadedFiles\BonosTransporte\" + mUser.Email + ".xlsx"))
            Return bono
        End If

        Return New BonoTransporteLote()

    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function validateFile(idBono As Int32) As List(Of BonoTransporteDetail)

        Dim mUser As User
        If Session.Item("User") IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
        Else
            HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If

        Dim bonos As List(Of BonoTransporteDetail) = New List(Of BonoTransporteDetail)

        Try
            Dim VALIDACION As Boolean = BonoTransporteDetail.ValidateBonosTransporte(idBono)
            If (VALIDACION) Then
                bonos = BonoTransporteDetail.GetBonosDetailByID(idBono, 0)
            Else
                Throw New Exception("Error al realizar las validaciones de la informacion, comuniquese con el administrador del sistema ")
            End If
        Catch ex As Exception
            Throw New Exception("Error al realizar las validaciones de la informacion, comuniquese con el administrador del sistema ")

        End Try

        Return bonos
    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UpsertBonosTransporte(idBono As Int32, statusBono As String, comment As String, idSociedad As String, idDivision As String, idPeriod As String) As BonoTransporteLote
        Dim bono = New BonoTransporteLote()
        Dim mUser As User
        If Session.Item("User") IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
        Else
            HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            Return Nothing
        End If
        Dim param = New BonoTransporteLote(idBono)
        param.CreationComment = comment
        param.DivisionID = idDivision
        param.SocietyId = idSociedad
        param.PeriodoId = idPeriod
        bono = BonoTransporteLote.UpsertBonos(mUser.Email, statusBono, param)
        'If (statusBono = "P") Then
        System.IO.File.Delete(Server.MapPath("~\UploadedFiles\BonosTransporte\" + mUser.Email + ".xlsx"))
        'End If
        Return bono
    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function getBonosTransporteDetail(idBono As Int32, onlyActive As Int32) As List(Of BonoTransporteDetail)
        Dim mUser As User
        If Session.Item("User") IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
        Else
            HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
        Dim bonos As List(Of BonoTransporteDetail) = New List(Of BonoTransporteDetail)

        Return BonoTransporteDetail.GetBonosDetailByID(idBono, onlyActive)
    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function getSocietyDivision() As List(Of SocietyDivision)
        Dim mUser As User
        If Session.Item("User") IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
        Else
            HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            Return Nothing
        End If

        Dim result As List(Of SocietyDivision) = New List(Of SocietyDivision)
        Try
            result = BonosDeTransporteCustom.GetSocietyDivisionByUser(mUser.Email, mUser.Model)
        Catch ex As Exception
            Throw New Exception("Error al obtener la sociedades y division del usuario ")
        End Try

        Return result
    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function getPeriod() As List(Of DatePeriod)
        Dim mUser As User
        If Session.Item("User") IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
        End If
        Dim result As List(Of DatePeriod) = New List(Of DatePeriod)
        Try
            result = DatePeriod.GetPeriod()
        Catch ex As Exception
            Throw New Exception("Error al obtener los periodos ")
        End Try
        Return result
    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function UploadValidations(idBono As Int32, idDivision As String, idSociety As String) As List(Of DictionaryItem)
        Dim Authorizer = New RemplazoICMTools()
        Dim mUser As User = New User()
        Dim mensaje As String = String.Empty
        Dim listaParaDevolver As New List(Of DictionaryItem)()
        Dim parametersConf As New MultipleParameter()
        Dim maskModel As String
        Try
            If Session.Item("User") IsNot Nothing Then
                mUser = CType(Session.Item("User"), User)
            Else
                HttpContext.Current.Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

            If mUser.Model = "DEBUG" Then
                maskModel = "femcoepdev"
            Else
                maskModel = mUser.Model
            End If

            Dim PayeeID As String = ws.GetPayeeByUserEmail(mUser.Email, maskModel)
            Dim LastHistoryPayee As String = ws.GetLastHistoryPayee(PayeeID, maskModel)
            Dim AuthorizedPosition As String = RemplazoICMTools.GetAuthorizedPosition(LastHistoryPayee, "BonosTransporte")
            Dim AuthorizedPayee As String = ws.GetAuthorizedPayee(AuthorizedPosition, maskModel)
            Dim FinalTable As New DataTable()
            Dim PayeeTable As New DataTable()

            If AuthorizedPayee Is Nothing Then
                Dim ListPayee As List(Of String) = RemplazoICMTools.GetPayeeList()
                Dim PayeeList As DataTable = ws.GetAuthorizedPayeeList(ListPayee, maskModel)
                FinalTable = ws.GetFinalTable(PayeeList, maskModel)
            Else
                FinalTable = ws.SemiFinalElseGetLastHistoryPayeeBT(AuthorizedPayee, maskModel)
            End If

            PayeeTable = ws.GetPayeesBTReemplazos(FinalTable, maskModel)

            Authorizer = RemplazoICMTools.GetAuthorizerByPosition(FinalTable, PayeeTable)

            If (Authorizer.IDPosition = String.Empty) Then
                mensaje = "No hay un autorizador disponible, favor de comunicarte con el administrador del sistema"
            End If

            parametersConf = MultipleParameter.ParametersByKey(idDivision, idSociety, "BONOSM001")

            If (parametersConf Is Nothing) Then
                mensaje = "No hay una media disponible para esta division , favor de comunicarte con el administrador del sistema"
            End If

        Catch ex As Exception
            mensaje = "Lo sentimos, ha ocurrido un error inesperado. Por favor, intente de nuevo. Si el problema persiste, contacte al administrador."
        End Try

        listaParaDevolver.Add(New DictionaryItem("Msg", mensaje))
        listaParaDevolver.Add(New DictionaryItem("Autorizador", Authorizer))
        listaParaDevolver.Add(New DictionaryItem("Media", parametersConf))

        Return listaParaDevolver
    End Function

End Class


