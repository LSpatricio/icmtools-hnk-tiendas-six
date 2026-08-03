Option Strict On
Option Explicit On

Imports System.Data
Imports Npgsql

Public Class PlantillaCorreo

#Region " Propiedades Públicas "

    ''' <summary>
    ''' Archivo adjunto
    ''' </summary>
    Public ArchivoAdjunto As String

    ''' <summary>
    ''' Parámetros del correo
    ''' </summary>
    Public Parametros As Dictionary(Of String, String)

#End Region

#Region " Propiedades Privadas "

    ''' <summary>
    ''' Cadena de conexión a PostgreSQL
    ''' </summary>
    Private ReadOnly NpgSQL As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString

#End Region

#Region " Métodos Públicos "

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()

    End Sub

    ''' <summary>
    ''' Método que envía un correo.
    ''' </summary>
    ''' <param name="id">Id de la Plantilla de Correo</param>
    Public Sub Enviar(id As Integer)
        Try
            Dim plantillaCorreoTable As New DataTable()
            Using conn As New NpgsqlConnection(NpgSQL)
                conn.Open()
                Dim sql As String = "SELECT * FROM ""PlantillaCorreo""(@p_id);"
                Using cmd As New NpgsqlCommand(sql, conn)
                    cmd.Parameters.AddWithValue("@p_id", id)
                    Using da As New NpgsqlDataAdapter(cmd)
                        da.Fill(plantillaCorreoTable)
                    End Using
                End Using
            End Using

            Dim plantillaCorreoRow As DataRow = plantillaCorreoTable.Rows(0)
            Dim destinatarios As String = plantillaCorreoRow.Field(Of String)("Destinatarios")
            Dim cc As String = plantillaCorreoRow.Field(Of String)("CC")
            Dim asunto As String = plantillaCorreoRow.Field(Of String)("Asunto")
            Dim mensaje As String = plantillaCorreoRow.Field(Of String)("Mensaje").Replace(vbLf, "").Replace(vbTab, "")
            If (Parametros IsNot Nothing And Parametros.Count > 0) Then
                For Each parametro As KeyValuePair(Of String, String) In Parametros
                    asunto = asunto.Replace(parametro.Key, parametro.Value)
                    mensaje = mensaje.Replace(parametro.Key, parametro.Value)
                Next
            End If

            If (plantillaCorreoTable IsNot Nothing And plantillaCorreoTable.Rows.Count > 0) Then
                Using ws As New ICMTools.WebServiceICMGeneral
                    Dim destinatariosList As New List(Of String)(destinatarios.Split(CChar(";")))
                    Dim ccList As New List(Of String)()
                    If (cc.Length > 0) Then ccList = New List(Of String)(cc.Split(CChar(";")))
                    ws.WebServiceSendMailWithFile(destinatariosList, ccList, asunto, mensaje, "femcoepprd", ArchivoAdjunto)
                End Using
            End If
        Catch ex As Exception
            Throw
        End Try
    End Sub

#End Region

End Class