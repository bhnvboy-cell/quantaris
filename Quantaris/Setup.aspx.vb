Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Web.Security

Public Partial Class Setup
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
    End Sub

    Protected Sub btnSetup_Click(sender As Object, e As EventArgs) Handles btnSetup.Click
        Dim messages As New List(Of String)

        Try
            ' 1. Ensure DEMO tenant exists
            Dim tenantId As Integer = 0
            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()

                ' Check tenant
                Dim cmd As New MySqlCommand("SELECT Id FROM Tenants WHERE TenantCode = 'DEMO'", conn)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then
                    tenantId = CInt(result)
                    messages.Add("Tenant 'DEMO' already exists (ID: " & tenantId & ")")
                Else
                    cmd = New MySqlCommand(
                        "INSERT INTO Tenants (TenantCode, TenantName, Subdomain, IsActive, IsTrial, MaxProjects, MaxUsers, PlanType, CreatedOn, CreatedByUserId) " &
                        "VALUES ('DEMO', 'Demo Company', 'demo', 1, 1, 10, 50, 'Enterprise', CURRENT_TIMESTAMP, 1)", conn)
                    cmd.ExecuteNonQuery()
                    cmd = New MySqlCommand("SELECT LAST_INSERT_ID()", conn)
                    tenantId = CInt(cmd.ExecuteScalar())
                    messages.Add("Created tenant 'DEMO' (ID: " & tenantId & ")")
                End If

                ' 2. Create admin user with known password
                Dim adminExists As Boolean = False
                cmd = New MySqlCommand("SELECT COUNT(1) FROM Users WHERE Username = 'admin' AND TenantId = @TenantId", conn)
                cmd.Parameters.AddWithValue("@TenantId", tenantId)
                adminExists = CInt(cmd.ExecuteScalar()) > 0

                If Not adminExists Then
                    ' Use AuthHelper to create user with proper hashing
                    Dim userId = AuthHelper.CreateUser("admin", "admin@demo.com", "Admin@123", "System", "Admin", "Owner", tenantId, 1)
                    messages.Add("Created user 'admin' / password 'Admin@123' (ID: " & userId & ")")
                Else
                    ' Reset password to known value
                    Dim salt = ""
                    Dim hash = ""
                    Using rng As New System.Security.Cryptography.RNGCryptoServiceProvider()
                        Dim saltBytes(31) As Byte
                        rng.GetBytes(saltBytes)
                        salt = Convert.ToBase64String(saltBytes)
                    End Using
                    Using sha256 As New System.Security.Cryptography.SHA256Managed()
                        Dim combined = "Admin@123" & salt
                        Dim bytes = Text.Encoding.UTF8.GetBytes(combined)
                        hash = Convert.ToBase64String(sha256.ComputeHash(bytes))
                    End Using

                    cmd = New MySqlCommand("UPDATE Users SET PasswordHash = @Hash, PasswordSalt = @Salt, IsActive = TRUE WHERE Username = 'admin' AND TenantId = @TenantId", conn)
                    cmd.Parameters.AddWithValue("@Hash", hash)
                    cmd.Parameters.AddWithValue("@Salt", salt)
                    cmd.Parameters.AddWithValue("@TenantId", tenantId)
                    cmd.ExecuteNonQuery()
                    messages.Add("Reset password for 'admin' to 'Admin@123'")
                End If

                ' 3. Create sample GreenBelt user
                Dim userExists As Boolean = False
                cmd = New MySqlCommand("SELECT COUNT(1) FROM Users WHERE Username = 'user1' AND TenantId = @TenantId", conn)
                cmd.Parameters.AddWithValue("@TenantId", tenantId)
                userExists = CInt(cmd.ExecuteScalar()) > 0

                If Not userExists Then
                    Dim userId = AuthHelper.CreateUser("user1", "user1@demo.com", "User@123", "Demo", "User", "GreenBelt", tenantId, 1)
                    messages.Add("Created user 'user1' / password 'User@123' (ID: " & userId & ")")
                End If
            End Using

            litResult.CssClass = "alert alert-success"
            litResult.Text = "<strong>Setup complete!</strong><br/>" & String.Join("<br/>", messages)
            btnSetup.Enabled = False
            btnSetup.Text = "Setup Already Run"
        Catch ex As Exception
            litResult.CssClass = "alert alert-danger"
            litResult.Text = "<strong>Setup failed:</strong> " & ex.Message
        End Try
    End Sub
End Class
