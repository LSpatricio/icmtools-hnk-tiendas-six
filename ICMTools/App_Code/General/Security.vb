Imports System.Data
Imports System.Data.SqlClient
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Web.Helpers
Imports ICMTools
Imports Npgsql
Imports NpgsqlTypes
Public Class ModelPermission
    Public Property IDModel As String
    Public Property Model As String
    Public Property ModelDescription As String

End Class
Public Class AppScreen
    Public Property IDModel As Long
    Public Property IDScreen As Long
    Public Property ScreenName As String
    Public Property URL As String
End Class

Public Class ScreenAggregator
    Public Property IDAggregator As Long
    Public Property AggregatorDescription As String
    Public Property Screens As New List(Of AppScreen)
End Class
Public Class MenuData
    Public Property SimpleItems As New List(Of AppScreen)
End Class

Public Class ScreenPermission
    Public Property IDModel As String
    Public Property Model As String
    Public Property IDPosition As String
    Public Property IDScreen As String
    Public Property Screen As String
    Public Property ScreenDescription As String
    Public Property ModuleDescription As String
    Public Property URL As String
    Public Property IsActive As String

    Public Shared Function Access(LastHistoryPayee As String, user As String, screen As String) As Boolean
        Dim NpgsqlConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
        Dim dt As New DataTable
        Dim resultList As New List(Of ScreenPermission)

        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM ""FEMCOEPSAP_spICMToolsScreenPermission""(@ScreenParameter, @Email, @LastHistoryPayee)", conn)
                    cmd.Parameters.AddWithValue("ScreenParameter", NpgsqlDbType.Varchar, screen)
                    cmd.Parameters.AddWithValue("Email", NpgsqlDbType.Varchar, user)
                    cmd.Parameters.AddWithValue("LastHistoryPayee", NpgsqlDbType.Varchar, LastHistoryPayee)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                    If dt IsNot Nothing Then
                        For Each row As DataRow In dt.Rows
                            Dim DataRow = New ScreenPermission()
                            DataRow.IDScreen = row.Item(0).ToString()
                            DataRow.Screen = row.Item(1).ToString()
                            DataRow.IDPosition = row.Item(2).ToString()
                            resultList.Add(DataRow)
                        Next
                        Return resultList.Count > 0
                    Else
                        Return False ' Devuelve una lista vacía si no hay datos
                    End If
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error de PostgreSQL: " & ex.Message)
            Return Nothing
        End Try
        Return Nothing
    End Function

    ''' <summary>
    '''Método que obtiene los modelos a los que el usuario tiene permisos 
    ''' </summary>
    ''' <param name="user"></param>
    ''' <returns></returns>
    Public Shared Function ModelPermission(user As String) As List(Of ModelPermission)
        Try
            Dim Npgsql As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
            Dim dt As New DataTable
            Dim resultList As New List(Of ModelPermission)

            Using conn As New NpgsqlConnection(Npgsql)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.""GetModelsByEmail""(@requestEmail);", conn)
                    cmd.Parameters.AddWithValue("requestEmail", user)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            If dt IsNot Nothing Then
                For Each row As DataRow In dt.Rows
                    Dim DataRow = New ModelPermission()
                    DataRow.IDModel = row.Item("IDModel").ToString()
                    DataRow.Model = row.Item("Model").ToString()
                    DataRow.ModelDescription = row.Item("ModelDescription").ToString()
                    resultList.Add(DataRow)
                Next
                Return resultList
            Else
                Return resultList ' Devuelve una lista vacía si no hay datos
            End If
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Método que obtiene las pantallas habilitadas para el modelo seleccionado
    ''' </summary>
    ''' <param name="idModel">Recibe el Modelo seleccionado para consultar las pantallas que pertenecen al modelo</param>
    ''' <returns></returns>
    Public Shared Function ScreenPermission(idModel As Integer) As MenuData
        Try
            Dim Npgsql As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
            Dim dt As New DataTable
            Dim aggregatedList As New List(Of ScreenAggregator)
            Dim simpleList As New List(Of AppScreen)
            Dim currentAggregator As ScreenAggregator = Nothing

            Using conn As New NpgsqlConnection(Npgsql)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM public.""GetScreensByModel""(@requestModel);", conn)
                    cmd.Parameters.AddWithValue("requestModel", idModel)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dt)
                    End Using
                End Using
            End Using

            If dt IsNot Nothing Then
                ' Lógica para procesar el DataTable y llenar las listas
                For Each row As DataRow In dt.Rows
                    Dim isAggregatorNull As Boolean = (row("IDAggregator") Is DBNull.Value)
                    Dim modelId As Long = CLng(row("IDModel"))
                    Dim screenId As Long = CLng(row("IDScreen"))
                    Dim screenName As String = CStr(row("ScreenName"))
                    Dim screenURL As String = CStr(row("ScreenURL"))

                    If isAggregatorNull Then
                        Dim simpleItem As New AppScreen With {
                .IDModel = modelId,
                .IDScreen = screenId,
                .ScreenName = screenName,
                .URL = screenURL
            }
                        simpleList.Add(simpleItem)
                    Else
                        Dim currentAggregatorId As Long = CLng(row("IDAggregator"))
                        If currentAggregator Is Nothing OrElse currentAggregator.IDAggregator <> currentAggregatorId Then
                            currentAggregator = New ScreenAggregator With {
                    .IDAggregator = currentAggregatorId,
                    .AggregatorDescription = CStr(row("AggregatorDescription"))
                }
                            aggregatedList.Add(currentAggregator)
                        End If
                        Dim menuItem As New AppScreen With {
                .IDModel = modelId,
                .IDScreen = screenId,
                .ScreenName = screenName,
                .URL = screenURL
            }
                        currentAggregator.Screens.Add(menuItem)
                    End If
                Next

                ' Crea una instancia de la clase contenedora y la retorna
                Dim menuData As New MenuData()
                menuData.SimpleItems = simpleList
                'menuData.AggregatedItems = aggregatedList

                Return menuData
            Else
                Return Nothing ' Devuelve una lista vacía si no hay datos
            End If
        Catch ex As Exception
            Throw
        End Try
    End Function

End Class