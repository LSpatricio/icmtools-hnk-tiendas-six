Public Class ArqueosExcelDto
    <ExcelSheet("EFECTIVIDAD", 6)>
    Property Efectividad As List(Of ArqueosEfectividadExcelDto)
    <ExcelSheet("EFICIENCIA", 6)>
    Property Eficiencia As List(Of ArqueosEficienciaExcelDto)

End Class
