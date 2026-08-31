Public Partial Class SiteMaster
    Inherits System.Web.UI.MasterPage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            If HttpContext.Current.User.Identity.IsAuthenticated Then
                Dim tenantCode = AuthHelper.GetCurrentTenantCode()
                Dim username = AuthHelper.GetCurrentUsername()
                litTenant.Text = tenantCode
                litUser.Text = username
                litUserInitial.Text = If(String.IsNullOrEmpty(username), "?", username.Trim().Substring(0, 1).ToUpper())
                litStatusTenant.Text = tenantCode
                litStatusUser.Text = username
            End If
        End If
    End Sub

    Protected Sub btnLogout_Click(sender As Object, e As EventArgs)
        AuthHelper.SignOut()
        Response.Redirect("~/Account/Login.aspx")
    End Sub

    Public ReadOnly Property CurrentTenantCode As String
        Get
            Return AuthHelper.GetCurrentTenantCode()
        End Get
    End Property
End Class
