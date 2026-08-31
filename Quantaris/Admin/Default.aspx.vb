Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Admin_Default
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin")
        If Not IsPostBack Then
            LoadTenantInfo()
            LoadUsers()
        End If
    End Sub

    Private Sub LoadTenantInfo()
        Using cmd As New MySqlCommand("SELECT * FROM Tenants WHERE Id = @Id")
            cmd.Parameters.AddWithValue("@Id", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim r = dt.Rows(0)
                txtTenantCode.Text = r("TenantCode").ToString()
                txtTenantName.Text = r("TenantName").ToString()
                txtPlanType.Text = r("PlanType").ToString()
                txtMaxProjects.Text = r("MaxProjects").ToString()
                txtMaxUsers.Text = r("MaxUsers").ToString()
                chkActive.Checked = CBool(r("IsActive"))
            End If
        End Using
    End Sub

    Private Sub LoadUsers()
        Using cmd As New MySqlCommand(
            "SELECT Id, Username, Email, FirstName, LastName, Role, IsActive, LastLoginOn FROM Users WHERE TenantId = @TenantId ORDER BY Username")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            rptUsers.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptUsers.DataBind()
        End Using
    End Sub

    Protected Sub btnSaveTenant_Click(sender As Object, e As EventArgs) Handles btnSaveTenant.Click
        Using cmd As New MySqlCommand(
            "UPDATE Tenants SET TenantName=@TenantName, IsActive=@IsActive WHERE Id=@Id")
            cmd.Parameters.AddWithValue("@TenantName", txtTenantName.Text.Trim())
            cmd.Parameters.AddWithValue("@IsActive", chkActive.Checked)
            cmd.Parameters.AddWithValue("@Id", CurrentTenantId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Update", "Tenant", CurrentTenantId, "Updated tenant settings")
        litMessage.Text = "<div class='alert alert-success alert-dismissible fade show' role='alert'>Tenant updated successfully.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
    End Sub

    Protected Sub btnAddUser_Click(sender As Object, e As EventArgs) Handles btnAddUser.Click
        Try
            Dim userId = AuthHelper.CreateUser(
                txtNewUsername.Text.Trim(),
                txtNewEmail.Text.Trim(),
                txtNewPassword.Text,
                txtNewFirstName.Text.Trim(),
                txtNewLastName.Text.Trim(),
                ddlNewRole.SelectedValue,
                CurrentTenantId,
                CurrentUserId)
            LogAudit("Create", "User", userId, "Created user: " & txtNewUsername.Text)
            litUserMessage.Text = "<div class='alert alert-success alert-dismissible fade show' role='alert'>User created successfully.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
            LoadUsers()
            txtNewUsername.Text = "" : txtNewEmail.Text = "" : txtNewPassword.Text = ""
            txtNewFirstName.Text = "" : txtNewLastName.Text = ""
            ddlNewRole.SelectedIndex = 0
        Catch ex As Exception
            litUserMessage.Text = "<div class='alert alert-danger alert-dismissible fade show' role='alert'>Error: " & ex.Message & "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
        End Try
    End Sub

    Protected Sub rptUsers_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim userId As Integer = CInt(e.CommandArgument)
        If e.CommandName = "ToggleActive" Then
            Using cmd As New MySqlCommand(
                "UPDATE Users SET IsActive = CASE WHEN IsActive THEN FALSE ELSE TRUE END WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", userId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "User", userId, "Toggled active status")
            LoadUsers()
        ElseIf e.CommandName = "UpdateRole" Then
            Dim newRole = DirectCast(e.Item.FindControl("ddlRoleEdit"), DropDownList).SelectedValue
            Using cmd As New MySqlCommand("UPDATE Users SET Role = @Role WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Role", newRole)
                cmd.Parameters.AddWithValue("@Id", userId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "User", userId, "Changed role to: " & newRole)
            LoadUsers()
        ElseIf e.CommandName = "DeleteUser" Then
            Using cmd As New MySqlCommand("DELETE FROM Users WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", userId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "User", userId, "Deleted user")
            LoadUsers()
        End If
    End Sub
End Class
