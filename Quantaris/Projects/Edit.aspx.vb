Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Data

Public Partial Class Projects_Edit
    Inherits BasePage

    Private _projectId As Integer = 0

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)

        If Not IsPostBack Then
            LoadProject()
            LoadOwners()
        End If
    End Sub

    Private Sub LoadProject()
        Dim row As DataRow = ProjectHelper.GetProject(_projectId, CurrentTenantId)
        If row Is Nothing Then
            Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
            Return
        End If
        txtCode.Text = row("ProjectCode").ToString()
        txtName.Text = row("ProjectName").ToString()
        txtDescription.Text = If(row("Description") Is DBNull.Value, "", row("Description").ToString())
        If row.Table.Columns.Contains("Status") AndAlso row("Status") IsNot DBNull.Value Then
            Dim item = ddlStatus.Items.FindByValue(row("Status").ToString())
            If item IsNot Nothing Then item.Selected = True
        End If
        lnkCancel.NavigateUrl = ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
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

        Dim row As DataRow = ProjectHelper.GetProject(_projectId, CurrentTenantId)
        If row IsNot Nothing AndAlso row("ProjectOwnerId") IsNot DBNull.Value Then
            Dim item = ddlOwner.Items.FindByValue(row("ProjectOwnerId").ToString())
            If item IsNot Nothing Then item.Selected = True
        End If
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not Page.IsValid Then Return
        Try
            ProjectHelper.UpdateProject(_projectId, CurrentTenantId, txtCode.Text.Trim(), txtName.Text.Trim(), txtDescription.Text.Trim(), CInt(ddlOwner.SelectedValue))
            LogAudit("Edit", "Project", _projectId, "Edited project: " & txtName.Text)
            Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        Catch ex As Exception
            litError.Text = "<div class='alert alert-danger alert-dismissible fade show' role='alert'>" &
                "<i class='bi bi-exclamation-triangle me-2'></i>Error updating project: " & ex.Message &
                "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
        End Try
    End Sub
End Class