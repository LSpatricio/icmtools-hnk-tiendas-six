Public Class EficienciaEfectividadExcelDto
    <ExcelSheet("EFECTIVIDAD", 6)>
    Property Efectividad As List(Of EfectividadExcelDto)
    <ExcelSheet("EFICIENCIA", 6)>
    Property Eficiencia As List(Of EficienciaExcelDto)

End Class
