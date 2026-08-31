Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Improve_Improve_PilotPlan
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadPilots()
            LoadFMEA()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadPilots()
        Using cmd As New MySqlCommand(
            "SELECT * FROM PilotPlans WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptPilots.DataSource = dt
            rptPilots.DataBind()
            litPilotCount.Text = dt.Rows.Count & " pilot(s)"
        End Using
    End Sub

    Private Sub LoadFMEA()
        Using cmd As New MySqlCommand(
            "SELECT * FROM FMEA_Records WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY RPN DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptFMEA.DataSource = dt
            rptFMEA.DataBind()
        End Using
    End Sub

    Public Function GetStatusBadge(status As Object) As String
        Dim s As String = status.ToString()
        Select Case s.ToLower()
            Case "planned" : Return "secondary"
            Case "inprogress" : Return "warning"
            Case "complete" : Return "success"
            Case Else : Return "secondary"
        End Select
    End Function

    Private Function ParseDate(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then Return DBNull.Value
        Dim result As Date
        If Date.TryParse(value.Trim(), result) Then Return result
        Return DBNull.Value
    End Function

    Protected Sub btnSavePilot_Click(sender As Object, e As EventArgs) Handles btnSavePilot.Click
        If Not Page.IsValid Then Return
        Dim editId As Integer = CInt(hfEditId.Value)

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE PilotPlans SET PlanName = @PlanName, Scope = @Scope, Duration = @Duration, " &
                "StartDate = @StartDate, EndDate = @EndDate, Metrics = @Metrics, " &
                "PokaYokeIdeas = @PokaYokeIdeas, ImplementationPlan = @ImplementationPlan, " &
                "Status = @Status WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@PlanName", txtPlanName.Text.Trim())
                cmd.Parameters.AddWithValue("@Scope", If(String.IsNullOrWhiteSpace(txtScope.Text), CObj(DBNull.Value), txtScope.Text.Trim()))
                cmd.Parameters.AddWithValue("@Duration", If(String.IsNullOrWhiteSpace(txtDuration.Text), CObj(DBNull.Value), txtDuration.Text.Trim()))
                cmd.Parameters.AddWithValue("@StartDate", ParseDate(txtStartDate.Text))
                cmd.Parameters.AddWithValue("@EndDate", ParseDate(txtEndDate.Text))
                cmd.Parameters.AddWithValue("@Metrics", If(String.IsNullOrWhiteSpace(txtMetrics.Text), CObj(DBNull.Value), txtMetrics.Text.Trim()))
                cmd.Parameters.AddWithValue("@PokaYokeIdeas", If(String.IsNullOrWhiteSpace(txtPokaYoke.Text), CObj(DBNull.Value), txtPokaYoke.Text.Trim()))
                cmd.Parameters.AddWithValue("@ImplementationPlan", If(String.IsNullOrWhiteSpace(txtImplementationPlan.Text), CObj(DBNull.Value), txtImplementationPlan.Text.Trim()))
                cmd.Parameters.AddWithValue("@Status", ddlPilotStatus.SelectedValue)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "PilotPlan", editId, "Updated pilot plan: " & txtPlanName.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO PilotPlans (TenantId, ProjectId, PlanName, Scope, Duration, StartDate, EndDate, " &
                "Metrics, PokaYokeIdeas, ImplementationPlan, Status, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @PlanName, @Scope, @Duration, @StartDate, @EndDate, " &
                "@Metrics, @PokaYokeIdeas, @ImplementationPlan, @Status, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@PlanName", txtPlanName.Text.Trim())
                cmd.Parameters.AddWithValue("@Scope", If(String.IsNullOrWhiteSpace(txtScope.Text), CObj(DBNull.Value), txtScope.Text.Trim()))
                cmd.Parameters.AddWithValue("@Duration", If(String.IsNullOrWhiteSpace(txtDuration.Text), CObj(DBNull.Value), txtDuration.Text.Trim()))
                cmd.Parameters.AddWithValue("@StartDate", ParseDate(txtStartDate.Text))
                cmd.Parameters.AddWithValue("@EndDate", ParseDate(txtEndDate.Text))
                cmd.Parameters.AddWithValue("@Metrics", If(String.IsNullOrWhiteSpace(txtMetrics.Text), CObj(DBNull.Value), txtMetrics.Text.Trim()))
                cmd.Parameters.AddWithValue("@PokaYokeIdeas", If(String.IsNullOrWhiteSpace(txtPokaYoke.Text), CObj(DBNull.Value), txtPokaYoke.Text.Trim()))
                cmd.Parameters.AddWithValue("@ImplementationPlan", If(String.IsNullOrWhiteSpace(txtImplementationPlan.Text), CObj(DBNull.Value), txtImplementationPlan.Text.Trim()))
                cmd.Parameters.AddWithValue("@Status", ddlPilotStatus.SelectedValue)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "PilotPlan", _projectId, "Created pilot plan: " & txtPlanName.Text)
        End If

        ClearPilotForm()
        LoadPilots()
    End Sub

    Protected Sub btnCancelPilot_Click(sender As Object, e As EventArgs) Handles btnCancelPilot.Click
        ClearPilotForm()
    End Sub

    Protected Sub rptPilots_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM PilotPlans WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "PilotPlan", id, "Deleted pilot plan")
            LoadPilots()
        ElseIf e.CommandName = "Edit" Then
            LoadPilotForEdit(id)
        End If
    End Sub

    Private Sub LoadPilotForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM PilotPlans WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfEditId.Value = id.ToString()
                txtPlanName.Text = row("PlanName").ToString()
                txtScope.Text = row("Scope").ToString()
                txtDuration.Text = row("Duration").ToString()
                txtStartDate.Text = If(row("StartDate") IsNot DBNull.Value, CDate(row("StartDate")).ToString("yyyy-MM-dd"), "")
                txtEndDate.Text = If(row("EndDate") IsNot DBNull.Value, CDate(row("EndDate")).ToString("yyyy-MM-dd"), "")
                txtMetrics.Text = row("Metrics").ToString()
                txtPokaYoke.Text = row("PokaYokeIdeas").ToString()
                txtImplementationPlan.Text = row("ImplementationPlan").ToString()
                If ddlPilotStatus.Items.FindByValue(row("Status").ToString()) IsNot Nothing Then
                    ddlPilotStatus.SelectedValue = row("Status").ToString()
                End If
                btnCancelPilot.Visible = True
                btnSavePilot.Text = "<i class='bi bi-check-lg me-1'></i>Update Pilot Plan"
            End If
        End Using
    End Sub

    Private Sub ClearPilotForm()
        txtPlanName.Text = ""
        txtScope.Text = ""
        txtDuration.Text = ""
        txtStartDate.Text = ""
        txtEndDate.Text = ""
        txtMetrics.Text = ""
        txtPokaYoke.Text = ""
        txtImplementationPlan.Text = ""
        ddlPilotStatus.SelectedIndex = 0
        hfEditId.Value = "0"
        btnCancelPilot.Visible = False
        btnSavePilot.Text = "<i class='bi bi-save me-1'></i>Save Pilot Plan"
    End Sub

    Protected Sub btnAddFMEA_Click(sender As Object, e As EventArgs) Handles btnAddFMEA.Click
        If String.IsNullOrWhiteSpace(txtFMEAStep.Text) OrElse String.IsNullOrWhiteSpace(txtFMEAFailure.Text) Then Return

        Dim severity As Integer = CInt(Val(txtFMEASeverity.Text))
        Dim occurrence As Integer = CInt(Val(txtFMEAOccurrence.Text))
        Dim detection As Integer = CInt(Val(txtFMEADetection.Text))
        If severity < 1 Then severity = 1
        If occurrence < 1 Then occurrence = 1
        If detection < 1 Then detection = 1
        If severity > 10 Then severity = 10
        If occurrence > 10 Then occurrence = 10
        If detection > 10 Then detection = 10

        Using cmd As New MySqlCommand(
            "INSERT INTO FMEA_Records (TenantId, ProjectId, ProcessStep, FailureMode, EffectOfFailure, CauseOfFailure, " &
            "Severity, Occurrence, Detection, CreatedOn, CreatedByUserId) " &
            "VALUES (@TenantId, @ProjectId, @ProcessStep, @FailureMode, @EffectOfFailure, @CauseOfFailure, " &
            "@Severity, @Occurrence, @Detection, CURRENT_TIMESTAMP, @CreatedByUserId)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@ProcessStep", txtFMEAStep.Text.Trim())
            cmd.Parameters.AddWithValue("@FailureMode", txtFMEAFailure.Text.Trim())
            cmd.Parameters.AddWithValue("@EffectOfFailure", If(String.IsNullOrWhiteSpace(txtFMEAEffect.Text), CObj(DBNull.Value), txtFMEAEffect.Text.Trim()))
            cmd.Parameters.AddWithValue("@CauseOfFailure", If(String.IsNullOrWhiteSpace(txtFMEACause.Text), CObj(DBNull.Value), txtFMEACause.Text.Trim()))
            cmd.Parameters.AddWithValue("@Severity", severity)
            cmd.Parameters.AddWithValue("@Occurrence", occurrence)
            cmd.Parameters.AddWithValue("@Detection", detection)
            cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Create", "FMEA", _projectId, "Added FMEA record: " & txtFMEAStep.Text)

        txtFMEAStep.Text = ""
        txtFMEAFailure.Text = ""
        txtFMEAEffect.Text = ""
        txtFMEACause.Text = ""
        txtFMEASeverity.Text = ""
        txtFMEAOccurrence.Text = ""
        txtFMEADetection.Text = ""
        LoadFMEA()
    End Sub

    Protected Sub rptFMEA_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM FMEA_Records WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "FMEA", id, "Deleted FMEA record")
            LoadFMEA()
        End If
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 11, "In Progress")
    End Sub
End Class
