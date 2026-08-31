Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Improve_Improve_SolutionGeneration
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadSolutions()
            LoadSelectionMatrix()
            UpdateStepStatus()
        End If
    End Sub

    Protected Sub ddlTechnique_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlTechnique.SelectedIndexChanged
        divLeanTool.Visible = (ddlTechnique.SelectedValue = "LeanTool")
    End Sub

    Private Sub LoadSolutions()
        Using cmd As New MySqlCommand(
            "SELECT * FROM SolutionIdeas WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptSolutions.DataSource = dt
            rptSolutions.DataBind()
            litIdeaCount.Text = dt.Rows.Count & " idea(s)"
        End Using
    End Sub

    Private Sub LoadSelectionMatrix()
        Using cmd As New MySqlCommand(
            "SELECT * FROM SolutionIdeas WHERE TenantId = @TenantId AND ProjectId = @ProjectId " &
            "AND SelectionScore IS NOT NULL ORDER BY SelectionScore DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptSelectionMatrix.DataSource = dt
            rptSelectionMatrix.DataBind()
        End Using
    End Sub

    Private Function ParseDecimal(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then Return DBNull.Value
        Dim result As Decimal
        If Decimal.TryParse(value.Trim(), result) Then Return result
        Return DBNull.Value
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not Page.IsValid Then Return
        Dim editId As Integer = CInt(hfEditId.Value)

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE SolutionIdeas SET IdeaTitle = @IdeaTitle, Description = @Description, " &
                "Technique = @Technique, LeanToolType = @LeanToolType, Category = @Category, " &
                "SelectionScore = @SelectionScore, SelectionCriteria = @SelectionCriteria, " &
                "IsSelected = @IsSelected WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@IdeaTitle", txtIdeaTitle.Text.Trim())
                cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(txtDescription.Text), CObj(DBNull.Value), txtDescription.Text.Trim()))
                cmd.Parameters.AddWithValue("@Technique", ddlTechnique.SelectedValue)
                cmd.Parameters.AddWithValue("@LeanToolType", If(ddlTechnique.SelectedValue = "LeanTool", ddlLeanToolType.SelectedValue, CObj(DBNull.Value)))
                cmd.Parameters.AddWithValue("@Category", If(String.IsNullOrWhiteSpace(txtCategory.Text), CObj(DBNull.Value), txtCategory.Text.Trim()))
                cmd.Parameters.AddWithValue("@SelectionScore", ParseDecimal(txtSelectionScore.Text))
                cmd.Parameters.AddWithValue("@SelectionCriteria", If(String.IsNullOrWhiteSpace(txtSelectionCriteria.Text), CObj(DBNull.Value), txtSelectionCriteria.Text.Trim()))
                cmd.Parameters.AddWithValue("@IsSelected", chkIsSelected.Checked)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "SolutionIdea", editId, "Updated solution idea: " & txtIdeaTitle.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO SolutionIdeas (TenantId, ProjectId, IdeaTitle, Description, Technique, LeanToolType, " &
                "Category, SelectionScore, SelectionCriteria, IsSelected, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @IdeaTitle, @Description, @Technique, @LeanToolType, " &
                "@Category, @SelectionScore, @SelectionCriteria, @IsSelected, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@IdeaTitle", txtIdeaTitle.Text.Trim())
                cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(txtDescription.Text), CObj(DBNull.Value), txtDescription.Text.Trim()))
                cmd.Parameters.AddWithValue("@Technique", ddlTechnique.SelectedValue)
                cmd.Parameters.AddWithValue("@LeanToolType", If(ddlTechnique.SelectedValue = "LeanTool", ddlLeanToolType.SelectedValue, CObj(DBNull.Value)))
                cmd.Parameters.AddWithValue("@Category", If(String.IsNullOrWhiteSpace(txtCategory.Text), CObj(DBNull.Value), txtCategory.Text.Trim()))
                cmd.Parameters.AddWithValue("@SelectionScore", ParseDecimal(txtSelectionScore.Text))
                cmd.Parameters.AddWithValue("@SelectionCriteria", If(String.IsNullOrWhiteSpace(txtSelectionCriteria.Text), CObj(DBNull.Value), txtSelectionCriteria.Text.Trim()))
                cmd.Parameters.AddWithValue("@IsSelected", chkIsSelected.Checked)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "SolutionIdea", _projectId, "Added solution idea: " & txtIdeaTitle.Text)
        End If

        ClearForm()
        LoadSolutions()
        LoadSelectionMatrix()
    End Sub

    Protected Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        ClearForm()
    End Sub

    Protected Sub rptSolutions_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM SolutionIdeas WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "SolutionIdea", id, "Deleted solution idea")
            LoadSolutions()
            LoadSelectionMatrix()
        ElseIf e.CommandName = "Edit" Then
            LoadSolutionForEdit(id)
        End If
    End Sub

    Private Sub LoadSolutionForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM SolutionIdeas WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfEditId.Value = id.ToString()
                txtIdeaTitle.Text = row("IdeaTitle").ToString()
                txtDescription.Text = row("Description").ToString()
                If ddlTechnique.Items.FindByValue(row("Technique").ToString()) IsNot Nothing Then
                    ddlTechnique.SelectedValue = row("Technique").ToString()
                End If
                ddlTechnique_SelectedIndexChanged(Nothing, Nothing)
                If row("LeanToolType") IsNot DBNull.Value AndAlso ddlLeanToolType.Items.FindByValue(row("LeanToolType").ToString()) IsNot Nothing Then
                    ddlLeanToolType.SelectedValue = row("LeanToolType").ToString()
                End If
                txtCategory.Text = row("Category").ToString()
                txtSelectionScore.Text = If(row("SelectionScore") IsNot DBNull.Value, row("SelectionScore").ToString(), "")
                txtSelectionCriteria.Text = row("SelectionCriteria").ToString()
                chkIsSelected.Checked = CBool(row("IsSelected"))
                btnCancel.Visible = True
                btnSave.Text = "<i class='bi bi-check-lg me-1'></i>Update Idea"
            End If
        End Using
    End Sub

    Private Sub ClearForm()
        txtIdeaTitle.Text = ""
        txtDescription.Text = ""
        ddlTechnique.SelectedIndex = 0
        divLeanTool.Visible = False
        ddlLeanToolType.SelectedIndex = 0
        txtCategory.Text = ""
        txtSelectionScore.Text = ""
        txtSelectionCriteria.Text = ""
        chkIsSelected.Checked = False
        hfEditId.Value = "0"
        btnCancel.Visible = False
        btnSave.Text = "<i class='bi bi-save me-1'></i>Save Idea"
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 10, "In Progress")
    End Sub
End Class
