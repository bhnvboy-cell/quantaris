Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Account_Login
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            If Not String.IsNullOrEmpty(Request.QueryString("tenant")) Then
                litTenantCode.Text = Request.QueryString("tenant").Trim()
                TenantResolver.SetTenantCookie(litTenantCode.Text)
            Else
                Dim tc = TenantResolver.ResolveTenantCode()
                If String.IsNullOrEmpty(tc) Then tc = "DEMO"
                litTenantCode.Text = tc
            End If
        End If
    End Sub

    Protected Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        Dim tenantCode = litTenantCode.Text.Trim()
        Dim tenantId = TenantResolver.ResolveTenantId()

        If tenantId = 0 Then
            divError.InnerText = "Invalid tenant code."
            divError.Visible = True
            Return
        End If

        Dim userId = AuthHelper.ValidateUser(txtUsername.Text.Trim(), txtPassword.Text, tenantId)
        If userId > 0 Then
            Logger.Log(tenantId, userId, "Login", "User", userId, "User logged in")
            AuthHelper.SignIn(txtUsername.Text.Trim() & "|" & tenantCode, tenantCode, chkRemember.Checked)

            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()
                Using cmd As New MySqlCommand("UPDATE Users SET LastLoginOn = CURRENT_TIMESTAMP WHERE Id = @Id", conn)
                    cmd.Parameters.AddWithValue("@Id", userId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using

            Dim returnUrl = Request.QueryString("ReturnUrl")
            If String.IsNullOrEmpty(returnUrl) Then
                returnUrl = "~/Projects/Default.aspx?tenant=" & tenantCode
            End If
            Response.Redirect(returnUrl)
        Else
            divError.InnerText = "Invalid username or password."
            divError.Visible = True
        End If
    End Sub
End Class
