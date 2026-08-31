Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Tools_ProcessCapability
    Inherits BasePage

    Private _projectId As Integer
    Private _cp As Double
    Private _cpk As Double
    Private _ppk As Double
    Private _mean As Double
    Private _stdDev As Double
    Private _stdDevOverall As Double
    Private _sigmaLevel As Double
    Private _dpmo As Double
    Private _sampleSize As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt", "User")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlCapability.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlCapability.Visible = False
                pnlNoProject.Visible = True
            End If
        End If
    End Sub

    Private Sub LoadProjects()
        Using cmd As New MySqlCommand(
            "SELECT Id, ProjectCode, ProjectName FROM QuantarisProjects WHERE TenantId = @TenantId ORDER BY ProjectName")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            ddlProject.DataSource = dt
            ddlProject.DataTextField = "ProjectName"
            ddlProject.DataValueField = "Id"
            ddlProject.DataBind()
            ddlProject.Items.Insert(0, New ListItem("-- Select Project --", ""))
        End Using
    End Sub

    Private Sub LoadMetrics()
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Exit Sub
        Using cmd As New MySqlCommand(
            "SELECT DISTINCT MetricName FROM Metrics WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND MetricName IS NOT NULL ORDER BY MetricName")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            ddlMetric.DataSource = dt
            ddlMetric.DataTextField = "MetricName"
            ddlMetric.DataValueField = "MetricName"
            ddlMetric.DataBind()
            ddlMetric.Items.Insert(0, New ListItem("-- Manual Entry --", ""))
        End Using
    End Sub

    Private Function GetSelectedProjectId() As Integer
        Dim pid As Integer = 0
        If ddlProject.SelectedValue <> "" Then Integer.TryParse(ddlProject.SelectedValue, pid)
        Return pid
    End Function

    Private Sub LoadHistory()
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Exit Sub
        Using cmd As New MySqlCommand(
            "SELECT * FROM ProcessCapability WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            rptHistory.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataBind()
        End Using
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlCapability.Visible = True
            pnlNoProject.Visible = False
            pnlResults.Visible = False
            pnlNoResults.Visible = True
            LoadMetrics()
            LoadHistory()
        Else
            pnlCapability.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub ddlMetric_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlMetric.SelectedValue <> "" Then
            Dim pid = GetSelectedProjectId()
            Using cmd As New MySqlCommand(
                "SELECT CurrentValue, Baseline, Target FROM Metrics WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND MetricName = @MetricName LIMIT 1")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", pid)
                cmd.Parameters.AddWithValue("@MetricName", ddlMetric.SelectedValue)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    txtMetricName.Text = ddlMetric.SelectedValue
                    If dt.Rows(0)("Target") IsNot DBNull.Value Then
                        txtTarget.Text = dt.Rows(0)("Target").ToString()
                    End If
                End If
            End Using
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadHistory()
    End Sub

    Protected Sub btnCalculate_Click(sender As Object, e As EventArgs)
        Try
            Dim usl As Double
            Dim lsl As Double
            Dim target As Double?
            Dim metricName = txtMetricName.Text.Trim()

            If Not Double.TryParse(txtUSL.Text.Trim(), usl) Then
                litMessage.Text = "<div class='alert alert-danger'>Invalid USL value.</div>"
                Return
            End If
            If Not Double.TryParse(txtLSL.Text.Trim(), lsl) Then
                litMessage.Text = "<div class='alert alert-danger'>Invalid LSL value.</div>"
                Return
            End If
            If usl <= lsl Then
                litMessage.Text = "<div class='alert alert-danger'>USL must be greater than LSL.</div>"
                Return
            End If
            Dim targetParsed As Double
            If Double.TryParse(txtTarget.Text.Trim(), targetParsed) Then
                target = targetParsed
            End If

            Dim rawData = txtDataPoints.Text.Trim()
            If String.IsNullOrEmpty(rawData) Then
                litMessage.Text = "<div class='alert alert-danger'>Please enter data points.</div>"
                Return
            End If

            Dim values = ParseDataPoints(rawData)
            If values.Count < 3 Then
                litMessage.Text = "<div class='alert alert-danger'>At least 3 data points are required.</div>"
                Return
            End If

            litMessage.Text = ""

            Dim n = values.Count
            Dim mean = values.Average()
            Dim variance = values.Sum(Function(v) Math.Pow(v - mean, 2))
            Dim stdDevSample = Math.Sqrt(variance / (n - 1))
            Dim stdDevOverall = Math.Sqrt(variance / n)

            Dim cp = (usl - lsl) / (6.0 * stdDevSample)
            Dim cpkUpper = (usl - mean) / (3.0 * stdDevSample)
            Dim cpkLower = (mean - lsl) / (3.0 * stdDevSample)
            Dim cpk = Math.Min(cpkUpper, cpkLower)

            Dim ppkUpper = (usl - mean) / (3.0 * stdDevOverall)
            Dim ppkLower = (mean - lsl) / (3.0 * stdDevOverall)
            Dim ppk = Math.Min(ppkUpper, ppkLower)

            Dim sigmaLevel = cpk * 3.0
            Dim dpmo = CalculateDPMO(mean, stdDevSample, usl, lsl)

            _cp = cp
            _cpk = cpk
            _ppk = ppk
            _mean = mean
            _stdDev = stdDevSample
            _stdDevOverall = stdDevOverall
            _sigmaLevel = sigmaLevel
            _dpmo = dpmo
            _sampleSize = n

            litSampleSize.Text = n.ToString()
            litMean.Text = mean.ToString("F4")
            litStdDev.Text = stdDevSample.ToString("F4")
            litStdDevOverall.Text = stdDevOverall.ToString("F4")
            litMin.Text = values.Min().ToString("F4")
            litMax.Text = values.Max().ToString("F4")

            Dim badgeSize = "fs-6 rounded-pill px-3 py-1"
            litCp.Text = "<span class='badge " & GetCapBadge(cp) & " " & badgeSize & "'>" & cp.ToString("F4") & "</span>"
            litCpk.Text = "<span class='badge " & GetCapBadge(cpk) & " " & badgeSize & "'>" & cpk.ToString("F4") & "</span>"
            litPpk.Text = "<span class='badge " & GetCapBadge(ppk) & " " & badgeSize & "'>" & ppk.ToString("F4") & "</span>"
            litSigmaLevel.Text = sigmaLevel.ToString("F2")
            litDPMO.Text = dpmo.ToString("N0")

            litInterpretation.Text = GetInterpretation(cp, cpk)

            litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Calculation complete.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"

            pnlResults.Visible = True
            pnlNoResults.Visible = False

        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Private Function ParseDataPoints(raw As String) As List(Of Double)
        Dim result As New List(Of Double)
        Dim parts = raw.Split(New Char() {",", " ", ";", vbCr, vbLf, vbTab}, StringSplitOptions.RemoveEmptyEntries)
        For Each p In parts
            Dim trimmed = p.Trim()
            Dim val As Double
            If Double.TryParse(trimmed, val) Then
                result.Add(val)
            End If
        Next
        Return result
    End Function

    Private Function CalculateDPMO(mean As Double, stdDev As Double, usl As Double, lsl As Double) As Double
        If stdDev <= 0 Then Return 0
        Dim zUpper = (usl - mean) / stdDev
        Dim zLower = (mean - lsl) / stdDev
        Dim probUpper = 1.0 - NormalCDF(zUpper)
        Dim probLower = 1.0 - NormalCDF(zLower)
        Dim totalDefects = probUpper + probLower
        Return totalDefects * 1000000.0
    End Function

    Private Function NormalCDF(x As Double) As Double
        Dim a1 = 0.254829592
        Dim a2 = -0.284496736
        Dim a3 = 1.421413741
        Dim a4 = -1.453152027
        Dim a5 = 1.061405429
        Dim p = 0.3275911

        Dim sign = 1
        If x < 0 Then sign = -1
        x = Math.Abs(x) / Math.Sqrt(2.0)

        Dim t = 1.0 / (1.0 + p * x)
        Dim y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x)

        Return 0.5 * (1.0 + sign * y)
    End Function

    Private Function GetInterpretation(cp As Double, cpk As Double) As String
        If cpk < 1.0 Then
            Return "<strong>Not Capable:</strong> Cpk = " & cpk.ToString("F4") & " is below 1.0. The process does not meet specifications. Immediate improvement is recommended."
        ElseIf cpk < 1.33 Then
            Return "<strong>Marginally Capable:</strong> Cpk = " & cpk.ToString("F4") & " is between 1.0 and 1.33. The process meets minimum requirements but needs close monitoring."
        ElseIf cpk < 1.67 Then
            Return "<strong>Capable:</strong> Cpk = " & cpk.ToString("F4") & " is between 1.33 and 1.67. The process is capable and well-centered."
        Else
            Return "<strong>Highly Capable:</strong> Cpk = " & cpk.ToString("F4") & " exceeds 1.67. World-class process performance."
        End If
    End Function

    Protected Function GetCapBadge(value As Object) As String
        Dim v As Double
        If value Is Nothing OrElse value Is DBNull.Value Then Return "bg-secondary"
        If Not Double.TryParse(value.ToString(), v) Then Return "bg-secondary"
        If v >= 1.67 Then Return "bg-success"
        If v >= 1.33 Then Return "bg-info text-dark"
        If v >= 1.0 Then Return "bg-warning text-dark"
        Return "bg-danger"
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then
            litMessage.Text = "<div class='alert alert-warning'>Please select a project.</div>"
            Return
        End If
        If Not pnlResults.Visible Then
            litMessage.Text = "<div class='alert alert-warning'>Please calculate first before saving.</div>"
            Return
        End If
        Try
            Dim mean = _mean
            Dim stdDev = _stdDev
            Dim stdDevOverall = _stdDevOverall
            Dim cp = _cp
            Dim cpk = _cpk
            Dim ppk = _ppk
            Dim sigmaLevel = _sigmaLevel
            Dim dpmo = _dpmo
            Dim sampleSize = _sampleSize

            Using cmd As New MySqlCommand(
                "INSERT INTO ProcessCapability (TenantId, ProjectId, MetricName, USL, LSL, Target, Mean, StdDev, " &
                "Cp, Cpk, Ppk, SigmaLevel, DPMO, SampleSize, DataPoints, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @MetricName, @USL, @LSL, @Target, @Mean, @StdDev, " &
                "@Cp, @Cpk, @Ppk, @SigmaLevel, @DPMO, @SampleSize, @DataPoints, @CreatedByUserId)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            cmd.Parameters.AddWithValue("@MetricName", IIf(txtMetricName.Text.Trim() = "", DBNull.Value, CObj(txtMetricName.Text.Trim())))
            cmd.Parameters.AddWithValue("@USL", CDbl(txtUSL.Text.Trim()))
            cmd.Parameters.AddWithValue("@LSL", CDbl(txtLSL.Text.Trim()))
            cmd.Parameters.AddWithValue("@Target", IIf(txtTarget.Text.Trim() = "", DBNull.Value, CObj(CDbl(txtTarget.Text.Trim()))))
            cmd.Parameters.AddWithValue("@Mean", mean)
            cmd.Parameters.AddWithValue("@StdDev", stdDev)
            cmd.Parameters.AddWithValue("@Cp", cp)
            cmd.Parameters.AddWithValue("@Cpk", cpk)
            cmd.Parameters.AddWithValue("@Ppk", ppk)
            cmd.Parameters.AddWithValue("@SigmaLevel", sigmaLevel)
            cmd.Parameters.AddWithValue("@DPMO", dpmo)
            cmd.Parameters.AddWithValue("@SampleSize", sampleSize)
            cmd.Parameters.AddWithValue("@DataPoints", txtDataPoints.Text.Trim())
            cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using

            LogAudit("Create", "ProcessCapability", Nothing, "Saved process capability analysis: " & txtMetricName.Text)
            litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Analysis saved successfully.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
            LoadHistory()

        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error saving: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        txtUSL.Text = ""
        txtLSL.Text = ""
        txtTarget.Text = ""
        txtMetricName.Text = ""
        txtDataPoints.Text = ""
        pnlResults.Visible = False
        pnlNoResults.Visible = True
        litMessage.Text = ""
    End Sub

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "LoadRecord" Then
            Using cmd As New MySqlCommand(
                "SELECT * FROM ProcessCapability WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    Dim r = dt.Rows(0)
                    txtUSL.Text = r("USL").ToString()
                    txtLSL.Text = r("LSL").ToString()
                    txtTarget.Text = If(r("Target") IsNot DBNull.Value, r("Target").ToString(), "")
                    txtMetricName.Text = If(r("MetricName") IsNot DBNull.Value, r("MetricName").ToString(), "")
                    txtDataPoints.Text = If(r("DataPoints") IsNot DBNull.Value, r("DataPoints").ToString(), "")

                    litSampleSize.Text = r("SampleSize").ToString()
                    litMean.Text = r("Mean").ToString()
                    litStdDev.Text = r("StdDev").ToString()
                    Dim cp = CDbl(r("Cp"))
                    Dim cpk = CDbl(r("Cpk"))
                    Dim ppk = CDbl(r("Ppk"))
                    Dim badgeSize = "fs-6 rounded-pill px-3 py-1"
                    litCp.Text = "<span class='badge " & GetCapBadge(cp) & " " & badgeSize & "'>" & cp.ToString("F4") & "</span>"
                    litCpk.Text = "<span class='badge " & GetCapBadge(cpk) & " " & badgeSize & "'>" & cpk.ToString("F4") & "</span>"
                    litPpk.Text = "<span class='badge " & GetCapBadge(ppk) & " " & badgeSize & "'>" & ppk.ToString("F4") & "</span>"
                    litSigmaLevel.Text = r("SigmaLevel").ToString()
                    litDPMO.Text = CDbl(r("DPMO")).ToString("N0")

                    _cp = cp
                    _cpk = cpk
                    _ppk = ppk
                    _mean = CDbl(r("Mean"))
                    _stdDev = CDbl(r("StdDev"))
                    _stdDevOverall = CDbl(r("StdDev")) ' use stddev for overall too
                    _sigmaLevel = CDbl(r("SigmaLevel"))
                    _dpmo = CDbl(r("DPMO"))
                    _sampleSize = CInt(r("SampleSize"))
                    litInterpretation.Text = GetInterpretation(_cp, _cpk)

                    pnlResults.Visible = True
                    pnlNoResults.Visible = False
                End If
            End Using
        ElseIf e.CommandName = "DeleteRecord" Then
            Using cmd As New MySqlCommand(
                "DELETE FROM ProcessCapability WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
                LogAudit("Delete", "ProcessCapability", id, "Deleted process capability analysis")
                LoadHistory()
            End Using
        End If
    End Sub
End Class
