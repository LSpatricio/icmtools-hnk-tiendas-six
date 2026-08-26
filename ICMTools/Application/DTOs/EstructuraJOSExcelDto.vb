Public Class EstructuraJOSExcelDto

    <ExcelColumn("Región", Requerido:=True, ValoresIgnorados:={})>
    Public Property Region As String
    <ExcelColumn("Cve Área Six", ValoresIgnorados:={})>
    Public Property CveAreaSix As String
    <ExcelColumn("Área Six", Requerido:=True, ValoresIgnorados:={})>
    Public Property AreaSix As String
    <ExcelColumn("GZ", Requerido:=True, ValoresIgnorados:={})>
    Public Property GZ As String
    <ExcelColumn("Ceco 169", ValoresIgnorados:={})>
    Public Property Ceco As String
    <ExcelColumn("CeBe 169", Requerido:=True, ValoresIgnorados:={})>
    Public Property CeBe As String
    <ExcelColumn("# Responsable", ValoresIgnorados:={})>
    Public Property NumeroResponsable As String
    <ExcelColumn("Responsable", ValoresIgnorados:={})>
    Public Property Responsable As String
End Class
