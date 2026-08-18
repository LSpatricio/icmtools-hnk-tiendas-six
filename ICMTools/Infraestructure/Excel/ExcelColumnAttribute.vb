<AttributeUsage(AttributeTargets.Property)>
Public Class ExcelColumnAttribute
    Inherits Attribute

    Public ReadOnly Property ColumnName As String
    Public ReadOnly Property ColumnIndex As Integer
    Public Property Requerido As Boolean

    Public Property ValoresIgnorados As String() = {}
    Public Property ColumnAliases As String() = {}


    Public Sub New(columnIndex As Integer, Optional columnName As String = Nothing)
        Me.ColumnName = columnName
        Me.ColumnIndex = columnIndex
    End Sub

    'Public Sub New(colmnIndex As Integer)
    '    Me.ColumnIndex = colmnIndex
    '    Me.ColumnName = Nothing

    'End Sub

End Class
