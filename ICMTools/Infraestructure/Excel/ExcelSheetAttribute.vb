<AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
Public Class ExcelSheetAttribute
    Inherits Attribute

    Public ReadOnly Property SheetName As String
    Public ReadOnly Property SheetIndex As Integer?
    Public ReadOnly Property HeaderRow As Integer

    Public ReadOnly Property SheetClass As Type

    Public Sub New(sheetName As String, headerRow As Integer)
        Me.SheetName = sheetName
        Me.SheetIndex = Nothing
        Me.HeaderRow = headerRow
    End Sub

    Public Sub New(sheetIndex As Integer, headerRow As Integer)
        Me.SheetName = Nothing
        Me.SheetIndex = sheetIndex
        Me.HeaderRow = headerRow
    End Sub

End Class