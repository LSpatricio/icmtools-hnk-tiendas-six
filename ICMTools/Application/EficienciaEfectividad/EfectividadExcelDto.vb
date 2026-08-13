Public Class EfectividadExcelDto

    <ExcelColumn(0, "Calendario Año / Mes", Requerido:=True, ValoresIgnorados:={"Resultado total"})>
    Public Property Mes As String

    <ExcelColumn(1, "Año natural/Semana - Inicio", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Semana As String

    <ExcelColumn(2, "Ruta Comercial", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Ruta As String

    <ExcelColumn(3, Requerido:=True, ValoresIgnorados:={})>
    Public Property Esquema As String

    <ExcelColumn(4, "ID KPI", Requerido:=True, ValoresIgnorados:={})>
    Public Property KPI As String

    <ExcelColumn(5, "Visitas de Clientes", Requerido:=False, ValoresIgnorados:={})>
    Public Property VisitasClientes As Integer

    <ExcelColumn(6, "KPIs con Visitas", Requerido:=False, ValoresIgnorados:={})>
    Public Property VisitasKPI As Integer

    <ExcelColumn(7, "KPIs Solucionados", Requerido:=False, ValoresIgnorados:={})>
    Public Property KPISolucionados As Integer

    <ExcelColumn(8, "% Efectividad
Asesoria", Requerido:=False, ValoresIgnorados:={})>
    Public Property Efectividad As Decimal


End Class
