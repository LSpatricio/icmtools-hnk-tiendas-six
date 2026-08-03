Imports System
Imports System.IO

Imports System.Collections.Generic
Imports System.Linq
Imports System.Web
Imports System.Web.UI
Imports System.Web.UI.WebControls
Imports AjaxControlToolkit

Imports ClosedXML.Excel

Imports System.Data.SqlClient
Imports System.ComponentModel.DataAnnotations
Imports System.Runtime.InteropServices


Public Class BonosUpload
    Inherits System.Web.UI.Page
    Private mUser As User
    Private mLog As Log
    Private ws As New WebServiceICMGeneral()


    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init

        '------Evitar Caché del Navegador--------
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
        '----------------------------------------

        mUser = CType(Session.Item("User"), User)
        Dim maskModel As String

        If mUser.Model = "DEBUG" Then
            maskModel = "femcoepdev"
        Else
            maskModel = mUser.Model
        End If

        Dim PayeeID As String = ws.GetPayeeByUserEmail(mUser.Email, maskModel)
        Dim LastHistoryPayee As String = ws.GetLastHistoryPayee(PayeeID, maskModel)

        If mUser Is Nothing Then
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        ElseIf Not ScreenPermission.Access(LastHistoryPayee, mUser.Email, "BONOSUP") Then
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Me.Master.PageIcon = "<i class='fas fa-upload fa-fw'></i>"
        Me.Master.PageName = "Carga Bonos de Transporte"



    End Sub


    Sub AsyncFileUpload1_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete
        If Session.Item("User") IsNot Nothing Then
            mUser = CType(Session.Item("User"), User)
            If (Not SaveDocument(mUser, AsyncFileUpload1)) Then
                Throw New System.Exception("Error al momento de guardar el archivo")
            End If

        Else
            Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
        End If
    End Sub

    Public Function ValidateDataRow(resultado As DataRow) As Boolean
        Return True
    End Function
    Public Function createDatatable() As System.Data.DataTable
        Dim dt As New System.Data.DataTable("BonosTransporte")
        dt.Columns.Add("IDBono", GetType(Int32))
        dt.Columns.Add("Payee", GetType(String))
        dt.Columns.Add("Date", GetType(String))
        dt.Columns.Add("CCNom", GetType(String))
        dt.Columns.Add("Amount", GetType(String))
        dt.Columns.Add("Reason", GetType(String))
        dt.Columns.Add("Status", GetType(String))
        dt.Columns.Add("MessageResponse", GetType(String))
        Return dt
    End Function

    Private Function SaveDocument(mUser As User, AsyncFileUpload1 As AsyncFileUpload) As Boolean
        Try
            Dim root = Server.MapPath("~\UploadedFiles\BonosTransporte")

            If Not My.Computer.FileSystem.DirectoryExists(root) Then
                My.Computer.FileSystem.CreateDirectory(root)
            End If

            Dim savePath As String = root + "\" + mUser.Email
            Dim Extension As String = Path.GetExtension(AsyncFileUpload1.FileName)
            AsyncFileUpload1.SaveAs(savePath + Extension)
            Return True
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Public Function GetDocumentArray(user As String, Extension As String) As Object(,)
        Dim root = Server.MapPath("~\UploadedFiles\BonosTransporte")
        Dim savePath As String = root + "\" + user
        Dim filePath As String = savePath + "." + Extension

        Using workbook As New XLWorkbook(filePath)
            Dim worksheet = workbook.Worksheet(1)
            If worksheet Is Nothing OrElse worksheet.LastCellUsed() Is Nothing Then Return Nothing

            Dim range = worksheet.RangeUsed()
            Dim numCols As Integer = range.ColumnCount()
            Dim dataRows As New List(Of Object())

            For Each row In range.Rows()
                If row.IsEmpty() Then Continue For

                Dim rowArray(numCols - 1) As Object
                Dim isRowEmpty As Boolean = True

                For j As Integer = 0 To numCols - 1
                    Dim cell = row.Cell(j + 1)
                    Dim cellValue As Object = Nothing

                    If Not cell.IsEmpty() Then
                        If cell.DataType = XLDataType.DateTime Then
                            cellValue = cell.GetDateTime().ToString("dd/MM/yyyy")
                        Else
                            cellValue = cell.GetFormattedString()
                        End If
                    End If

                    rowArray(j) = cellValue
                    If j < 5 AndAlso cellValue IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(cellValue.ToString()) Then isRowEmpty = False
                Next

                If Not isRowEmpty Then dataRows.Add(rowArray)
            Next

            If dataRows.Count = 0 Then Return Nothing

            Dim finalNumRows As Integer = dataRows.Count
            Dim finalNumCols As Integer = If(dataRows.Count > 0 AndAlso dataRows(0).Length > 0, dataRows(0).Length, 0)
            Dim finalArray(finalNumRows, finalNumCols) As Object
            For i As Integer = 1 To finalNumRows
                For j As Integer = 1 To finalNumCols
                    finalArray(i, j) = dataRows(i - 1)(j - 1)
                Next
            Next

            Return finalArray
        End Using
    End Function

End Class