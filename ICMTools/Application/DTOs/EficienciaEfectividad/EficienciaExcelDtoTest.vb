Public Class EficienciaExcelDtoTest

    <ExcelColumn("Mes / Año", Requerido:=True, ValoresIgnorados:={"Resultado total"})>
    Public Property Mes As String

    <ExcelColumn("Semana", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Semana As String

    <ExcelColumn("Ruta de Venta", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Ruta As String

    <ExcelColumn("% Eficiencia de vista", ValoresIgnorados:={})>
    Public Property Eficiencia As Decimal?


End Class
