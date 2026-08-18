Public Class RegistrosADCDetalleExcelDto

    <ExcelColumn(0, "ID", Requerido:=True, ValoresIgnorados:={})>
    Public Property ID As Integer

    <ExcelColumn(1, "Conteo de Archivos", Requerido:=False, ValoresIgnorados:={})>
    Public Property ConteoArchivos As Integer

    <ExcelColumn(2, "CeCo", Requerido:=True, ValoresIgnorados:={})>
    Public Property Ceco As Decimal

    <ExcelColumn(3, "FechaApproval", Requerido:=True, ValoresIgnorados:={})>
    Public Property FechaAprobacion As Date

    <ExcelColumn(4, "Accion", Requerido:=False, ValoresIgnorados:={})>
    Public Property Accion As String

    <ExcelColumn(5, "Ruta", Requerido:=True, ValoresIgnorados:={})>
    Public Property Ruta As String

    <ExcelColumn(6, "Region", Requerido:=True, ValoresIgnorados:={})>
    Public Property Region As String

    <ExcelColumn(7, "ComentarioAnalista", Requerido:=False, ValoresIgnorados:={})>
    Public Property ComentarioAnalista As String

    <ExcelColumn(8, "Nombre Tienda", Requerido:=True, ValoresIgnorados:={})>
    Public Property NombreTienda As String

    <ExcelColumn(9, "Cargado Por", Requerido:=False, ValoresIgnorados:={})>
    Public Property CargadoPor As String

    <ExcelColumn(10, "Revisado Expins", Requerido:=False, ValoresIgnorados:={})>
    Public Property RevisadoExpins As String

    <ExcelColumn(11, "Estatus2", Requerido:=False, ValoresIgnorados:={})>
    Public Property Estatus As String


End Class
