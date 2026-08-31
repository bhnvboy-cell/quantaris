Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Improve_Improve_ImpactValidation
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadCostBenefit()
            LoadMetrics()
            LoadCapability()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadCostBenefit()
        Using cmd As New MySqlCommand(
            "SELECT * FROM PilotPlans WHERE TenantId = @TenantId AND ProjectId = @ProjectId " &
            "AND Metrics IS NOT NULL ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfCostBenefitId.Value = row("Id").ToString()
                Dim json As String = row("Metrics").ToString()
                If Not String.IsNullOrEmpty(json) Then
                    Try
                        Dim parts As String() = json.Split("|"c)
                        If parts.Length >= 3 Then
                            txtImplCost.Text = parts(0)
                            txtAnnualSavings.Text = parts(1)
                            txtPaybackPeriod.Text = parts(2)
                            If parts.Length >= 4 Then
                                txtNonFinancialBenefits.Text = parts(3)
                            End If
                        End If
                    Catch
                    End Try
                End If
            End If
        End Using
    End Sub

    Private Sub LoadMetrics()
        Using cmd As New MySqlCommand(
            "SELECT * FROM Metrics WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptMetrics.DataSource = dt
            rptMetrics.DataBind()
        End Using
    End Sub

    Private Sub LoadCapability()
        Using cmd As New MySqlCommand(
            "SELECT * FROM Metrics WHERE TenantId = @TenantId AND ProjectId = @ProjectId " &
            "AND SigmaLevel IS NOT NULL ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                txtNewSigma.Text = If(row("SigmaLevel") IsNot DBNull.Value, row("SigmaLevel").ToString(), "")
                txtNewDPMO.Text = If(row("DPMO") IsNot DBNull.Value, row("DPMO").ToString(), "")
                txtNewCp.Text = If(row("CapabilityCp") IsNot DBNull.Value, row("CapabilityCp").ToString(), "")
                txtNewCpk.Text = If(row("CapabilityCpk") IsNot DBNull.Value, row("CapabilityCpk").ToString(), "")
                If ddlNormality.Items.FindByValue(row("NormalityTestResult").ToString()) IsNot Nothing Then
                    ddlNormality.SelectedValue = row("NormalityTestResult").ToString()
                End If
            End If
        End Using
    End Sub

    Private Function ParseDecimal(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then Return DBNull.Value
        Dim result As Decimal
        If Decimal.TryParse(value.Trim(), result) Then Return result
        Return DBNull.Value
    End Function

    Protected Sub btnSaveCostBenefit_Click(sender As Object, e As EventArgs) Handles btnSaveCostBenefit.Click
        If Not Page.IsValid Then Return
        Dim implCost As String = txtImplCost.Text.Trim()
        Dim savings As String = txtAnnualSavings.Text.Trim()

        Dim payback As String = ""
        Dim costVal As Decimal = 0
        Dim savingsVal As Decimal = 0
        If Decimal.TryParse(implCost, costVal) AndAlso Decimal.TryParse(savings, savingsVal) AndAlso savingsVal > 0 Then
            Dim months As Decimal = (costVal / savingsVal) * 12
            payback = months.ToString("F1")
            txtPaybackPeriod.Text = payback
        Else
            payback = "N/A"
            txtPaybackPeriod.Text = payback
        End If

        Dim json As String = String.Join("|", {implCost, savings, payback, txtNonFinancialBenefits.Text.Trim()})
        Dim editId As Integer = CInt(hfCostBenefitId.Value)

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE PilotPlans SET Metrics = @Metrics WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@Metrics", json)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "CostBenefit", editId, "Updated cost-benefit analysis")
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO PilotPlans (TenantId, ProjectId, PlanName, Metrics, Status, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @PlanName, @Metrics, @Status, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@PlanName", "Cost-Benefit Analysis")
                cmd.Parameters.AddWithValue("@Metrics", json)
                cmd.Parameters.AddWithValue("@Status", "Planned")
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "CostBenefit", _projectId, "Saved cost-benefit analysis")
            LoadCostBenefit()
        End If
    End Sub

    Protected Sub btnSaveMetrics_Click(sender As Object, e As EventArgs) Handles btnSaveMetrics.Click
        For Each item As RepeaterItem In rptMetrics.Items
            Dim txtAfter As TextBox = DirectCast(item.FindControl("txtAfterValue"), TextBox)
            Dim hfId As HiddenField = DirectCast(item.FindControl("hfMetricId"), HiddenField)
            If txtAfter IsNot Nothing AndAlso hfId IsNot Nothing Then
                Dim metricId As Integer = CInt(hfId.Value)
                Dim afterVal As Object = ParseDecimal(txtAfter.Text)

                Using cmd As New MySqlCommand(
                    "UPDATE Metrics SET CurrentValue = @CurrentValue WHERE Id = @Id AND TenantId = @TenantId")
                    cmd.Parameters.AddWithValue("@Id", metricId)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@CurrentValue", afterVal)
                    DbHelper.ExecuteNonQuery(cmd)
                End Using

                If afterVal IsNot DBNull.Value Then
                    Using cmd As New MySqlCommand(
                        "INSERT INTO MetricsHistory (TenantId, ProjectId, MetricId, MetricName, Value, RecordedOn, CreatedOn, CreatedByUserId) " &
                        "SELECT @TenantId, @ProjectId, @MetricId, MetricName, @Value, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @CreatedByUserId " &
                        "FROM Metrics WHERE Id = @MetricId")
                        cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                        cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                        cmd.Parameters.AddWithValue("@MetricId", metricId)
                        cmd.Parameters.AddWithValue("@Value", afterVal)
                        cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                        DbHelper.ExecuteNonQuery(cmd)
                    End Using
                End If
            End If
        Next
        LogAudit("Update", "MetricsAfter", _projectId, "Updated after-values for metrics")
        LoadMetrics()
    End Sub

    Protected Sub rptMetrics_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
    End Sub

    Protected Sub btnSaveCapability_Click(sender As Object, e As EventArgs) Handles btnSaveCapability.Click
        Using cmd As New MySqlCommand(
            "UPDATE Metrics SET SigmaLevel = @SigmaLevel, DPMO = @DPMO, " &
            "CapabilityCp = @CapabilityCp, CapabilityCpk = @CapabilityCpk, " &
            "NormalityTestResult = @Normality " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND SigmaLevel IS NOT NULL")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@SigmaLevel", ParseDecimal(txtNewSigma.Text))
            cmd.Parameters.AddWithValue("@DPMO", ParseDecimal(txtNewDPMO.Text))
            cmd.Parameters.AddWithValue("@CapabilityCp", ParseDecimal(txtNewCp.Text))
            cmd.Parameters.AddWithValue("@CapabilityCpk", ParseDecimal(txtNewCpk.Text))
            cmd.Parameters.AddWithValue("@Normality", ddlNormality.SelectedValue)
            DbHelper.ExecuteNonQuery(cmd)
        End Using

        Using cmd As New MySqlCommand(
            "INSERT INTO MetricsHistory (TenantId, ProjectId, MetricId, MetricName, Value, RecordedOn, CreatedOn, CreatedByUserId) " &
            "SELECT @TenantId, @ProjectId, Id, MetricName, SigmaLevel, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP, @CreatedByUserId " &
            "FROM Metrics WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND SigmaLevel IS NOT NULL LIMIT 1")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using

        LogAudit("Update", "Capability", _projectId, "Updated process capability after improvement")
        LoadCapability()
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 12, "In Progress")
    End Sub
End Class
