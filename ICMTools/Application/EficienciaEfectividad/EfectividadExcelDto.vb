Public Class EfectividadExcelDto

    <ExcelColumn("Calendario Año / Mes", Requerido:=True, ValoresIgnorados:={"Resultado total"})>
    Public Property Mes As String

    <ExcelColumn("Año natural/Semana - Inicio", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Semana As String

    <ExcelColumn("Ruta Comercial", Requerido:=True, ValoresIgnorados:={"Resultado"})>
    Public Property Ruta As String

    <ExcelColumn(3, Requerido:=True)>
    Public Property Esquema As String

    <ExcelColumn("ID KPI", Requerido:=True)>
    Public Property KPI As String

    <ExcelColumn("Visitas de Clientes", Requerido:=False)>
    Public Property VisitasClientes As Integer

    <ExcelColumn("KPIs con Visitas", Requerido:=False)>
    Public Property VisitasKPI As Integer

    <ExcelColumn("KPIs Solucionados", Requerido:=False)>
    Public Property KPISolucionados As Integer

    <ExcelColumn("% Efectividad
Asesoria", Requerido:=False)>
    Public Property Efectividad As Decimal


End Class
