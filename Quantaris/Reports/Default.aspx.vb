Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Reports_Default
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt")
        If Request.QueryString("export") = "csv" Then
            ExportCSV()
            Return
        End If
        If Not IsPostBack Then
            LoadDMAICCompletion()
            LoadFinancialImpact()
            LoadHighRPN()
            LoadBestPractices()
        End If
    End Sub

    Private Sub ExportCSV()
        Dim sb As New System.Text.StringBuilder()
        sb.AppendLine("Project Code,Project Name,Phase,Define%,Measure%,Analyze%,Improve%,Control%,Overall%")

        Using cmd As New MySqlCommand("SELECT ProjectCode, ProjectName, Phase, " &
            "ROUND(CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Define') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Define' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Define') ELSE 0 END, 0) AS DefinePct, " &
            "ROUND(CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Measure') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Measure' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Measure') ELSE 0 END, 0) AS MeasurePct, " &
            "ROUND(CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Analyze') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Analyze' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Analyze') ELSE 0 END, 0) AS AnalyzePct, " &
            "ROUND(CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Improve') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Improve' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Improve') ELSE 0 END, 0) AS ImprovePct, " &
            "ROUND(CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Control') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Control' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Control') ELSE 0 END, 0) AS ControlPct, " &
            "ROUND(CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id) > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id) ELSE 0 END, 0) AS OverallPct " &
            "FROM QuantarisProjects p WHERE p.TenantId = @TenantId ORDER BY p.CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            For Each row As DataRow In dt.Rows
                sb.AppendLine(String.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                    row("ProjectCode"), row("ProjectName"), row("Phase"),
                    row("DefinePct"), row("MeasurePct"), row("AnalyzePct"),
                    row("ImprovePct"), row("ControlPct"), row("OverallPct")))
            Next
        End Using

        Response.Clear()
        Response.ContentType = "text/csv"
        Response.AddHeader("Content-Disposition", "attachment;filename=Reports_" & DateTime.Now.ToString("yyyyMMdd_HHmmss") & ".csv")
        Response.Write(sb.ToString())
        Response.End()
    End Sub

    Private Sub LoadDMAICCompletion()
        Dim sql As String = "SELECT p.Id, p.ProjectCode, p.ProjectName, p.Phase, " &
            "CAST(ROUND( " &
            "CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Define') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Define' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Define') " &
            "ELSE 0 END, 0) AS SIGNED) AS DefinePct, " &
            "CAST(ROUND( " &
            "CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Measure') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Measure' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Measure') " &
            "ELSE 0 END, 0) AS SIGNED) AS MeasurePct, " &
            "CAST(ROUND( " &
            "CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Analyze') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Analyze' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Analyze') " &
            "ELSE 0 END, 0) AS SIGNED) AS AnalyzePct, " &
            "CAST(ROUND( " &
            "CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Improve') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Improve' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Improve') " &
            "ELSE 0 END, 0) AS SIGNED) AS ImprovePct, " &
            "CAST(ROUND( " &
            "CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Control') > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Control' AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Phase = 'Control') " &
            "ELSE 0 END, 0) AS SIGNED) AS ControlPct, " &
            "CAST(ROUND( " &
            "CASE WHEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id) > 0 " &
            "THEN (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id AND s.Status = 'Complete') * 100.0 " &
            "/ (SELECT COUNT(*) FROM DMAICSteps s WHERE s.ProjectId = p.Id) " &
            "ELSE 0 END, 0) AS SIGNED) AS OverallPct " &
            "FROM QuantarisProjects p " &
            "WHERE p.TenantId = @TenantId " &
            "ORDER BY p.CreatedOn DESC"
        Using cmd As New MySqlCommand(sql)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            rptCompletion.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptCompletion.DataBind()
        End Using
    End Sub

    Private Sub LoadFinancialImpact()
        Using cmd As New MySqlCommand(
            "SELECT ProjectCode, ProjectName, FinancialImpact, Status FROM QuantarisProjects " &
            "WHERE TenantId = @TenantId AND FinancialImpact IS NOT NULL ORDER BY FinancialImpact DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptFinancial.DataSource = dt
            rptFinancial.DataBind()
            Dim total = dt.Compute("SUM(FinancialImpact)", "")
            litTotalImpact.Text = If(total IsNot DBNull.Value, String.Format("{0:N2}", total), "0.00")
        End Using
    End Sub

    Private Sub LoadHighRPN()
        Using cmd As New MySqlCommand(
            "SELECT f.*, p.ProjectCode, p.ProjectName FROM FMEA_Records f " &
            "INNER JOIN QuantarisProjects p ON f.ProjectId = p.Id " &
            "WHERE f.TenantId = @TenantId AND f.RPN >= 100 ORDER BY f.RPN DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            rptRPN.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptRPN.DataBind()
        End Using
    End Sub

    Private Sub LoadBestPractices()
        Using cmd As New MySqlCommand(
            "SELECT b.*, p.ProjectCode, p.ProjectName, t.TenantCode, t.TenantName " &
            "FROM BestPractices b " &
            "INNER JOIN QuantarisProjects p ON b.ProjectId = p.Id " &
            "INNER JOIN Tenants t ON b.TenantId = t.Id " &
            "WHERE b.TenantId = @TenantId " &
            "ORDER BY b.IsReplicated, b.CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            rptBestPractices.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptBestPractices.DataBind()
        End Using
    End Sub

    Protected Function GetStatusBadge(status As Object) As String
        If status Is Nothing OrElse status Is DBNull.Value Then Return "bg-info"
        Select Case status.ToString().ToLower()
            Case "complete" : Return "bg-success"
            Case "inprogress" : Return "bg-warning text-dark"
            Case "onhold" : Return "bg-secondary"
            Case Else : Return "bg-info"
        End Select
    End Function
End Class
