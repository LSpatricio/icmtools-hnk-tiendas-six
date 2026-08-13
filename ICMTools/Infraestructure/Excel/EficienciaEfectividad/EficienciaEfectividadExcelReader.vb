Imports System.Diagnostics.Eventing
Imports System.DirectoryServices.ActiveDirectory
Imports System.IO
Imports System.Reflection
Imports ExcelDataReader
Imports SixLabors.Fonts.Tables.General


Public Class EficienciaEfectividadExcelReader

    Public ReadOnly Property _excelReader As ExcelReader

    Public Sub New()

        _excelReader = New ExcelReader()

    End Sub
    Public Function ValidacionesEficienciaEfectividad(
    rutaArchivo As String,
    filaEncabezado As Integer,
    nombreHoja As String,
    mapeoColumnas As Dictionary(Of PropertyInfo, ExcelColumnAttribute)) As List(Of ExcelValidationError)

        Using stream = File.Open(
        rutaArchivo,
        FileMode.Open,
        FileAccess.Read
    )

            Using reader = ExcelReaderFactory.CreateReader(stream)


                Return _excelReader.ValidacionesInformacion(reader, filaEncabezado, nombreHoja, mapeoColumnas)
                'moverse a la hoja pedida


            End Using
        End Using

    End Function

End Class
