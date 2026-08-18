<AttributeUsage(AttributeTargets.Property)>
Public Class ExcelColumnAttribute
    Inherits Attribute

    Public ReadOnly Property ColumnName As String
    Public Property ColumnIndex As Integer
    Public Property Requerido As Boolean

    Public Property ValoresIgnorados As String() = {}
    Public Property ColumnAliases As String() = {}


    Public Sub New(columnName As String)
        Me.ColumnName = columnName
    End Sub



End Class
