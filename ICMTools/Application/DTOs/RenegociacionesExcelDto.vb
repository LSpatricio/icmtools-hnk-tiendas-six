Public Class RenegociacionesExcelDto
    <ExcelSheet("Cpt.Re-Negociaciones", "STG_RENEGOCIACIONES", 4)>
    Public Property CptReNegociaciones As List(Of RenegociacionesCptReNegociacionesExcelDto)
End Class

Public Class RenegociacionesCptReNegociacionesExcelDto
    Public Property MesPeriodo As Long
    <ExcelColumn("Rev")> Public Property Rev As String
    <ExcelColumn("Cliente", Requerido:=True)> Public Property Cliente As String
    <ExcelColumn("Negocio")> Public Property Negocio As String
    <ExcelColumn("Region")> Public Property Region As String
    <ExcelColumn("GZ")> Public Property GZ As String
    <ExcelColumn("Mes")> Public Property Mes As String
    <ExcelColumn("Semana")> Public Property Semana As String
    <ExcelColumn("Motivo")> Public Property Motivo As String
    <ExcelColumn("TipoRenta")> Public Property TipoRenta As String
    <ExcelColumn("ReNegociador")> Public Property Renegociador As String
    <ExcelColumn("No. De Empleado", Requerido:=True)> Public Property NumeroEmpleado As String
    <ExcelColumn("Numero de contrato", Requerido:=True)> Public Property NumeroContrato As String
    <ExcelColumn("F.Inicio", Requerido:=True)> Public Property FechaInicio As DateTime
    <ExcelColumn("F.Termino")> Public Property FechaTermino As DateTime
    <ExcelColumn("$ Renta Ant")> Public Property RentaAnterior As Decimal
    <ExcelColumn("mismo contrato")> Public Property MismoContrato As String
    <ExcelColumn("Numero contrato nuevo")> Public Property NumeroContratoNuevo As String
    <ExcelColumn("Estatus")> Public Property Estatus As String
    <ExcelColumn("F.Inicio")> Public Property FechaInicioNuevo As DateTime
    <ExcelColumn("F.Termino")> Public Property FechaTerminoNuevo As DateTime
    <ExcelColumn("$ Nueva Renta")> Public Property NuevaRenta As Decimal
    <ExcelColumn("Meses Adelanto")> Public Property MesesAdelantados As String
    <ExcelColumn("$ Tot Adto")> Public Property TotalAdto As String
    <ExcelColumn("Renta Ant C/Inflacion")> Public Property RentaAntesConInflacion As Decimal
    <ExcelColumn("% Incremento")> Public Property PorcentajeIncremento As Decimal
    <ExcelColumn("Anual")> Public Property ImpactoAnual As Decimal
    <ExcelColumn("Flujo Anual")> Public Property FlujoAnual As Decimal
    <ExcelColumn("> Inflacion")> Public Property Inflacion As Decimal
    <ExcelColumn("Mes Inicio")> Public Property MesInicioContrato As String
    <ExcelColumn("Ano Inicio")> Public Property AnioInicioContrato As String
    <ExcelColumn("Meses a Pagar ano act")> Public Property MesesAPagarAnuales As String
    <ExcelColumn("Anual sin infl")> Public Property AnualSinInflacion As Decimal
    <ExcelColumn("Tipo Renta Fiscal")> Public Property TipoRentaFiscal As String
    <ExcelColumn("Renegociadas LY")> Public Property Renegociadas As String
    <ExcelColumn("Aportacion Comerciante")> Public Property AportacionComerciante As String
    <ExcelColumn("Nuevo incremento")> Public Property NuevoIncremento As String
    <ExcelColumn("Contrato incluye Agua")> Public Property ContratoConAgua As String
    <ExcelColumn("Subsidios")> Public Property Subsidio As String
    <ExcelColumn("Rev 1")> Public Property Rev1 As String
    <ExcelColumn("Rev 2")> Public Property Rev2 As String
    <ExcelColumn("Rev Contr")> Public Property RevContrato As String
    <ExcelColumn("Formula sub")> Public Property FormulaSub As String
    <ExcelColumn("Rev 3")> Public Property Rev3 As Decimal
    <ExcelColumn("Rev CECO")> Public Property RevCeco As String
    <ExcelColumn("Rev 4")> Public Property Rev4 As Decimal
    <ExcelColumn("Excepciones")> Public Property Excepciones As String
    <ExcelColumn("JOS")> Public Property JOS As String
    <ExcelColumn("Rene VS Plan")> Public Property RenegociacionVSPlan As String
    <ExcelColumn("Incr/Decr")> Public Property IncrementoDecremento As String
    <ExcelColumn("Aportacion")> Public Property Aportacion As Decimal
    <ExcelColumn("Resultado")> Public Property Resultado As String
    <ExcelColumn("Comentarios de Region")> Public Property ComentariosRegion As String
    <ExcelColumn("Renta Incluye Agua")> Public Property RentaConAgua As String
    <ExcelColumn("Monto Agua")> Public Property MontoAgua As Decimal
End Class
