Imports Microsoft.VisualBasic
Imports System.Configuration
Imports System.Data

<Serializable()>
Public Class User

#Region "Variables Locales o Privadas"

    Private mModel As String
    Private mUserName As String
    Private mUserEmail As String
    Private mAuthenticated As Boolean

#End Region

#Region "Propiedades"
    Public Property Model() As String
        Get
            Return mModel
        End Get
        Set(ByVal Value As String)
            mModel = Value
        End Set
    End Property

    Public Property Email() As String
        Get
            Return mUserEmail
        End Get
        Set(ByVal Value As String)
            mUserEmail = Value
        End Set
    End Property
    Public Property Name() As String
        Get
            Return mUserName
        End Get
        Set(ByVal Value As String)
            mUserName = Value
        End Set
    End Property
    Public Property Authenticated() As Boolean
        Get
            Return mAuthenticated
        End Get
        Set(ByVal Value As Boolean)
            mAuthenticated = Value
        End Set
    End Property


#End Region

#Region "Funciones y Procedimientos"
    Public Sub Authenticate()

        ''El metodo de Auth se reemplaza por un metodo de consulta WebQuery a VaricentICM

    End Sub

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
#End Region

End Class
