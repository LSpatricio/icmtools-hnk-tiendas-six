Imports System.Data
Imports System.Data.SqlClient
Imports Microsoft.VisualBasic

Public Class Societies
    Private _SocietyValue As String
    Private _SocietyName As String

    Public Property SocietyValue() As String
        Get
            Return _SocietyValue
        End Get
        Private Set(ByVal value As String)
            _SocietyValue = value
        End Set
    End Property

    Public Property SocietyName() As String
        Get
            Return _SocietyName
        End Get
        Private Set(ByVal value As String)
            _SocietyName = value
        End Set
    End Property

    Public Sub New(value As String, name As String)
        SocietyValue = value
        SocietyName = name
    End Sub

End Class

Public Class SocietyDivision
    Public Property PayeeID As String
    Public Property idSociedad As String
    Public Property sociedad As String
    Public Property idDivision As String
    Public Property division As String
    Public Property email As String
    Public Shared Function GetSocietyDivisionByUserSQL(user As String) As List(Of SocietyDivision)
        Return Nothing
    End Function
End Class