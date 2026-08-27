Public Class ArqueosExcelDto
    <ExcelColumn("Codigo de Listado", Requerido:=True, ValoresIgnorados:={})>
    Public Property CodigoListado As String

    ' El Excel compartido no maneja aliases. El archivo real de Arqueos usa "No. SAP".
    ' Se usa Decimal para que sea compatible con ExcelService.EsTipoValido sin modificar
    ' la infraestructura compartida. La regla de entero se valida en ArqueosService.
    <ExcelColumn("No. SAP", Requerido:=True, ValoresIgnorados:={})>
    Public Property NumeroSAP As Decimal?

    <ExcelColumn("Almacen", Requerido:=True, ValoresIgnorados:={})>
    Public Property Almacen As String

    <ExcelColumn("Tipo de Listado", Requerido:=True, ValoresIgnorados:={})>
    Public Property TipoListado As String

    <ExcelColumn("Estatus", Requerido:=True, ValoresIgnorados:={})>
    Public Property Estatus As String

    ' En el reporte de Arqueos esta columna puede venir como texto, por ejemplo:
    ' 2026-04-01 08:17:21 a.m.
    ' Se recibe como String para evitar que ExcelReader intente asignar el valor crudo
    ' directamente a un DataColumn DateTime. La conversion se realiza en ArqueosService.
    <ExcelColumn("Fecha de Creacion", Requerido:=True, ValoresIgnorados:={})>
    Public Property FechaCreacion As String

    <ExcelColumn("Nombre", Requerido:=True, ValoresIgnorados:={})>
    Public Property Nombre As String

    <ExcelColumn("Usuario-Creador", Requerido:=True, ValoresIgnorados:={})>
    Public Property UsuarioCreador As String

    <ExcelColumn("Usuario-Creador-Perfil", Requerido:=True, ValoresIgnorados:={})>
    Public Property UsuarioCreadorPerfil As String

    <ExcelColumn("Usuario-Autorizador", ValoresIgnorados:={})>
    Public Property UsuarioAutorizador As String

    <ExcelColumn("Usuario-Autorizador-Perfil", ValoresIgnorados:={})>
    Public Property UsuarioAutorizadorPerfil As String

    <ExcelColumn("Tipo de Cierre", ValoresIgnorados:={})>
    Public Property TipoCierre As String

    <ExcelColumn("Inicio de Conteo", Requerido:=True, ValoresIgnorados:={})>
    Public Property FechaInicioConteo As DateTime?

    <ExcelColumn("Fin de Conteo", Requerido:=True, ValoresIgnorados:={})>
    Public Property FechaFinConteo As DateTime?

    <ExcelColumn("Cierre de Conteo", Requerido:=True, ValoresIgnorados:={})>
    Public Property FechaCierreConteo As DateTime?

    <ExcelColumn("Termino de Conteo", ValoresIgnorados:={})>
    Public Property FechaTerminoConteo As DateTime?

    <ExcelColumn("Subinventario", Requerido:=True, ValoresIgnorados:={})>
    Public Property Subinventario As String

    <ExcelColumn("ID Producto", Requerido:=True, ValoresIgnorados:={})>
    Public Property IdProducto As String

    <ExcelColumn("Codigo de Producto", Requerido:=True, ValoresIgnorados:={})>
    Public Property CodigoProducto As Decimal?

    <ExcelColumn("Nombre del Producto", Requerido:=True, ValoresIgnorados:={})>
    Public Property NombreProducto As String

    <ExcelColumn("Unidad", Requerido:=True, ValoresIgnorados:={})>
    Public Property Unidad As String

    <ExcelColumn("Cantidad Sistema", Requerido:=True, ValoresIgnorados:={})>
    Public Property CantidadSistema As Decimal?

    <ExcelColumn("Diferencia", Requerido:=True, ValoresIgnorados:={})>
    Public Property Diferencia As Decimal?

    <ExcelColumn("Faltante", Requerido:=True, ValoresIgnorados:={})>
    Public Property Faltante As Decimal?

    <ExcelColumn("Sobrante", Requerido:=True, ValoresIgnorados:={})>
    Public Property Sobrante As Decimal?

    <ExcelColumn("$ Faltante Precio/Cons", Requerido:=True, ValoresIgnorados:={})>
    Public Property FaltantePrecioCons As Decimal?

    <ExcelColumn("$ Sobrante Precio/Cons", Requerido:=True, ValoresIgnorados:={})>
    Public Property SobrantePrecioCons As Decimal?

    <ExcelColumn("Comentarios", ValoresIgnorados:={})>
    Public Property Comentario As String
End Class
