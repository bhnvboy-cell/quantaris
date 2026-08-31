Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Data

Public Partial Class Projects_Create
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            LoadOwners()
        End If
    End Sub

    Private Sub LoadOwners()
        Dim cmd As New MySqlCommand(
            "SELECT Id, CONCAT_WS(' ', FirstName, LastName) AS FullName FROM Users WHERE TenantId = @TenantId AND IsActive = TRUE ORDER BY FirstName")
        cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
        Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
        ddlOwner.DataSource = dt
        ddlOwner.DataTextField = "FullName"
        ddlOwner.DataValueField = "Id"
        ddlOwner.DataBind()
        ddlOwner.Items.Insert(0, New ListItem("-- Select Owner --", "0"))
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not Page.IsValid Then Return
        Try
            Dim projectId = ProjectHelper.CreateProject(
                CurrentTenantId,
                txtCode.Text.Trim(),
                txtName.Text.Trim(),
                txtDescription.Text.Trim(),
                CInt(ddlOwner.SelectedValue),
                CurrentUserId)

            LogAudit("Create", "Project", projectId, "Created project: " & txtName.Text)
            Response.Redirect("~/DMAIC/Dashboard.aspx?projectId=" & projectId & "&tenant=" & CurrentTenantCode)
        Catch ex As Exception
            litError.Text = "<div class='alert alert-danger alert-dismissible fade show' role='alert'>" &
                "<i class='bi bi-exclamation-triangle me-2'></i>Error creating project: " & ex.Message &
                "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
        End Try
    End Sub
End Class
