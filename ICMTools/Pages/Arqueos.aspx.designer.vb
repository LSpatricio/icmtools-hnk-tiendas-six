Option Strict On
Option Explicit On

Partial Public Class Arqueos
    Protected WithEvents myForm As Global.System.Web.UI.HtmlControls.HtmlForm
    Protected WithEvents SelectPeriod As Global.System.Web.UI.WebControls.DropDownList
    Protected WithEvents ScriptManager1 As Global.System.Web.UI.ScriptManager
    Protected WithEvents FileUploader As Global.AjaxControlToolkit.AsyncFileUpload
    Protected WithEvents myThrobber As Global.System.Web.UI.WebControls.Label

    Public Shadows ReadOnly Property Master() As ICMTools.MasterPage
        Get
            Return CType(MyBase.Master, ICMTools.MasterPage)
        End Get
    End Property
End Class
