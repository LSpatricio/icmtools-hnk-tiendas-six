Imports System.Web.Http
Public Module WebApiConfig
    Public Sub Register(ByVal config As HttpConfiguration)
        ''Habilita el enrutamiento basado en atributos
        config.MapHttpAttributeRoutes()

        ''Convención de enrutamiento por defecto para la API
        config.Routes.MapHttpRoute(
            name:="DefaultApi",
            routeTemplate:="api/{controller}/{id}",
            defaults:=New With {.id = RouteParameter.Optional}
        )

        ''Forza a la API a devolver siempre un JSON, evitando XML
        Dim json = config.Formatters.JsonFormatter
        json.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.Objects
        config.Formatters.Remove(config.Formatters.XmlFormatter)
    End Sub
End Module
