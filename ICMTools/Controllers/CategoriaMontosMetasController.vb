Imports System.Web.Http

Public Class CategoriaMontosMetasController
    Inherits ApiController

    ReadOnly sc As New SharedController

    <HttpPost>
    <Route("api/categoriamontosmetas/insertdata")>
    Public Function InsertData(<FromBody> request As FileController.ValidateFileRequest) As IHttpActionResult
        Try
            Dim tBody As String = Nothing
            Dim rTable As String = Nothing
            tBody = "<tr><td>Ejecución Completada Exitosamente</td><td>Se ejecutó correctamente el proceso externo<ul><strong>Carga de Categoría de Montos de Metas favor de revisar el archivo anexo de errores para revisar </strong></td></tr>"
            rTable = sc.TableBuilder(tBody, 3)
            Return Ok(New With {.d = True, .r = rTable, .f = String.Empty})
        Catch ex As Exception
            Return InternalServerError(ex)
        End Try
    End Function

End Class