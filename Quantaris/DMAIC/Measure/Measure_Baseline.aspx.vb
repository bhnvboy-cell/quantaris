Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Measure_Baseline
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadMetrics()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadMetrics()
        Using cmd As New MySqlCommand("SELECT * FROM Metrics " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptMetrics.DataSource = dt
            rptMetrics.DataBind()
            litRecordCount.Text = dt.Rows.Count & " record(s)"
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
                "UPDATE Metrics SET MetricName = @MetricName, DataType = @DataType, " &
                "Baseline = @Baseline, Target = @Target, CurrentValue = @CurrentValue, " &
                "SigmaLevel = @SigmaLevel, DPMO = @DPMO, CapabilityCp = @CapabilityCp, " &
                "CapabilityCpk = @CapabilityCpk, NormalityTestResult = @Normality " &
                "WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@MetricName", txtMetricName.Text.Trim())
                cmd.Parameters.AddWithValue("@DataType", ddlDataType.SelectedValue)
                cmd.Parameters.AddWithValue("@Baseline", ParseDecimal(txtBaseline.Text))
                cmd.Parameters.AddWithValue("@Target", ParseDecimal(txtTarget.Text))
                cmd.Parameters.AddWithValue("@CurrentValue", ParseDecimal(txtCurrent.Text))
                cmd.Parameters.AddWithValue("@SigmaLevel", ParseDecimal(txtSigmaLevel.Text))
                cmd.Parameters.AddWithValue("@DPMO", ParseDecimal(txtDPMO.Text))
                cmd.Parameters.AddWithValue("@CapabilityCp", ParseDecimal(txtCp.Text))
                cmd.Parameters.AddWithValue("@CapabilityCpk", ParseDecimal(txtCpk.Text))
                cmd.Parameters.AddWithValue("@Normality", ddlNormality.SelectedValue)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "Metric", editId, "Updated metric " & txtMetricName.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO Metrics (TenantId, ProjectId, MetricName, DataType, Baseline, Target, CurrentValue, " &
                "SigmaLevel, DPMO, CapabilityCp, CapabilityCpk, NormalityTestResult, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @MetricName, @DataType, @Baseline, @Target, @CurrentValue, " &
                "@SigmaLevel, @DPMO, @CapabilityCp, @CapabilityCpk, @Normality, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@MetricName", txtMetricName.Text.Trim())
                cmd.Parameters.AddWithValue("@DataType", ddlDataType.SelectedValue)
                cmd.Parameters.AddWithValue("@Baseline", ParseDecimal(txtBaseline.Text))
                cmd.Parameters.AddWithValue("@Target", ParseDecimal(txtTarget.Text))
                cmd.Parameters.AddWithValue("@CurrentValue", ParseDecimal(txtCurrent.Text))
                cmd.Parameters.AddWithValue("@SigmaLevel", ParseDecimal(txtSigmaLevel.Text))
                cmd.Parameters.AddWithValue("@DPMO", ParseDecimal(txtDPMO.Text))
                cmd.Parameters.AddWithValue("@CapabilityCp", ParseDecimal(txtCp.Text))
                cmd.Parameters.AddWithValue("@CapabilityCpk", ParseDecimal(txtCpk.Text))
                cmd.Parameters.AddWithValue("@Normality", ddlNormality.SelectedValue)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "Metric", _projectId, "Added metric " & txtMetricName.Text)
        End If

        ClearForm()
        LoadMetrics()
    End Sub

    Protected Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        ClearForm()
    End Sub

    Protected Sub rptMetrics_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM Metrics WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "Metric", id, "Deleted metric")
            LoadMetrics()
        ElseIf e.CommandName = "Edit" Then
            LoadMetricForEdit(id)
        End If
    End Sub

    Private Sub LoadMetricForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM Metrics WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfEditId.Value = id.ToString()
                txtMetricName.Text = row("MetricName").ToString()
                If ddlDataType.Items.FindByValue(row("DataType").ToString()) IsNot Nothing Then
                    ddlDataType.SelectedValue = row("DataType").ToString()
                End If
                txtBaseline.Text = If(row("Baseline") IsNot DBNull.Value, row("Baseline").ToString(), "")
                txtTarget.Text = If(row("Target") IsNot DBNull.Value, row("Target").ToString(), "")
                txtCurrent.Text = If(row("CurrentValue") IsNot DBNull.Value, row("CurrentValue").ToString(), "")
                txtSigmaLevel.Text = If(row("SigmaLevel") IsNot DBNull.Value, row("SigmaLevel").ToString(), "")
                txtDPMO.Text = If(row("DPMO") IsNot DBNull.Value, row("DPMO").ToString(), "")
                txtCp.Text = If(row("CapabilityCp") IsNot DBNull.Value, row("CapabilityCp").ToString(), "")
                txtCpk.Text = If(row("CapabilityCpk") IsNot DBNull.Value, row("CapabilityCpk").ToString(), "")
                If ddlNormality.Items.FindByValue(row("NormalityTestResult").ToString()) IsNot Nothing Then
                    ddlNormality.SelectedValue = row("NormalityTestResult").ToString()
                End If
                btnCancel.Visible = True
                btnSave.Text = "Update"
            End If
        End Using
    End Sub

    Private Sub ClearForm()
        txtMetricName.Text = ""
        ddlDataType.SelectedIndex = 0
        txtBaseline.Text = ""
        txtTarget.Text = ""
        txtCurrent.Text = ""
        txtSigmaLevel.Text = ""
        txtDPMO.Text = ""
        txtCp.Text = ""
        txtCpk.Text = ""
        ddlNormality.SelectedIndex = 0
        hfEditId.Value = "0"
        btnCancel.Visible = False
        btnSave.Text = "<i class='bi bi-check-lg'></i> Save"
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 6, "In Progress")
    End Sub
End Class
