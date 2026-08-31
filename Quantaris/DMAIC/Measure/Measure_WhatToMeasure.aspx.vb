Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

Public Partial Class DMAIC_Measure_WhatToMeasure
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadData()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadData()
        ' Load COPQ, Wastes, VSM from ProjectCharters.Timeline JSON
        Using cmd As New MySqlCommand("SELECT Timeline FROM ProjectCharters " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC LIMIT 1")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim result = DbHelper.ExecuteScalar(cmd)
            If result IsNot Nothing AndAlso Not String.IsNullOrEmpty(result.ToString()) Then
                Dim json = result.ToString()
                Try
                    Dim serializer As New JavaScriptSerializer()
                    Dim data = serializer.Deserialize(Of Dictionary(Of String, Object))(json)
                    If data.ContainsKey("appraisalCosts") Then txtAppraisalCosts.Text = data("appraisalCosts").ToString()
                    If data.ContainsKey("preventionCosts") Then txtPreventionCosts.Text = data("preventionCosts").ToString()
                    If data.ContainsKey("internalFailure") Then txtInternalFailure.Text = data("internalFailure").ToString()
                    If data.ContainsKey("externalFailure") Then txtExternalFailure.Text = data("externalFailure").ToString()
                    If data.ContainsKey("vsm") Then txtVSM.Text = data("vsm").ToString()
                    If data.ContainsKey("wasteDefects") Then chkDefects.Checked = CBool(data("wasteDefects"))
                    If data.ContainsKey("wasteOverproduction") Then chkOverproduction.Checked = CBool(data("wasteOverproduction"))
                    If data.ContainsKey("wasteWaiting") Then chkWaiting.Checked = CBool(data("wasteWaiting"))
                    If data.ContainsKey("wasteTalent") Then chkTalent.Checked = CBool(data("wasteTalent"))
                    If data.ContainsKey("wasteTransportation") Then chkTransportation.Checked = CBool(data("wasteTransportation"))
                    If data.ContainsKey("wasteInventory") Then chkInventory.Checked = CBool(data("wasteInventory"))
                    If data.ContainsKey("wasteMotion") Then chkMotion.Checked = CBool(data("wasteMotion"))
                    If data.ContainsKey("wasteExtraProcessing") Then chkExtraProcessing.Checked = CBool(data("wasteExtraProcessing"))
                    If data.ContainsKey("workTime") Then txtWorkTime.Text = data("workTime").ToString()
                    If data.ContainsKey("demand") Then txtDemand.Text = data("demand").ToString()
                    If data.ContainsKey("taktTime") Then txtTaktTime.Text = data("taktTime").ToString()
                Catch
                End Try
            End If
        End Using
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim data As New Dictionary(Of String, Object)()
        data("appraisalCosts") = txtAppraisalCosts.Text.Trim()
        data("preventionCosts") = txtPreventionCosts.Text.Trim()
        data("internalFailure") = txtInternalFailure.Text.Trim()
        data("externalFailure") = txtExternalFailure.Text.Trim()
        data("vsm") = txtVSM.Text.Trim()
        data("wasteDefects") = chkDefects.Checked
        data("wasteOverproduction") = chkOverproduction.Checked
        data("wasteWaiting") = chkWaiting.Checked
        data("wasteTalent") = chkTalent.Checked
        data("wasteTransportation") = chkTransportation.Checked
        data("wasteInventory") = chkInventory.Checked
        data("wasteMotion") = chkMotion.Checked
        data("wasteExtraProcessing") = chkExtraProcessing.Checked
        data("workTime") = txtWorkTime.Text.Trim()
        data("demand") = txtDemand.Text.Trim()
        data("taktTime") = txtTaktTime.Text.Trim()

        Dim serializer As New JavaScriptSerializer()
        Dim json As String = serializer.Serialize(data)

        ' Upsert into ProjectCharters.Timeline
        Dim charterId As Object = Nothing
        Using cmd As New MySqlCommand("SELECT Id FROM ProjectCharters " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC LIMIT 1")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            charterId = DbHelper.ExecuteScalar(cmd)
        End Using

        If charterId IsNot Nothing Then
            Using cmd As New MySqlCommand("UPDATE ProjectCharters SET Timeline = @Timeline WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", CInt(charterId))
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@Timeline", json)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "Charter", CInt(charterId), "Updated WhatToMeasure data")
        Else
            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()
                Using cmd As New MySqlCommand(
                    "INSERT INTO ProjectCharters (TenantId, ProjectId, Timeline, CreatedOn, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @Timeline, CURRENT_TIMESTAMP, @CreatedByUserId)", conn)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                    cmd.Parameters.AddWithValue("@Timeline", json)
                    cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
            LogAudit("Create", "Charter", _projectId, "Created WhatToMeasure data")
        End If

        ' Save Takt Time to Metrics table
        SaveTaktTimeToMetrics()

        LogAudit("Save", "WhatToMeasure", _projectId, "Saved WhatToMeasure step data")
    End Sub

    Private Sub SaveTaktTimeToMetrics()
        If String.IsNullOrWhiteSpace(txtWorkTime.Text) OrElse String.IsNullOrWhiteSpace(txtDemand.Text) Then Return

        Dim workTime As Decimal, demand As Decimal, takt As Decimal

        If Decimal.TryParse(txtWorkTime.Text.Trim(), workTime) Then
            UpsertMetric("Available Work Time", "Minutes", workTime)
        End If
        If Decimal.TryParse(txtDemand.Text.Trim(), demand) Then
            UpsertMetric("Customer Demand", "Units", demand)
        End If
        If Decimal.TryParse(txtTaktTime.Text.Trim(), takt) Then
            UpsertMetric("Takt Time", "Minutes/Unit", takt)
        End If
    End Sub

    Private Sub UpsertMetric(metricName As String, dataType As String, value As Decimal)
        Dim exists As Boolean = False
        Using cmd As New MySqlCommand("SELECT COUNT(1) FROM Metrics " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND MetricName = @MetricName")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@MetricName", metricName)
            Dim count = CInt(DbHelper.ExecuteScalar(cmd))
            exists = count > 0
        End Using

        If exists Then
            Using cmd As New MySqlCommand(
                "UPDATE Metrics SET CurrentValue = @Value WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND MetricName = @MetricName")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@MetricName", metricName)
                cmd.Parameters.AddWithValue("@Value", value)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO Metrics (TenantId, ProjectId, MetricName, DataType, CurrentValue, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @MetricName, @DataType, @Value, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@MetricName", metricName)
                cmd.Parameters.AddWithValue("@DataType", dataType)
                cmd.Parameters.AddWithValue("@Value", value)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
        End If
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 4, "In Progress")
    End Sub
End Class
