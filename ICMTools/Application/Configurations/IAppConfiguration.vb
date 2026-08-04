Public Interface IAppConfiguration

    ReadOnly Property Maintenance As Boolean

    ReadOnly Property HomePage As String

    Function ObtenerModelo(model As String) As String

End Interface