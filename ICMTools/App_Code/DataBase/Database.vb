
Imports System.Data.SqlClient
Imports System.Data

Imports System
Imports System.Reflection

Public Class DataBase
    Implements IDisposable

    Private mConnectionString As String
    Private mSqlCnn As SqlConnection
    Private mSqlTran As SqlTransaction

    Public Enum EnumExecutionType
        Scalar
        NonQuery
    End Enum

    ''' <summary>
    ''' Create database object
    ''' </summary>
    ''' <param name="ConnectionString">Database Connection String</param>
    ''' <remarks></remarks>
    Public Sub New(ByVal ConnectionString As String)
        mConnectionString = ConnectionString
        Try
            mSqlCnn = New SqlConnection(mConnectionString)
        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        End Try
    End Sub

#Region "Connection"

    ''' <summary>
    ''' Get or Set database connection string
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ConnectionString() As String
        Get
            Return mConnectionString
        End Get
        Set(ByVal value As String)
            mConnectionString = value
        End Set
    End Property

    ''' <summary>
    ''' Get database connection object
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property Connection() As SqlConnection
        Get
            Return mSqlCnn
        End Get
    End Property

    ''' <summary>
    ''' Test active connection, open an close it to check
    ''' </summary>
    ''' <remarks></remarks>
    Public Function ConnectionTest() As Boolean
        Dim bResult As Boolean = False
        Try
            If mSqlCnn IsNot Nothing Then
                If mSqlCnn.State <> ConnectionState.Open Then
                    mSqlCnn.Open()
                    mSqlCnn.Close()
                    bResult = True
                End If
            End If
            Return bResult
        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        End Try

    End Function

    ''' <summary>
    ''' Closes active connection object and its transaction object if exist, also destroy objects)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ConnectionFinalize()
        Try
            If mSqlCnn IsNot Nothing Then
                If mSqlCnn.State = ConnectionState.Open Then
                    mSqlCnn.Close()
                End If
                If mSqlTran IsNot Nothing Then
                    mSqlTran.Dispose()
                End If
                mSqlCnn.Dispose()
            End If
        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        End Try

    End Sub


#End Region

#Region "Transactions"

    ''' <summary>
    ''' Create a transaction over the existing connection object
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CreateTransaction()
        Try
            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
                mSqlTran = mSqlCnn.BeginTransaction()
            End If
        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        End Try
    End Sub

    ''' <summary>
    ''' Cancel a created transaction, uses RollBack
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub RollBackTransaction()
        Try
            If mSqlTran IsNot Nothing Then
                mSqlTran.Rollback()
            End If
        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        End Try
    End Sub

    ''' <summary>
    ''' Finalize the transaction created, uses Commit and destroy the transaction object
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CommitTransaction()
        Try
            If mSqlTran IsNot Nothing Then
                mSqlTran.Commit()
                mSqlTran.Dispose()
                mSqlTran = Nothing
            End If
        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        End Try
    End Sub

#End Region

#Region "DataTables"

    ''' <summary>
    ''' Get a DataTable from a stored procedure using parameters
    ''' </summary>
    ''' <param name="StoredProcedureName">Name of the stored procedure to execute</param>
    ''' <param name="Params">Array of stored procedure parameters</param>
    ''' <remarks></remarks>
    Public Function GetDataAsDataTable(ByVal StoredProcedureName As String, ByVal ParamArray Params() As SqlClient.SqlParameter) As DataTable
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlAdp As SqlClient.SqlDataAdapter
        Dim sqlTbl As DataTable

        sqlCmd = New SqlClient.SqlCommand
        sqlAdp = New SqlClient.SqlDataAdapter(sqlCmd)

        Try

            If mSqlCnn IsNot Nothing AndAlso mSqlCnn.State <> ConnectionState.Open Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = StoredProcedureName
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandType = CommandType.StoredProcedure
            sqlCmd.CommandTimeout = 60

            sqlCmd.Parameters.AddRange(Params)

            sqlTbl = New DataTable
            sqlAdp.Fill(sqlTbl)

            Return sqlTbl

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If mSqlCnn IsNot Nothing And mSqlCnn.State = ConnectionState.Open Then
                mSqlCnn.Close()
            End If
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
            If sqlAdp IsNot Nothing Then
                sqlAdp.Dispose()
            End If
        End Try

    End Function

    ''' <summary>
    ''' Get a DataTable from a Query
    ''' </summary>
    ''' <param name="Query">SQL query instruction to execute</param>
    ''' <param name="QueryType">Type of Query.</param>
    ''' <remarks></remarks>
    Public Function GetDataAsDataTable(ByVal Query As String, ByVal QueryType As CommandType) As DataTable
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlAdp As SqlClient.SqlDataAdapter
        Dim sqlTbl As DataTable

        sqlCmd = New SqlClient.SqlCommand
        sqlAdp = New SqlClient.SqlDataAdapter(sqlCmd)

        Try

            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = Query
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandType = QueryType

            sqlTbl = New DataTable
            sqlAdp.Fill(sqlTbl)

            Return sqlTbl

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If mSqlCnn IsNot Nothing And mSqlCnn.State = ConnectionState.Open Then
                mSqlCnn.Close()
            End If
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
            If sqlAdp IsNot Nothing Then
                sqlAdp.Dispose()
            End If
        End Try

    End Function

    ''' <summary>
    ''' Get a DataTable from a stored procedure using parameters
    ''' </summary>
    ''' <param name="StoredProcedureName">Name of the stored procedure to execute</param>
    ''' <remarks></remarks>
    Public Function GetDataAsDataTable(ByVal StoredProcedureName As String) As DataTable
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlAdp As SqlClient.SqlDataAdapter
        Dim sqlTbl As DataTable

        sqlCmd = New SqlClient.SqlCommand
        sqlAdp = New SqlClient.SqlDataAdapter(sqlCmd)

        Try

            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = StoredProcedureName
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandType = CommandType.StoredProcedure

            sqlTbl = New DataTable
            sqlAdp.Fill(sqlTbl)

            Return sqlTbl

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If mSqlCnn IsNot Nothing And mSqlCnn.State = ConnectionState.Open Then
                mSqlCnn.Close()
            End If
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
            If sqlAdp IsNot Nothing Then
                sqlAdp.Dispose()
            End If
        End Try

    End Function

    ''' <summary>
    ''' Get a DataTable from a stored procedure using timeout period
    ''' </summary>
    ''' <param name="StoredProcedureName">Name of the stored procedure to execute</param>
    ''' <param name="TimeOut">Timeout period</param>
    ''' <remarks></remarks>
    Public Function GetDataAsDataTable(ByVal StoredProcedureName As String, ByVal TimeOut As Integer) As DataTable
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlAdp As SqlClient.SqlDataAdapter
        Dim sqlTbl As DataTable

        sqlCmd = New SqlClient.SqlCommand
        sqlAdp = New SqlClient.SqlDataAdapter(sqlCmd)

        Try
            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = StoredProcedureName
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandTimeout = TimeOut
            sqlCmd.CommandType = CommandType.StoredProcedure

            sqlTbl = New DataTable
            sqlAdp.Fill(sqlTbl)

            Return sqlTbl

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If mSqlCnn IsNot Nothing And mSqlCnn.State = ConnectionState.Open Then
                mSqlCnn.Close()
            End If
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
            If sqlAdp IsNot Nothing Then
                sqlAdp.Dispose()
            End If
        End Try

    End Function

    ''' <summary>
    ''' Get a DataTable from a stored procedure using parameters and a timeout period
    ''' </summary>
    ''' <param name="StoredProcedureName">Name of the stored procedure to execute</param>
    ''' <param name="Params">Array of stored procedure parameters</param>
    ''' <param name="TimeOut">Timeout period</param>
    ''' <remarks></remarks>
    Public Function GetDataAsDataTable(ByVal StoredProcedureName As String, ByVal TimeOut As Integer, ByVal ParamArray Params() As SqlClient.SqlParameter) As DataTable
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlAdp As SqlClient.SqlDataAdapter
        Dim sqlTbl As DataTable

        sqlCmd = New SqlClient.SqlCommand
        sqlAdp = New SqlClient.SqlDataAdapter(sqlCmd)

        Try
            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = StoredProcedureName
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandTimeout = TimeOut
            sqlCmd.CommandType = CommandType.StoredProcedure

            sqlCmd.Parameters.AddRange(Params)

            sqlTbl = New DataTable
            sqlAdp.Fill(sqlTbl)

            Return sqlTbl

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If mSqlCnn IsNot Nothing And mSqlCnn.State = ConnectionState.Open Then
                mSqlCnn.Close()
            End If
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
            If sqlAdp IsNot Nothing Then
                sqlAdp.Dispose()
            End If
        End Try

    End Function

#End Region

#Region "DataReaders"

    ''' <summary>
    ''' Get a DataReader from a stored procedure using parameters
    ''' </summary>
    ''' <param name="StoredProcedureName">Name of the stored procedure to execute</param>
    ''' <param name="Params">Array of stored procedure parameters</param>
    ''' <remarks></remarks>
    Public Function GetDataAsReader(ByVal StoredProcedureName As String, ByVal ParamArray Params() As SqlClient.SqlParameter) As SqlClient.SqlDataReader
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlRdr As SqlClient.SqlDataReader

        sqlCmd = New SqlCommand With {
            .CommandTimeout = 0
        }

        Try
            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = StoredProcedureName
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandType = CommandType.StoredProcedure

            sqlCmd.Parameters.AddRange(Params)

            sqlRdr = sqlCmd.ExecuteReader(CommandBehavior.CloseConnection)

            Return sqlRdr

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
        End Try

    End Function

    ''' <summary>
    ''' Get a DataReader from a stored procedure using parameters and a timeout period
    ''' </summary>
    ''' <param name="StoredProcedureName">Name of the stored procedure to execute</param>
    ''' <param name="Params">Array of stored procedure parameters</param>
    ''' <param name="TimeOut">Timeout period</param>
    ''' <remarks></remarks>
    Public Function GetDataAsReader(ByVal StoredProcedureName As String, ByVal TimeOut As Integer, ByVal ParamArray Params() As SqlClient.SqlParameter) As SqlClient.SqlDataReader
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlRdr As SqlClient.SqlDataReader

        sqlCmd = New SqlClient.SqlCommand

        Try
            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = StoredProcedureName
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandTimeout = TimeOut
            sqlCmd.CommandType = CommandType.StoredProcedure

            sqlCmd.Parameters.AddRange(Params)

            sqlRdr = sqlCmd.ExecuteReader(CommandBehavior.CloseConnection)

            Return sqlRdr

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
        End Try

    End Function

    ''' <summary>
    ''' Get a DataReader from a Query string and timeout period
    ''' </summary>    
    ''' <param name="SQLQuery">Query string to ejecute</param>
    ''' <param name="TimeOut">Timeout period</param>
    ''' <remarks></remarks>
    Public Function GetDataAsReader(ByVal SQLQuery As String, ByVal TimeOut As Integer) As SqlClient.SqlDataReader
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlRdr As SqlClient.SqlDataReader

        sqlCmd = New SqlClient.SqlCommand

        Try
            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = SQLQuery
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandTimeout = TimeOut
            sqlCmd.CommandType = CommandType.Text

            sqlRdr = sqlCmd.ExecuteReader(CommandBehavior.CloseConnection)

            Return sqlRdr

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
        End Try

    End Function

    ''' <summary>
    ''' Get a DataReader from a Query string
    ''' </summary>    
    ''' <param name="SQLQuery">Query string to ejecute</param>
    ''' <remarks></remarks>
    Public Function GetDataAsReader(ByVal SQLQuery As String) As SqlClient.SqlDataReader
        Dim sqlCmd As SqlClient.SqlCommand
        Dim sqlRdr As SqlClient.SqlDataReader

        sqlCmd = New SqlClient.SqlCommand

        Try
            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = SQLQuery
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandType = CommandType.Text

            sqlRdr = sqlCmd.ExecuteReader(CommandBehavior.CloseConnection)

            Return sqlRdr

        Catch sqlex As SqlException
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            Throw New Exception(ex.Message, ex)
        Finally
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
        End Try

    End Function

#End Region

#Region "Execution"

    ''' <summary>
    ''' Execute Query instruction
    ''' </summary>
    ''' <param name="Query">Query command to execute .</param>
    ''' <param name="ExecType">Type of execution (Scalar, NonQuerty)</param>
    ''' <remarks></remarks>
    Public Function ExecuteQueryInstruction(ByVal Query As String, ByVal ExecType As EnumExecutionType) As Object
        Dim sqlCmd As SqlClient.SqlCommand
        Dim oResult As Object = Nothing

        sqlCmd = New SqlClient.SqlCommand

        Try
            If mSqlCnn IsNot Nothing AndAlso mSqlCnn.State <> ConnectionState.Open Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = Query
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandType = CommandType.Text

            If mSqlTran IsNot Nothing Then
                sqlCmd.Transaction = mSqlTran
            End If

            Select Case ExecType
                Case Is = DataBase.EnumExecutionType.NonQuery
                    oResult = sqlCmd.ExecuteNonQuery
                Case Is = DataBase.EnumExecutionType.Scalar
                    oResult = sqlCmd.ExecuteScalar
            End Select
            Return oResult
        Catch sqlex As SqlException
            RollBackTransaction()
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            RollBackTransaction()
            Throw New Exception(ex.Message, ex)
        Finally
            If mSqlTran Is Nothing Then
                If mSqlCnn IsNot Nothing And mSqlCnn.State = ConnectionState.Open Then
                    mSqlCnn.Close()
                End If
            End If
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
        End Try

    End Function

    ''' <summary>
    ''' Execute stored procedure
    ''' </summary>
    ''' <param name="StoredProcedureName">Name of the stored procedure to execute</param>
    ''' <param name="Params">Array of stored procedure parameters</param>
    ''' <param name="ExecType">Type of execution (Scalar, NonQuerty)</param>
    ''' <remarks></remarks>
    Public Function ExecuteStoredProcedure(ByVal StoredProcedureName As String, ByVal ExecType As EnumExecutionType, ByVal ParamArray Params() As SqlClient.SqlParameter) As Object
        Dim sqlCmd As SqlClient.SqlCommand
        Dim oResult As Object = Nothing

        sqlCmd = New SqlClient.SqlCommand

        Try
            If mSqlCnn IsNot Nothing AndAlso mSqlCnn.State <> ConnectionState.Open Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = StoredProcedureName
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandType = CommandType.StoredProcedure
            sqlCmd.Parameters.AddRange(Params)
            If mSqlTran IsNot Nothing Then
                sqlCmd.Transaction = mSqlTran
            End If

            Select Case ExecType
                Case Is = DataBase.EnumExecutionType.NonQuery
                    oResult = sqlCmd.ExecuteNonQuery
                Case Is = DataBase.EnumExecutionType.Scalar
                    oResult = sqlCmd.ExecuteScalar
            End Select
            Return oResult

        Catch sqlex As SqlException
            RollBackTransaction()
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            RollBackTransaction()
            Throw New Exception(ex.Message, ex)
        Finally
            If mSqlTran Is Nothing Then
                If mSqlCnn IsNot Nothing And mSqlCnn.State = ConnectionState.Open Then
                    mSqlCnn.Close()
                End If
            End If
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
        End Try

        Return oResult

    End Function

    ''' <summary>
    ''' Execute stored procedure with timeout period
    ''' </summary>
    ''' <param name="StoredProcedureName">Name of the stored procedure to execute</param>
    ''' <param name="Params">Array of stored procedure parameters</param>
    ''' <param name="ExecType">Type of execution (Scalar, NonQuerty)</param>
    ''' <param name="TimeOut">Timeout period</param>
    ''' <remarks></remarks>
    Public Function ExecuteStoredProcedure(ByVal StoredProcedureName As String, ByVal ExecType As EnumExecutionType, ByVal TimeOut As Integer, ByVal ParamArray Params() As SqlClient.SqlParameter) As Object
        Dim sqlCmd As SqlClient.SqlCommand
        Dim oResult As Object = Nothing

        sqlCmd = New SqlClient.SqlCommand

        Try
            If mSqlCnn IsNot Nothing AndAlso mSqlCnn.State <> ConnectionState.Open Then
                mSqlCnn.Open()
            End If

            sqlCmd.CommandText = StoredProcedureName
            sqlCmd.Connection = mSqlCnn
            sqlCmd.CommandTimeout = TimeOut
            sqlCmd.CommandType = CommandType.StoredProcedure
            sqlCmd.Parameters.AddRange(Params)
            If mSqlTran IsNot Nothing Then
                sqlCmd.Transaction = mSqlTran
            End If

            Select Case ExecType
                Case Is = DataBase.EnumExecutionType.NonQuery
                    oResult = sqlCmd.ExecuteNonQuery
                Case Is = DataBase.EnumExecutionType.Scalar
                    oResult = sqlCmd.ExecuteScalar
            End Select
            Return oResult

        Catch sqlex As SqlException
            RollBackTransaction()
            Throw New Exception(sqlex.Message, sqlex)
        Catch ex As Exception
            RollBackTransaction()
            Throw New Exception(ex.Message, ex)
        Finally
            If mSqlTran Is Nothing Then
                If mSqlCnn IsNot Nothing And mSqlCnn.State = ConnectionState.Open Then
                    mSqlCnn.Close()
                End If
            End If
            If sqlCmd IsNot Nothing Then
                sqlCmd.Dispose()
            End If
        End Try

    End Function

#End Region

    Private disposedValue As Boolean = False

    ' IDisposable
    Protected Overridable Sub Dispose(ByVal disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: free other state (managed objects).
            End If

            ' TODO: free your own state (unmanaged objects).
            ' TODO: set large fields to null.

            mConnectionString = Nothing

            If mSqlCnn IsNot Nothing Then
                mSqlCnn.Close()
                mSqlCnn.Dispose()
            End If
            mSqlCnn = Nothing

            If mSqlTran IsNot Nothing Then
                mSqlTran.Dispose()
            End If
            mSqlTran = Nothing

        End If
        Me.disposedValue = True
    End Sub

#Region " IDisposable Support "
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class