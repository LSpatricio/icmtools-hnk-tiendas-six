Imports System.IO
Imports AjaxControlToolkit

Public Class FileClass
    Dim mLog As Log

    Public Function GetNormalizePath(filePath As String) As String
        Try
            If String.IsNullOrWhiteSpace(filePath) Then Return Nothing

            ' Unifica separadores
            Dim raw = filePath.Replace("/"c, Path.DirectorySeparatorChar).Replace("\"c, Path.DirectorySeparatorChar)
            Dim parts = raw.Split(New Char() {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries)
            Dim clean As New List(Of String)

            For Each p In parts
                Dim seg = p.Trim()
                If seg.Length = 0 Then Continue For
                If seg = "." OrElse seg = ".." Then Return Nothing

                ' Elimina caracteres inválidos
                For Each ch In Path.GetInvalidFileNameChars()
                    If ch <> ":" Then
                        seg = seg.Replace(ch, "_"c)
                    End If
                Next

                ' Quita espacios y puntos finales
                seg = seg.Trim().TrimEnd("."c, " "c)
                If seg.Length = 0 Then Return Nothing

                clean.Add(seg)
            Next

            If clean.Count = 0 Then Return Nothing
            Return String.Join(Path.DirectorySeparatorChar, clean)
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Genera una versión segura del nombre de archivo eliminando saltos de línea y otros caracteres que puedan causar errores en rutas o sistemas de archivos.
    ''' </summary>
    ''' <param name="fileName">Nombre original del archivo.</param>
    ''' <returns>Nombre de archivo sanitizado y apto para operaciones de almacenamiento.</returns>
    Public Function GetSafeFileName(fileName As String) As String
        If String.IsNullOrWhiteSpace(fileName) Then
            Throw New ArgumentException("El nombre de archivo no puede estar vacío.")
        End If

        ' 1. Eliminar saltos de línea y espacios extremos
        Dim safeFileName As String = fileName.Trim().
        Replace(Environment.NewLine, "").
        Replace(vbCr, "").
        Replace(vbLf, "")

        ' 2. Eliminar rutas relativas (../ o ..\)
        safeFileName = safeFileName.Replace("../", "").Replace("..\", "")

        ' 3. Eliminar caracteres inválidos para nombres de archivo
        Dim invalidChars As String = Regex.Escape(New String(Path.GetInvalidFileNameChars()))
        safeFileName = Regex.Replace(safeFileName, "[" & invalidChars & "]", "")

        ' 4. Limitar longitud (ej. 255 caracteres)
        If safeFileName.Length > 255 Then
            safeFileName = safeFileName.Substring(0, 255)
        End If

        Return safeFileName
    End Function

    Public Function GetSafeFilePath(filePath) As String
        Try
            Dim onlyPath As String = Path.GetDirectoryName(filePath)
            Dim onlyFile As String = Path.GetFileName(filePath)

            Dim safePath As String = GetNormalizePath(onlyPath)
            Dim safeFile As String = GetSafeFileName(onlyFile)

            Dim safeFilePath As String = Path.Combine(safePath, safeFile)
            Return safeFilePath
        Catch ex As Exception
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Método que guarda el archivo importado por el usuario.
    ''' </summary>
    ''' <param name="asyncFileUpload">Objeto AsyncFileUpload.</param>
    ''' <param name="folder">Carpeta del archivo.</param>
    Public Sub SaveUploadedFile(asyncFileUpload As AsyncFileUpload, folder As String)
        mLog = New Log
        Dim fullFilePath As String = ""
        Dim normalizedBase As String = ""
        Try
            Dim user As User = CType(HttpContext.Current.Session.Item("User"), User)
            Dim fileName = user.Email

            Dim extension As String = Path.GetExtension(asyncFileUpload.FileName).ToLower()
            Dim fullFileName = String.Concat(fileName, extension)
            Dim safeName As String = Path.GetFileNameWithoutExtension(fullFileName)
            Dim finalFileName As String = safeName & extension

            Dim filePath As String = HttpContext.Current.Server.MapPath(folder)
            fullFilePath = Path.Combine(filePath, finalFileName)
            Dim normalizedFull = Path.GetFullPath(fullFilePath)
            normalizedBase = Path.GetFullPath(filePath)

            ''mLog.NoSessionInsertLog("GuardarArchivo", "ARCHIVO IMPORTADO", $"Archivo de guardado en la ruta {normalizedBase}")
            ''mLog.NoSessionInsertLog("GuardarArchivo", "ARCHIVO IMPORTADO DEBUG!!!!", $"{fullFileName} fullFileName || {normalizedBase} normalizedBase || safeName {safeName} || finalFileName {finalFileName} filepath {filePath} || fullFilePath {fullFilePath} || normalizedFull {normalizedFull}")

            If Not normalizedFull.StartsWith(normalizedBase) Then
                Throw New Exception("Ruta inválida.")
            End If

            If Not Directory.Exists(normalizedBase) Then
                Directory.CreateDirectory(normalizedBase)
            End If

            asyncFileUpload.SaveAs(fullFilePath)
        Catch ex As Exception
            mLog.insertLog("FileClass", "SaveUploadedFile", $"File: {fullFilePath}. Folder: {normalizedBase}. Error: {ex.Message}")
            Throw
        End Try
    End Sub

End Class
