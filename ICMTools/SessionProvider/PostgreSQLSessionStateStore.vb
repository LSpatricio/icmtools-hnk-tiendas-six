Imports System.IO
Imports System.Configuration
Imports System.Web.Configuration
Imports System.Web.SessionState
Imports Npgsql
Imports System.Collections.Specialized

Public Class PostgreSQLSessionStateStore
    ''La clase hereda de SessionStateStoreProviderBase, lo que nos obliga a implementar una sere de metodos que ASP.NET usarpa para gestionar el estado de sesion
    Inherits SessionStateStoreProviderBase

    ''Variables para lamacenar la configuración obtenida desde el WebConfig
    Private _connectionString As String
    Private _applicationName As String

#Region "Inicializacion"
    ''Este método se llama una sola vez por el runtime de ASP cuando la App se inicia, su propósito es inicializar el proveedor con la config definida en el WebConfig
    Public Overrides Sub Initialize(name As String, config As NameValueCollection)
        ''Llama al método Initialize de la clase Base
        MyBase.Initialize(name, config)
        _applicationName = If(String.IsNullOrEmpty(config("applicationName")), System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath, config("applicationName"))

        Dim connectionStringName As String = config("connectionStringName")
        If String.IsNullOrEmpty(connectionStringName) Then
            Throw New ConfigurationErrorsException("El atributo 'connectionStringName' es obligatorio para el proveedor de sesión.")
        End If

        _connectionString = WebConfigurationManager.ConnectionStrings(connectionStringName).ConnectionString
        If String.IsNullOrEmpty(_connectionString) Then
            Throw New ConfigurationErrorsException("No se encontró la cadena de conexión '" & connectionStringName & "' en el archivo de configuración.")
        End If
    End Sub
#End Region

#Region "Métodos Principales del Proveedor"
    ''El método es llamado al final de una petición para guardar los datos de la sesión en la BD, además de liberar el bloqueo sobre el registro de la sesión
    Public Overrides Sub SetAndReleaseItemExclusive(context As HttpContext, id As String, item As SessionStateStoreData, lockId As Object, newItem As Boolean)
        Try
            ''Serializa los datos de la sesión a un array de bytes para almacenarlo en la columna bytea de PostgreSQL
            Dim sessionItemsBytes As Byte()

            Using ms As New MemoryStream()
                Using writer As New BinaryWriter(ms)
                    If item.Items.Count > 0 Then
                        CType(item.Items, SessionStateItemCollection).Serialize(writer)
                    End If
                    sessionItemsBytes = ms.ToArray()
                End Using
            End Using

            '<summary>
            ''Si la sesión es nueva (newItem = true), inserta un nuevo registro.
            ''Si la sesion ya existe, actualiza el registr existente.
            ''En ambos casos, libera el bloqueo sobre el registro de la sesión.
            '</summary>
            Dim sql As String = "
            INSERT INTO public.aspnet_sessions (session_id, application_name, created_at, expires_at, lock_at, lock_id, timeout, locked, session_items, flags)
            VALUES (@session_id, @application_name, NOW(), @expires_at, NOW(), 0, @timeout, FALSE, @session_items, 0)
            ON CONFLICT (session_id, application_name) DO UPDATE SET
                expires_at = @expires_at,
                session_items = @session_items,
                locked = FALSE
            WHERE public.aspnet_sessions.session_id = @session_id AND public.aspnet_sessions.application_name = @application_name AND public.aspnet_sessions.lock_id = @lock_id;
        "

            Using conn As New NpgsqlConnection(_connectionString)
                Using cmd As New NpgsqlCommand(sql, conn)
                    ''Asigna los parámetros para el comando SQL
                    cmd.Parameters.AddWithValue("@session_id", id)
                    cmd.Parameters.AddWithValue("@application_name", _applicationName)
                    cmd.Parameters.AddWithValue("@expires_at", DateTime.UtcNow.AddMinutes(item.Timeout))
                    cmd.Parameters.AddWithValue("@timeout", item.Timeout)
                    cmd.Parameters.AddWithValue("@session_items", sessionItemsBytes)
                    cmd.Parameters.AddWithValue("@lock_id", CInt(lockId))

                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    '<summary>
    ''Este método se llama al inicio de una petición para obtener los datos de la sesión desde la BD.
    ''También establece un bloqueo en el registro para evitar que otras peticiones concurrentes del mismo ususario lo modifiquen.
    '<summary/>
    Public Overrides Function GetItemExclusive(context As HttpContext, id As String, ByRef locked As Boolean, ByRef lockAge As TimeSpan, ByRef lockId As Object, ByRef actions As SessionStateActions) As SessionStateStoreData
        Try
            Dim sessionData As SessionStateStoreData = Nothing
            Dim currentLockId As Integer = 0

            '<summary>
            ''El query intenta obtener el registro el registro y bloquearlo atómicamente
            ''1. Actualiza el registro para marcarlo como bloqueado (locked = TRUE) y estampa un nuevo lock_id
            ''2. La cláusula WHERE asegura que solo se pueda bloquear si no está bloqueado al momento del query, o si el bloqueo expiró.
            ''3. La cláusula RETURNING devuelve los valores de la fila que fue actualizada (bloqueada).
            '<summary/>
            Dim sql As String = "
            UPDATE public.aspnet_sessions
            SET locked = TRUE,
                lock_at = NOW(),
                lock_id = lock_id + 1
            WHERE session_id = @session_id
              AND application_name = @application_name
              AND (locked = FALSE OR lock_at < @lock_expiry_time)
            RETURNING session_items, flags, timeout, lock_id;
        "

            Using conn As New NpgsqlConnection(_connectionString)
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@session_id", id)
                    cmd.Parameters.AddWithValue("@application_name", _applicationName)
                    ''Se considera que un bloqueo puede expirar despues de un tiempo de espera razonable
                    cmd.Parameters.AddWithValue("@lock_expiry_time", DateTime.UtcNow.AddSeconds(-30))

                    conn.Open()
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader(CommandBehavior.SingleRow)
                        If reader.Read() Then
                            locked = False

                            Dim sessionItemBytes As Byte() = CType(reader("session_items"), Byte())
                            Dim timeout As Integer = Convert.ToInt32(reader("timeout"))
                            currentLockId = Convert.ToInt32(reader("lock_id"))
                            lockId = currentLockId

                            ''Deserializa el array de Bytes de vuelta a un objeto de sesion.
                            Using ms As New MemoryStream(sessionItemBytes)
                                Dim items As ISessionStateItemCollection = New SessionStateItemCollection()
                                If ms.Length > 0 Then
                                    Dim reader2 As New BinaryReader(ms)
                                    items = SessionStateItemCollection.Deserialize(reader2)
                                End If
                                sessionData = New SessionStateStoreData(items, SessionStateUtility.GetSessionStaticObjects(context), timeout)
                            End Using
                        Else
                            locked = True
                            lockAge = TimeSpan.Zero
                            lockId = 0
                        End If
                    End Using
                End Using
            End Using

            Return sessionData
        Catch ex As Exception
            Throw
        End Try
    End Function

    '<summary>
    ''Libera el bloqueo de un item de sesión sin modificar su contenido
    '<summary/>
    Public Overrides Sub ReleaseItemExclusive(context As HttpContext, id As String, lockId As Object)
        Try
            Dim sql As String = "
            UPDATE public.aspnet_sessions
            SET locked = FALSE
            WHERE session_id = @session_id AND application_name = @application_name AND lock_id = @lock_id;
        "
            Using conn As New NpgsqlConnection(_connectionString)
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@session_id", id)
                    cmd.Parameters.AddWithValue("@application_name", _applicationName)
                    cmd.Parameters.AddWithValue("@lock_id", lockId)

                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    '<summary>
    ''Elimina un item de sesion de la BD. Se llama, por ejemplo, cuando se invoca Session.Abandon().
    '<summary/>
    Public Overrides Sub RemoveItem(context As HttpContext, id As String, lockId As Object, item As SessionStateStoreData)
        Try
            Dim sql As String = "
            DELETE FROM public.aspnet_sessions
            WHERE session_id = @session_id AND application_name = @application_name AND lock_id = @lock_id;
        "
            Using conn As New NpgsqlConnection(_connectionString)
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@session_id", id)
                    cmd.Parameters.AddWithValue("@application_name", _applicationName)
                    cmd.Parameters.AddWithValue("@lock_id", lockId)

                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    '<summary>
    ''Crea un registro vacío en la BD para una nueva sesión
    ''Esto es necesario para sesiones sin cookies, donde el ID de sesión se genera antes de que se almacenen datos.
    '<summary/>
    Public Overrides Sub CreateUninitializedItem(context As HttpContext, id As String, timeout As Integer)
        Try
            Dim sql As String = "
            INSERT INTO public.aspnet_sessions (session_id, application_name, created_at, expires_at, lock_at, lock_id, timeout, locked, session_items, flags)
            VALUES (@session_id, @application_name, NOW(), @expires_at, NOW(), 0, @timeout, FALSE, NULL, 1);
        "
            Using conn As New NpgsqlConnection(_connectionString)
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@session_id", id)
                    cmd.Parameters.AddWithValue("@application_name", _applicationName)
                    cmd.Parameters.AddWithValue("@expires_at", DateTime.UtcNow.AddMinutes(timeout))
                    cmd.Parameters.AddWithValue("@timeout", timeout)

                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub

    '<summary>
    ''Actualiza la fecha de expiración de una sesión para mantenerla viva
    '<summary/>
    Public Overrides Sub ResetItemTimeout(context As HttpContext, id As String)
        Try
            Dim sessionStateConfig As SessionStateSection = CType(ConfigurationManager.GetSection("system.web/sessionState"), SessionStateSection)
            Dim timeoutInMinutes As Integer = CType(sessionStateConfig.Timeout.TotalMinutes, Integer)
            Dim sql As String = "
            UPDATE public.aspnet_sessions
            SET expires_at = @expires_at
            WHERE session_id = @session_id AND application_name = @application_name;    
        "
            Using conn As New NpgsqlConnection(_connectionString)
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@session_id", id)
                    cmd.Parameters.AddWithValue("@application_name", _applicationName)
                    cmd.Parameters.AddWithValue("@expires_at", DateTime.UtcNow.AddMinutes(timeoutInMinutes))

                    conn.Open()
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw
        End Try
    End Sub
#End Region

#Region "Métodos Auxiliares y Desechables"
    Public Overrides Function CreateNewStoreData(context As HttpContext, timeout As Integer) As SessionStateStoreData
        Return New SessionStateStoreData(New SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout)
    End Function

    Public Overrides Function GetItem(context As HttpContext, id As String, ByRef locked As Boolean, ByRef lockAge As TimeSpan, ByRef lockId As Object, ByRef actions As SessionStateActions) As SessionStateStoreData
        Return GetItemExclusive(context, id, locked, lockAge, lockId, actions)
    End Function

    ''Metodos que no son necesarios para la implementacion del proveedor, pero que la clase requiere
    Public Overrides Sub Dispose()
        ''No hay anda que desechar, ya que las conexiones se gestionan con bloques Using
    End Sub

    Public Overrides Sub InitializeRequest(context As HttpContext)
        ''No se requiere inicializacion por peticion''
    End Sub

    Public Overrides Sub EndRequest(context As HttpContext)
        ''No se require limpieza por petición''
    End Sub

    Public Overrides Function SetItemExpireCallback(expireCallback As SessionStateItemExpireCallback) As Boolean
        ''Este proveedor de sesion no soporta la devolución de llamada de expiración''
        ''Devuelve False como indicacion de que esta caracteristica no esta implementada
        Return False
    End Function
#End Region
End Class