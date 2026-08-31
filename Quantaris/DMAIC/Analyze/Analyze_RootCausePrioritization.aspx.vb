Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Analyze_Analyze_RootCausePrioritization
    Inherits BasePage

    Private _projectId As Integer

    Private Const PARETO_CATEGORY As String = "Pareto"
    Private Const MULTIVOTING_CATEGORY As String = "MultiVoting"
    Private Const MATRIX_CATEGORY As String = "ControlImpact"

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadPareto()
            LoadMultiVoting()
            LoadMatrix()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadPareto()
        Using cmd As New MySqlCommand(
            "SELECT * FROM RootCauseAnalysis " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND Category = @Category ORDER BY Id")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@Category", PARETO_CATEGORY)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)

            Dim totalFreq As Double = 0
            For Each row As DataRow In dt.Rows
                totalFreq += CDbl(Val(row("Sequence").ToString()))
            Next

            Dim cumul As Double = 0
            For Each row As DataRow In dt.Rows
                Dim freq As Double = CDbl(Val(row("Sequence").ToString()))
                Dim pct As Double = If(totalFreq > 0, (freq / totalFreq) * 100, 0)
                cumul += pct
                row("Effect") = cumul.ToString("F2")
            Next

            rptPareto.DataSource = dt
            rptPareto.DataBind()
            litParetoCount.Text = dt.Rows.Count & " cause(s)"
        End Using
    End Sub

    Private Sub LoadMultiVoting()
        Using cmd As New MySqlCommand(
            "SELECT Cause FROM RootCauseAnalysis " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND Category = @Category ORDER BY CreatedOn DESC LIMIT 1")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@Category", MULTIVOTING_CATEGORY)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                txtMultiVoting.Text = dt.Rows(0)("Cause").ToString()
            End If
        End Using
    End Sub

    Private Sub LoadMatrix()
        Using cmd As New MySqlCommand(
            "SELECT * FROM RootCauseAnalysis " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND Category = @Category ORDER BY Id")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@Category", MATRIX_CATEGORY)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptMatrix.DataSource = dt
            rptMatrix.DataBind()
            litMatrixCount.Text = dt.Rows.Count & " entry(s)"
        End Using
    End Sub

    Protected Sub btnAddPareto_Click(sender As Object, e As EventArgs) Handles btnAddPareto.Click
        If String.IsNullOrWhiteSpace(txtParetoCause.Text) Then Return
        Dim freq As Integer = CInt(Val(txtParetoFrequency.Text))
        Dim pct As Double = CDbl(Val(txtParetoPercent.Text))

        Using cmd As New MySqlCommand(
            "INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Category, Cause, Effect, Sequence, IsFishbone, CreatedByUserId, CreatedOn) " &
            "VALUES (@TenantId, @ProjectId, @Category, @Cause, @Effect, @Sequence, FALSE, @CreatedByUserId, CURRENT_TIMESTAMP)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@Category", PARETO_CATEGORY)
            cmd.Parameters.AddWithValue("@Cause", txtParetoCause.Text.Trim())
            cmd.Parameters.AddWithValue("@Effect", pct.ToString("F2"))
            cmd.Parameters.AddWithValue("@Sequence", freq)
            cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Create", "ParetoEntry", _projectId, "Pareto chart entry saved")
        LoadPareto()
    End Sub

    Protected Sub rptPareto_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "Delete" Then
            Dim id As Integer = CInt(e.CommandArgument)
            Using cmd As New MySqlCommand("DELETE FROM RootCauseAnalysis WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "Pareto", id, "Deleted Pareto entry")
            LoadPareto()
        End If
    End Sub

    Protected Sub btnSaveMultiVoting_Click(sender As Object, e As EventArgs) Handles btnSaveMultiVoting.Click
        Dim exists As Boolean
        Using chkCmd As New MySqlCommand("SELECT COUNT(1) FROM RootCauseAnalysis WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND Category = @Category")
            chkCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            chkCmd.Parameters.AddWithValue("@ProjectId", _projectId)
            chkCmd.Parameters.AddWithValue("@Category", MULTIVOTING_CATEGORY)
            exists = CInt(DbHelper.ExecuteScalar(chkCmd)) > 0
        End Using
        If exists Then
            Using updCmd As New MySqlCommand("UPDATE RootCauseAnalysis SET Cause = @Cause, CreatedOn = CURRENT_TIMESTAMP WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND Category = @Category")
                updCmd.Parameters.AddWithValue("@Cause", txtMultiVoting.Text.Trim())
                updCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                updCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                updCmd.Parameters.AddWithValue("@Category", MULTIVOTING_CATEGORY)
                DbHelper.ExecuteNonQuery(updCmd)
            End Using
        Else
            Using insCmd As New MySqlCommand("INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Category, Cause, IsFishbone, CreatedByUserId, CreatedOn) VALUES (@TenantId, @ProjectId, @Category, @Cause, FALSE, @CreatedByUserId, CURRENT_TIMESTAMP)")
                insCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                insCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                insCmd.Parameters.AddWithValue("@Category", MULTIVOTING_CATEGORY)
                insCmd.Parameters.AddWithValue("@Cause", txtMultiVoting.Text.Trim())
                insCmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(insCmd)
            End Using
        End If
        LogAudit("Save", "MultiVoting", _projectId, "Saved multi-voting results")
    End Sub

    Protected Sub btnAddMatrix_Click(sender As Object, e As EventArgs) Handles btnAddMatrix.Click
        If String.IsNullOrWhiteSpace(txtMatrixCause.Text) Then Return
        Dim priority As Integer = CInt(Val(txtMatrixPriority.Text))
        Dim control As String = ddlMatrixControl.SelectedValue
        Dim impact As String = ddlMatrixImpact.SelectedValue
        Dim effect As String = control & "|" & impact

        Using cmd As New MySqlCommand(
            "INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Category, Cause, Effect, Sequence, IsFishbone, CreatedByUserId, CreatedOn) " &
            "VALUES (@TenantId, @ProjectId, @Category, @Cause, @Effect, @Sequence, FALSE, @CreatedByUserId, CURRENT_TIMESTAMP)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@Category", MATRIX_CATEGORY)
            cmd.Parameters.AddWithValue("@Cause", txtMatrixCause.Text.Trim())
            cmd.Parameters.AddWithValue("@Effect", effect)
            cmd.Parameters.AddWithValue("@Sequence", priority)
            cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Create", "ControlImpactMatrix", _projectId, "Added matrix entry: " & txtMatrixCause.Text)
        txtMatrixCause.Text = ""
        txtMatrixPriority.Text = ""
        ddlMatrixControl.SelectedIndex = 0
        ddlMatrixImpact.SelectedIndex = 0
        LoadMatrix()
    End Sub

    Protected Sub rptMatrix_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "Delete" Then
            Dim id As Integer = CInt(e.CommandArgument)
            Using cmd As New MySqlCommand("DELETE FROM RootCauseAnalysis WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "ControlImpactMatrix", id, "Deleted matrix entry")
            LoadMatrix()
        End If
    End Sub

    Public Function GetControlBadge(control As Object) As String
        Dim c As String = control.ToString()
        Select Case c.ToLower()
            Case "high" : Return "success"
            Case "medium" : Return "warning"
            Case "low" : Return "danger"
            Case Else : Return "secondary"
        End Select
    End Function

    Public Function GetImpactBadge(impact As Object) As String
        Dim i As String = impact.ToString()
        Select Case i.ToLower()
            Case "high" : Return "danger"
            Case "medium" : Return "warning"
            Case "low" : Return "secondary"
            Case Else : Return "secondary"
        End Select
    End Function

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 8, "In Progress")
    End Sub
End Class
