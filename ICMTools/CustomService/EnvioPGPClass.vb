Imports ClassLibrary_PGP_TO_SFTP
Imports Npgsql
Imports NpgsqlTypes

Public Class EnvioPGPClass

#Region " Propiedades Públicas "

    ''' <summary>
    ''' Nombre del archivo.
    ''' </summary>
    ''' <returns>Regresa el nombre del archivo.</returns>
    Public Property Archivo As String

    ''' <summary>
    ''' Consulta SQL que enviará la información PGP.
    ''' </summary>
    ''' <returns>Regresa la consulta sql.</returns>
    Public Property ConsultaSql As String

    ''' <summary>
    ''' Modelo que se usuará por omisión
    ''' </summary>
    ''' <returns>Modelo por Omisión.</returns>
    Public Property ModeloOmision As String

    ''' <summary>
    ''' Pantalla
    ''' </summary>
    ''' <returns>Regresa la enumeración.</returns>
    Public Property Pantalla As enuPantalla = enuPantalla.Ninguna

    ''' <summary>
    ''' Enumeración Pantalla.
    ''' </summary>
    Public Enum enuPantalla
        Ninguna = -1
        Prueba = 0
        Clasificaciones = 1
        Excepciones = 2
        Tiendas = 3
        TiendasExcepciones = 99
        TiendasGanadoras = 5
        PagosManuales = 7
        Entrada = 8
        EntradaEnfoque = 9
        EntradaVentas = 10
        MultiTiendaFijoEntrada = 11
        EmpleadosActivos = 12
        MontoDistribuible = 14
        PorcentajeVenta = 15
        VentaMonto = 16
        VentaUnidades = 17
        ConfiguracionDistribuciones = 22
        ConfiguracionPorcentajeVenta = 23
        HistoricoEmpleadosActivos = 26
        Metas = 27
        Ventas = 28
        EmpleadosLideres = 30
    End Enum

#End Region

#Region " Propiedades Privadas "

    ''' <summary>
    ''' Modelo.
    ''' </summary>
    Private _Modelo As String = ""

    ''' <summary>
    ''' Cadena de conexión.
    ''' </summary>
    Private ReadOnly _NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ''' <summary>
    ''' Cantidad de registros enviados.
    ''' </summary>
    Private _Registros As Integer = 0

    ''' <summary>
    ''' Usuario.
    ''' </summary>
    Private _Usuario As String = ""

#End Region

#Region " Métodos Públicos "

    ''' <summary>
    ''' Método que envía el archivo PGP
    ''' </summary>
    Public Sub Enviar()
        Dim mensaje As String = "OK"
        Try
            CargarValores()
            ValidarParametrosRequeridos()

            Dim mUser As User = CType(HttpContext.Current.Session.Item("User"), User)
            If mUser Is Nothing Then
                mensaje = "Usuario no autenticado o sesión expirada."
            End If

            If (mensaje.Equals("OK")) Then
                _Usuario = mUser.Email
                _Modelo = If(String.IsNullOrEmpty(mUser.Model) Or mUser.Model = "DEBUG", ModeloOmision, mUser.Model)
                mensaje = EnviarArchivoPGP()
            End If

            RegistrarEnvio(mensaje)

            If Not mensaje.Equals("OK") And Not mensaje.Equals("Sin Registros") Then
                Throw New Exception("El archivo PGP no pudo ser enviado")
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

#End Region

#Region " Métodos Privados "

    ''' <summary>
    ''' Método que carga los valores.
    ''' </summary>
    Private Sub CargarValores()
        Try
            If (Pantalla = enuPantalla.Prueba) Then
                Archivo = "ICMTools-Prueba"
                ConsultaSql = "SELECT * FROM ""ICMToolsScreens"";"
                ModeloOmision = "femcovsprd"
            ElseIf (Pantalla = enuPantalla.Clasificaciones) Then
                Archivo = "Clasificaciones"
                ConsultaSql = "SELECT * FROM DataObjetivesClasificaciones;"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.Excepciones) Then
                Archivo = "Excepciones"
                ConsultaSql = "SELECT * FROM ""Excepciones_PGP""();"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.Tiendas) Then
                Archivo = "Tiendas"
                ConsultaSql = "SELECT idsociety, idpersonaldivision, idstore, startdate, enddate, amount FROM cfgstoretransportationaid;"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.TiendasExcepciones) Then
                Archivo = "ExepcionesDeTiendas"
                ConsultaSql = "SELECT idsociety, idpersonaldivision, idstore, payeeid, startdate, enddate, amount FROM cfgexceptiontransportationaid;"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.TiendasGanadoras) Then
                Archivo = "TiendasGanadoras"
                ConsultaSql = "SELECT ""Tienda"", ""Concurso"", ""ValDescripcion"", ""ValStatus"" FROM TiendasGanadorasConcurso WHERE ""ValStatus"" = '1'"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.PagosManuales) Then
                Archivo = "PagosManuales"
                ConsultaSql = "SELECT EmpleadoID, CentroTrabajoID, ComponenteID, Fecha, Monto, Comentarios, aprobado, fechaAprobacion, Aprobador, Insercion FROM FEMCO_DTPagosManuales;"
                ModeloOmision = "femcodev"
            ElseIf (Pantalla = enuPantalla.MultiTiendaFijoEntrada) Then
                Archivo = "MultiTiendaFijo-Entrada"
                ConsultaSql = "SELECT CasoTabulador, CRPLAZA_A, CRTIENDA_A, TDA_A, CRPLAZA_B, CRTIENDA_B, TDA_B, BEGDA, ENDDA, LGART, IDS, DesError, DateInsert FROM VALIDACION_COMPLEJIDAD2;"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.EmpleadosLideres) Then
                Archivo = "EmpleadosLideres"
                ConsultaSql = "SELECT ""TiendaID"", ""EmpleadoID"", ""ConceptoEvaluadoID"", ""Fecha"", ""Calificacion"", ""CalificacionTexto"", ""Insercion"", ""CalificacionTextoID"", ""Usuario"" FROM ""FEMCO_dtOxxoTdaEvaluaciones"";"
                ModeloOmision = "femcodev"
            ElseIf (Pantalla = enuPantalla.Entrada) Then
                Archivo = "MultiTiendaVariable-Entrada"
                ConsultaSql = "SELECT ""CASOTABULADOR"", ""CRPLAZA_A"", ""CRTIENDA_A"", ""TDA_A"", ""CRPLAZA_B"", ""CRTIENDA_B"", ""TDA_B"", ""BEGDA"", ""ENDDA"", ""LGART"", ""IDS"", ""DateInsert"" FROM ""FEMCOEPSAP_VALIDACION_VARIABLE"" WHERE ""IDS"" = 1;"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.EntradaEnfoque) Then
                Archivo = "MultiTiendaVariable-Enfoque"
                ConsultaSql = "SELECT ""TIENDA"", ""ENFOQUE"", ""BEGDA"", ""ENDDA"" FROM ""FEMCOEPSAP_ENFOQUE_TDA"";"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.EntradaVentas) Then
                Archivo = "MultiTiendaVariable-Ventas"
                ConsultaSql = "SELECT ""IDSTORE"", ""FECHA"", ""VTACONTABLE"", ""TRAFICO"", ""IDSTATUS"" FROM ""FEMCOEPSAP_VTA_TDA_MULTI_HISTORY"";"
                ModeloOmision = "femcoepdev"
            ElseIf (Pantalla = enuPantalla.MontoDistribuible) Then
                Archivo = "Categoria-MontoDistribuible"
                ConsultaSql = "SELECT idsociety, idplaza, plaza, idstore, store, amount, taxamount, idwagetype FROM public.distributable_category_amount WHERE idstatus = 1;"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.EmpleadosActivos) Then
                Archivo = "Categoria-EmpleadosActivos"
                ConsultaSql = "SELECT ""IDSociety"", ""IDPlaza"", ""Plaza"", ""IDStore"", ""Store"", ""IDPayee"", ""IDPosition"" FROM ""FEMCOVS_ActiveEmployees_CI"" WHERE ""IDStatus"" = true;"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.PorcentajeVenta) Then
                Archivo = "Categoria-PorcentajeVenta"
                ConsultaSql = "SELECT ""categoriaId"", ""sociedadId"", ""plazaId"", ""distritoId"", ""tiendaId"", ""PorcentajeSociedad"", ""IDStatus"" FROM ""FEMCOVS_CfgPorcentajeVentaSociedad"" WHERE ""IDStatus"" = true;"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.VentaMonto) Then
                Archivo = "Categoria-VentaMonto"
                ConsultaSql = "SELECT ""IDSociety"", ""IDPlaza"", ""Plaza"", ""IDStore"", ""Store"", ""IDWageType"", ""SoldUnits"", ""IDStatus"", ""StatusDetail""  FROM ""FEMCOVS_AmountCategorySales"" WHERE ""IDStatus"" = true;"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.VentaUnidades) Then
                Archivo = "Categoria-VentaUnidades"
                ConsultaSql = "SELECT idplaza, plaza, idstore, store, idpayee, idcashier, idwagetype, soldunits FROM femcovs_unitscategorysales WHERE idstatus = 1;"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.ConfiguracionDistribuciones) Then
                Archivo = "IncentivoCerveza-ConfiguracionDistribuciones"
                ConsultaSql = "SELECT * FROM configdistributiondetalles;"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.ConfiguracionPorcentajeVenta) Then
                Archivo = "IncentivoCerveza-ConfiguracionPorcentajeVentas"
                ConsultaSql = "SELECT idsociety, idplaza, iddistrict, idstore, value FROM public.femcovs_configsalespercentage WHERE idstatus = '1';"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.HistoricoEmpleadosActivos) Then
                Archivo = "IncentivoCerveza-HistoricoEmpleadosActivos"
                ConsultaSql = "SELECT id_society, id_plaza, id_store, payee_id, start_date, end_date, role FROM femcovs_hist_active_employees WHERE id_status = '1';"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.Ventas) Then
                Archivo = "VentaSugerida-Ventas"
                ConsultaSql = "SELECT * FROM suggestedsalesales WHERE idstatus = true;"
                ModeloOmision = "femcovsdev"
            ElseIf (Pantalla = enuPantalla.Metas) Then
                Archivo = "VentaSugerida-ImportMetas"
                ConsultaSql = "SELECT IDPlaza, IDStore, DateStart, DateEnd, IDPromotionType, Target FROM suggestedsaletargets;"
                ModeloOmision = "femcovsdev"
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

    ''' <summary>
    ''' Método que envía el archivo PGP.
    ''' </summary>
    ''' <returns>Regresa OK si el archivo fue enviado, caso contrario regresa el mensaje de error.</returns>
    Private Function EnviarArchivoPGP() As String
        Dim retorno As String = "OK"
        Try

            Dim pgp As New DataTable()
            Using conn As New NpgsqlConnection(_NpgSQL)
                Using cmd As New NpgsqlCommand(ConsultaSql, conn)
                    cmd.CommandTimeout = 60 * 30
                    conn.Open()
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(pgp)
                    End Using
                End Using
            End Using

            _Registros = pgp.Rows.Count

            If _Registros > 0 Then
                If Not Main_PGPtoSFTP.Proceso(Archivo, pgp, _Modelo) Then
                    retorno = "Archivo No Enviado"
                End If
            Else
                retorno = "Sin Registros"
            End If

        Catch ex As Exception
            retorno = ex.Message
        End Try
        Return retorno
    End Function

    ''' <summary>
    ''' Método que registra el envío del archivo PGP.
    ''' </summary>
    ''' <param name="mensaje">OK si el archivo fue enviado, caso contrario tiene el mensaje de error.</param>
    Private Sub RegistrarEnvio(mensaje As String)
        Try
            Dim enviado As Boolean = mensaje.Equals("OK")

            Using conn As New NpgsqlConnection(_NpgSQL)
                conn.Open()
                Const consultaSql As String = "CALL public.registrarenviopgp(@p_archivo, @p_modelo, @p_usuario, @p_registros, @p_enviado, @p_mensaje);"
                Using cmd As New NpgsqlCommand(consultaSql, conn)
                    cmd.CommandTimeout = 60 * 5
                    cmd.Parameters.AddWithValue("p_archivo", NpgsqlDbType.Varchar, Archivo)
                    cmd.Parameters.AddWithValue("p_modelo", NpgsqlDbType.Varchar, _Modelo)
                    cmd.Parameters.AddWithValue("p_usuario", NpgsqlDbType.Varchar, _Usuario)
                    cmd.Parameters.AddWithValue("p_registros", NpgsqlDbType.Integer, _Registros)
                    cmd.Parameters.AddWithValue("p_enviado", NpgsqlDbType.Boolean, enviado)
                    cmd.Parameters.AddWithValue("p_mensaje", NpgsqlDbType.Varchar, mensaje)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    Private Sub ValidarParametrosRequeridos()
        Try

            If String.IsNullOrWhiteSpace(Archivo) Then
                Throw New ArgumentException("El nombre de archivo (Archivo) no puede ser vacío.")
            End If

            If String.IsNullOrWhiteSpace(ConsultaSql) Then
                Throw New ArgumentException("La consulta SQL (ConsultaSql) no puede ser vacía.")
            End If

            If String.IsNullOrWhiteSpace(ModeloOmision) Then
                Throw New ArgumentException("El modelo por omisión (ModeloOmision) no puede ser vacío.")
            End If

        Catch ex As Exception
            Throw
        End Try
    End Sub

#End Region

End Class
