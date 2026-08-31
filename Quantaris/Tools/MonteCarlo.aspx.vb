Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Text
Imports System.Text.RegularExpressions

Public Partial Class Tools_MonteCarlo
    Inherits BasePage

    Private _rng As Random = New Random()

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlMC.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlMC.Visible = False
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
            "SELECT m.*, " &
            "(SELECT ROUND(AVG(ResultValue), 4) FROM MonteCarloResults WHERE ModelId = m.Id) AS MeanResult, " &
            "(SELECT ROUND(STDDEV(ResultValue), 4) FROM MonteCarloResults WHERE ModelId = m.Id) AS StdDevResult " &
            "FROM MonteCarloModels m " &
            "WHERE m.TenantId = @TenantId AND m.ProjectId = @ProjectId ORDER BY m.CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataSource = dt
            rptHistory.DataBind()
        End Using
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlMC.Visible = True
            pnlNoProject.Visible = False
            pnlResults.Visible = False
            pnlNoResults.Visible = True
            LoadHistory()
        Else
            pnlMC.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then LoadHistory()
    End Sub

    Protected Function GetParamHint(distType As String) As String
        Select Case distType
            Case "Normal" : Return "Normal(Mean, StdDev)"
            Case "Uniform" : Return "Uniform(Min, Max)"
            Case "Triangular" : Return "Triangular(Min, Mode, Max)"
            Case Else : Return ""
        End Select
    End Function

    Protected Sub btnRun_Click(sender As Object, e As EventArgs)
        Dim iterations As Integer
        If Not Integer.TryParse(txtIterations.Text.Trim(), iterations) OrElse iterations < 100 OrElse iterations > 100000 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Iterations must be 100-100,000.</div>"
            Return
        End If

        Dim formula = txtFormula.Text.Trim()
        If formula = "" Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter an output formula.</div>"
            Return
        End If

        ' Read inputs from form
        Dim inputNames As New List(Of String)
        Dim inputDists As New List(Of String)
        Dim inputParams As New List(Of Double())

        Dim idx = 0
        While True
            Dim name = Request.Form("inp_name_" & idx)
            If name Is Nothing OrElse name.Trim() = "" Then
                If idx = 0 Then
                    litMessage.Text = "<div class='alert alert-warning py-1 small'>Add at least one input variable.</div>"
                    Return
                End If
                Exit While
            End If
            Dim dist = Request.Form("inp_dist_" & idx)
            Dim p1Str = Request.Form("inp_p1_" & idx)
            Dim p2Str = Request.Form("inp_p2_" & idx)

            Dim p1, p2 As Double
            If Not Double.TryParse(p1Str, p1) Then p1 = 0
            If dist = "Triangular" Then
                ' For triangular, we need a third param mode. Use p3 or just use mode from p2
                If Not Double.TryParse(p2Str, p2) Then p2 = 0
            Else
                If Not Double.TryParse(p2Str, p2) Then p2 = 0
            End If

            inputNames.Add(name.Trim())
            inputDists.Add(dist)
            inputParams.Add(New Double() {p1, p2})

            idx += 1
        End While

        ViewState("InputNames") = String.Join(",", inputNames.ToArray())
        ViewState("InputDists") = String.Join(",", inputDists.ToArray())
        ViewState("InputParams") = String.Join(";", inputParams.Select(Function(p) p(0) & "," & p(1)).ToArray())

        ' Run simulation
        Dim results(iterations - 1) As Double

        For i = 0 To iterations - 1
            Dim vars As New Dictionary(Of String, Double)
            For j = 0 To inputNames.Count - 1
                vars(inputNames(j)) = GenerateRandom(inputDists(j), inputParams(j))
            Next

            Dim result = EvaluateFormula(formula, vars)
            If Double.IsNaN(result) OrElse Double.IsInfinity(result) Then result = 0
            results(i) = result
        Next

        Array.Sort(results)

        ' Statistics
        Dim mean = results.Average()
        Dim sumSq = 0.0
        For Each v In results
            sumSq += (v - mean) ^ 2
        Next
        Dim stdDev = Math.Sqrt(sumSq / results.Length)

        litMean.Text = mean.ToString("F4")
        litStdDev.Text = stdDev.ToString("F4")
        litMin.Text = results(0).ToString("F4")
        litMax.Text = results(results.Length - 1).ToString("F4")

        ' Percentiles
        litP5.Text = results(CInt(Math.Floor(results.Length * 0.05))).ToString("F4")
        litP95.Text = results(CInt(Math.Floor(results.Length * 0.95)) - 1).ToString("F4")

        ' Histogram
        litHistogram.Text = RenderHistogram(results, mean, stdDev)

        ' Process capability
        Dim uslStr = txtUSL.Text.Trim()
        Dim lslStr = txtLSL.Text.Trim()
        Dim usl, lsl As Double
        Dim hasUSL = Double.TryParse(uslStr, usl)
        Dim hasLSL = Double.TryParse(lslStr, lsl)

        If hasUSL OrElse hasLSL Then
            pnlCapability.Visible = True
            Dim cp As Double = 0
            Dim cpk As Double = 0

            If stdDev > 0 Then
                Dim USL_val = If(hasUSL, usl, Double.MaxValue)
                Dim LSL_val = If(hasLSL, lsl, Double.MinValue)
                cp = (USL_val - LSL_val) / (6 * stdDev)

                Dim cpu = (USL_val - mean) / (3 * stdDev)
                Dim cpl = (mean - LSL_val) / (3 * stdDev)
                cpk = Math.Min(cpu, cpl)
            End If

            ' Defect rate
            Dim defectCount = 0
            For Each v In results
                If (hasLSL AndAlso v < lsl) OrElse (hasUSL AndAlso v > usl) Then
                    defectCount += 1
                End If
            Next
            Dim defectRate = CDbl(defectCount) / iterations * 100

            litCp.Text = cp.ToString("F4")
            litCpk.Text = cpk.ToString("F4")
            litDefectRate.Text = defectRate.ToString("F2") & "%"
        Else
            pnlCapability.Visible = False
        End If

        ' Store for saving
        ViewState("Results") = String.Join(",", results.Select(Function(r) r.ToString("F6")).ToArray())
        ViewState("Mean") = mean.ToString("F6")
        ViewState("StdDev") = stdDev.ToString("F6")

        pnlResults.Visible = True
        pnlNoResults.Visible = False
        btnSave.Visible = True
        litMessage.Text = "<div class='alert alert-success py-1 small'>Simulation complete.</div>"
    End Sub

    Private Function GenerateRandom(distType As String, params As Double()) As Double
        Select Case distType
            Case "Normal"
                Dim mean = params(0)
                Dim std = If(params(1) > 0, params(1), 1)
                Return BoxMuller(mean, std)
            Case "Uniform"
                Dim lo = params(0)
                Dim hi = If(params(1) > params(0), params(1), params(0) + 1)
                Return lo + _rng.NextDouble() * (hi - lo)
            Case "Triangular"
                Dim lo = params(0)
                Dim hi = If(params.Length > 2, params(2), lo + 1)
                Dim mode = params(1)
                If mode < lo OrElse mode > hi Then mode = (lo + hi) / 2
                Return Triangular(lo, mode, hi)
            Case Else
                Return 0
        End Select
    End Function

    Private Function BoxMuller(mean As Double, std As Double) As Double
        Dim u1 = 1.0 - _rng.NextDouble()
        Dim u2 = 1.0 - _rng.NextDouble()
        Dim z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)
        Return mean + std * z
    End Function

    Private Function Triangular(lo As Double, mode As Double, hi As Double) As Double
        Dim u = _rng.NextDouble()
        Dim f = (mode - lo) / (hi - lo)
        If u <= f Then
            Return lo + Math.Sqrt(u * (hi - lo) * (mode - lo))
        Else
            Return hi - Math.Sqrt((1 - u) * (hi - lo) * (hi - mode))
        End If
    End Function

    Private Function EvaluateFormula(formula As String, vars As Dictionary(Of String, Double)) As Double
        Dim expr = formula
        For Each kv In vars
            expr = Regex.Replace(expr, "\b" & kv.Key & "\b", kv.Value.ToString("F6"), RegexOptions.None)
        Next
        Try
            Dim dt As New Data.DataTable()
            Dim result = dt.Compute(expr, "")
            Return CDbl(result)
        Catch
            Return 0
        End Try
    End Function

    Private Function RenderHistogram(results As Double(), mean As Double, stdDev As Double) As String
        Dim nBins = 30
        Dim min = results(0)
        Dim max = results(results.Length - 1)
        Dim range = max - min
        If range = 0 Then Return "<div class='text-muted small'>No variation in results.</div>"

        Dim binSize = range / nBins
        Dim bins(nBins - 1) As Integer
        Dim maxCount = 0

        For Each v In results
            Dim binIdx = CInt(Math.Floor((v - min) / binSize))
            If binIdx >= nBins Then binIdx = nBins - 1
            bins(binIdx) += 1
            If bins(binIdx) > maxCount Then maxCount = bins(binIdx)
        Next

        Dim sb As New StringBuilder()
        For i = 0 To nBins - 1
            Dim pct = If(maxCount > 0, CDbl(bins(i)) / maxCount * 100, 0)
            Dim barLabel = (min + i * binSize + binSize / 2).ToString("F1")
            sb.Append("<div class='mc-bar' style='height:" & pct.ToString("F0") & "%' title='" & barLabel & ": " & bins(i) & "'></div>")
        Next
        Return sb.ToString()
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then
            litMessage.Text = "<div class='alert alert-warning'>No project selected.</div>"
            Return
        End If

        Dim modelName = txtModelName.Text.Trim()
        If modelName = "" Then modelName = "Monte Carlo - " & DateTime.Today.ToString("MMM dd, yyyy")

        Dim resultsStr = TryCast(ViewState("Results"), String)
        Dim meanStr = TryCast(ViewState("Mean"), String)
        Dim stdStr = TryCast(ViewState("StdDev"), String)
        Dim iterations = CInt(txtIterations.Text.Trim())

        If resultsStr Is Nothing Then
            litMessage.Text = "<div class='alert alert-warning'>No results to save. Run simulation first.</div>"
            Return
        End If

        Dim conn = DbHelper.GetConnection()
        Try
            conn.Open()
            Using tx = conn.BeginTransaction()
                Using cmd As New MySqlCommand(
                    "INSERT INTO MonteCarloModels (TenantId, ProjectId, ModelName, Iterations, OutputFormula, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @Name, @Iterations, @Formula, @UID)", conn, tx)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", pid)
                    cmd.Parameters.AddWithValue("@Name", modelName)
                    cmd.Parameters.AddWithValue("@Iterations", iterations)
                    cmd.Parameters.AddWithValue("@Formula", txtFormula.Text.Trim())
                    cmd.Parameters.AddWithValue("@UID", CurrentUserId)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "SELECT LAST_INSERT_ID()"
                    cmd.Parameters.Clear()
                    Dim modelId = CInt(cmd.ExecuteScalar())

                    ' Save inputs
                    Dim names = TryCast(ViewState("InputNames"), String).Split(","c)
                    Dim dists = TryCast(ViewState("InputDists"), String).Split(","c)
                    Dim paramsList = TryCast(ViewState("InputParams"), String).Split(";"c)

                    For i = 0 To names.Length - 1
                        Dim parts = paramsList(i).Split(","c)
                        Dim p1 As Double = 0
                        Dim p2 As Double = 0
                        Double.TryParse(parts(0), p1)
                        If parts.Length > 1 Then Double.TryParse(parts(1), p2)

                        Using icmd As New MySqlCommand(
                            "INSERT INTO MonteCarloInputs (ModelId, InputName, DistributionType, Param1, Param2) " &
                            "VALUES (@ModelId, @Name, @Dist, @P1, @P2)", conn, tx)
                            icmd.Parameters.AddWithValue("@ModelId", modelId)
                            icmd.Parameters.AddWithValue("@Name", names(i))
                            icmd.Parameters.AddWithValue("@Dist", dists(i))
                            icmd.Parameters.AddWithValue("@P1", p1)
                            icmd.Parameters.AddWithValue("@P2", If(p2 <> 0, p2, CObj(DBNull.Value)))
                            icmd.ExecuteNonQuery()
                        End Using
                    Next

                    ' Save results
                    Dim vals = resultsStr.Split(","c)
                    For i = 0 To vals.Length - 1
                        Dim v As Double
                        If Double.TryParse(vals(i), v) Then
                            Using rcmd As New MySqlCommand(
                                "INSERT INTO MonteCarloResults (ModelId, ResultIndex, ResultValue) VALUES (@ModelId, @Idx, @Val)",
                                conn, tx)
                                rcmd.Parameters.AddWithValue("@ModelId", modelId)
                                rcmd.Parameters.AddWithValue("@Idx", i)
                                rcmd.Parameters.AddWithValue("@Val", v)
                                rcmd.ExecuteNonQuery()
                            End Using
                        End If
                    Next

                    tx.Commit()
                End Using
            End Using
            LogAudit("Create", "MonteCarloModel", Nothing, "Saved Monte Carlo: " & modelName)
            litMessage.Text = "<div class='alert alert-success py-1 small'>Model saved (" & iterations & " iterations).</div>"
            LoadHistory()
        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger py-1 small'>Error: " & ex.Message & "</div>"
        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        pnlResults.Visible = False
        pnlNoResults.Visible = True
        btnSave.Visible = False
        litMessage.Text = ""
    End Sub

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "LoadModel" Then
            ' Load model into UI
        ElseIf e.CommandName = "DeleteModel" Then
            Using cmd As New MySqlCommand("DELETE FROM MonteCarloModels WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "MonteCarloModel", id, "Deleted Monte Carlo model")
            LoadHistory()
        End If
    End Sub
End Class
