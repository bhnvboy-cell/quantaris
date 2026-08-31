Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Control_Control_BestPractices
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadBestPractices()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 14, "In Progress")
    End Sub

    Private Sub LoadBestPractices()
        Using cmd As New MySqlCommand(
            "SELECT * FROM BestPractices WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptBestPractices.DataSource = dt
            rptBestPractices.DataBind()
            litCount.Text = dt.Rows.Count & " practice(s)"
        End Using
    End Sub

    Protected Function Truncate(value As Object, maxLen As Integer) As String
        If value Is Nothing OrElse IsDBNull(value) Then Return String.Empty
        Dim s As String = value.ToString()
        If s.Length <= maxLen Then Return s
        Return s.Substring(0, maxLen) & "..."
    End Function

    Private Function ParseDate(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then Return DBNull.Value
        Dim result As Date
        If Date.TryParse(value.Trim(), result) Then Return result
        Return DBNull.Value
    End Function

    Private Sub ClearForm()
        txtTitle.Text = ""
        txtDescription.Text = ""
        txtApplicableAreas.Text = ""
        txtSites.Text = ""
        txtTeams.Text = ""
        chkIsReplicated.Checked = False
        txtReplicationDate.Text = ""
        hfEditId.Value = "0"
        btnCancel.Visible = False
        btnSave.Text = "Save Best Practice"
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not Page.IsValid Then Return
        Dim editId As Integer = CInt(hfEditId.Value)

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE BestPractices SET Title = @Title, Description = @Description, " &
                "ApplicableAreas = @ApplicableAreas, Sites = @Sites, Teams = @Teams, " &
                "IsReplicated = @IsReplicated, ReplicatedOn = @ReplicatedOn WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim())
                cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(txtDescription.Text), CObj(DBNull.Value), txtDescription.Text.Trim()))
                cmd.Parameters.AddWithValue("@ApplicableAreas", If(String.IsNullOrWhiteSpace(txtApplicableAreas.Text), CObj(DBNull.Value), txtApplicableAreas.Text.Trim()))
                cmd.Parameters.AddWithValue("@Sites", If(String.IsNullOrWhiteSpace(txtSites.Text), CObj(DBNull.Value), txtSites.Text.Trim()))
                cmd.Parameters.AddWithValue("@Teams", If(String.IsNullOrWhiteSpace(txtTeams.Text), CObj(DBNull.Value), txtTeams.Text.Trim()))
                cmd.Parameters.AddWithValue("@IsReplicated", chkIsReplicated.Checked)
                cmd.Parameters.AddWithValue("@ReplicatedOn", If(chkIsReplicated.Checked, ParseDate(txtReplicationDate.Text), CObj(DBNull.Value)))
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "BestPractice", editId, "Updated best practice: " & txtTitle.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO BestPractices (TenantId, ProjectId, Title, Description, ApplicableAreas, Sites, Teams, " &
                "IsReplicated, ReplicatedOn, CreatedByUserId, CreatedOn) " &
                "VALUES (@TenantId, @ProjectId, @Title, @Description, @ApplicableAreas, @Sites, @Teams, " &
                "@IsReplicated, @ReplicatedOn, @CreatedByUserId, CURRENT_TIMESTAMP)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim())
                cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(txtDescription.Text), CObj(DBNull.Value), txtDescription.Text.Trim()))
                cmd.Parameters.AddWithValue("@ApplicableAreas", If(String.IsNullOrWhiteSpace(txtApplicableAreas.Text), CObj(DBNull.Value), txtApplicableAreas.Text.Trim()))
                cmd.Parameters.AddWithValue("@Sites", If(String.IsNullOrWhiteSpace(txtSites.Text), CObj(DBNull.Value), txtSites.Text.Trim()))
                cmd.Parameters.AddWithValue("@Teams", If(String.IsNullOrWhiteSpace(txtTeams.Text), CObj(DBNull.Value), txtTeams.Text.Trim()))
                cmd.Parameters.AddWithValue("@IsReplicated", chkIsReplicated.Checked)
                cmd.Parameters.AddWithValue("@ReplicatedOn", If(chkIsReplicated.Checked, ParseDate(txtReplicationDate.Text), CObj(DBNull.Value)))
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "BestPractice", _projectId, "Added best practice: " & txtTitle.Text)
        End If

        ClearForm()
        LoadBestPractices()
    End Sub

    Protected Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        ClearForm()
    End Sub

    Protected Sub rptBestPractices_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        Select Case e.CommandName
            Case "Delete"
                Using cmd As New MySqlCommand("DELETE FROM BestPractices WHERE Id = @Id AND TenantId = @TenantId")
                    cmd.Parameters.AddWithValue("@Id", id)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    DbHelper.ExecuteNonQuery(cmd)
                End Using
                LogAudit("Delete", "BestPractice", id, "Deleted best practice")
                LoadBestPractices()

            Case "Edit"
                LoadBestPracticeForEdit(id)

            Case "ToggleReplicate"
                Using cmd As New MySqlCommand(
                    "UPDATE BestPractices SET IsReplicated = CASE WHEN IsReplicated THEN FALSE ELSE TRUE END, " &
                    "ReplicatedOn = CASE WHEN NOT IsReplicated THEN CURRENT_TIMESTAMP ELSE NULL END WHERE Id = @Id AND TenantId = @TenantId")
                    cmd.Parameters.AddWithValue("@Id", id)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    DbHelper.ExecuteNonQuery(cmd)
                End Using
                LogAudit("Update", "BestPractice", id, "Toggled replication status")
                LoadBestPractices()
        End Select
    End Sub

    Private Sub LoadBestPracticeForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM BestPractices WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfEditId.Value = id.ToString()
                txtTitle.Text = row("Title").ToString()
                txtDescription.Text = row("Description").ToString()
                txtApplicableAreas.Text = row("ApplicableAreas").ToString()
                txtSites.Text = row("Sites").ToString()
                txtTeams.Text = row("Teams").ToString()
                chkIsReplicated.Checked = CBool(row("IsReplicated"))
                txtReplicationDate.Text = If(row("ReplicatedOn") IsNot DBNull.Value, CDate(row("ReplicatedOn")).ToString("yyyy-MM-dd"), "")
                btnCancel.Visible = True
                btnSave.Text = "Update Best Practice"
            End If
        End Using
    End Sub
End Class
