Imports Serilog
Imports Serilog.Sinks.MSSqlServer
Imports System.Data

Public Class LoggerConfig

    Private ReadOnly _configuration As IAppConfiguration

    Public Sub New()
        _configuration = New AppConfiguration()
    End Sub

    Public Sub Configurar()

        Dim columnOptions As New ColumnOptions()

        columnOptions.Store.Clear()

        columnOptions.Store.Add(StandardColumn.TimeStamp)
        columnOptions.Store.Add(StandardColumn.Level)
        columnOptions.Store.Add(StandardColumn.Message)
        columnOptions.Store.Add(StandardColumn.Exception)

        columnOptions.TimeStamp.ColumnName = "Fecha"
        columnOptions.Level.ColumnName = "Nivel"
        columnOptions.Message.ColumnName = "Mensaje"
        columnOptions.Exception.ColumnName = "Excepcion"

        columnOptions.AdditionalColumns = New List(Of SqlColumn) From {
            New SqlColumn With {
                .ColumnName = "IdCarga",
                .PropertyName = "IdCarga",
                .DataType = SqlDbType.UniqueIdentifier
            },
            New SqlColumn With {
                .ColumnName = "Periodo",
                .PropertyName = "Periodo",
                .DataType = SqlDbType.VarChar,
                .DataLength = 20
            },
            New SqlColumn With {
                .ColumnName = "Pantalla",
                .PropertyName = "Pantalla",
                .DataType = SqlDbType.VarChar,
                .DataLength = 100
            },
            New SqlColumn With {
                .ColumnName = "Usuario",
                .PropertyName = "Usuario",
                .DataType = SqlDbType.VarChar,
                .DataLength = 100
            }
        }

        Log.Logger = New LoggerConfiguration() _
            .MinimumLevel.Information() _
            .WriteTo.MSSqlServer(
                connectionString:=_configuration.ConnectionString,
                sinkOptions:=New MSSqlServerSinkOptions With {
                    .TableName = "LOGS",
                    .SchemaName = "dbo",
                    .AutoCreateSqlTable = False
                },
                columnOptions:=columnOptions
            ) _
            .CreateLogger()

    End Sub

End Class