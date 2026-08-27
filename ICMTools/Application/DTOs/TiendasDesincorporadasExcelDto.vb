Public Class TiendasDesincorporadasExcelDto
    <ExcelColumn("CeCo", Requerido:=True, ValoresIgnorados:={})>
    Public Property Ceco As String

    <ExcelColumn("Descripción", ValoresIgnorados:={})>
    Public Property Descripcion As String

    <ExcelColumn("Región SIX", Requerido:=True, ValoresIgnorados:={})>
    Public Property Region As String

    <ExcelColumn("GZ SIX", Requerido:=True, ValoresIgnorados:={})>
    Public Property GZ As String

    <ExcelColumn("Desc JOS (Resp) (19)", ValoresIgnorados:={})>
    Public Property DescJOS As String

    <ExcelColumn("Desc ACS (Resp) (21)", ValoresIgnorados:={})>
    Public Property DescACS As String

    <ExcelColumn("Madura (22)", ValoresIgnorados:={})>
    Public Property Madura As String

    <ExcelColumn("Tipo de Cierre", ValoresIgnorados:={})>
    Public Property TipoCierre As String

    <ExcelColumn("Status", Requerido:=True, ValoresIgnorados:={})>
    Public Property Status As String

    <ExcelColumn("Mes Baja", Requerido:=True, ValoresIgnorados:={})>
    Public Property MesBaja As String

    <ExcelColumn("Tipo Baja", Requerido:=True, ValoresIgnorados:={})>
    Public Property TipoBaja As String
End Class
