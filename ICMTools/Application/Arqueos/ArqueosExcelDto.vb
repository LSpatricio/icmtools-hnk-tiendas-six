Public Class ArqueosExcelDto
    <ExcelSheet("p1", 1)>
    Property p1 As List(Of ArqueosDetalleExcelDto)
    <ExcelSheet("p2", 1)>
    Property p2 As List(Of ArqueosDetalleExcelDto)

End Class
