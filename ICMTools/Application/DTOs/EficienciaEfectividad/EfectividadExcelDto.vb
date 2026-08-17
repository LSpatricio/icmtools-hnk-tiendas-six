Public Class EfectividadExcelDto

    <ExcelColumn("Calendario Año / Mes", Requerido:=True, ValoresIgnorados:={"Resultado total"})>
    Public Property Mes As String

    <ExcelColumn("Año natural/Semana - Inicio", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Semana As String

    <ExcelColumn("Ruta Comercial", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Ruta As String

    <ExcelColumn("3", Requerido:=True, ValoresIgnorados:={})>
    Public Property Esquema As String

    <ExcelColumn("ID KPI", Requerido:=True, ValoresIgnorados:={})>
    Public Property KPI As String

    <ExcelColumn("Visitas de Clientes", Requerido:=False, ValoresIgnorados:={})>
    Public Property VisitasClientes As Integer

    <ExcelColumn("KPIs con Visitas", Requerido:=False, ValoresIgnorados:={})>
    Public Property VisitasKPI As Integer

    <ExcelColumn("KPIs Solucionados", Requerido:=False, ValoresIgnorados:={})>
    Public Property KPISolucionados As Integer

    <ExcelColumn("% Efectividad
Asesoria", Requerido:=False, ValoresIgnorados:={})>
    Public Property Efectividad As Decimal


End Class
