Imports System.Data
Imports System.Data.SqlClient

Public Class ParametersConfiguration
#Region "Variables y propiedades de la clase"
    Private _ParameterID As Integer
    Private _ParameterIDKey As String

    Private _ParameterIDModule As Integer
    Private _ParameterModule As String

    Private _ParameterName As String
    Private _ParameterValue As String
    Private _ParameterlastUpdate As String
    Private _ParameterUserUpdate As String
    Private _ParameterType As String
    Private _ParameterTypeName As String

    Public Property ParameterID As Integer
        Get
            Return _ParameterID
        End Get
        Set(ByVal value As Integer)
            _ParameterID = value
        End Set
    End Property

    Public Property ParameterIDKey() As String
        Get
            Return _ParameterIDKey
        End Get
        Set(ByVal value As String)
            _ParameterIDKey = value
        End Set
    End Property

    Public Property ParameterIDModule() As Integer
        Get
            Return _ParameterIDModule
        End Get
        Set(ByVal value As Integer)
            _ParameterIDModule = value
        End Set
    End Property

    Public Property ParameterModule() As String
        Get
            Return _ParameterModule
        End Get
        Set(ByVal value As String)
            _ParameterModule = value
        End Set
    End Property

    Public Property ParameterName() As String
        Get
            Return _ParameterName
        End Get
        Set(ByVal value As String)
            _ParameterName = value
        End Set
    End Property

    Public Property ParameterValue() As String
        Get
            Return _ParameterValue
        End Get
        Set(ByVal value As String)
            _ParameterValue = value
        End Set
    End Property


    Public Property ParameterlastUpdate() As String
        Get
            Return _ParameterlastUpdate
        End Get
        Set(ByVal value As String)
            _ParameterlastUpdate = value
        End Set
    End Property

    Public Property ParameterUserUpdate() As String
        Get
            Return _ParameterUserUpdate
        End Get
        Set(ByVal value As String)
            _ParameterUserUpdate = value
        End Set
    End Property

    Public Property ParameterType() As String
        Get
            Return _ParameterType
        End Get
        Set(ByVal value As String)
            _ParameterType = value
        End Set
    End Property

    Public Property ParameterTypeName() As String
        Get
            Return _ParameterTypeName
        End Get
        Set(ByVal value As String)
            _ParameterTypeName = value
        End Set
    End Property

#End Region
End Class

Public Class MultipleParameter
#Region "Propiedades"
    Public Property IDKey As String
    Public Property Parameter As String
    Public Property Value As String
    Public Property IDModule As String
    Public Property ModuleName As String
    Public Property IDKeyModule As String
    Public Property IDSociety As String
    Public Property IDPersonalDivision As String
    Public Property IDParameter As String
    'solo lectura
    Public Property StartDate As String
    Public Property EndDate As String

    Public Sub New()

    End Sub
#End Region
    Public Shared Function ParametersByKey(idDivision As String, idSociety As String, idKeyConf As String) As MultipleParameter
        Dim cs = ConfigurationManager.ConnectionStrings("TSQL_CONNECTION")
        If cs Is Nothing OrElse String.IsNullOrWhiteSpace(cs.ConnectionString) Then
            Throw New InvalidOperationException("Falta o está vacía la connectionString 'TSQL_CONNECTION'.")
        End If

        Dim CnnFEMCO_Transfer As String = cs.ConnectionString
        Dim dt As DataTable = Nothing
        Dim resultList As New List(Of MultipleParameter)

        Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)
            dt = dbFactory.GetDataAsDataTable("[FEMCOEPSAP].[spICMToolsParametersByKey]",
                                              New SqlParameter("@idDivision", idDivision),
                                              New SqlParameter("@idSociety", idSociety),
                                              New SqlParameter("@idKeyConf", idKeyConf))
        End Using

        If dt IsNot Nothing Then

            For Each row As DataRow In dt.Rows
                Dim rowObject = New MultipleParameter()
                rowObject.IDKey = row.Item(0).ToString()
                rowObject.Parameter = row.Item(1).ToString()
                rowObject.Value = row.Item(2).ToString()
                rowObject.IDModule = row.Item(3).ToString()
                rowObject.ModuleName = row.Item(4).ToString()
                rowObject.IDKeyModule = row.Item(5).ToString()
                rowObject.IDSociety = row.Item(6).ToString()
                rowObject.IDPersonalDivision = row.Item(7).ToString()
                rowObject.IDParameter = row.Item(8).ToString()
                rowObject.StartDate = row.Item(9).ToString()
                rowObject.EndDate = row.Item(10).ToString()
                resultList.Add(rowObject)
            Next
            Return resultList.FirstOrDefault()


        Else
            Return New MultipleParameter() ' Devuelve una lista vacía si no hay datos

        End If
    End Function

End Class
