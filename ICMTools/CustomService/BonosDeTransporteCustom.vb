Imports System.Data.SqlClient
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes

Public Class BonosDeTransporteCustom
#Region "Variables Locales"
    Private NpgsqlConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
#End Region
#Region "Propiedades"
    Public Property PayeeID As String
    Public Property idSociedad As String
    Public Property sociedad As String
    Public Property idDivision As String
    Public Property division As String
    Public Property email As String
#End Region
#Region "Funciones"
    '''<summary>Obtiene las Sociedades de Division por usuario </summary>
    '''<param name="user">El usuario que lanza la solicitud</param>
    '''<param name="model">El modelo desde donde se accede</param>
    '''<returns>Una lista con las Sociedades de Division a las que tiene acceso el usuario y modelo</returns>
    Public Shared Function GetSocietyDivisionByUser(user As String, model As String) As List(Of SocietyDivision)
        Dim ws As New WebServiceICMGeneral()
        Dim resultList As New List(Of SocietyDivision)
        Dim maskModel As String

        If model = "DEBUG" Then
            maskModel = "femcoepdev"
        Else
            maskModel = model
        End If

        Dim PayeeID = ws.GetPayeeByUserEmail(user, maskModel)
        Dim dt As DataTable = ws.GetDivisionByUserBT(PayeeID, maskModel)

        If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
            For Each row As DataRow In dt.Rows
                Dim DataRow = New SocietyDivision()
                DataRow.PayeeID = row.Item(0).ToString()
                DataRow.idSociedad = row.Item(1).ToString()
                DataRow.sociedad = row.Item(2).ToString()
                DataRow.idDivision = row.Item(3).ToString()
                DataRow.division = row.Item(4).ToString()
                DataRow.email = user
                resultList.Add(DataRow)
            Next
            Return resultList
        Else
            Return New List(Of SocietyDivision) ' Devuelve una lista vacía si no hay datos
        End If
    End Function
#End Region
End Class
