Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Text
Imports System.Text.RegularExpressions

Public Partial Class Tools_GageRnR
    Inherits BasePage

    Private _studyId As Integer
    Private _partCount As Integer
    Private _appraiserCount As Integer
    Private _trialCount As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlGRR.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlGRR.Visible = False
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

    Private Function GetSelectedProjectId() As Integer
        Dim pid As Integer = 0
        If ddlProject.SelectedValue <> "" Then Integer.TryParse(ddlProject.SelectedValue, pid)
        Return pid
    End Function

    Private Sub LoadHistory()
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Exit Sub
        Using cmd As New MySqlCommand(
            "SELECT * FROM GageRnRStudies WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataSource = dt
            rptHistory.DataBind()
        End Using
    End Sub

    Private Function GetGridValue(gridId As String, partIndex As Integer, appraiserIndex As Integer, trialIndex As Integer) As Double
        Dim key = String.Format("g_{0}_{1}_{2}_{3}", gridId, partIndex, appraiserIndex, trialIndex)
        Dim val = Request.Form(key)
        If val IsNot Nothing Then
            Dim result As Double
            Double.TryParse(val.Trim(), result)
            Return result
        End If
        Return 0
    End Function

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlGRR.Visible = True
            pnlNoProject.Visible = False
            pnlResults.Visible = False
            pnlGrid.Visible = False
            pnlNoGrid.Visible = True
            LoadHistory()
        Else
            pnlGRR.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then LoadHistory()
    End Sub

    Protected Sub btnSetup_Click(sender As Object, e As EventArgs)
        Dim pc As Integer
        Dim ac As Integer
        Dim tc As Integer

        If Not Integer.TryParse(txtPartCount.Text.Trim(), pc) OrElse pc < 2 OrElse pc > 20 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Parts must be 2-20.</div>"
            Return
        End If
        If Not Integer.TryParse(txtAppraiserCount.Text.Trim(), ac) OrElse ac < 2 OrElse ac > 10 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Appraisers must be 2-10.</div>"
            Return
        End If
        If Not Integer.TryParse(txtTrialCount.Text.Trim(), tc) OrElse tc < 2 OrElse tc > 5 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Trials must be 2-5.</div>"
            Return
        End If

        _partCount = pc
        _appraiserCount = ac
        _trialCount = tc
        ViewState("PartCount") = pc
        ViewState("AppraiserCount") = ac
        ViewState("TrialCount") = tc

        ' Generate names
        Dim partNames = ParseNames(txtPartNames.Text.Trim(), pc, "Part-")
        Dim appraiserNames = ParseNames(txtAppraiserNames.Text.Trim(), ac, "Appraiser ")

        ViewState("PartNames") = String.Join(",", partNames)
        ViewState("AppraiserNames") = String.Join(",", appraiserNames)

        litGrid.Text = RenderGrid(pc, ac, tc, partNames, appraiserNames, Nothing)
        pnlGrid.Visible = True
        pnlNoGrid.Visible = False
        btnCalculate.Visible = True
        btnSave.Visible = False
        pnlResults.Visible = False
        litMessage.Text = ""
    End Sub

    Private Function ParseNames(raw As String, expectedCount As Integer, prefix As String) As String()
        Dim names As New List(Of String)
        If Not String.IsNullOrEmpty(raw) Then
            Dim parts = raw.Split(New Char() {","}, StringSplitOptions.RemoveEmptyEntries)
            For i = 0 To Math.Min(parts.Length, expectedCount) - 1
                names.Add(parts(i).Trim())
            Next
        End If
        While names.Count < expectedCount
            names.Add(prefix & (names.Count + 1))
        End While
        Return names.ToArray()
    End Function

    Private Function RenderGrid(pc As Integer, ac As Integer, tc As Integer,
                                 partNames As String(), appraiserNames As String(),
                                 measurements As Dictionary(Of String, Double)) As String
        Dim sb As New StringBuilder()
        Dim gridId = "grr_" & DateTime.Now.Ticks
        ViewState("GridId") = gridId

        sb.Append("<div class='table-responsive'>")
        sb.Append("<table class='table table-bordered table-sm grr-table mb-0'>")
        sb.Append("<thead class='table-dark'>")
        sb.Append("<tr><th style='min-width:100px;'>Part</th>")

        For a = 0 To ac - 1
            For t = 0 To tc - 1
                sb.Append("<th class='text-center'>" & HttpUtility.HtmlEncode(appraiserNames(a)) & "<br/><span class='text-muted' style='font-weight:400;font-size:10px;'>Trial " & (t + 1) & "</span></th>")
            Next
        Next

        sb.Append("</tr></thead><tbody>")

        For p = 0 To pc - 1
            sb.Append("<tr>")
            sb.Append("<td class='part-cell'>" & HttpUtility.HtmlEncode(partNames(p)) & "</td>")
            For a = 0 To ac - 1
                For t = 0 To tc - 1
                    Dim key = String.Format("g_{0}_{1}_{2}_{3}", gridId, p, a, t)
                    Dim val = ""
                    If measurements IsNot Nothing AndAlso measurements.ContainsKey(key) Then
                        val = measurements(key).ToString("F4")
                    End If
                    sb.Append("<td class='text-center p-1'><input type='text' name='" & key & "' value='" & val & "' class='form-control form-control-sm text-center' /></td>")
                Next
            Next
            sb.Append("</tr>")
        Next

        sb.Append("</tbody></table></div>")
        Return sb.ToString()
    End Function

    Private Function ReadGridValues(gridId As String) As Dictionary(Of String, Double)
        Dim data As New Dictionary(Of String, Double)
        For p = 0 To _partCount - 1
            For a = 0 To _appraiserCount - 1
                For t = 0 To _trialCount - 1
                    Dim key = String.Format("g_{0}_{1}_{2}_{3}", gridId, p, a, t)
                    Dim val = Request.Form(key)
                    If val IsNot Nothing Then
                        Dim d As Double
                        If Double.TryParse(val.Trim(), d) Then
                            data(key) = d
                        End If
                    End If
                Next
            Next
        Next
        Return data
    End Function

    Protected Sub btnCalculate_Click(sender As Object, e As EventArgs)
        Dim gridId = TryCast(ViewState("GridId"), String)
        If gridId Is Nothing Then
            litMessage.Text = "<div class='alert alert-warning'>Please setup the grid first.</div>"
            Return
        End If

        _partCount = CInt(ViewState("PartCount"))
        _appraiserCount = CInt(ViewState("AppraiserCount"))
        _trialCount = CInt(ViewState("TrialCount"))
        Dim partNames = TryCast(ViewState("PartNames"), String).Split(","c)
        Dim appraiserNames = TryCast(ViewState("AppraiserNames"), String).Split(","c)

        Dim rawData = ReadGridValues(gridId)
        If rawData.Count = 0 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>No measurement data entered.</div>"
            Return
        End If

        ' Build data cube: values(part, appraiser, trial)
        Dim n = _partCount
        Dim a = _appraiserCount
        Dim t = _trialCount
        Dim values(n, a, t) As Double
        Dim hasData = False

        For p = 0 To n - 1
            For ap = 0 To a - 1
                For tr = 0 To t - 1
                    Dim key = String.Format("g_{0}_{1}_{2}_{3}", gridId, p, ap, tr)
                    If rawData.ContainsKey(key) Then
                        values(p, ap, tr) = rawData(key)
                        If rawData(key) <> 0 Then hasData = True
                    End If
                Next
            Next
        Next

        If Not hasData Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter measurement values.</div>"
            Return
        End If

        ' Compute means
        ' Part-Appraiser means
        Dim partAppraiserMean(n, a) As Double
        For p = 0 To n - 1
            For ap = 0 To a - 1
                Dim sum = 0.0
                For tr = 0 To t - 1
                    sum += values(p, ap, tr)
                Next
                partAppraiserMean(p, ap) = sum / t
            Next
        Next

        ' Part means
        Dim partMean(n) As Double
        For p = 0 To n - 1
            Dim sum = 0.0
            For ap = 0 To a - 1
                For tr = 0 To t - 1
                    sum += values(p, ap, tr)
                Next
            Next
            partMean(p) = sum / (a * t)
        Next

        ' Appraiser means
        Dim appraiserMean(a) As Double
        For ap = 0 To a - 1
            Dim sum = 0.0
            For p = 0 To n - 1
                For tr = 0 To t - 1
                    sum += values(p, ap, tr)
                Next
            Next
            appraiserMean(ap) = sum / (n * t)
        Next

        ' Grand mean
        Dim grandMean = 0.0
        For p = 0 To n - 1
            For ap = 0 To a - 1
                For tr = 0 To t - 1
                    grandMean += values(p, ap, tr)
                Next
            Next
        Next
        grandMean /= (n * a * t)

        ' Sum of Squares
        Dim ssPart = 0.0
        Dim ssAppraiser = 0.0
        Dim ssInteraction = 0.0
        Dim ssEquipment = 0.0
        Dim ssTotal = 0.0

        For p = 0 To n - 1
            ssPart += (partMean(p) - grandMean) ^ 2
        Next
        ssPart *= (a * t)

        For ap = 0 To a - 1
            ssAppraiser += (appraiserMean(ap) - grandMean) ^ 2
        Next
        ssAppraiser *= (n * t)

        For p = 0 To n - 1
            For ap = 0 To a - 1
                ssInteraction += (partAppraiserMean(p, ap) - partMean(p) - appraiserMean(ap) + grandMean) ^ 2
            Next
        Next
        ssInteraction *= t

        For p = 0 To n - 1
            For ap = 0 To a - 1
                For tr = 0 To t - 1
                    ssEquipment += (values(p, ap, tr) - partAppraiserMean(p, ap)) ^ 2
                Next
            Next
        Next

        For p = 0 To n - 1
            For ap = 0 To a - 1
                For tr = 0 To t - 1
                    ssTotal += (values(p, ap, tr) - grandMean) ^ 2
                Next
            Next
        Next

        ' Degrees of Freedom
        Dim dfPart = n - 1
        Dim dfAppraiser = a - 1
        Dim dfInteraction = (n - 1) * (a - 1)
        Dim dfEquipment = n * a * (t - 1)
        Dim dfTotal = n * a * t - 1

        ' Mean Squares
        Dim msPart = ssPart / dfPart
        Dim msAppraiser = ssAppraiser / dfAppraiser
        Dim msInteraction = ssInteraction / dfInteraction
        Dim msEquipment = ssEquipment / dfEquipment

        ' Variance Components (clamp negative to 0)
        Dim varEV = msEquipment
        Dim varInteraction = Math.Max(0, (msInteraction - msEquipment) / t)
        Dim varAV = Math.Max(0, (msAppraiser - msInteraction) / (n * t))
        Dim varPart = Math.Max(0, (msPart - msInteraction) / (a * t))
        Dim varGRR = varEV + varAV + varInteraction
        Dim varTV = varGRR + varPart

        ' Standard Deviations
        Dim sdEV = Math.Sqrt(varEV)
        Dim sdAV = Math.Sqrt(varAV)
        Dim sdInteraction = Math.Sqrt(varInteraction)
        Dim sdGRR = Math.Sqrt(varGRR)
        Dim sdPart = Math.Sqrt(varPart)
        Dim sdTV = Math.Sqrt(varTV)

        ' Study Variation (5.15 = 99% confidence)
        Dim svEV = sdEV * 5.15
        Dim svAV = sdAV * 5.15
        Dim svInteraction = sdInteraction * 5.15
        Dim svGRR = sdGRR * 5.15
        Dim svPart = sdPart * 5.15
        Dim svTV = sdTV * 5.15

        ' Percentages
        Dim pctGRR = If(varTV > 0, (varGRR / varTV) * 100, 0)
        Dim pctEV = If(varTV > 0, (varEV / varTV) * 100, 0)
        Dim pctAV = If(varTV > 0, (varAV / varTV) * 100, 0)
        Dim pctInteraction = If(varTV > 0, (varInteraction / varTV) * 100, 0)
        Dim pctPart = If(varTV > 0, (varPart / varTV) * 100, 0)

        Dim pctSV_GRR = If(sdTV > 0, (sdGRR / sdTV) * 100, 0)
        Dim pctSV_EV = If(sdTV > 0, (sdEV / sdTV) * 100, 0)
        Dim pctSV_AV = If(sdTV > 0, (sdAV / sdTV) * 100, 0)
        Dim pctSV_Int = If(sdTV > 0, (sdInteraction / sdTV) * 100, 0)
        Dim pctSV_Part = If(sdTV > 0, (sdPart / sdTV) * 100, 0)

        ' ndc
        Dim ndc = 0
        If varGRR > 0 Then
            ndc = CInt(Math.Floor(1.41 * (sdPart / sdGRR)))
        End If

        ' Assessment
        Dim assessment As String
        Dim assessClass As String
        If pctGRR < 10 Then
            assessment = "Excellent"
            assessClass = "bg-success"
        ElseIf pctGRR < 30 Then
            assessment = "Acceptable"
            assessClass = "bg-warning text-dark"
        Else
            assessment = "Unacceptable"
            assessClass = "bg-danger"
        End If

        ' Build results JSON for saving
        Dim resultsJson = String.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}|{10}|{11}",
            pctGRR.ToString("F2"), ndc.ToString(),
            varGRR.ToString("F6"), sdGRR.ToString("F4"), svGRR.ToString("F4"), pctSV_GRR.ToString("F2"),
            varEV.ToString("F6"), sdEV.ToString("F4"), svEV.ToString("F4"),
            varAV.ToString("F6"), sdAV.ToString("F4"), svAV.ToString("F4"))

        ' Store in ViewState for saving
        ViewState("Results") = resultsJson
        ViewState("Conclusion") = assessment

        ' Render grid with values
        litGrid.Text = RenderGrid(_partCount, _appraiserCount, _trialCount, partNames, appraiserNames, rawData)

        ' Display results
        Dim cardClass = If(pctGRR < 10, "grr-pass", If(pctGRR < 30, "grr-warn", "grr-fail"))
        Dim cardBg = If(pctGRR < 10, "grr-pass", If(pctGRR < 30, "grr-warn", "grr-fail"))

        litPctGRR.Text = pctGRR.ToString("F1") & "%"
        litGRRAssessment.Text = "<span class='badge " & assessClass & " rounded-pill px-3'>" & assessment & "</span>"
        litNDC.Text = ndc.ToString()
        litTV.Text = sdTV.ToString("F4")
        litStudyVar.Text = svTV.ToString("F4")

        ' Set card color
        litPctGRR.Text = "<span style='color:" & If(pctGRR < 10, "#155724", If(pctGRR < 30, "#856404", "#721c24")) & ";'>" & pctGRR.ToString("F1") & "%</span>"

        ' Variance components table
        litVarGRR.Text = varGRR.ToString("F6")
        litSD_GRR.Text = sdGRR.ToString("F4")
        litSV_GRR.Text = svGRR.ToString("F4")
        litPctContGRR.Text = pctGRR.ToString("F2") & "%"
        litPctSV_GRR.Text = pctSV_GRR.ToString("F2") & "%"

        litVarEV.Text = varEV.ToString("F6")
        litSD_EV.Text = sdEV.ToString("F4")
        litSV_EV.Text = svEV.ToString("F4")
        litPctContEV.Text = pctEV.ToString("F2") & "%"
        litPctSV_EV.Text = pctSV_EV.ToString("F2") & "%"

        litVarAV.Text = varAV.ToString("F6")
        litSD_AV.Text = sdAV.ToString("F4")
        litSV_AV.Text = svAV.ToString("F4")
        litPctContAV.Text = pctAV.ToString("F2") & "%"
        litPctSV_AV.Text = pctSV_AV.ToString("F2") & "%"

        litVarPV.Text = varPart.ToString("F6")
        litSD_PV.Text = sdPart.ToString("F4")
        litSV_PV.Text = svPart.ToString("F4")
        litPctContPV.Text = pctPart.ToString("F2") & "%"
        litPctSV_PV.Text = pctSV_Part.ToString("F2") & "%"

        litVarTV.Text = varTV.ToString("F6")
        litSD_TV.Text = sdTV.ToString("F4")
        litSV_TV.Text = svTV.ToString("F4")

        ' Interpretation
        Dim interp As String
        If pctGRR < 10 Then
            interp = "<strong>Excellent:</strong> %GRR = " & pctGRR.ToString("F1") & "% (under 10%). The measurement system is acceptable for all applications including process capability analysis."
        ElseIf pctGRR < 20 Then
            interp = "<strong>Good:</strong> %GRR = " & pctGRR.ToString("F1") & "% (under 20%). The measurement system is acceptable for most applications."
        ElseIf pctGRR < 30 Then
            interp = "<strong>Marginal:</strong> %GRR = " & pctGRR.ToString("F1") & "% (between 20-30%). The measurement system may be acceptable based on importance of application, cost of gage, cost of repair, and other factors."
        Else
            interp = "<strong>Unacceptable:</strong> %GRR = " & pctGRR.ToString("F1") & "% (over 30%). Every effort should be made to improve the measurement system."
        End If

        If ndc < 5 Then
            interp &= " <strong>ndc = " & ndc & "</strong> (below 5). The measurement system cannot distinguish between parts effectively."
        Else
            interp &= " <strong>ndc = " & ndc & "</strong> (â‰¥ 5). The measurement system can distinguish between parts."
        End If

        litInterpretation.Text = interp

        pnlResults.Visible = True
        btnSave.Visible = True
        pnlGrid.Visible = True
        litMessage.Text = "<div class='alert alert-success py-1 small'>Calculation complete.</div>"
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then
            litMessage.Text = "<div class='alert alert-warning'>No project selected.</div>"
            Return
        End If

        Dim studyName = txtStudyName.Text.Trim()
        If studyName = "" Then studyName = "Gage R&R - " & DateTime.Today.ToString("MMM dd, yyyy")

        Dim partCount = CInt(ViewState("PartCount"))
        Dim appraiserCount = CInt(ViewState("AppraiserCount"))
        Dim trialCount = CInt(ViewState("TrialCount"))
        Dim partNames = TryCast(ViewState("PartNames"), String)
        Dim appraiserNames = TryCast(ViewState("AppraiserNames"), String)
        Dim resultsJson = TryCast(ViewState("Results"), String)
        Dim conclusion = TryCast(ViewState("Conclusion"), String)

        ' Collect measurements
        Dim gridId = TryCast(ViewState("GridId"), String)
        Dim measurements = ReadGridValues(gridId)
        Dim measurementsJson = SerializeMeasurements(measurements)

        Try
            Using cmd As New MySqlCommand(
                "INSERT INTO GageRnRStudies (TenantId, ProjectId, StudyName, PartCount, AppraiserCount, TrialCount, " &
                "PartNames, AppraiserNames, MeasurementsJson, ResultsJson, Conclusion, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @Name, @PC, @AC, @TC, @PN, @AN, @MJ, @RJ, @Conc, @UID)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", pid)
                cmd.Parameters.AddWithValue("@Name", studyName)
                cmd.Parameters.AddWithValue("@PC", partCount)
                cmd.Parameters.AddWithValue("@AC", appraiserCount)
                cmd.Parameters.AddWithValue("@TC", trialCount)
                cmd.Parameters.AddWithValue("@PN", partNames)
                cmd.Parameters.AddWithValue("@AN", appraiserNames)
                cmd.Parameters.AddWithValue("@MJ", measurementsJson)
                cmd.Parameters.AddWithValue("@RJ", resultsJson)
                cmd.Parameters.AddWithValue("@Conc", conclusion)
                cmd.Parameters.AddWithValue("@UID", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "GageRnR", Nothing, "Saved Gage R&R study: " & studyName)
            litMessage.Text = "<div class='alert alert-success py-1 small'>Study saved.</div>"
            LoadHistory()
        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger py-1 small'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Private Function SerializeMeasurements(data As Dictionary(Of String, Double)) As String
        Dim sb As New StringBuilder()
        For Each kv In data
            If sb.Length > 0 Then sb.Append(";")
            sb.Append(kv.Key & "=" & kv.Value.ToString("F4"))
        Next
        Return sb.ToString()
    End Function

    Private Function DeserializeMeasurements(raw As String) As Dictionary(Of String, Double)
        Dim result As New Dictionary(Of String, Double)
        If String.IsNullOrEmpty(raw) Then Return result
        Dim pairs = raw.Split(";"c)
        For Each pair In pairs
            Dim parts = pair.Split("="c)
            If parts.Length = 2 Then
                Dim d As Double
                If Double.TryParse(parts(1), d) Then
                    result(parts(0)) = d
                End If
            End If
        Next
        Return result
    End Function

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        pnlGrid.Visible = False
        pnlNoGrid.Visible = True
        pnlResults.Visible = False
        btnCalculate.Visible = False
        btnSave.Visible = False
        litMessage.Text = ""
    End Sub

    Protected Function GetGRRBadge(conclusion As String) As String
        Select Case conclusion.ToLower()
            Case "excellent" : Return "bg-success"
            Case "acceptable" : Return "bg-warning text-dark"
            Case "unacceptable" : Return "bg-danger"
            Case Else : Return "bg-secondary"
        End Select
    End Function

    Protected Function GetGRRValue(resultsJson As Object) As String
        If resultsJson Is Nothing OrElse resultsJson Is DBNull.Value Then Return ""
        Dim parts = resultsJson.ToString().Split("|"c)
        If parts.Length > 0 Then Return parts(0) & "%"
        Return ""
    End Function

    Protected Function GetNDCValue(resultsJson As Object) As String
        If resultsJson Is Nothing OrElse resultsJson Is DBNull.Value Then Return ""
        Dim parts = resultsJson.ToString().Split("|"c)
        If parts.Length > 1 Then Return parts(1)
        Return ""
    End Function

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "LoadStudy" Then
            Using cmd As New MySqlCommand("SELECT * FROM GageRnRStudies WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    Dim r = dt.Rows(0)
                    txtStudyName.Text = r("StudyName").ToString()
                    txtPartCount.Text = r("PartCount").ToString()
                    txtAppraiserCount.Text = r("AppraiserCount").ToString()
                    txtTrialCount.Text = r("TrialCount").ToString()
                    txtPartNames.Text = r("PartNames").ToString().Replace(",", ", ")
                    txtAppraiserNames.Text = r("AppraiserNames").ToString().Replace(",", ", ")

                    _partCount = CInt(r("PartCount"))
                    _appraiserCount = CInt(r("AppraiserCount"))
                    _trialCount = CInt(r("TrialCount"))
                    ViewState("PartCount") = _partCount
                    ViewState("AppraiserCount") = _appraiserCount
                    ViewState("TrialCount") = _trialCount

                    Dim partNames = r("PartNames").ToString().Split(","c)
                    Dim appraiserNames = r("AppraiserNames").ToString().Split(","c)
                    ViewState("PartNames") = r("PartNames").ToString()
                    ViewState("AppraiserNames") = r("AppraiserNames").ToString()

                    Dim measurements = DeserializeMeasurements(If(r("MeasurementsJson") IsNot DBNull.Value, r("MeasurementsJson").ToString(), ""))
                    litGrid.Text = RenderGrid(_partCount, _appraiserCount, _trialCount, partNames, appraiserNames, measurements)
                    pnlGrid.Visible = True
                    pnlNoGrid.Visible = False
                    btnCalculate.Visible = True
                    btnSave.Visible = False
                    pnlResults.Visible = False
                    litMessage.Text = "<div class='alert alert-info py-1 small'>Study loaded. Click Calculate to re-run analysis.</div>"
                End If
            End Using
        ElseIf e.CommandName = "DeleteStudy" Then
            Using cmd As New MySqlCommand("DELETE FROM GageRnRStudies WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "GageRnR", id, "Deleted Gage R&R study")
            LoadHistory()
        End If
    End Sub
End Class
