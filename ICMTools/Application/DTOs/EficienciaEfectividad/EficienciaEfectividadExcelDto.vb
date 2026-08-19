Public Class EficienciaEfectividadExcelDto
    <ExcelSheet("EFECTIVIDAD", "STG_EFECTIVIDAD", 6)>
    Property Efectividad As List(Of EfectividadExcelDto)
    <ExcelSheet("EFICIENCIA", "STG_EFICIENCIA", 6)>
    Property Eficiencia As List(Of EficienciaExcelDto)

End Class
