Public Class ArqueosEficienciaExcelDto

    <ExcelColumn(0, "Mes / Año", Requerido:=True, ValoresIgnorados:={"Resultado total"})>
    Public Property Mes As String

    <ExcelColumn(1, "Semana", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Semana As String

    <ExcelColumn(2, "Ruta de Venta", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Ruta As String

    <ExcelColumn(3, "% Eficiencia de vista", ValoresIgnorados:={})>
    Public Property Eficiencia As Decimal?


End Class
