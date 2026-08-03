
Public Class Modules

    Private _ModuleIDModule As Integer
    Private _ModuleIDKey As String
    Private _ModuleName As String
    Private _ModuleModeloName As String

    Public Property ModuleIDModule() As Integer
        Get
            Return _ModuleIDModule
        End Get
        Set(ByVal value As Integer)
            _ModuleIDModule = value
        End Set
    End Property


    Public Property ModuleIDKey() As String
        Get
            Return _ModuleIDKey
        End Get
        Set(ByVal value As String)
            _ModuleIDKey = value
        End Set
    End Property


    Public Property ModuleName() As String
        Get
            Return _ModuleName
        End Get
        Set(ByVal value As String)
            _ModuleName = value
        End Set
    End Property

    Public Property ModuleModeloName() As String
        Get
            Return _ModuleModeloName
        End Get
        Set(ByVal value As String)
            _ModuleModeloName = value
        End Set
    End Property

End Class
