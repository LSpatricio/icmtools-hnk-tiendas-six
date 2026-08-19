Public Class EstructuraNegociosExcelDto
    <ExcelColumn("Ceco", Requerido:=True, ValoresIgnorados:={})>
    Public Property Ceco As String
    <ExcelColumn("Descripción", ValoresIgnorados:={})>
    Public Property Descripcion As String
    <ExcelColumn("Región SIX", Requerido:=True, ValoresIgnorados:={})>
    Public Property Region As String
    <ExcelColumn("GZ SIX", Requerido:=True, ValoresIgnorados:={})>
    Public Property GZ As String
    <ExcelColumn("Status Tienda", ValoresIgnorados:={})>
    Public Property EstatusTienda As String
    <ExcelColumn("Numero de Comerciante", Requerido:=True, ValoresIgnorados:={})>
    Public Property NumeroComerciante As String
    <ExcelColumn("Nombre de Comerciante", ValoresIgnorados:={})>
    Public Property NombreComerciante As String

    <ExcelColumn("Fecha Ingreso", Requerido:=True, ValoresIgnorados:={})>
    Public Property FechaIngreso As String

    <ExcelColumn("Status SK", ValoresIgnorados:={})>
    Public Property EstatusSK As String

    <ExcelColumn("Fecha Mov", ValoresIgnorados:={})>
    Public Property FechaMovimiento As String

    <ExcelColumn("Telefono SK", ValoresIgnorados:={})>
    Public Property TelefonoSK As String

    <ExcelColumn("Correo SK", ValoresIgnorados:={})>
    Public Property CorreoSK As String

    <ExcelColumn("GOS", ValoresIgnorados:={})>
    Public Property GOS As String

    <ExcelColumn("Cve JOS", ValoresIgnorados:={})>
    Public Property CveJOS As String

    <ExcelColumn("CveAcsComercial", ValoresIgnorados:={})>
    Public Property CveAcsComercial As String

    <ExcelColumn("Cve Acs Control", ValoresIgnorados:={})>
    Public Property CveAcsControl As String

    <ExcelColumn("No. Empleado JOS", ValoresIgnorados:={})>
    Public Property EmpleadoJOS As String

    <ExcelColumn("Nombre JOS", ValoresIgnorados:={})>
    Public Property NombreJOS As String

    <ExcelColumn("No. Empleado Acs Comercial", ValoresIgnorados:={})>
    Public Property NumeroEmpleadoAcsCom As String

    <ExcelColumn("Nombre Acs Comercial", ValoresIgnorados:={})>
    Public Property NombreAcsComercial As String

    <ExcelColumn("Celular RED ACS Comercial", ValoresIgnorados:={})>
    Public Property CelularAcsComercial As String

    <ExcelColumn("Correo ACS comercial", ValoresIgnorados:={})>
    Public Property CorreoACSComercial As String

    <ExcelColumn("No. Empleado Acs Control", ValoresIgnorados:={})>
    Public Property NumeroEmpleadoAcsControl As String

    <ExcelColumn("Nombre Acs Control", ValoresIgnorados:={})>
    Public Property NombreAcsControl As String

    <ExcelColumn("Celular RED ACS Control", ValoresIgnorados:={})>
    Public Property CelularAcsControl As String

    <ExcelColumn("Correo ACS control", ValoresIgnorados:={})>
    Public Property CorreoAcsControl As String

    <ExcelColumn("GZ SIX2", ValoresIgnorados:={})>
    Public Property GZSIX2 As String

    <ExcelColumn("Cve JOS2", ValoresIgnorados:={})>
    Public Property CveJOSVal As String

    <ExcelColumn("Cve Acs Comercial2", ValoresIgnorados:={})>
    Public Property CveAcsComercialVal As String

    <ExcelColumn("Cve Acs Control2", ValoresIgnorados:={})>
    Public Property CveAcsControlVal As String

    <ExcelColumn("No. Empleado Atraccion", ValoresIgnorados:={})>
    Public Property NumeroEmpleadoAtraccion As String

    <ExcelColumn("Nombre Atraccion", ValoresIgnorados:={})>
    Public Property NombreEmpleadoAtraccion As String

    <ExcelColumn("Celular RED Atraccion", ValoresIgnorados:={})>
    Public Property CelularRedAtraccion As String

    <ExcelColumn("No. Empleado Coordinador", ValoresIgnorados:={})>
    Public Property NumeroEmpleadoCoordinador As String

    <ExcelColumn("|", ValoresIgnorados:={})>
    Public Property NombreCoordinador As String
End Class
