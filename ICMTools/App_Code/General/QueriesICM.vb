Imports System.Data
Imports DocumentFormat.OpenXml.Drawing.Diagrams
Imports Npgsql
Imports NpgsqlTypes

Public Class QueriesICM

#Region " Variables Privadas "

    ''' <summary>
    ''' Cadena de conexión a Postgres
    ''' </summary>
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

#End Region

#Region " Métodos Públicos "

    ''' <summary>
    ''' Ejecuta un query de ICM
    ''' </summary>
    ''' <param name="id">Id del query</param>
    ''' <param name="modelo">Modelo</param>
    ''' <returns>Regresa los resultados del query</returns>
    Public Function GetQuery(id As Integer, modelo As String, Optional parametros As Dictionary(Of String, String) = Nothing) As DataTable
        Dim result As New DataTable
        Dim dataQuery As New DataTable
        Try
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Const sql As String = "SELECT * FROM ""GetICMToolsQueryICM""(@p_id)"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("p_id", NpgsqlDbType.Integer, id)
                    Using adapter As New NpgsqlDataAdapter(cmd)
                        adapter.Fill(dataQuery)
                    End Using
                End Using
            End Using

            If (dataQuery Is Nothing Or dataQuery.Rows.Equals(0)) Then
                Throw New Exception("Query ICM no encontrado")
            End If

            Using ws As New ICMTools.WebServiceICMGeneral()
                Dim rowQuery As DataRow = dataQuery.Rows(0)
                Dim tabla As String = rowQuery.Field(Of String)("Tabla")
                Dim consultaTemp As String = rowQuery.Field(Of String)("Consulta")
                Dim consulta As String = consultaTemp.Replace(vbLf, " ").Replace("""", "\""")
                If (parametros IsNot Nothing AndAlso parametros.Count > 0) Then
                    For Each parametro As KeyValuePair(Of String, String) In parametros
                        consulta = consulta.Replace(parametro.Key, parametro.Value)
                    Next
                End If
                result = ws.ConsultaICMAPIQuery(tabla, consulta, modelo)
            End Using

            Return result
        Catch ex As Exception
            Throw
        End Try
    End Function

#End Region

End Class
