Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Define_Define_Charter
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadCharter()
            LoadStakeholders()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadCharter()
        Using cmd As New MySqlCommand("SELECT * FROM ProjectCharters " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfCharterId.Value = row("Id").ToString()
                txtProblemStatement.Text = row("ProblemStatement").ToString()
                txtGoalStatement.Text = row("GoalStatement").ToString()
                txtBusinessCase.Text = row("BusinessCase").ToString()
                txtElevatorSpeech.Text = row("ElevatorSpeech").ToString()
                txtScopeIncluded.Text = row("ScopeIncluded").ToString()
                txtScopeExcluded.Text = row("ScopeExcluded").ToString()
                txtTimeline.Text = row("Timeline").ToString()
            End If
        End Using
    End Sub

    Private Sub LoadStakeholders()
        Using cmd As New MySqlCommand("SELECT * FROM Stakeholders " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            rptStakeholders.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptStakeholders.DataBind()
        End Using
    End Sub

    Protected Sub btnSaveCharter_Click(sender As Object, e As EventArgs) Handles btnSaveCharter.Click
        Dim charterId As Integer = CInt(hfCharterId.Value)

        If charterId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE ProjectCharters SET ProblemStatement = @ProblemStatement, GoalStatement = @GoalStatement, " &
                "BusinessCase = @BusinessCase, ScopeIncluded = @ScopeIncluded, ScopeExcluded = @ScopeExcluded, " &
                "Timeline = @Timeline, ElevatorSpeech = @ElevatorSpeech " &
                "WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", charterId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProblemStatement", txtProblemStatement.Text.Trim())
                cmd.Parameters.AddWithValue("@GoalStatement", txtGoalStatement.Text.Trim())
                cmd.Parameters.AddWithValue("@BusinessCase", txtBusinessCase.Text.Trim())
                cmd.Parameters.AddWithValue("@ScopeIncluded", txtScopeIncluded.Text.Trim())
                cmd.Parameters.AddWithValue("@ScopeExcluded", txtScopeExcluded.Text.Trim())
                cmd.Parameters.AddWithValue("@Timeline", txtTimeline.Text.Trim())
                cmd.Parameters.AddWithValue("@ElevatorSpeech", txtElevatorSpeech.Text.Trim())
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "Charter", charterId, "Updated project charter")
        Else
            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()
                Using cmd As New MySqlCommand(
                    "INSERT INTO ProjectCharters (TenantId, ProjectId, ProblemStatement, GoalStatement, BusinessCase, " &
                    "ScopeIncluded, ScopeExcluded, Timeline, ElevatorSpeech, CreatedOn, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @ProblemStatement, @GoalStatement, @BusinessCase, " &
                    "@ScopeIncluded, @ScopeExcluded, @Timeline, @ElevatorSpeech, CURRENT_TIMESTAMP, @CreatedByUserId)", conn)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                    cmd.Parameters.AddWithValue("@ProblemStatement", txtProblemStatement.Text.Trim())
                    cmd.Parameters.AddWithValue("@GoalStatement", txtGoalStatement.Text.Trim())
                    cmd.Parameters.AddWithValue("@BusinessCase", txtBusinessCase.Text.Trim())
                    cmd.Parameters.AddWithValue("@ScopeIncluded", txtScopeIncluded.Text.Trim())
                    cmd.Parameters.AddWithValue("@ScopeExcluded", txtScopeExcluded.Text.Trim())
                    cmd.Parameters.AddWithValue("@Timeline", txtTimeline.Text.Trim())
                    cmd.Parameters.AddWithValue("@ElevatorSpeech", txtElevatorSpeech.Text.Trim())
                    cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "SELECT LAST_INSERT_ID()"
                    cmd.Parameters.Clear()
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing Then hfCharterId.Value = CInt(result).ToString()
                End Using
            End Using
            LogAudit("Create", "Charter", _projectId, "Created project charter")
        End If
    End Sub

    Protected Sub btnAddStakeholder_Click(sender As Object, e As EventArgs) Handles btnAddStakeholder.Click
        If String.IsNullOrWhiteSpace(txtStakeholderName.Text) Then Return

        Dim editId As Integer = CInt(hfStakeholderEditId.Value)

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE Stakeholders SET Name = @Name, Role = @Role, Interest = @Interest, Influence = @Influence, " &
                "ARMI_Type = @ARMI_Type, RACI_Type = @RACI_Type WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@Name", txtStakeholderName.Text.Trim())
                cmd.Parameters.AddWithValue("@Role", txtStakeholderRole.Text.Trim())
                cmd.Parameters.AddWithValue("@Interest", ddlInterest.SelectedValue)
                cmd.Parameters.AddWithValue("@Influence", ddlInfluence.SelectedValue)
                cmd.Parameters.AddWithValue("@ARMI_Type", ddlARMI.SelectedValue)
                cmd.Parameters.AddWithValue("@RACI_Type", ddlRACI.SelectedValue)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "Stakeholder", editId, "Updated stakeholder " & txtStakeholderName.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO Stakeholders (TenantId, ProjectId, Name, Role, Interest, Influence, ARMI_Type, RACI_Type, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @Name, @Role, @Interest, @Influence, @ARMI_Type, @RACI_Type, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@Name", txtStakeholderName.Text.Trim())
                cmd.Parameters.AddWithValue("@Role", txtStakeholderRole.Text.Trim())
                cmd.Parameters.AddWithValue("@Interest", ddlInterest.SelectedValue)
                cmd.Parameters.AddWithValue("@Influence", ddlInfluence.SelectedValue)
                cmd.Parameters.AddWithValue("@ARMI_Type", ddlARMI.SelectedValue)
                cmd.Parameters.AddWithValue("@RACI_Type", ddlRACI.SelectedValue)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "Stakeholder", _projectId, "Added stakeholder " & txtStakeholderName.Text)
        End If

        ClearStakeholderForm()
        LoadStakeholders()
    End Sub

    Protected Sub rptStakeholders_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM Stakeholders WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "Stakeholder", id, "Deleted stakeholder")
            LoadStakeholders()
        ElseIf e.CommandName = "Edit" Then
            LoadStakeholderForEdit(id)
        End If
    End Sub

    Private Sub LoadStakeholderForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM Stakeholders WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfStakeholderEditId.Value = id.ToString()
                txtStakeholderName.Text = row("Name").ToString()
                txtStakeholderRole.Text = row("Role").ToString()
                If ddlInterest.Items.FindByValue(row("Interest").ToString()) IsNot Nothing Then
                    ddlInterest.SelectedValue = row("Interest").ToString()
                End If
                If ddlInfluence.Items.FindByValue(row("Influence").ToString()) IsNot Nothing Then
                    ddlInfluence.SelectedValue = row("Influence").ToString()
                End If
                If ddlARMI.Items.FindByValue(row("ARMI_Type").ToString()) IsNot Nothing Then
                    ddlARMI.SelectedValue = row("ARMI_Type").ToString()
                End If
                If ddlRACI.Items.FindByValue(row("RACI_Type").ToString()) IsNot Nothing Then
                    ddlRACI.SelectedValue = row("RACI_Type").ToString()
                End If
                btnAddStakeholder.Text = "Update"
            End If
        End Using
    End Sub

    Private Sub ClearStakeholderForm()
        txtStakeholderName.Text = ""
        txtStakeholderRole.Text = ""
        ddlInterest.SelectedIndex = 0
        ddlInfluence.SelectedIndex = 0
        ddlARMI.SelectedIndex = 0
        ddlRACI.SelectedIndex = 0
        hfStakeholderEditId.Value = "0"
        btnAddStakeholder.Text = "<i class='bi bi-plus-lg'></i>"
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 2, "In Progress")
    End Sub
End Class
