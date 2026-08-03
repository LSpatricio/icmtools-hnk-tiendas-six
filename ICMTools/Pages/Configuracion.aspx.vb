Imports System
Imports System.IO
Imports System.Data
Imports System.ServiceModel.Channels
Imports System.ServiceModel.Configuration
Imports Microsoft.Office.Core



Public Class BonosTransporteConfiguracion
    Inherits System.Web.UI.Page

    Private mUser As User
    Private mLog As Log
    Dim ws As New WebServiceICMGeneral()

    Private Sub Page_Init(sender As Object, e As EventArgs) Handles Me.Init

        '------Evitar Caché del Navegador--------
        Response.Expires = -10000
        Response.AddHeader("pragma", "no-cache")
        Response.AddHeader("cache-control", "private")
        Response.CacheControl = "no-cache"
        '----------------------------------------
        Try
            Me.Master.PageIcon = "<i class='fas fa-cogs fa-fw'></i>"
            Me.Master.PageName = "Configuración"

            If Session.Item("User") IsNot Nothing Then
                mUser = CType(Session.Item("User"), User)
                mLog = New Log
                Dim maskModel As String

                If mUser.Model = "DEBUG" Then
                    maskModel = "femcoepdev"
                Else
                    maskModel = mUser.Model
                End If

                Dim PayeeID As String = ws.GetPayeeByUserEmail(mUser.Email, maskModel)
                Dim LastHistoryPayee As String = ws.GetLastHistoryPayee(PayeeID, maskModel)

                If Not ScreenPermission.Access(LastHistoryPayee, mUser.Email, "CONFIG") Then
                    Dim mensaje As String = "El usuario: " & mUser.Name & ", No tiene acceso al modulo: 'CONFIG' "
                    mLog.insertLog("Configuración", "Configuración", mensaje)

                    Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
                Else

                    mLog.insertLog("Configuración", "Configuración", "Acceso al Modulo de Configuración")
                End If

            Else
                Response.Redirect(ConfigurationManager.AppSettings("LoginPage"), False)
            End If




        Catch ex As Exception
            Me.Master.MessageBoxShow("Error en page_load", ex.Message, "Fuente:" & ex.InnerException.Source, htmlMessageIcon.IconError)
        End Try
    End Sub







End Class