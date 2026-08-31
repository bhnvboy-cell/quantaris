Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Measure_PlanCollectData
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadPlans()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadPlans()
        Using cmd As New MySqlCommand("SELECT * FROM MeasurementPlans " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptPlans.DataSource = dt
            rptPlans.DataBind()
            litRecordCount.Text = dt.Rows.Count & " record(s)"
        End Using
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not Page.IsValid Then Return
        Dim editId As Integer = CInt(hfEditId.Value)

        Dim sampleSize As Object
        Dim s As String = txtSampleSize.Text.Trim()
        If String.IsNullOrEmpty(s) Then
            sampleSize = DBNull.Value
        Else
            Dim ss As Integer
            If Integer.TryParse(s, ss) Then
                sampleSize = ss
            Else
                sampleSize = DBNull.Value
            End If
        End If

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE MeasurementPlans SET MetricName = @MetricName, OperationalDefinition = @OpDefinition, " &
                "DataSource = @DataSource, SamplingPlan = @SamplingPlan, SampleSize = @SampleSize, " &
                "Frequency = @Frequency, Who = @Who, MSA_Type = @MSAType, MSA_Result = @MSAResult, MSA_Acceptable = @MSAAcceptable " &
                "WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@MetricName", txtMetricName.Text.Trim())
                cmd.Parameters.AddWithValue("@OpDefinition", txtOpDefinition.Text.Trim())
                cmd.Parameters.AddWithValue("@DataSource", txtDataSource.Text.Trim())
                cmd.Parameters.AddWithValue("@SamplingPlan", txtSamplingPlan.Text.Trim())
                cmd.Parameters.AddWithValue("@SampleSize", sampleSize)
                cmd.Parameters.AddWithValue("@Frequency", ddlFrequency.SelectedValue)
                cmd.Parameters.AddWithValue("@Who", txtWho.Text.Trim())
                cmd.Parameters.AddWithValue("@MSAType", ddlMSAType.SelectedValue)
                cmd.Parameters.AddWithValue("@MSAResult", txtMSAResult.Text.Trim())
                cmd.Parameters.AddWithValue("@MSAAcceptable", chkMSAAcceptable.Checked)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "MeasurementPlan", editId, "Updated plan " & txtMetricName.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO MeasurementPlans (TenantId, ProjectId, MetricName, OperationalDefinition, DataSource, " &
                "SamplingPlan, SampleSize, Frequency, Who, MSA_Type, MSA_Result, MSA_Acceptable, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @MetricName, @OpDefinition, @DataSource, @SamplingPlan, @SampleSize, " &
                "@Frequency, @Who, @MSAType, @MSAResult, @MSAAcceptable, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@MetricName", txtMetricName.Text.Trim())
                cmd.Parameters.AddWithValue("@OpDefinition", txtOpDefinition.Text.Trim())
                cmd.Parameters.AddWithValue("@DataSource", txtDataSource.Text.Trim())
                cmd.Parameters.AddWithValue("@SamplingPlan", txtSamplingPlan.Text.Trim())
                cmd.Parameters.AddWithValue("@SampleSize", sampleSize)
                cmd.Parameters.AddWithValue("@Frequency", ddlFrequency.SelectedValue)
                cmd.Parameters.AddWithValue("@Who", txtWho.Text.Trim())
                cmd.Parameters.AddWithValue("@MSAType", ddlMSAType.SelectedValue)
                cmd.Parameters.AddWithValue("@MSAResult", txtMSAResult.Text.Trim())
                cmd.Parameters.AddWithValue("@MSAAcceptable", chkMSAAcceptable.Checked)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "MeasurementPlan", _projectId, "Added plan " & txtMetricName.Text)
        End If

        ClearForm()
        LoadPlans()
    End Sub

    Protected Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        ClearForm()
    End Sub

    Protected Sub rptPlans_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM MeasurementPlans WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "MeasurementPlan", id, "Deleted measurement plan")
            LoadPlans()
        ElseIf e.CommandName = "Edit" Then
            LoadPlanForEdit(id)
        End If
    End Sub

    Private Sub LoadPlanForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM MeasurementPlans WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfEditId.Value = id.ToString()
                txtMetricName.Text = row("MetricName").ToString()
                txtOpDefinition.Text = row("OperationalDefinition").ToString()
                txtDataSource.Text = row("DataSource").ToString()
                txtSamplingPlan.Text = row("SamplingPlan").ToString()
                txtSampleSize.Text = If(row("SampleSize") IsNot DBNull.Value, row("SampleSize").ToString(), "")
                If ddlFrequency.Items.FindByValue(row("Frequency").ToString()) IsNot Nothing Then
                    ddlFrequency.SelectedValue = row("Frequency").ToString()
                End If
                txtWho.Text = row("Who").ToString()
                If ddlMSAType.Items.FindByValue(row("MSA_Type").ToString()) IsNot Nothing Then
                    ddlMSAType.SelectedValue = row("MSA_Type").ToString()
                End If
                txtMSAResult.Text = row("MSA_Result").ToString()
                chkMSAAcceptable.Checked = If(row("MSA_Acceptable") IsNot DBNull.Value, CBool(row("MSA_Acceptable")), False)
                btnCancel.Visible = True
                btnSave.Text = "Update"
            End If
        End Using
    End Sub

    Private Sub ClearForm()
        txtMetricName.Text = ""
        txtOpDefinition.Text = ""
        txtDataSource.Text = ""
        txtSamplingPlan.Text = ""
        txtSampleSize.Text = ""
        ddlFrequency.SelectedIndex = 0
        txtWho.Text = ""
        ddlMSAType.SelectedIndex = 0
        txtMSAResult.Text = ""
        chkMSAAcceptable.Checked = False
        hfEditId.Value = "0"
        btnCancel.Visible = False
        btnSave.Text = "<i class='bi bi-check-lg'></i> Save"
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 5, "In Progress")
    End Sub
End Class
