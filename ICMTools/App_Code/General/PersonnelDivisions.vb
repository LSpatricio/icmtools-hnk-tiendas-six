Imports Microsoft.VisualBasic

Public Class PersonnelDivisions
    Private _PersonnelDivisionValue As String
    Private _PersonnelDivisionName As String

    Public Property PersonnelDivisionValue() As String
        Get
            Return _PersonnelDivisionValue
        End Get
        Private Set(ByVal value As String)
            _PersonnelDivisionValue = value
        End Set
    End Property

    Public Property PersonnelDivisionName() As String
        Get
            Return _PersonnelDivisionName
        End Get
        Private Set(ByVal value As String)
            _PersonnelDivisionName = value
        End Set
    End Property

    Public Sub New(value As String, name As String)
        PersonnelDivisionValue = value
        PersonnelDivisionName = name
    End Sub

End Class
