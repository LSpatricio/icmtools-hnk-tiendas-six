Imports Microsoft.VisualBasic

Public Class Excepciones

    Private mEmpleado As String
    Private mFecha As Date
    Private mCCNom As String
    Private mValor As Double
    Private mMotivo As String

    Public Property Empleado() As String
        Get
            Return mEmpleado
        End Get
        Private Set(ByVal value As String)
            mEmpleado = value
        End Set
    End Property

    Public Property Fecha() As Date
        Get
            Return mFecha
        End Get
        Private Set(ByVal value As Date)
            mFecha = value
        End Set
    End Property

    Public Property CCNom() As String
        Get
            Return mCCNom
        End Get
        Private Set(ByVal value As String)
            mCCNom = value
        End Set
    End Property

    Public Property Valor() As Double
        Get
            Return mValor
        End Get
        Private Set(ByVal value As Double)
            mValor = value
        End Set
    End Property

    Public Property Motivo() As String
        Get
            Return mMotivo
        End Get
        Private Set(ByVal value As String)
            mMotivo = value
        End Set
    End Property

    Public Sub New(PayeeID As String, ExceptionDate As Date, IDWageType As String, Amount As Double, Reason As String)
        Empleado = PayeeID
        Fecha = ExceptionDate
        CCNom = IDWageType
        Valor = Amount
        Motivo = Reason
    End Sub

End Class
