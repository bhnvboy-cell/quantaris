Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

Public Partial Class DMAIC_Control_Control_Institutionalize
    Inherits BasePage

    Private _projectId As Integer
    Private _json As New JavaScriptSerializer()

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadVMB()
            LoadHuddle()
            LoadControlPlans()
            LoadSOP()
            LoadGemba()
            LoadCharts()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 13, "In Progress")
    End Sub

    Private Function Serialize(ParamArray keysAndValues() As String) As String
        Dim dict As New Dictionary(Of String, String)
        For i As Integer = 0 To keysAndValues.Length - 1 Step 2
            If i + 1 < keysAndValues.Length Then
                dict(keysAndValues(i)) = keysAndValues(i + 1)
            End If
        Next
        Return _json.Serialize(dict)
    End Function

    Private Function LoadMetaRecord(sectionTag As String) As Dictionary(Of String, String)
        Using cmd As New MySqlCommand(
            "SELECT ControlMethod FROM ControlPlans WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND ProcessStep = @ProcessStep")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@ProcessStep", sectionTag)
            Dim result = DbHelper.ExecuteScalar(cmd)
            If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                Return _json.Deserialize(Of Dictionary(Of String, String))(result.ToString())
            End If
        End Using
        Return Nothing
    End Function

    Private Sub SaveMetaRecord(sectionTag As String, jsonData As String)
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using chkCmd As New MySqlCommand("SELECT COUNT(1) FROM ControlPlans WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND ProcessStep = @ProcessStep", conn)
                chkCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                chkCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                chkCmd.Parameters.AddWithValue("@ProcessStep", sectionTag)
                Dim exists As Boolean = CInt(chkCmd.ExecuteScalar()) > 0
                If exists Then
                    Using updCmd As New MySqlCommand("UPDATE ControlPlans SET ControlMethod = @ControlMethod WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND ProcessStep = @ProcessStep", conn)
                        updCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                        updCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                        updCmd.Parameters.AddWithValue("@ProcessStep", sectionTag)
                        updCmd.Parameters.AddWithValue("@ControlMethod", jsonData)
                        updCmd.ExecuteNonQuery()
                    End Using
                Else
                    Using insCmd As New MySqlCommand("INSERT INTO ControlPlans (TenantId, ProjectId, ProcessStep, ControlMethod, CreatedByUserId, CreatedOn) VALUES (@TenantId, @ProjectId, @ProcessStep, @ControlMethod, @CreatedByUserId, CURRENT_TIMESTAMP)", conn)
                        insCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                        insCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                        insCmd.Parameters.AddWithValue("@ProcessStep", sectionTag)
                        insCmd.Parameters.AddWithValue("@ControlMethod", jsonData)
                        insCmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                        insCmd.ExecuteNonQuery()
                    End Using
                End If
            End Using
        End Using
    End Sub

    Private Sub LoadVMB()
        Dim data = LoadMetaRecord("META_VMB")
        If data IsNot Nothing Then
            txtVMBDescription.Text = If(data.ContainsKey("Description"), data("Description"), "")
            txtVMBLocation.Text = If(data.ContainsKey("Location"), data("Location"), "")
            If data.ContainsKey("Frequency") AndAlso ddlVMBFrequency.Items.FindByValue(data("Frequency")) IsNot Nothing Then
                ddlVMBFrequency.SelectedValue = data("Frequency")
            End If
        End If
    End Sub

    Protected Sub btnSaveVMB_Click(sender As Object, e As EventArgs) Handles btnSaveVMB.Click
        Dim jsonData = Serialize("Description", txtVMBDescription.Text.Trim(), "Location", txtVMBLocation.Text.Trim(), "Frequency", ddlVMBFrequency.SelectedValue)
        SaveMetaRecord("META_VMB", jsonData)
        LogAudit("Update", "ControlPlan", _projectId, "Saved VMB data")
        lblVMBSaved.Visible = True
    End Sub

    Private Sub LoadHuddle()
        Dim data = LoadMetaRecord("META_HUDDLE")
        If data IsNot Nothing Then
            txtHuddleSchedule.Text = If(data.ContainsKey("Schedule"), data("Schedule"), "")
            txtHuddleLocation.Text = If(data.ContainsKey("Location"), data("Location"), "")
            txtHuddleDuration.Text = If(data.ContainsKey("Duration"), data("Duration"), "")
            txtHuddleParticipants.Text = If(data.ContainsKey("Participants"), data("Participants"), "")
            txtHuddleAgenda.Text = If(data.ContainsKey("Agenda"), data("Agenda"), "")
        End If
    End Sub

    Protected Sub btnSaveHuddle_Click(sender As Object, e As EventArgs) Handles btnSaveHuddle.Click
        Dim jsonData = Serialize("Schedule", txtHuddleSchedule.Text.Trim(), "Location", txtHuddleLocation.Text.Trim(),
            "Duration", txtHuddleDuration.Text.Trim(), "Participants", txtHuddleParticipants.Text.Trim(),
            "Agenda", txtHuddleAgenda.Text.Trim())
        SaveMetaRecord("META_HUDDLE", jsonData)
        LogAudit("Update", "ControlPlan", _projectId, "Saved team huddle data")
        lblHuddleSaved.Visible = True
    End Sub

    Private Sub LoadControlPlans()
        Using cmd As New MySqlCommand(
            "SELECT * FROM ControlPlans WHERE TenantId = @TenantId AND ProjectId = @ProjectId " &
            "AND ProcessStep NOT LIKE 'META_%' ORDER BY CreatedOn ASC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptControlPlans.DataSource = dt
            rptControlPlans.DataBind()
            litControlPlanCount.Text = dt.Rows.Count & " entry(ies)"
        End Using
    End Sub

    Protected Sub btnSaveCP_Click(sender As Object, e As EventArgs) Handles btnSaveCP.Click
        If Not Page.IsValid Then Return
        Dim editId As Integer = CInt(hfControlPlanEditId.Value)

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE ControlPlans SET ProcessStep = @ProcessStep, ControlMethod = @ControlMethod, " &
                "Specification = @Specification, SampleSize = @SampleSize, Frequency = @Frequency, " &
                "ReactionPlan = @ReactionPlan, Responsible = @Responsible WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProcessStep", txtCPProcessStep.Text.Trim())
                cmd.Parameters.AddWithValue("@ControlMethod", If(String.IsNullOrWhiteSpace(txtCPControlMethod.Text), CObj(DBNull.Value), txtCPControlMethod.Text.Trim()))
                cmd.Parameters.AddWithValue("@Specification", If(String.IsNullOrWhiteSpace(txtCPSpecification.Text), CObj(DBNull.Value), txtCPSpecification.Text.Trim()))
                cmd.Parameters.AddWithValue("@SampleSize", CIntIf(txtCPSampleSize.Text))
                cmd.Parameters.AddWithValue("@Frequency", If(String.IsNullOrWhiteSpace(ddlCPFrequency.SelectedValue), CObj(DBNull.Value), ddlCPFrequency.SelectedValue))
                cmd.Parameters.AddWithValue("@ReactionPlan", If(String.IsNullOrWhiteSpace(txtCPReactionPlan.Text), CObj(DBNull.Value), txtCPReactionPlan.Text.Trim()))
                cmd.Parameters.AddWithValue("@Responsible", If(String.IsNullOrWhiteSpace(txtCPResponsible.Text), CObj(DBNull.Value), txtCPResponsible.Text.Trim()))
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "ControlPlan", editId, "Updated control plan entry: " & txtCPProcessStep.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO ControlPlans (TenantId, ProjectId, ProcessStep, ControlMethod, Specification, " &
                "SampleSize, Frequency, ReactionPlan, Responsible, CreatedByUserId, CreatedOn) " &
                "VALUES (@TenantId, @ProjectId, @ProcessStep, @ControlMethod, @Specification, " &
                "@SampleSize, @Frequency, @ReactionPlan, @Responsible, @CreatedByUserId, CURRENT_TIMESTAMP)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@ProcessStep", txtCPProcessStep.Text.Trim())
                cmd.Parameters.AddWithValue("@ControlMethod", If(String.IsNullOrWhiteSpace(txtCPControlMethod.Text), CObj(DBNull.Value), txtCPControlMethod.Text.Trim()))
                cmd.Parameters.AddWithValue("@Specification", If(String.IsNullOrWhiteSpace(txtCPSpecification.Text), CObj(DBNull.Value), txtCPSpecification.Text.Trim()))
                cmd.Parameters.AddWithValue("@SampleSize", CIntIf(txtCPSampleSize.Text))
                cmd.Parameters.AddWithValue("@Frequency", If(String.IsNullOrWhiteSpace(ddlCPFrequency.SelectedValue), CObj(DBNull.Value), ddlCPFrequency.SelectedValue))
                cmd.Parameters.AddWithValue("@ReactionPlan", If(String.IsNullOrWhiteSpace(txtCPReactionPlan.Text), CObj(DBNull.Value), txtCPReactionPlan.Text.Trim()))
                cmd.Parameters.AddWithValue("@Responsible", If(String.IsNullOrWhiteSpace(txtCPResponsible.Text), CObj(DBNull.Value), txtCPResponsible.Text.Trim()))
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "ControlPlan", _projectId, "Added control plan entry: " & txtCPProcessStep.Text)
        End If

        ClearCPForm()
        LoadControlPlans()
    End Sub

    Protected Sub btnCancelCP_Click(sender As Object, e As EventArgs) Handles btnCancelCP.Click
        ClearCPForm()
    End Sub

    Protected Sub rptControlPlans_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM ControlPlans WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "ControlPlan", id, "Deleted control plan entry")
            LoadControlPlans()
        ElseIf e.CommandName = "Edit" Then
            LoadCPForEdit(id)
        End If
    End Sub

    Private Sub LoadCPForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM ControlPlans WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfControlPlanEditId.Value = id.ToString()
                txtCPProcessStep.Text = row("ProcessStep").ToString()
                txtCPControlMethod.Text = row("ControlMethod").ToString()
                txtCPSpecification.Text = row("Specification").ToString()
                txtCPSampleSize.Text = If(row("SampleSize") IsNot DBNull.Value, row("SampleSize").ToString(), "")
                If row("Frequency") IsNot DBNull.Value AndAlso ddlCPFrequency.Items.FindByValue(row("Frequency").ToString()) IsNot Nothing Then
                    ddlCPFrequency.SelectedValue = row("Frequency").ToString()
                End If
                txtCPReactionPlan.Text = row("ReactionPlan").ToString()
                txtCPResponsible.Text = row("Responsible").ToString()
                btnCancelCP.Visible = True
                btnSaveCP.Text = "Update Entry"
            End If
        End Using
    End Sub

    Private Sub ClearCPForm()
        txtCPProcessStep.Text = ""
        txtCPControlMethod.Text = ""
        txtCPSpecification.Text = ""
        txtCPSampleSize.Text = ""
        ddlCPFrequency.SelectedIndex = 0
        txtCPReactionPlan.Text = ""
        txtCPResponsible.Text = ""
        hfControlPlanEditId.Value = "0"
        btnCancelCP.Visible = False
        btnSaveCP.Text = "Add Entry"
    End Sub

    Private Sub LoadSOP()
        Dim data = LoadMetaRecord("META_SOP")
        If data IsNot Nothing AndAlso data.ContainsKey("References") Then
            txtSOPReferences.Text = data("References")
        End If
    End Sub

    Protected Sub btnSaveSOP_Click(sender As Object, e As EventArgs) Handles btnSaveSOP.Click
        Dim jsonData = Serialize("References", txtSOPReferences.Text.Trim())
        SaveMetaRecord("META_SOP", jsonData)
        LogAudit("Update", "ControlPlan", _projectId, "Saved SOP references")
        lblSOPSaved.Visible = True
    End Sub

    Private Sub LoadGemba()
        Dim data = LoadMetaRecord("META_GEMBA")
        If data IsNot Nothing Then
            txtGembaDate.Text = If(data.ContainsKey("ObservationDate"), data("ObservationDate"), "")
            txtGembaArea.Text = If(data.ContainsKey("Area"), data("Area"), "")
            txtGembaObservations.Text = If(data.ContainsKey("Observations"), data("Observations"), "")
            txtGembaActions.Text = If(data.ContainsKey("ActionItems"), data("ActionItems"), "")
        End If
    End Sub

    Protected Sub btnSaveGemba_Click(sender As Object, e As EventArgs) Handles btnSaveGemba.Click
        Dim jsonData = Serialize("ObservationDate", txtGembaDate.Text.Trim(), "Area", txtGembaArea.Text.Trim(),
            "Observations", txtGembaObservations.Text.Trim(), "ActionItems", txtGembaActions.Text.Trim())
        SaveMetaRecord("META_GEMBA", jsonData)
        LogAudit("Update", "ControlPlan", _projectId, "Saved Go Gemba data")
        lblGembaSaved.Visible = True
    End Sub

    Private Sub LoadCharts()
        Using cmd As New MySqlCommand(
            "SELECT cp.Id, cp.ProcessStep, cp.ControlMethod, cp.Specification, cp.SampleSize, cp.Frequency, cp.ReactionPlan, cp.Responsible " &
            "FROM ControlPlans cp WHERE cp.TenantId = @TenantId AND cp.ProjectId = @ProjectId AND cp.ProcessStep = 'META_CHART' ORDER BY cp.CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)

            Dim chartDt As New DataTable()
            chartDt.Columns.Add("Id", GetType(Integer))
            chartDt.Columns.Add("ChartTypeName", GetType(String))
            chartDt.Columns.Add("DataPointCount", GetType(Integer))
            chartDt.Columns.Add("CenterLine", GetType(Decimal))
            chartDt.Columns.Add("UCL", GetType(Decimal))
            chartDt.Columns.Add("LCL", GetType(Decimal))

            For Each row As DataRow In dt.Rows
                Dim meta = _json.Deserialize(Of Dictionary(Of String, String))(If(row("ControlMethod") IsNot DBNull.Value, row("ControlMethod").ToString(), "{}"))
                Dim r = chartDt.NewRow()
                r("Id") = CInt(row("Id"))
                r("ChartTypeName") = If(meta.ContainsKey("ChartType"), meta("ChartType"), "")
                r("DataPointCount") = If(meta.ContainsKey("DataPointCount"), CInt(meta("DataPointCount")), 0)
                Dim cl As Decimal = 0 : Decimal.TryParse(If(meta.ContainsKey("CL"), meta("CL"), "0"), cl) : r("CenterLine") = cl
                Dim ucl As Decimal = 0 : Decimal.TryParse(If(meta.ContainsKey("UCL"), meta("UCL"), "0"), ucl) : r("UCL") = ucl
                Dim lcl As Decimal = 0 : Decimal.TryParse(If(meta.ContainsKey("LCL"), meta("LCL"), "0"), lcl) : r("LCL") = lcl
                chartDt.Rows.Add(r)
            Next

            rptCharts.DataSource = chartDt
            rptCharts.DataBind()

            If dt.Rows.Count > 0 Then
                LoadChartForEdit(CInt(dt.Rows(0)("Id")))
            End If
        End Using
    End Sub

    Protected Sub btnCalcChart_Click(sender As Object, e As EventArgs) Handles btnCalcChart.Click
        Dim lines = txtChartDataPoints.Text.Trim().Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
        Dim values As New List(Of Double)
        For Each line In lines
            Dim trimmed = line.Trim()
            Dim v As Double
            If Double.TryParse(trimmed, v) Then
                values.Add(v)
            End If
        Next
        If values.Count < 2 Then
            lblChartSaved.Text = "<i class='bi bi-exclamation-circle'></i> Need at least 2 data points"
            lblChartSaved.CssClass = "text-danger small ms-2"
            lblChartSaved.Visible = True
            Return
        End If

        Dim n = values.Count
        Dim mean = values.Average()
        Dim variance = values.Select(Function(x) (x - mean) ^ 2).Sum() / (n - 1)
        Dim stdDev = Math.Sqrt(variance)
        Dim minVal = values.Min()
        Dim maxVal = values.Max()

        txtChartN.Text = n.ToString()
        txtChartMean.Text = mean.ToString("N4")
        txtChartStdDev.Text = stdDev.ToString("N4")
        txtChartRange.Text = (maxVal - minVal).ToString("N4")

        If String.IsNullOrWhiteSpace(txtChartCL.Text) Then txtChartCL.Text = mean.ToString("N4")
        If String.IsNullOrWhiteSpace(txtChartUCL.Text) Then txtChartUCL.Text = (mean + 3 * stdDev).ToString("N4")
        If String.IsNullOrWhiteSpace(txtChartLCL.Text) Then txtChartLCL.Text = (mean - 3 * stdDev).ToString("N4")

        divChartSummary.Visible = True
        lblChartSaved.Visible = False
    End Sub

    Protected Sub btnSaveChart_Click(sender As Object, e As EventArgs) Handles btnSaveChart.Click
        If ddlChartType.SelectedIndex = 0 Then Return

        Dim lines = txtChartDataPoints.Text.Trim().Split({vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
        Dim values As New List(Of Double)
        For Each line In lines
            Dim trimmed = line.Trim()
            Dim v As Double
            If Double.TryParse(trimmed, v) Then values.Add(v)
        Next

        Dim chartMeta As New Dictionary(Of String, String)()
        chartMeta("ChartType") = ddlChartType.SelectedValue
        chartMeta("DataPointCount") = values.Count.ToString()
        chartMeta("CL") = txtChartCL.Text.Trim()
        chartMeta("UCL") = txtChartUCL.Text.Trim()
        chartMeta("LCL") = txtChartLCL.Text.Trim()
        chartMeta("Mean") = txtChartMean.Text.Trim()
        chartMeta("StdDev") = txtChartStdDev.Text.Trim()
        chartMeta("Range") = txtChartRange.Text.Trim()
        chartMeta("RawData") = String.Join(",", values.Select(Function(v) v.ToString("G")))

        Dim jsonData = _json.Serialize(chartMeta)
        SaveMetaRecord("META_CHART", jsonData)

        ' Store individual data points in MetricsHistory
        Dim metricName As String = "ControlChart_" & ddlChartType.SelectedValue
        For Each v In values
            Using cmd As New MySqlCommand(
                "INSERT INTO MetricsHistory (TenantId, ProjectId, MetricName, Value, RecordedOn, CreatedByUserId, CreatedOn, Notes) " &
                "VALUES (@TenantId, @ProjectId, @MetricName, @Value, CURRENT_TIMESTAMP, @CreatedByUserId, CURRENT_TIMESTAMP, @Notes)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@MetricName", metricName)
                cmd.Parameters.AddWithValue("@Value", v)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                cmd.Parameters.AddWithValue("@Notes", "Control chart data point - " & ddlChartType.SelectedValue)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
        Next

        LogAudit("Create", "ControlChart", _projectId, "Saved control chart: " & ddlChartType.SelectedValue)
        ClearChartForm()
        LoadCharts()
        lblChartSaved.CssClass = "text-success small ms-2"
        lblChartSaved.Text = "<i class='bi bi-check-circle'></i> Saved"
        lblChartSaved.Visible = True
    End Sub

    Protected Sub rptCharts_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM ControlPlans WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "ControlChart", id, "Deleted control chart")
            LoadCharts()
        End If
    End Sub

    Private Sub LoadChartForEdit(cpId As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM ControlPlans WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", cpId)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row = dt.Rows(0)
                If row("ControlMethod") IsNot DBNull.Value Then
                    Dim meta = _json.Deserialize(Of Dictionary(Of String, String))(row("ControlMethod").ToString())
                    If meta.ContainsKey("ChartType") AndAlso ddlChartType.Items.FindByValue(meta("ChartType")) IsNot Nothing Then
                        ddlChartType.SelectedValue = meta("ChartType")
                    End If
                    If meta.ContainsKey("RawData") Then
                        Dim rawValues = meta("RawData").Split(","c)
                        txtChartDataPoints.Text = String.Join(vbCrLf, rawValues)
                    End If
                    txtChartCL.Text = If(meta.ContainsKey("CL"), meta("CL"), "")
                    txtChartUCL.Text = If(meta.ContainsKey("UCL"), meta("UCL"), "")
                    txtChartLCL.Text = If(meta.ContainsKey("LCL"), meta("LCL"), "")
                    txtChartN.Text = If(meta.ContainsKey("DataPointCount"), meta("DataPointCount"), "")
                    txtChartMean.Text = If(meta.ContainsKey("Mean"), meta("Mean"), "")
                    txtChartStdDev.Text = If(meta.ContainsKey("StdDev"), meta("StdDev"), "")
                    txtChartRange.Text = If(meta.ContainsKey("Range"), meta("Range"), "")
                    divChartSummary.Visible = True
                    hfChartEditId.Value = cpId.ToString()
                End If
            End If
        End Using
    End Sub

    Private Sub ClearChartForm()
        ddlChartType.SelectedIndex = 0
        txtChartDataPoints.Text = ""
        txtChartCL.Text = ""
        txtChartUCL.Text = ""
        txtChartLCL.Text = ""
        txtChartN.Text = ""
        txtChartMean.Text = ""
        txtChartStdDev.Text = ""
        txtChartRange.Text = ""
        divChartSummary.Visible = False
        hfChartEditId.Value = "0"
    End Sub

    Private Function CIntIf(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then Return DBNull.Value
        Dim result As Integer
        If Integer.TryParse(value.Trim(), result) Then Return result
        Return DBNull.Value
    End Function
End Class
