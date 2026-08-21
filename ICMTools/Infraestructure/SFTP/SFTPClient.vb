Imports System.IO
Imports System.Threading.Tasks
Imports RenciSftpClient = Renci.SshNet.SftpClient

Public Class SftpClient

    Private ReadOnly _configuration As IAppConfiguration

    Public Sub New()
        _configuration = New AppConfiguration()
    End Sub

    Public Async Function SubirArchivoAsync(
        rutaArchivo As String
    ) As Task

        Await Task.Run(
                Sub()

                    Using client As New RenciSftpClient(
                    _configuration.SftpHost,
                    _configuration.SftpPort,
                    _configuration.SftpUsername,
                    _configuration.SftpPassword
                )

                        client.Connect()

                        Try

                            Using fileStream As FileStream =
                            File.OpenRead(rutaArchivo)

                                Dim nombreArchivo As String =
                                Path.GetFileName(rutaArchivo)

                                Dim rutaRemota As String =
                                $"{_configuration.SftpRemotePath.TrimEnd("/"c)}/{nombreArchivo}"

                                client.UploadFile(
                                fileStream,
                                rutaRemota,
                                True
                            )

                            End Using

                        Finally

                            If client.IsConnected Then
                                client.Disconnect()
                            End If

                        End Try

                    End Using

                End Sub
        )

    End Function

End Class