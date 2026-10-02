Public Class SA132ExcelDto
    ' Columnas usadas del reporte SA132:
    ' CeBeCategoria, CeBe, Categoria y SumaML.
    ' Fecha se normaliza internamente al primer dia del mes actual y no viene en el Excel.

    <ExcelColumn("", Requerido:=False, ValoresIgnorados:={})>
    Public Property Fecha As String

    <ExcelColumn("CeBeCategoria", Requerido:=True, ValoresIgnorados:={})>
    Public Property CeBeCategoria As String

    <ExcelColumn("CeBe", Requerido:=True, ValoresIgnorados:={})>
    Public Property CeBe As String

    <ExcelColumn("Categoria", Requerido:=False, ValoresIgnorados:={})>
    Public Property Categoria As String

    <ExcelColumn("Sum of ML", Requerido:=False, ValoresIgnorados:={})>
    Public Property SumaML As String
End Class
