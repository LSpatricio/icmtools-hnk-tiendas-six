Option Strict On
Option Explicit On

Imports DocumentFormat.OpenXml.Vml.Office
Imports Npgsql
Imports NpgsqlTypes

Public Class PostgreService
#Region "Variables Locales"
    ''' <summary>
    ''' Cadena de Conexión
    ''' </summary>
    Private ReadOnly NpgsqlConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

    ''' <summary>
    ''' Logger
    ''' </summary>
    Public Sub New()
        mLog = New Log()
    End Sub

    Private ReadOnly mLog As Log
#End Region

#Region "Diccionario"
    ''' <summary>
    ''' Estructura para definir los parametros de PGSQL por cada pantalla 
    ''' </summary>
    Private Structure ScreenConfig
        Public Table As String
        Public Key1Field As String
        Public Key1Value As String
        Public Key2Field As String
        Public Key2Value As String
        Public StatusField As String
        Public MessageField As String
        Public StatusValue As String
        Public MessageValue As String
    End Structure

#End Region

#Region "Metodos Publicos"
    Public Function ActionTryCount(User As String, Key As String, Action As String) As Integer
        Dim Result As Object
        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT attempts FROM public.fn_icmtools_manage_upload_attempts(@Usr, @Key, @Action);", conn)
                    cmd.Parameters.AddWithValue("Usr", NpgsqlDbType.Varchar, User)
                    cmd.Parameters.AddWithValue("Key", NpgsqlDbType.Varchar, Key)
                    cmd.Parameters.AddWithValue("Action", NpgsqlDbType.Varchar, Action)
                    Result = cmd.ExecuteScalar()
                End Using
            End Using

            Return If(Result IsNot Nothing AndAlso Not IsDBNull(Result), Convert.ToInt32(Result), 0)
        Catch ex As Exception
            Throw ex
        End Try
    End Function

    Public Sub ActionResetAll()
        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("CALL public.""ResetAll""();", conn)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Public Sub AbortLoad(Usr As String, Screen As String)

        Dim ScreenMap As New List(Of ScreenConfig)

        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM ""GetScreenMap""(@Screen)", conn)
                    cmd.Parameters.AddWithValue("Screen", NpgsqlDbType.Varchar, Screen)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()

                        Dim getValue = Function(f As String) If(reader(f) Is Nothing OrElse IsDBNull(reader(f)), String.Empty, reader(f).ToString().Trim())

                        While reader.Read
                            ScreenMap.Add(New ScreenConfig With {
                                .Table = getValue("Table"),
                                .Key1Field = getValue("Key1Field"),
                                .Key1Value = getValue("Key1Value"),
                                .Key2Field = getValue("Key2Field"),
                                .Key2Value = getValue("Key2Value"),
                                .StatusField = getValue("StatusField"),
                                .StatusValue = getValue("StatusValue"),
                                .MessageField = getValue("MessageField"),
                                .MessageValue = getValue("MessageValue")
                            })
                        End While
                    End Using
                End Using

                If ScreenMap.Count = 0 OrElse String.IsNullOrWhiteSpace(ScreenMap(0).Table) Then
                    Dim errorMsg As String = $"La Pantalla {Screen} no esta mapeada en PostgreService."
                    mLog.InsertApplicationLog(Screen, "AbortLoad", "Error", errorMsg)
                    Throw New Exception(errorMsg)
                End If

                Dim key2Query As String = ScreenMap(0).Key2Value
                key2Query = key2Query.Replace("@Key1Value@", $"'{Usr}'")
                key2Query = key2Query.Replace("@Key1Field@", $"""{ScreenMap(0).Key1Field}""")

                Using cmd As New NpgsqlCommand("CALL public.""AbortLoad""(@table, @k1field, @usr, @k2field, @k2value, @sfield, @status, @mfield, @mssg);", conn)
                    cmd.Parameters.AddWithValue("table", NpgsqlDbType.Varchar, ScreenMap(0).Table)
                    cmd.Parameters.AddWithValue("k1field", NpgsqlDbType.Varchar, ScreenMap(0).Key1Field)
                    cmd.Parameters.AddWithValue("usr", NpgsqlDbType.Varchar, Usr)
                    cmd.Parameters.AddWithValue("k2field", NpgsqlDbType.Varchar, ScreenMap(0).Key2Field)
                    cmd.Parameters.AddWithValue("k2value", NpgsqlDbType.Varchar, key2Query)
                    cmd.Parameters.AddWithValue("sfield", NpgsqlDbType.Varchar, ScreenMap(0).StatusField)
                    cmd.Parameters.AddWithValue("status", NpgsqlDbType.Varchar, ScreenMap(0).StatusValue)
                    cmd.Parameters.AddWithValue("mfield", NpgsqlDbType.Varchar, ScreenMap(0).MessageField)
                    cmd.Parameters.AddWithValue("mssg", NpgsqlDbType.Varchar, ScreenMap(0).MessageValue)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

        Catch ex As Exception
            mLog.InsertApplicationLog(Screen, "AbortLoad", "Error", $"Ocurrio un error al abortar la carga: {ex}.")
            mLog.NotificacionError(ex, $"{Screen} - AbortLoad")
        End Try
    End Sub
#End Region
End Class
