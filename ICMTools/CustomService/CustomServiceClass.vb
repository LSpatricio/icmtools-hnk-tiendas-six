Imports DocumentFormat.OpenXml.Office2010.PowerPoint
Imports DocumentFormat.OpenXml.Spreadsheet
Imports Newtonsoft.Json
Imports Npgsql
Imports NpgsqlTypes
Public Class CustomServiceClass

#Region "Variables Locales"
    Private NpgsqlConn As String = ConfigurationManager.ConnectionStrings("PGSQL_CONNECTION").ConnectionString
#End Region
#Region "Funciones"
    ''' <summary>
    ''' Obtiene las Sociedades desde una API y las convierte a una Lista.
    ''' </summary>
    '''<param name="User">El usuario que lanza la solicitud</param>
    '''<param name="Model">El modelo desde donde se accede</param>
    '''<returns>Una lista con las Sociedades a las que tiene acceso el usuario y modelo</returns>
    Public Function GetSocieties(Model As String, User As String) As List(Of Societies)

        Dim ws As New WebServiceICMGeneral()
        Dim maskModel As String = Nothing

        If Model = "DEBUG" Then
            maskModel = "femcoepdev"
        Else
            maskModel = Model
        End If

        Dim PayeeID As String = ws.GetPayeeByUserEmail(User, maskModel)
        Dim ExternalTables As DataTable = ws.GetSocietiesExternalTables(PayeeID, maskModel)
        Dim jsonTable As String = JsonConvert.SerializeObject(ExternalTables)
        Dim SocietiesList As New List(Of Societies)
        Dim responseTable As DataTable = New DataTable()

        Try
            Using conn As New NpgsqlConnection(NpgsqlConn)
                conn.Open()
                Using cmd As New NpgsqlCommand("SELECT * FROM getsocieties(@jTable)", conn)
                    cmd.Parameters.AddWithValue("jTable", NpgsqlDbType.Json, jsonTable)
                    Using reader As NpgsqlDataReader = cmd.ExecuteReader()
                        responseTable.Load(reader)
                    End Using
                    For Each row As DataRow In responseTable.Rows
                        Dim Society As New Societies(row("idsociety").ToString(), row("description").ToString())
                        SocietiesList.Add(Society)
                    Next
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error de PostgreSQL: " & ex.Message)
        End Try

        Return SocietiesList
    End Function
    ''' <summary>
    ''' Obtiene las Divisiones de Personal desde una API y las convierte a una Lista.
    ''' </summary>
    '''<param name="User">El usuario que lanza la solicitud</param>
    '''<param name="Model">El modelo desde donde se accede</param>
    '''<returns>Una lista con las Divisiones de Personal a las que tiene acceso el usuario y modelo</returns>
    Public Function GetPersonnelDivisions(Model As String, User As String, Society As String) As List(Of PersonnelDivisions)
        Dim ws As New WebServiceICMGeneral()
        Dim maskModel As String = Nothing

        If Model = "DEBUG" Then
            maskModel = "femcoepdev"
        Else
            maskModel = Model
        End If

        Dim PersonnelDivisionList As New List(Of PersonnelDivisions)
        Dim PayeeID As String = ws.GetPayeeByUserEmail(User, maskModel)

        If maskModel = "femcoepdev" Or maskModel = "femcoepqa" Or maskModel = "femcoepprd" Then
            Dim ExternalTables As DataTable = ws.GetPersonnelDExternalTables(PayeeID, maskModel, Society)
            If ExternalTables IsNot Nothing AndAlso ExternalTables.Rows.Count > 0 Then
                For Each Row As DataRow In ExternalTables.Rows
                    Dim division As New PersonnelDivisions(Row.Item("PersonalDivision").ToString(), Row.Item("Description").ToString())
                    PersonnelDivisionList.Add(division)
                Next
            End If
            Return PersonnelDivisionList
        Else
            Dim PersonnelDivisions As DataTable = ws.GetPersonnelDivisions(PayeeID, maskModel, Society)
            If PersonnelDivisions IsNot Nothing AndAlso PersonnelDivisions.Rows.Count > 0 Then
                For Each Row As DataRow In PersonnelDivisions.Rows
                    Dim division As New PersonnelDivisions(Row.Item("IDPersonalDivision").ToString(), Row.Item("Description").ToString())
                    PersonnelDivisionList.Add(division)
                Next
            End If
            Return PersonnelDivisionList
        End If

        Return Nothing
    End Function
    ''' <summary>
    ''' Obtiene las Divisiones de Personal desde una API y las convierte a una Lista.
    ''' </summary>
    '''<param name="User">El usuario que lanza la solicitud</param>
    '''<param name="Model">El modelo desde donde se accede</param>
    '''<returns>Una lista con las Divisiones de Personal a las que tiene acceso el usuario y modelo</returns>
    Public Function GetPersonnelDivisionsex(Model As String, User As String, Society As String) As List(Of PersonnelDivisions)
        Dim ws As New WebServiceICMGeneral()
        Dim maskModel As String = Nothing

        If Model = "DEBUG" Then
            maskModel = "femcoepdev"
        Else
            maskModel = Model
        End If

        Dim PersonnelDivisionList As New List(Of PersonnelDivisions)
        Dim PayeeID As String = ws.GetPayeeByUserEmail(User, maskModel)

        If maskModel = "femcoepdev" Or maskModel = "femcoepqa" Or maskModel = "femcoepprd" Then
            Dim ExternalTables As DataTable = ws.GetPersonnelDExternalTables(PayeeID, maskModel, Society)
            If ExternalTables IsNot Nothing AndAlso ExternalTables.Rows.Count > 0 Then
                'Dim allItem As New PersonnelDivisions("-1", "(!) TODAS")
                'PersonnelDivisionList.Add(allItem)
                For Each Row As DataRow In ExternalTables.Rows
                    Dim division As New PersonnelDivisions(Row.Item("PersonalDivision").ToString(), Row.Item("Description").ToString())
                    PersonnelDivisionList.Add(division)
                Next
            End If
            Return PersonnelDivisionList
        Else
            Dim PersonnelDivisions As DataTable = ws.GetPersonnelDivisions(PayeeID, maskModel, Society)
            If PersonnelDivisions IsNot Nothing AndAlso PersonnelDivisions.Rows.Count > 0 Then
                For Each Row As DataRow In PersonnelDivisions.Rows
                    Dim division As New PersonnelDivisions(Row.Item("IDPersonalDivision").ToString(), Row.Item("Description").ToString())
                    PersonnelDivisionList.Add(division)
                Next
            End If
            Return PersonnelDivisionList
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Obtiene los periodos desde una API y las convierte a una Lista.
    ''' </summary>
    '''<param name="Model">El modelo desde donde se accede</param>
    '''<param name="Limit">El Limite establecido para obtener fechas</param>
    '''<returns>Una lista con los Periodos a las que tiene acceso el usuario y modelo</returns>
    Public Function GetPeriods(Model As String, Limit As Integer) As List(Of Periods)
        Dim ws As New WebServiceICMGeneral()
        Dim PeriodsList As New List(Of Periods)
        Dim maskModel As String = Nothing

        If Model = "DEBUG" Then
            maskModel = "femcoepqa"
        Else
            maskModel = Model
        End If

        Dim PeriodsTable As DataTable = ws.GetExternalPeriods(maskModel, Limit)
        If PeriodsTable IsNot Nothing AndAlso PeriodsTable.Rows.Count > 0 Then
            For Each Row As DataRow In PeriodsTable.Rows
                Dim period As New Periods(Row.Item("PeriodId").ToString(), Row.Item("PeriodName").ToString())
                PeriodsList.Add(period)
            Next
        End If

        Dim NominaCriticaTable As DataTable = ws.GetNominaCritica(maskModel)
        If NominaCriticaTable IsNot Nothing AndAlso NominaCriticaTable.Rows.Count > 0 Then
            For Each Row As DataRow In NominaCriticaTable.Rows
                Dim period As New Periods(Row.Item("PeriodId").ToString(), Row.Item("PeriodName").ToString())
                PeriodsList.Add(period)
            Next
        End If

        Return PeriodsList
    End Function


    Public Function CheckUpdates(Model As String, Tables As List(Of String)) As List(Of String)
        Try
            Dim ws As New WebServiceICMGeneral()

            Dim LastId As Integer = 0

            Dim cols As String = "MAX(\""AuditID_\"") AS \""MaxAuditID\"", substring(LOWER(\""Message_\"") FROM 'tabla\\s+(\\w+)') AS \""Tabla\"""
            Dim params As String = $"WHERE \""Event_\"" LIKE ANY (ARRAY['%Insertar%', '%Actualizar%', '%Suprimir%']) 
	                                AND \""Message_\"" LIKE ANY (ARRAY['% CfgDateStringPeriod %', '% HistoryPayee %', '% Time_ %', '% Payee_ %', '% CatPersonalDivision %', '% CfgOracleSAP %', '% CfgWebPermission %', '% CfgStoreHierarchy %', '% CatPersonalDivision %', '% CatJobKey %']) 
                                    AND \""AuditID_\"" > " & LastId & " GROUP BY \""Tabla\""
                                    ORDER BY \""MaxAuditID\"" DESC"
            Dim desactualizadas As New DataTable()
            Dim response As New List(Of String)


            desactualizadas = ws.ConsultaICMAPIQuery(cols, "Audit_", Model, params)

            Return response

        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Error al consultar el Audit_: " & ex.Message)
            Throw
        End Try
    End Function
#End Region
End Class
