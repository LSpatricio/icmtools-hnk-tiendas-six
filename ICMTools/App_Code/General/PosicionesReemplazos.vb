Imports System.Data
Imports System.Data.SqlClient
Imports System.Web.Http.Results
Imports Microsoft.VisualBasic
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Public Class PosicionesReemplazos

    Private _ReplacementIDPosition As String
    Private _ReplacementPosition As String

    Public Property ReplacementIDPosition() As String
        Get
            Return _ReplacementIDPosition
        End Get
        Set(ByVal value As String)
            _ReplacementIDPosition = value
        End Set
    End Property

    Public Property ReplacementPosition() As String
        Get
            Return _ReplacementPosition
        End Get
        Set(ByVal value As String)
            _ReplacementPosition = value
        End Set
    End Property

End Class

Public Class RemplazoICMTools
#Region "Propiedades"
    Public Property PayeeID As String
    Public Property IDPosition As String
    Public Property IDSociety As String
    Public Property IDPersonalDivision As String
    Public Property IDJobKey As String
    Public Property IDCostCenter As String
    Public Property statusPayee As String
    Public Property Email As String
    Public Property Name As String
#End Region
    Public Shared Function GetAuthorizerByPosition(FinalTable As DataTable, PayeeTable As DataTable) As RemplazoICMTools
        Dim NpgsqlConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
        Dim result As New RemplazoICMTools
        Dim dt As New DataTable()
        Dim FinalJson As String = JsonConvert.SerializeObject(FinalTable)
        Dim PayeeJson As String = JsonConvert.SerializeObject(PayeeTable)

        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT public.""FEMCOEPSAP_spICMToolsReemplazosValidate""(@FinalTable, @PayeeTable)", conn)
                    cmd.Parameters.AddWithValue("FinalTable", NpgsqlDbType.Json, FinalJson)
                    cmd.Parameters.AddWithValue("PayeeTable", NpgsqlDbType.Json, PayeeJson)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using

                    If dt IsNot Nothing Then
                        For Each row As DataRow In dt.Rows
                            result.PayeeID = row.Item(0).ToString()
                            result.IDPosition = row.Item(1).ToString()
                            result.IDSociety = row.Item(2).ToString()
                            result.IDPersonalDivision = row.Item(3).ToString()
                            result.IDJobKey = row.Item(4).ToString()
                            result.IDCostCenter = row.Item(5).ToString()
                            result.statusPayee = row.Item(6).ToString()
                            result.Email = row.Item(7).ToString()
                            result.Name = row.Item(8).ToString()
                        Next
                        Return result
                    Else
                        Return New RemplazoICMTools()
                    End If
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error de PostgreSQL: " & ex.Message)
        End Try
        Return Nothing
    End Function

    Public Shared Function GetAuthorizedPosition(IDPosition As String, moduleKey As String) As String
        Dim NpgsqlConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
        Dim result As String = Nothing

        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM ""FEMCOEPSAP_spICMToolsGetAuthorizedPosition""(@IDPosition, @ModuleKey)", conn)
                    cmd.Parameters.AddWithValue("IDPosition", NpgsqlDbType.Varchar, IDPosition)
                    cmd.Parameters.AddWithValue("ModuleKey", NpgsqlDbType.Varchar, moduleKey)
                    Dim ScalarResult As Object = cmd.ExecuteScalar().ToString()
                    If ScalarResult IsNot Nothing AndAlso Not Convert.IsDBNull(ScalarResult) Then
                        result = ScalarResult.ToString()
                    End If
                End Using
            End Using

            Return result

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error de PostgreSQL: " & ex.Message)
        End Try
        Return Nothing
    End Function

    Public Shared Function GetPayeeList() As List(Of String)
        Dim NpgsqlConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
        Dim dt As New DataTable()
        Dim PayeeList As New List(Of String)()

        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT idpositionreplacement FROM ICMToolsReplacement", conn)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        dt.Load(reader)
                    End Using
                End Using
            End Using

            If dt IsNot Nothing AndAlso dt.Rows.Count > 0 Then
                PayeeList = dt.AsEnumerable().Select(Function(row) row.Field(Of String)(0)).ToList()
            End If
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error de PostgreSQL: " & ex.Message)
        End Try
        Return PayeeList
    End Function
End Class