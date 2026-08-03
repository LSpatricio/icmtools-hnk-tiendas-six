Public Class MediasAnuales
    Private _MediaAnualID As Integer
    Private _MediaAnualSociedadID As String
    Private _MediaAnualDivisionID As String

    Private _MediaAnualModuloID As String
    Private _MediaAnualModuloName As String
    Private _MediaAnualParametroID As String
    Private _MediaAnualParametroName As String
    Private _MediaAnualValor As String
    Private _MediaAnualDesde As String
    Private _MediaAnualHasta As String
    Private _MediaAnualFechaUltimoCambio As String
    Private _MediaAnualUsuarioUltimoCambio As String
    Private _MediaAnualActivo As Boolean
    Private _MediaAnualActivoDescripcion As String



    Public Property MediaAnualID() As Integer
        Get
            Return _MediaAnualID
        End Get
        Set(ByVal value As Integer)
            _MediaAnualID = value
        End Set
    End Property

    Public Property MediaAnualSociedadID() As String
        Get
            Return _MediaAnualSociedadID
        End Get
        Set(ByVal value As String)
            _MediaAnualSociedadID = value
        End Set
    End Property

    Public Property MediaAnualDivisionID() As String
        Get
            Return _MediaAnualDivisionID
        End Get
        Set(ByVal value As String)
            _MediaAnualDivisionID = value
        End Set
    End Property

    Public Property MediaAnualModuloID() As String
        Get
            Return _MediaAnualModuloID
        End Get
        Set(ByVal value As String)
            _MediaAnualModuloID = value
        End Set
    End Property

    Public Property MediaAnualModuloName() As String
        Get
            Return _MediaAnualModuloName
        End Get
        Set(ByVal value As String)
            _MediaAnualModuloName = value
        End Set
    End Property

    Public Property MediaAnualParametroID() As String
        Get
            Return _MediaAnualParametroID
        End Get
        Set(ByVal value As String)
            _MediaAnualParametroID = value
        End Set
    End Property

    Public Property MediaAnualParametroName() As String
        Get
            Return _MediaAnualParametroName
        End Get
        Set(ByVal value As String)
            _MediaAnualParametroName = value
        End Set
    End Property

    Public Property MediaAnualValor() As String
        Get
            Return _MediaAnualValor
        End Get
        Set(ByVal value As String)
            _MediaAnualValor = value
        End Set
    End Property

    Public Property MediaAnualFechaUltimoCambio() As String
        Get
            Return _MediaAnualFechaUltimoCambio
        End Get
        Set(ByVal value As String)
            _MediaAnualFechaUltimoCambio = value
        End Set
    End Property

    Public Property MediaAnualUsuarioUltimoCambio() As String
        Get
            Return _MediaAnualUsuarioUltimoCambio
        End Get
        Set(ByVal value As String)
            _MediaAnualUsuarioUltimoCambio = value
        End Set
    End Property

    Public Property MediaAnualDesde() As String
        Get
            Return _MediaAnualDesde
        End Get
        Set(ByVal value As String)
            _MediaAnualDesde = value
        End Set
    End Property

    Public Property MediaAnualHasta() As String
        Get
            Return _MediaAnualHasta
        End Get
        Set(ByVal value As String)
            _MediaAnualHasta = value
        End Set
    End Property


    Public Property MediaAnualActivo() As Boolean
        Get
            Return _MediaAnualActivo
        End Get
        Set(ByVal value As Boolean)
            _MediaAnualActivo = value
        End Set
    End Property

    Public Property MediaAnualActivoDescripcion() As String
        Get
            Return _MediaAnualActivoDescripcion
        End Get
        Set(ByVal value As String)
            _MediaAnualActivoDescripcion = value
        End Set
    End Property

End Class
