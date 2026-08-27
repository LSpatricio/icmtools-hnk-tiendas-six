Public Class EficienciaExcelDto
    <ExcelColumn("Concat_JOS", Requerido:=True, ValoresIgnorados:={})>
    Public Property Empleado As String
    <ExcelColumn("Average of porcentaje_eficiencia_float", ValoresIgnorados:={})>
    Public Property Promedio As String

End Class
