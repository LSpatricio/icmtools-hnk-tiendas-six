Public Class ArqueosDetalleExcelDto

    <ExcelColumn(0, "Código de Listado", Requerido:=True)>
    Public Property CodigoListado As String

    <ExcelColumn(1, "No. SAP", Requerido:=True)>
    Public Property NoSap As Integer

    <ExcelColumn(2, "Almacén", Requerido:=True)>
    Public Property Almacen As String

    <ExcelColumn(3, "Tipo de Listado", Requerido:=True)>
    Public Property TipoListado As String

    <ExcelColumn(4, "Estatus", Requerido:=True)>
    Public Property Estatus As String

    <ExcelColumn(5, "Fecha de Creación", Requerido:=True)>
    Public Property FechaCreacion As DateTime

    <ExcelColumn(6, "Nombre", Requerido:=True)>
    Public Property Nombre As String

    <ExcelColumn(7, "Usuario-Creador", Requerido:=True)>
    Public Property UsuarioCreador As String

    <ExcelColumn(8, "Usuario-Creador-Perfil", Requerido:=True)>
    Public Property UsuarioCreadorPerfil As String

    <ExcelColumn(9, "Usuario-Autorizador", Requerido:=False)>
    Public Property UsuarioAutorizador As String

    <ExcelColumn(10, "Usuario-Autorizador-Perfil", Requerido:=False)>
    Public Property UsuarioAutorizadorPerfil As String

    <ExcelColumn(11, "Tipo de Cierre", Requerido:=False)>
    Public Property TipoCierre As String

    <ExcelColumn(12, "Inicio de Conteo", Requerido:=True)>
    Public Property FechaInicioConteo As DateTime

    <ExcelColumn(13, "Fin de Conteo", Requerido:=True)>
    Public Property FechaFinConteo As DateTime

    <ExcelColumn(14, "Cierre de Conteo", Requerido:=True)>
    Public Property FechaCierreConteo As DateTime

    <ExcelColumn(15, "Término de Conteo", Requerido:=False)>
    Public Property FechaTerminoConteo As DateTime

    <ExcelColumn(16, "Subinventario", Requerido:=True)>
    Public Property Subinventario As String

    <ExcelColumn(17, "ID Producto", Requerido:=True)>
    Public Property IdProducto As String

    <ExcelColumn(18, "Código de Producto", Requerido:=True)>
    Public Property CodigoProducto As Integer

    <ExcelColumn(19, "Nombre del Producto", Requerido:=True)>
    Public Property NombreProducto As String

    <ExcelColumn(20, "Unidad", Requerido:=True)>
    Public Property Unidad As String

    <ExcelColumn(21, "Cantidad Sistema", Requerido:=True)>
    Public Property CantidadSistema As Decimal

    <ExcelColumn(22, "Diferencia", Requerido:=True)>
    Public Property Diferencia As Decimal

    <ExcelColumn(23, "Faltante", Requerido:=True)>
    Public Property Faltante As Decimal

    <ExcelColumn(24, "Sobrante", Requerido:=True)>
    Public Property Sobrante As Decimal

    <ExcelColumn(25, "$ Faltante Precio/Cons", Requerido:=True)>
    Public Property FaltantePrecioCons As Decimal

    <ExcelColumn(26, "$ Sobrante Precio/Cons", Requerido:=True)>
    Public Property SobrantePrecioCons As Decimal

    <ExcelColumn(27, "Comentarios", Requerido:=False)>
    Public Property Comentario As String

End Class
