Imports System.ComponentModel.DataAnnotations
Imports System.IO
Imports AjaxControlToolkit
Imports System.Text
Imports DocumentFormat.OpenXml.Office.MetaAttributes

Public Class ImportVentas
    Inherits System.Web.UI.Page

    Private mUser As User
    Private mLog As Log
    Public UserEmail As String

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-trophy fa-fw'></i>"
            Me.Master.PageName = "Ventas"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)

                UserEmail = mUser.Email

                mLog = New Log
                mLog.insertLog("Importación de Ventas", "ACCESO", "Acceso a Importación de Ventas")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Sub AsyncFileUpload1_UploadedComplete(ByVal sender As Object, ByVal e As AsyncFileUploadEventArgs) Handles AsyncFileUpload1.UploadedComplete

        If Session.Item("User") Is Nothing Then
            Return
        End If

        mUser = CType(Session.Item("User"), User)
        Dim fileExtension As String = Path.GetExtension(e.FileName)
        Dim baseSavePath As String = Server.MapPath("~\UploadedFiles\VentaSugerida\ImportVentas\")
        Dim originalFileName As String = $"{Guid.NewGuid()}{fileExtension}"
        Dim originalFullFilePath As String = Path.Combine(baseSavePath, originalFileName)
        Dim maxChunkSize As Long = 50 * 1024 * 1024 ''50MB 
        Dim ChunkList As New List(Of String)

        If Not Directory.Exists(baseSavePath) Then
            Directory.CreateDirectory(baseSavePath)
        End If

        AsyncFileUpload1.SaveAs(originalFullFilePath)
        mLog = New Log
        mLog.insertLog("Importacion de Ventas", "ARCHIVO IMPORTADO", "Archivo grande recibido: " & e.FileName)

        Try
            If fileExtension.ToLower() = ".csv" Then
                ChunkList = SplitCsvFileIntoChunks(originalFullFilePath, maxChunkSize)
            ElseIf fileExtension.ToLower() = ".xlsx" Then
                Return
            End If

            Session("VentasChunkFiles") = ChunkList

            mLog.insertLog("Importación de Ventas", "DIVISION DE ARCHIVO", $"Archivo dividido en {ChunkList.Count} partes.")
        Catch ex As Exception
            mLog.insertLog("Importación de Ventas", "ERROR DE DIVISIÓN", "Error al dividir el archivo: " + ex.Message)
        Finally
            If File.Exists(originalFullFilePath) Then
                File.Delete(originalFullFilePath)
                mLog.insertLog("Importación de Ventas", "LIMPIEZA", "Archivo grande original Eliminado")
            End If
        End Try
    End Sub

    Private Function SplitCsvFileIntoChunks(sourceFilePath As String, maxChunkSizeInBytes As Long) As List(Of String)
        Try
            Dim generatedChunkFilePaths As New List(Of String)
            Dim chunkIndex As Integer = 1
            Dim fileID = Path.GetFileNameWithoutExtension(sourceFilePath)
            Dim baseDirectory = Path.GetDirectoryName(sourceFilePath)
            Dim headerLine As String

            Using tempReader As New StreamReader(sourceFilePath)
                headerLine = tempReader.ReadLine()
            End Using

            If String.IsNullOrWhiteSpace(headerLine) Then
                Throw New ArgumentException("La cabecera (headerLine) no puede estar vacía.", "headerLine")
            End If

            Using reader As New StreamReader(sourceFilePath)
                If Not reader.EndOfStream Then
                    reader.ReadLine()
                End If

                While Not reader.EndOfStream
                    Dim currentChunkFilePath = Path.Combine(baseDirectory, $"{fileID}_Parte {chunkIndex}.csv")
                    generatedChunkFilePaths.Add(currentChunkFilePath)

                    Using writer As New StreamWriter(currentChunkFilePath, False, System.Text.Encoding.UTF8)
                        writer.WriteLine(headerLine)
                        Dim currentChunkSize As Long = System.Text.Encoding.UTF8.GetByteCount(headerLine + Environment.NewLine)

                        While Not reader.EndOfStream AndAlso currentChunkSize < maxChunkSizeInBytes
                            Dim dataLine = reader.ReadLine()
                            If dataLine IsNot Nothing Then
                                writer.WriteLine(dataLine)
                                currentChunkSize += System.Text.Encoding.UTF8.GetByteCount(dataLine + Environment.NewLine)
                            End If
                        End While
                    End Using
                    chunkIndex += 1
                End While
            End Using
            Return generatedChunkFilePaths
        Catch ex As Exception
            Throw
        End Try
    End Function

End Class