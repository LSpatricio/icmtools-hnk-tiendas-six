Imports System.Data
Imports System.Data.SqlClient
Imports Microsoft.VisualBasic

Public Class Periods
    Private _PeriodoValue As String
    Private _PeriodoName As String

    Public Property PeriodoValue() As String
        Get
            Return _PeriodoValue
        End Get
        Private Set(ByVal value As String)
            _PeriodoValue = value
        End Set
    End Property

    Public Property PeriodoName() As String
        Get
            Return _PeriodoName
        End Get
        Private Set(ByVal value As String)
            _PeriodoName = value
        End Set
    End Property

    Public Sub New(value As String, name As String)
        PeriodoValue = value
        PeriodoName = name
    End Sub

End Class

Public Class DatePeriod
#Region "Propiedades"
    Public Property IDPeriod As String
    Public Property PeriodName As String
    Public Property year As String
    Public Property Month As String
    Public Property Week As String
    Public Property Datestart As String
    Public Property DateEnd As String
#End Region
    Public Shared Function GetPeriod() As List(Of DatePeriod)
        Dim cs = ConfigurationManager.ConnectionStrings("TSQL_CONNECTION")
        If cs Is Nothing OrElse String.IsNullOrWhiteSpace(cs.ConnectionString) Then
            Throw New InvalidOperationException("Falta o está vacía la connectionString 'TSQL_CONNECTION'.")
        End If
        Dim CnnFEMCO_Transfer As String = cs.ConnectionString

        Dim dt As DataTable = Nothing
        Dim resultList As New List(Of DatePeriod)

        Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)
            dt = dbFactory.GetDataAsDataTable("[FEMCOEPSAP].[spICMToolspPeriod]", New SqlParameter("@datePart", 2))
        End Using

        If dt IsNot Nothing Then
            For Each row As DataRow In dt.Rows
                Dim DataRow = New DatePeriod()
                DataRow.IDPeriod = row.Item(0).ToString()
                DataRow.PeriodName = row.Item(1).ToString()
                DataRow.year = row.Item(2).ToString()
                DataRow.Month = row.Item(3).ToString()
                DataRow.Week = row.Item(4).ToString()
                DataRow.Datestart = row.Item(5).ToString()
                DataRow.DateEnd = row.Item(6).ToString()
                resultList.Add(DataRow)
            Next
            Return resultList
        Else
            Return New List(Of DatePeriod) ' Devuelve una lista vacía si no hay datos
        End If
    End Function
End Class
