Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Account_Register
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Dim tc = TenantResolver.ResolveTenantCode()
            If String.IsNullOrEmpty(tc) Then tc = "DEMO"
            litTenantCode.Text = tc
        End If
    End Sub

    Protected Sub btnRegister_Click(sender As Object, e As EventArgs) Handles btnRegister.Click
        Dim tenantCode = litTenantCode.Text.Trim()
        Dim tenantId = TenantResolver.ResolveTenantId(tenantCode)

        If tenantId = 0 Then
            divError.InnerText = "Invalid tenant. Please provide a valid tenant code."
            divError.Visible = True
            Return
        End If

        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Dim checkCmd As New MySqlCommand("SELECT COUNT(1) FROM Users WHERE (Username = @Username OR Email = @Email) AND TenantId = @TenantId", conn)
            checkCmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim())
            checkCmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim())
            checkCmd.Parameters.AddWithValue("@TenantId", tenantId)
            Dim exists = CInt(checkCmd.ExecuteScalar())
            If exists > 0 Then
                divError.InnerText = "Username or Email already exists for this tenant."
                divError.Visible = True
                Return
            End If
        End Using

        Try
            Dim userId = AuthHelper.CreateUser(
                txtUsername.Text.Trim(),
                txtEmail.Text.Trim(),
                txtPassword.Text,
                txtFirstName.Text.Trim(),
                txtLastName.Text.Trim(),
                "User",
                tenantId,
                1)

            Logger.Log(tenantId, userId, "Register", "User", userId, "New user registered")
            divError.Attributes("class") = "alert alert-success"
            divError.InnerText = "Registration successful! You can now log in."
            divError.Visible = True
            btnRegister.Enabled = False
        Catch ex As Exception
            divError.InnerText = "Registration failed: " & ex.Message
            divError.Visible = True
        End Try
    End Sub
End Class
