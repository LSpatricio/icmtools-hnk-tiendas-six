Imports System.ComponentModel
Imports System.Data.SqlClient
Imports System.Web.Script.Services
Imports System.Web.Services


' Para permitir que se llame a este servicio web desde un script, usando ASP.NET AJAX, quite la marca de comentario de la línea siguiente.
<System.Web.Script.Services.ScriptService()>
<System.Web.Services.WebService(Namespace:="http://tempuri.org/")>
<System.Web.Services.WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<ToolboxItem(False)>
Public Class WebServiceParametros
    Inherits System.Web.Services.WebService

    'Private mUser As New User

#Region "Variables Locales"

    Private ReadOnly CnnFEMCO_Transfer As String = String.Empty
    Private mLog As Log

#End Region

#Region " [ Constructor ] "

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
        Try
            Dim cnn = ConfigurationManager.ConnectionStrings("TSQL_CONNECTION")
            If Not cnn Is Nothing And Not String.IsNullOrWhiteSpace(cnn.ConnectionString) Then
                CnnFEMCO_Transfer = cnn.ConnectionString
            End If
        Catch ex As Exception
        End Try
    End Sub

#End Region

#Region "Parametros"
    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SelectParameters_Ajustado() As Response(Of ParametersConfiguration)

        Dim Response As New Response(Of ParametersConfiguration) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of ParametersConfiguration)
            }



        Try
            If Session Is Nothing OrElse Session("User") Is Nothing Then
                Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
                Return Response
            End If

            Dim dt As DataTable = Nothing
            mLog = New Log
            mLog.insertLog("PARAMETROS", "CARGA_DATOS", "Inicia la carga de los Parametros")


            Using dbFactory As New DataBase(CnnFEMCO_Transfer)

                dt = dbFactory.GetDataAsDataTable("FEMCOEPSAP.spICMToolsParametersGrid")
                If dt IsNot Nothing Then

                    For Each row As DataRow In dt.Rows
                        Dim Parameter As New ParametersConfiguration()
                        Parameter.ParameterID = row.Item(0)
                        Parameter.ParameterIDKey = row.Item(1).ToString()
                        Parameter.ParameterIDModule = row.Item(2)
                        Parameter.ParameterModule = row.Item(3).ToString()
                        Parameter.ParameterName = row.Item(4).ToString()
                        Parameter.ParameterValue = row.Item(5)
                        Parameter.ParameterlastUpdate = row.Item(6)
                        Parameter.ParameterUserUpdate = row.Item(7).ToString()
                        Parameter.ParameterType = row.Item(8).ToString()
                        Parameter.ParameterTypeName = row.Item(9).ToString()
                        Response.Datos.Add(Parameter)
                    Next
                Else
                    Response.Mensaje = "No se encontraron registros"

                End If
                Response.Exito = True
                Response.Mensaje = "Datos cargados correctamente"

            End Using
        Catch ex As Exception
            Dim mensaje As String = "Error en la carga de los Parametros, " + ex.Message
            mLog.insertLog("PARAMETROS", "CARGA_DATOS", mensaje)
            Response.Mensaje = mensaje
        End Try
        Return Response

    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GuardarDatosParametros(IDKey As String, Valor As String, TipoParametro As String, User As String) As Response(Of ParametersConfiguration)
        Dim Response As New Response(Of ParametersConfiguration) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of ParametersConfiguration)
            }



        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If

        Dim mensaje As String = ""

        Dim ResponseSql As New Object
        mLog = New Log
        mLog.insertLog("PARAMETROS", "GUARDA_DATOS", "Guardado de Parametro: " + IDKey)


        Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)

            Dim pID As New SqlParameter("@IDKey", IDKey)
            Dim pValor As New SqlParameter("@Value", Valor)
            Dim pTipoParametro As New SqlParameter("@TypeParameter", TipoParametro)
            Dim pUser As New SqlParameter("@UserUpdate", User)

            Try
                ResponseSql = dbFactory.ExecuteStoredProcedure("FEMCOEPSAP.spICMToolsParametersUpdate", DataBase.EnumExecutionType.NonQuery, pID, pValor, pTipoParametro, pUser)
                If Not ResponseSql Is Nothing Then
                    Response.Exito = True ' Si se ejecuto correctamente                    
                    mLog.insertLog("PARAMETROS", "GUARDA_DATOS", "Guardado de Parametro: " + IDKey)
                Else
                    mensaje = "No se pudo guardar el parametro: " + IDKey
                    mLog.insertLog("PARAMETROS", "GUARDA_DATOS", mensaje)
                    Response.Mensaje = mensaje
                End If
            Catch ex As Exception
                mensaje = "Error en el guardado de Parametro: " + IDKey
                mLog.insertLog("PARAMETROS", "GUARDA_DATOS", mensaje)
                Response.Mensaje = mensaje
            End Try

        End Using


        Return Response
    End Function

#End Region

#Region "Reemplazos"


    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SelectReplacements_Ajustado(FiltroPosiciones As String, IDPosicionExcluir As String) As Response(Of ReplacementsConfiguration)

        Dim Response As New Response(Of ReplacementsConfiguration) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of ReplacementsConfiguration)
            }

        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If


        Dim dt As DataTable = Nothing

        mLog = New Log
        mLog.insertLog("REEMPLAZOS", "CARGA_DATOS", "Inicia la carga de los Reemplazos")

        Try

            Using dbFactory As New DataBase(CnnFEMCO_Transfer)

                Dim p01 As New SqlParameter("@filtroPosiciones", FiltroPosiciones)
                Dim p02 As New SqlParameter("@PosicionExcluir", IDPosicionExcluir)

                dt = dbFactory.GetDataAsDataTable("FEMCOEPSAP.spICMToolsRemplazosGrid", p01, p02)
                If dt IsNot Nothing Then

                    For Each row As DataRow In dt.Rows
                        Dim Reemplazo As New ReplacementsConfiguration()
                        Reemplazo.ReplacementIDPosition = row.Item(0)
                        Reemplazo.ReplacementPosition = row.Item(1).ToString()
                        Reemplazo.ReplacementPayeeID = row.Item(2)
                        Reemplazo.ReplacementPayeeName = row.Item(3).ToString()
                        Reemplazo.ReplacementIDSociety = row.Item(4).ToString()
                        Reemplazo.ReplacementSocietyName = row.Item(5)
                        Reemplazo.ReplacementIDPersonalDivision = row.Item(6)
                        Reemplazo.ReplacementPersonlDivisionName = row.Item(7).ToString()
                        Reemplazo.ReplacementActiveReplacement = row.Item(8).ToString()
                        Response.Datos.Add(Reemplazo)
                    Next
                Else
                    Response.Mensaje = "No se encontraron registros"

                End If

            End Using


            Response.Exito = True
            Response.Mensaje = "Datos cargados correctamente"
        Catch ex As Exception
            Dim mensaje As String = "Error en la carga de los Reemplazos, " + ex.Message
            mLog.insertLog("REEMPLAZOS", "CARGA_DATOS", mensaje)
            Response.Mensaje = mensaje

        End Try

        'End If

        Return Response
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GuardarDatosReemplazos_Ajustado(PosicionID As String, TipoReemplazo As Integer, UsuarioReemplazo As String, Periodo As String, Hasta As String, User As String) As Response(Of ReplacementsConfiguration)

        Dim Response As New Response(Of ReplacementsConfiguration) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of ReplacementsConfiguration)
            }


        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If

        Dim mensaje As String = ""


        Dim ResponseSql As New Object
        mLog = New Log
        mLog.insertLog("REEMPLAZOS", "GUARDA_DATOS", "Guardado de Reempleazo para la posición: " + PosicionID)

        Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)

            Dim pPosicionID As New SqlParameter("@IDPosition", PosicionID)
            Dim pTipoReemplazo As New SqlParameter("@ReplacementType", TipoReemplazo)
            Dim pPeriodo As New SqlParameter("@StartDate", Periodo)
            Dim pHasta As New SqlParameter("@EndDate", Hasta)
            Dim pUsuarioReemplazo As New SqlParameter("@IDPositionRemplacement", UsuarioReemplazo)

            Dim pUser As New SqlParameter("@userUpdate", User)

            Try
                ResponseSql = dbFactory.ExecuteStoredProcedure("FEMCOEPSAP.spICMToolsReemplazosInsert", DataBase.EnumExecutionType.NonQuery, pPosicionID, pTipoReemplazo, pPeriodo, pHasta, pUsuarioReemplazo, pUser)
                If Not ResponseSql Is Nothing Then
                    Response.Exito = True ' Si se ejecuto correctamente
                    mensaje = "Guardado del Reemplazo para la posición: " + PosicionID
                    mLog.insertLog("REEMPLAZOS", "GUARDA_DATOS", mensaje)
                    Response.Mensaje = mensaje

                End If
            Catch ex As Exception
                mensaje = "Error en el guardado de Reempleazo para la posición: " + PosicionID + "," + ex.Message
                mLog.insertLog("REEMPLAZOS", "GUARDA_DATOS", mensaje)
                Response.Mensaje = mensaje
            End Try

        End Using


        Return Response
    End Function

#End Region

#Region "Posiciones"

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function ObtenerPosicionesReemplazos_Ajustado() As Response(Of PosicionesReemplazos)


        Dim Response As New Response(Of PosicionesReemplazos) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of PosicionesReemplazos)
            }


        Try
            If Session Is Nothing OrElse Session("User") Is Nothing Then
                Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
                Return Response
            End If

            Dim dt As DataTable = Nothing

            mLog = New Log
            mLog.insertLog("POSICIONES_REEMPLAZOS", "CARGAR_POSICIONES", "Inicia la carga de las posiciones para Reemplazos")

            Using dbFactory As New DataBase(CnnFEMCO_Transfer)

                dt = dbFactory.GetDataAsDataTable("FEMCOEPSAP.spICMToolsPosicionesSelect")
                If dt IsNot Nothing Then

                    For Each row As DataRow In dt.Rows
                        Dim Posicion As New PosicionesReemplazos()
                        Posicion.ReplacementPosition = row.Item(0).ToString()

                        Response.Datos.Add(Posicion)
                    Next

                End If

            End Using

            Response.Exito = True
            Response.Mensaje = "Datos cargados correctamente"

        Catch ex As Exception
            Dim mensaje As String = "Error en la carga de las posiciones para Reemplazos, " + ex.Message
            mLog.insertLog("POSICIONES_REEMPLAZOS", "CARGAR_POSICIONES", mensaje)
            Response.Mensaje = mensaje
        End Try

        'End If
        Return Response
    End Function

#End Region

#Region "Configuraciones"

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function ObtenerConfiguracionesParametros_Ajustado(Sociedad As String, Division As String) As Response(Of MediasAnuales)

        Dim Response As New Response(Of MediasAnuales) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of MediasAnuales)
            }

        Try
            Dim dt As DataTable = Nothing


            mLog = New Log
            mLog.insertLog("CONFIGURACIONES", "CARGA_DATOS", "Inicia la carga de las configuraciones por división")

            Using dbFactory As New DataBase(CnnFEMCO_Transfer)

                Dim p1 As New SqlParameter("@Sociedad", Sociedad)
                Dim p2 As New SqlParameter("@Division", Division)

                dt = dbFactory.GetDataAsDataTable("FEMCOEPSAP.spICMToolsConfigurationParameterSelect", p1, p2)
                If dt IsNot Nothing Then

                    For Each row As DataRow In dt.Rows
                        Dim MediaAnual As New MediasAnuales()
                        MediaAnual.MediaAnualID = row.Item(0).ToString()
                        MediaAnual.MediaAnualSociedadID = row.Item(1).ToString()
                        MediaAnual.MediaAnualDivisionID = row.Item(2).ToString()
                        MediaAnual.MediaAnualModuloID = row.Item(3).ToString()
                        MediaAnual.MediaAnualModuloName = row.Item(4).ToString()
                        MediaAnual.MediaAnualParametroID = row.Item(5).ToString()
                        MediaAnual.MediaAnualParametroName = row.Item(6).ToString()
                        MediaAnual.MediaAnualValor = row.Item(7).ToString()
                        MediaAnual.MediaAnualDesde = row.Item(8).ToString()
                        MediaAnual.MediaAnualHasta = row.Item(9).ToString()
                        MediaAnual.MediaAnualFechaUltimoCambio = row.Item(10).ToString()
                        MediaAnual.MediaAnualUsuarioUltimoCambio = row.Item(11).ToString()
                        MediaAnual.MediaAnualActivo = CType(row.Item(12), Boolean)
                        MediaAnual.MediaAnualActivoDescripcion = row.Item(13).ToString()

                        Response.Datos.Add(MediaAnual)
                    Next

                End If

            End Using
            Response.Exito = True
            Response.Mensaje = "Datos cargados correctamente"

        Catch ex As Exception
            Dim mensaje As String = "Error en la carga de las configuraciones por división, " + ex.Message
            mLog.insertLog("CONFIGURACIONES", "CARGA_DATOS", mensaje)
            Response.Mensaje = mensaje
        End Try

        Return Response

    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GuardarDatosConfiguracionesParametros_Ajustado(IDConfiguration As String, Valor As String, StartDate As Date,
                                                          EndDate As Date, IsActive As Boolean, User As String) As Response(Of MediasAnuales)

        Dim Response As New Response(Of MediasAnuales) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of MediasAnuales)
            }

        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If

        Dim mensaje As String = ""
        Dim ResponseSql As New Object

        Try
            mLog = New Log
            mLog.insertLog("CONFIGURACION", "GUARDA_DATOS", "Guardado de Configuración: " + IDConfiguration)

            Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)

                Dim pID As New SqlParameter("@IDConfiguration", IDConfiguration)
                Dim pValor As New SqlParameter("@Valor", Valor)
                Dim pStartDate As New SqlParameter("@StartDate", StartDate)
                Dim pEndDate As New SqlParameter("@EndDate", EndDate)
                Dim pIsActive As New SqlParameter("@IsActive", IsActive)
                Dim pUser As New SqlParameter("@UserUpdate", User)

                ResponseSql = dbFactory.ExecuteStoredProcedure("FEMCOEPSAP.spICMToolsConfigurationParameterUpdate", DataBase.EnumExecutionType.NonQuery,
                                                       pID, pValor, pStartDate, pEndDate, pIsActive, pUser)
                If Not ResponseSql Is Nothing Then
                    Response.Exito = True ' Si se ejecuto correctamente
                    mensaje = "Guardado de configuración: " + IDConfiguration
                    mLog.insertLog("CONFIGURACION", "GUARDA_DATOS", mensaje)
                    Response.Mensaje = mensaje
                Else
                    mensaje = "No se realizo el guardado de la configuración: " + IDConfiguration
                    mLog.insertLog("CONFIGURACION", "GUARDA_DATOS", mensaje)
                    Response.Mensaje = mensaje
                End If

            End Using
        Catch ex As Exception
            mensaje = "Error al guardar la Configuración: " + IDConfiguration + ", " + ex.Message
            mLog.insertLog("CONFIGURACION", "GUARDA_DATOS", mensaje)
            Response.Mensaje = mensaje
        End Try

        Return Response
    End Function

    <WebMethod(EnableSession:=True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function GuardarDatosNuevaConfiguracionParametro_Ajustado(SociedadID As String, DivisionID As String, ModuloID As Integer, ParametroID As Integer,
                                                            Valor As String, StartDate As Date, EndDate As Date, IsActive As Boolean, User As String) As Response(Of MediasAnuales)

        Dim Response As New Response(Of MediasAnuales) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of MediasAnuales)
            }

        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If

        Dim mensaje As String = ""
        Dim ResponseSql As New Object


        Try
            mLog = New Log
            mLog.insertLog("CONFIGURACION", "GUARDA_DATOS_NUEVA", "Guardado de Configuración Nueva")

            Using dbFactory As DataBase = New DataBase(CnnFEMCO_Transfer)

                Dim pIDSociety As New SqlParameter("@IDSociety", SociedadID)
                Dim IDPersonalDivision As New SqlParameter("@IDPersonalDivision", DivisionID)
                Dim pIDModule As New SqlParameter("@IDModule", ModuloID)
                Dim pIDParameter As New SqlParameter("@IDParameter", ParametroID)
                Dim pValor As New SqlParameter("@Valor", Valor)
                Dim pDesde As New SqlParameter("@StartDate", StartDate)
                Dim pHasta As New SqlParameter("@EndDate", EndDate)
                Dim pUser As New SqlParameter("@userUpdate", User)
                Dim pIsActive As New SqlParameter("@IsActive", IsActive)


                ResponseSql = dbFactory.ExecuteStoredProcedure("FEMCOEPSAP.spICMToolsConfigurationParameterInsert", DataBase.EnumExecutionType.NonQuery,
                                                          pIDSociety, IDPersonalDivision, pIDModule, pIDParameter, pValor, pDesde, pHasta, pUser, pIsActive)
                If Not ResponseSql Is Nothing Then
                    Response.Exito = True ' Si se ejecuto correctamente
                    mensaje = "Guardado Exitoso de la Configuración Nueva"
                    mLog.insertLog("CONFIGURACION", "GUARDA_DATOS_NUEVA", mensaje)
                    Response.Mensaje = mensaje
                Else
                    mensaje = "Guardado Sin exito de Configuración Nueva"
                    mLog.insertLog("CONFIGURACION", "GUARDA_DATOS_NUEVA", mensaje)
                    Response.Mensaje = mensaje
                End If


            End Using

        Catch ex As Exception
            mensaje = "Error en el Guardado de Configuración Nueva," + ex.Message
            mLog.insertLog("CONFIGURACION", "GUARDA_DATOS_NUEVA", mensaje)
            Response.Mensaje = mensaje
        End Try



        Return Response
    End Function

#End Region

#Region "Combos"

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SelectSociedadesCombo_Ajustado(User As String) As Response(Of Societies)
        Dim Response As New Response(Of Societies) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of Societies)
            }

        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If

        Dim mensaje As String = ""
        Dim dt As DataTable = Nothing
        Try
            mLog = New Log
            mLog.insertLog("CONFIGURACIONES", "CARGA_DATOS", "Inicia la carga de las Sociedades")
            Using dbFactory As New DataBase(CnnFEMCO_Transfer)

                Dim pUser As New SqlParameter("@User", User)

                dt = dbFactory.GetDataAsDataTable("FEMCOEPSAP.spICMToolsConfigurationParameterSociety", pUser)
                If dt IsNot Nothing Then

                    For Each row As DataRow In dt.Rows
                        Dim descripcion = row.Item(0).ToString() + "-" + row.Item(1).ToString()
                        Dim Sociedad As New Societies(row.Item(0), descripcion)

                        Response.Datos.Add(Sociedad)
                    Next

                End If

            End Using

            Response.Exito = True
            Response.Mensaje = "Datos cargados con Exito"

        Catch ex As Exception
            mensaje = "Error en la carga de las Sociedades, " + ex.Message
            mLog.insertLog("CONFIGURACIONES", "CARGA_DATOS", mensaje)
            Response.Mensaje = mensaje
        End Try

        Return Response
    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SelectDivisionesSociedadCombo_Ajustado(SociedadID As String) As Response(Of PersonnelDivisions)

        Dim Response As New Response(Of PersonnelDivisions) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of PersonnelDivisions)
            }

        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If

        Dim mensaje As String = ""
        Dim dt As DataTable = Nothing


        Try
            mLog = New Log
            mLog.insertLog("CONFIGURACIONES", "CARGA_DATOS", "Inicia la carga de las Divisiones de Personal")

            Using dbFactory As New DataBase(CnnFEMCO_Transfer)


                Dim IDSociety As New SqlParameter("@IDSociety", SociedadID)

                dt = dbFactory.GetDataAsDataTable("FEMCOEPSAP.spICMToolsConfigurationParameterDivision", IDSociety)
                If dt IsNot Nothing Then

                    For Each row As DataRow In dt.Rows
                        Dim descripcion = row.Item(2).ToString() + "-" + row.Item(3).ToString()
                        Dim Division As New PersonnelDivisions(row.Item(2), descripcion)

                        Response.Datos.Add(Division)
                    Next


                End If

            End Using

            Response.Exito = True
            Response.Mensaje = "Datos cargados con Exito"
        Catch ex As Exception
            mensaje = "Error en la carga de las Divisiones de Personal, " + ex.Message
            mLog.insertLog("CONFIGURACIONES", "CARGA_DATOS", mensaje)
            Response.Mensaje = mensaje
        End Try



        Return Response
    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SelectModulesCombo_Ajustado() As Response(Of Modules)

        Dim Response As New Response(Of Modules) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of Modules)
            }

        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If

        Dim mensaje As String = ""
        Dim dt As DataTable = Nothing

        Try
            mLog = New Log
            mLog.insertLog("REEMPLAZOS", "CARGA_DATOS", "Inicia la carga de los Modulos")

            Using dbFactory As New DataBase(CnnFEMCO_Transfer)


                dt = dbFactory.GetDataAsDataTable("FEMCOEPSAP.spICMToolsModuleSelect")
                If dt IsNot Nothing Then

                    For Each row As DataRow In dt.Rows
                        Dim Modulo As New Modules()
                        Modulo.ModuleIDModule = row.Item(0)
                        Modulo.ModuleIDKey = row.Item(1).ToString()
                        Modulo.ModuleName = row.Item(2).ToString()
                        Modulo.ModuleModeloName = row.Item(3).ToString()

                        Response.Datos.Add(Modulo)
                    Next

                End If

            End Using

            Response.Exito = True
            Response.Mensaje = "Datos cargados con Exito"

        Catch ex As Exception
            mensaje = "Error en la carga de los Modulos, " + ex.Message
            mLog.insertLog("REEMPLAZOS", "CARGA_DATOS", mensaje)
            Response.Mensaje = mensaje
        End Try

        Return Response
    End Function

    <WebMethod(True)>
    <ScriptMethod(ResponseFormat:=ResponseFormat.Json)>
    Public Function SelectParametersCombo_Ajustado(SociedadID As String, DivisionID As String, ModuloID As Integer) As Response(Of ParametersConfiguration)

        Dim Response As New Response(Of ParametersConfiguration) With {
            .Exito = False,
            .Mensaje = "Error Inesperado",
            .Datos = New List(Of ParametersConfiguration)
            }

        If Session Is Nothing OrElse Session("User") Is Nothing Then
            Response.Mensaje = "Sesión expirada. Por favor inicie sesión nuevamente."
            Return Response
        End If

        Dim mensaje As String = ""
        Dim dt As DataTable = Nothing

        Try
            mLog = New Log
            mLog.insertLog("REEMPLAZOS", "CARGA_DATOS", "Inicia la carga del combo de Parametros")

            Using dbFactory As New DataBase(CnnFEMCO_Transfer)

                Dim pSociedad As New SqlParameter("@IDSociety", SociedadID)
                Dim pDivision As New SqlParameter("@IDDivision", DivisionID)
                Dim pIDModule As New SqlParameter("@IDModule", ModuloID)


                dt = dbFactory.GetDataAsDataTable("FEMCOEPSAP.spICMToolsParametersSelect", pSociedad, pDivision, pIDModule)
                If dt IsNot Nothing Then

                    For Each row As DataRow In dt.Rows
                        Dim Parametro As New ParametersConfiguration()
                        Parametro.ParameterID = row.Item(0)
                        Parametro.ParameterIDKey = row.Item(1).ToString()
                        Parametro.ParameterIDModule = row.Item(2).ToString()
                        Parametro.ParameterValue = row.Item(3).ToString()

                        Response.Datos.Add(Parametro)
                    Next
                End If

            End Using

            Response.Exito = True
            Response.Mensaje = "Datos cargados con Exito"

        Catch ex As Exception
            mensaje = "Error en la carga del combo de Parametros, " + ex.Message
            mLog.insertLog("REEMPLAZOS", "CARGA_DATOS", mensaje)
            Response.Mensaje = mensaje
        End Try


        Return Response
    End Function

#End Region

End Class