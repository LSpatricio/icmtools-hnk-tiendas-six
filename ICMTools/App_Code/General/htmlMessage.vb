Imports System.Web.UI

Public Enum htmlMessageIcon
    IconInfo1 'Primary
    IconInfo2 'Secondary
    IconError 'Danger
    IconWarning 'Warning
End Enum

Public Class htmlMessage

    Private mTitle As String
    Private mMessagePrimary As String
    Private mMessageSecondary As String

    Private mGenericControl1 As HtmlGenericControl
    Private mGenericControl2 As HtmlGenericControl
    Private mGenericControl3 As HtmlGenericControl
    Private mGenericControl4 As HtmlGenericControl
    Private mGenericControl5 As HtmlGenericControl

    Private mMessageType As htmlMessageIcon

    Public Property Title() As String
        Get
            Return mTitle
        End Get
        Set(value As String)
            mTitle = value
        End Set
    End Property

    Public Property MessagePrimary() As String
        Get
            Return mMessagePrimary
        End Get
        Set(value As String)
            mMessagePrimary = value
        End Set
    End Property

    Public Property MessagensSecondary() As String
        Get
            Return mMessageSecondary
        End Get
        Set(value As String)
            mMessageSecondary = value
        End Set
    End Property

    Public Property MessageType As htmlMessageIcon
        Get
            Return mMessageType
        End Get
        Set(value As htmlMessageIcon)
            mMessageType = value
        End Set
    End Property

    Public Sub New(Page As Page, htmlIdMessage As String, htmlIdTitle As String, htmlIdMessagePrimary As String, htmlIdMessageSecondary As String, htmlIdMessageIcon As String)

        'Message div
        mGenericControl1 = Page.FindControl(htmlIdMessage)
        'Title control div
        mGenericControl2 = Page.FindControl(htmlIdTitle)
        'Message primary div
        mGenericControl3 = Page.FindControl(htmlIdMessagePrimary)
        'Message secondary div
        mGenericControl4 = Page.FindControl(htmlIdMessageSecondary)
        'Message icon div
        mGenericControl5 = Page.FindControl(htmlIdMessageIcon)

    End Sub

    Public Sub New(Page As MasterPage, htmlIdMessage As String, htmlIdTitle As String, htmlIdMessagePrimary As String, htmlIdMessageSecondary As String, htmlIdMessageIcon As String)

        'Message div
        mGenericControl1 = Page.FindControl(htmlIdMessage)
        'Title control div
        mGenericControl2 = Page.FindControl(htmlIdTitle)
        'Message primary div
        mGenericControl3 = Page.FindControl(htmlIdMessagePrimary)
        'Message secondary div
        mGenericControl4 = Page.FindControl(htmlIdMessageSecondary)
        'Message icon div
        mGenericControl5 = Page.FindControl(htmlIdMessageIcon)

    End Sub

    Public Sub Show(Title As String, MessagePrimary As String, MessageSecondary As String, type As htmlMessageIcon)
        'Message div
        mGenericControl1.Style.Remove("Display")
        'Title control div
        mGenericControl2.InnerText = Title
        'Message primary div
        mGenericControl3.InnerHtml = MessagePrimary
        'Message secondary div
        mGenericControl4.InnerText = MessageSecondary

        'Message secondary div
        Select Case mMessageType
            Case htmlMessageIcon.IconError
                mGenericControl1.Attributes.Add("Class", "alert alert-danger fade show")
                mGenericControl5.Attributes.Add("Class", "fas fa-exclamation-circle")
            Case htmlMessageIcon.IconWarning
                mGenericControl1.Attributes.Add("Class", "alert alert-warning fade show")
                mGenericControl5.Attributes.Add("Class", "fas fa-exclamation-triangle")
            Case htmlMessageIcon.IconInfo1
                mGenericControl1.Attributes.Add("Class", "alert alert-info fade show")
                mGenericControl5.Attributes.Add("Class", "fas fa-info-circle")
            Case htmlMessageIcon.IconInfo2
                mGenericControl1.Attributes.Add("Class", "alert alert-secondary fade show")
                mGenericControl5.Attributes.Add("Class", "fas fa-info")
        End Select

    End Sub

    Public Sub Show()

        'Message div
        mGenericControl1.Style.Remove("Display")

        'Title control div
        If mTitle.Length = 0 Then
            mGenericControl2.InnerHtml = "No Title"
        Else
            mGenericControl2.InnerText = mTitle
        End If
        'Message primary div
        If mMessagePrimary.Length = 0 Then
            mGenericControl3.InnerHtml = "No message detail..."
        Else
            mGenericControl3.InnerHtml = mMessagePrimary
        End If
        'Message secondary div
        If mMessageSecondary.Length = 0 Then
            mGenericControl4.InnerHtml = "No message footer..."
        Else
            mGenericControl4.InnerText = mMessageSecondary
        End If

        'Message secondary div
        Select Case mMessageType
            Case htmlMessageIcon.IconError
                mGenericControl1.Attributes.Add("Class", "alert alert-danger fade show")
                mGenericControl5.Attributes.Add("Class", "fas fa-exclamation-circle")
            Case htmlMessageIcon.IconWarning
                mGenericControl1.Attributes.Add("Class", "alert alert-warning fade show")
                mGenericControl5.Attributes.Add("Class", "fas fa-exclamation-triangle")
            Case htmlMessageIcon.IconInfo1
                mGenericControl1.Attributes.Add("Class", "alert alert-info fade show")
                mGenericControl5.Attributes.Add("Class", "fas fa-info-circle")
            Case htmlMessageIcon.IconInfo2
                mGenericControl1.Attributes.Add("Class", "alert alert-secondary fade show")
                mGenericControl5.Attributes.Add("Class", "fas fa-info")
        End Select


    End Sub

End Class
