Public Class RegistrosADCDetalleExcelDto

    <ExcelColumn("ID", Requerido:=True, ValoresIgnorados:={})>
    Public Property ID As Integer

    <ExcelColumn("Conteo de Archivos", Requerido:=False, ValoresIgnorados:={})>
    Public Property ConteoArchivos As Integer

    <ExcelColumn("CeCo", Requerido:=True, ValoresIgnorados:={})>
    Public Property Ceco As Decimal

    <ExcelColumn("FechaApproval", Requerido:=True, ValoresIgnorados:={})>
    Public Property FechaAprobacion As Date

    <ExcelColumn("Accion", Requerido:=False, ValoresIgnorados:={})>
    Public Property Accion As String

    <ExcelColumn("Ruta", Requerido:=True, ValoresIgnorados:={})>
    Public Property Ruta As String

    <ExcelColumn("Region", Requerido:=True, ValoresIgnorados:={})>
    Public Property Region As String

    <ExcelColumn("ComentarioAnalista", Requerido:=False, ValoresIgnorados:={})>
    Public Property ComentarioAnalista As String

    <ExcelColumn("Nombre Tienda", Requerido:=True, ValoresIgnorados:={})>
    Public Property NombreTienda As String

    <ExcelColumn("Cargado Por", Requerido:=False, ValoresIgnorados:={})>
    Public Property CargadoPor As String

    <ExcelColumn("Revisado Expins", Requerido:=False, ValoresIgnorados:={})>
    Public Property RevisadoExpins As String

    <ExcelColumn("Estatus2", Requerido:=False, ValoresIgnorados:={})>
    Public Property Estatus As String


End Class
