Public Class ExceptionsReportHistory
    Inherits Page
    Private mUser As User
    Private mLog As Log

    ''' <summary>
    ''' Evento Init del objeto Page.
    ''' </summary>
    ''' <param name="sender">sender</param>
    ''' <param name="e">e</param>
    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
    End Sub

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            Me.Master.PageIcon = "<i class='fas fa-history fa-fw'></i>"
            Me.Master.PageName = "Reporte de Historial de cargas"

            If Not Session.Item("User") Is Nothing Then
                mUser = CType(Session.Item("User"), User)
                CargarControles()
                mLog = New Log
                mLog.insertLog("EXCEPCIONES", "ACCESO", "Acceso a Reporte de Historial de cargas")
            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If
        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub

    Public Sub CargarControles()
        Try
            Dim serviceData As New CustomServiceClass

            Dim NullOption As New ListItem
            NullOption.Text = "Seleccione"
            NullOption.Value = "-1"

            Dim AllOption As New ListItem
            AllOption.Text = "Todos"
            AllOption.Value = "0"

            SelectPeriod.Items.Add(NullOption)
            SelectSociety.Items.Add(NullOption)
            SelectPersonnelDivision.Items.Add(NullOption)

            Dim AvailablePeriodos As New List(Of Periods)
            AvailablePeriodos = serviceData.GetPeriods(mUser.Model, 2)

            For Each Periodo As Periods In AvailablePeriodos

                Dim NewPeriodo As New ListItem

                NewPeriodo.Text = Periodo.PeriodoName
                NewPeriodo.Value = Periodo.PeriodoValue

                SelectPeriod.Items.Add(NewPeriodo)
            Next

            Dim AvailableSocieties As New List(Of Societies)

            AvailableSocieties = serviceData.GetSocieties(mUser.Model, mUser.Email)

            For Each Society As Societies In AvailableSocieties

                Dim NewSociety As New ListItem

                NewSociety.Text = Society.SocietyName
                NewSociety.Value = Society.SocietyValue

                SelectSociety.Items.Add(NewSociety)

            Next

        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en CargarControles", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub
End Class