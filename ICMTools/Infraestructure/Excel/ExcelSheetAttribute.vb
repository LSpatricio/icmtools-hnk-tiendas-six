<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
Public Class ExcelSheetAttribute
    Inherits Attribute

    Public ReadOnly Property SheetName As String
    Public ReadOnly Property TableName As String
    Public ReadOnly Property HeaderRow As Integer

    Public ReadOnly Property SheetClass As Type

    Public Sub New(sheetName As String, tableName As String, headerRow As Integer)
        Me.SheetName = sheetName
        Me.TableName = tableName
        Me.HeaderRow = headerRow
    End Sub



End Class