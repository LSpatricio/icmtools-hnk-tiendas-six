Public Class ReplacementsConfiguration
    Private _ReplacementIDPosition As String
    Private _ReplacementPosition As String
    Private _ReplacementPayeeID As String
    Private _ReplacementPayeeName As String
    Private _ReplacementIDSociety As String
    Private _ReplacementSocietyName As String
    Private _ReplacementIDPersonalDivision As String
    Private _ReplacementPersonlDivisionName As String
    Private _ReplacementActiveReplacement As String


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

    Public Property ReplacementPayeeID() As String
        Get
            Return _ReplacementPayeeID
        End Get
        Set(ByVal value As String)
            _ReplacementPayeeID = value
        End Set
    End Property

    Public Property ReplacementPayeeName() As String
        Get
            Return _ReplacementPayeeName
        End Get
        Set(ByVal value As String)
            _ReplacementPayeeName = value
        End Set
    End Property

    Public Property ReplacementIDSociety() As String
        Get
            Return _ReplacementIDSociety
        End Get
        Set(ByVal value As String)
            _ReplacementIDSociety = value
        End Set
    End Property

    Public Property ReplacementSocietyName() As String
        Get
            Return _ReplacementSocietyName
        End Get
        Set(ByVal value As String)
            _ReplacementSocietyName = value
        End Set
    End Property

    Public Property ReplacementIDPersonalDivision() As String
        Get
            Return _ReplacementIDPersonalDivision
        End Get
        Set(ByVal value As String)
            _ReplacementIDPersonalDivision = value
        End Set
    End Property

    Public Property ReplacementPersonlDivisionName() As String
        Get
            Return _ReplacementPersonlDivisionName
        End Get
        Set(ByVal value As String)
            _ReplacementPersonlDivisionName = value
        End Set
    End Property

    Public Property ReplacementActiveReplacement() As String
        Get
            Return _ReplacementActiveReplacement
        End Get
        Set(ByVal value As String)
            _ReplacementActiveReplacement = value
        End Set
    End Property
End Class
