Public Class EficienciaEfectividadExcelDto
    <ExcelSheet("EFECTIVIDAD", "STG_EFECTIVIDAD", 6)>
    Property Efectividad As List(Of EfectividadExcelDtoTest)
    <ExcelSheet("EFICIENCIA", "STG_EFICIENCIA", 6)>
    Property Eficiencia As List(Of EficienciaExcelDtoTest)

End Class
