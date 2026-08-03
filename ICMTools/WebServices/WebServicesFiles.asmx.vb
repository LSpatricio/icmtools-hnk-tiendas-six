Imports System.Web.Services
Imports System.Web.Services.Protocols
Imports System.ComponentModel

' To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
'Se descomenta la linia siguiente para permitir llamar desde script usando AJAX
<System.Web.Script.Services.ScriptService()> _
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")> _
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)> _
<ToolboxItem(False)> _
Public Class WebServicesFiles
    Inherits System.Web.Services.WebService

    Private mUser As User

    'Se habilita uso de variables de sesion
    'EnableSession = True
    <WebMethod(True)> _
    Public Function CheckFileExists(FileType As String, Extension As String) As Boolean
        Try
            If Not Session.Item("User") Is Nothing Then

                mUser = CType(Session.Item("User"), User)

                Dim FileToFind As String = Server.MapPath("~\UploadedFiles\" + FileType + "\" + mUser.Email + Extension)
                If System.IO.File.Exists(FileToFind) Then
                    Return True
                Else
                    Return False
                End If

                Return False

            End If

            Return False

        Catch ex As Exception
            Return ex.Message
        End Try

    End Function

End Class