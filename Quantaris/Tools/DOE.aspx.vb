Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Text

Public Partial Class Tools_DOE
    Inherits BasePage

    Private Property DesignDataTable As DataTable
        Get
            Return CType(ViewState("DesignDataTable"), DataTable)
        End Get
        Set(value As DataTable)
            ViewState("DesignDataTable") = value
        End Set
    End Property

    Private Property FactorNameList As String()
        Get
            Return CType(ViewState("FactorNameList"), String())
        End Get
        Set(value As String())
            ViewState("FactorNameList") = value
        End Set
    End Property

    Private Property StoredANOVA As String
        Get
            Return CStr(ViewState("StoredANOVA"))
        End Get
        Set(value As String)
            ViewState("StoredANOVA") = value
        End Set
    End Property

    Private Property StoredEffects As String
        Get
            Return CStr(ViewState("StoredEffects"))
        End Get
        Set(value As String)
            ViewState("StoredEffects") = value
        End Set
    End Property

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlDOE.Visible = True
                pnlNoProject.Visible = False
                ShowFactorRows()
                LoadHistory()
            Else
                pnlDOE.Visible = False
                pnlNoProject.Visible = True
            End If
        Else
            If DesignDataTable IsNot Nothing Then
                CollectResponses()
                RebuildDesignMatrix()
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
            "SELECT Id, ExperimentName, DesignType, FactorCount, Runs, ResponseName, CreatedOn FROM DOE_Experiments WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            rptHistory.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataBind()
        End Using
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlDOE.Visible = True
            pnlNoProject.Visible = False
            ShowFactorRows()
            ClearDesign()
            LoadHistory()
        Else
            pnlDOE.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadHistory()
    End Sub

    Protected Sub ddlFactorCount_SelectedIndexChanged(sender As Object, e As EventArgs)
        ShowFactorRows()
        ClearDesign()
    End Sub

    Private Sub ShowFactorRows()
        Dim count = CInt(ddlFactorCount.SelectedValue)
        fr1.Visible = count >= 1
        fr2.Visible = count >= 2
        fr3.Visible = count >= 3
        fr4.Visible = count >= 4
        fr5.Visible = count >= 5
        fr6.Visible = count >= 6
        fr7.Visible = count >= 7
    End Sub

    Private Function GetFactorName(idx As Integer) As String
        Dim txt As TextBox = CType(Me.FindControl("txtFacName_" & idx), TextBox)
        If txt IsNot Nothing AndAlso txt.Text.Trim() <> "" Then Return txt.Text.Trim()
        Return Chr(64 + idx)
    End Function

    Private Function GetFactorLow(idx As Integer) As Double
        Dim txt As TextBox = CType(Me.FindControl("txtFacLow_" & idx), TextBox)
        If txt IsNot Nothing Then
            Dim val As Decimal
            If Decimal.TryParse(txt.Text.Trim(), val) Then Return CDbl(val)
        End If
        Return -1.0
    End Function

    Private Function GetFactorHigh(idx As Integer) As Double
        Dim txt As TextBox = CType(Me.FindControl("txtFacHigh_" & idx), TextBox)
        If txt IsNot Nothing Then
            Dim val As Decimal
            If Decimal.TryParse(txt.Text.Trim(), val) Then Return CDbl(val)
        End If
        Return 1.0
    End Function

    Private Sub ClearDesign()
        DesignDataTable = Nothing
        FactorNameList = Nothing
        pnlDesign.Visible = False
        pnlNoDesign.Visible = True
        pnlResults.Visible = False
        StoredANOVA = Nothing
        StoredEffects = Nothing
    End Sub

    Private Sub CollectResponses()
        Dim dt = DesignDataTable
        If dt Is Nothing Then Exit Sub
        For Each dr As DataRow In dt.Rows
            Dim run = dr("Run").ToString()
            Dim resp = Request.Form("resp_" & run)
            If resp IsNot Nothing Then
                dr("Response") = resp
            End If
        Next
        DesignDataTable = dt
    End Sub

    Private Sub RebuildDesignMatrix()
        Dim dt = DesignDataTable
        If dt Is Nothing Then
            pnlDesign.Visible = False
            pnlNoDesign.Visible = True
            Exit Sub
        End If
        Dim names = FactorNameList
        Dim sb As New StringBuilder()
        sb.Append("<table class='table table-bordered table-sm mb-0 text-center' style='font-size:0.85rem;'>")
        sb.Append("<thead class='table-dark'><tr><th>Run</th>")
        For i As Integer = 1 To dt.Columns.Count - 2
            Dim fname = If(names IsNot Nothing AndAlso i - 1 < names.Length, names(i - 1), "F" & i)
            sb.AppendFormat("<th>{0}</th>", fname)
        Next
        sb.AppendFormat("<th>{0}</th></tr></thead><tbody>", txtResponseName.Text.Trim())
        For Each dr As DataRow In dt.Rows
            Dim run = dr("Run").ToString()
            sb.AppendFormat("<tr><td class='fw-bold'>{0}</td>", run)
            For i As Integer = 1 To dt.Columns.Count - 2
                sb.AppendFormat("<td>{0}</td>", CInt(dr("F" & i)).ToString())
            Next
            Dim respVal = dr("Response").ToString()
            sb.AppendFormat("<td><input type='text' class='form-control form-control-sm text-center' id='resp_{0}' name='resp_{0}' value='{1}' style='width:80px;' /></td></tr>", run, HttpUtility.HtmlEncode(respVal))
        Next
        sb.Append("</tbody></table>")
        litDesignMatrix.Text = sb.ToString()
        pnlDesign.Visible = True
        pnlNoDesign.Visible = False
    End Sub

    Private Function GetDesignTypeLabel(dt As String) As String
        Select Case dt
            Case "FullFactorial" : Return "Full Factorial"
            Case "Fractional" : Return "Fractional Factorial"
            Case "PlackettBurman" : Return "Plackett-Burman"
            Case Else : Return dt
        End Select
    End Function

    Protected Sub btnGenerateDesign_Click(sender As Object, e As EventArgs)
        Try
            Dim factorCount = CInt(ddlFactorCount.SelectedValue)
            Dim designType = ddlDesignType.SelectedValue
            Dim runs As Integer

            Select Case designType
                Case "Fractional"
                    runs = CInt(Math.Pow(2, factorCount - 1))
                Case "PlackettBurman"
                    If factorCount <= 3 Then runs = 4 Else runs = 8
                Case Else
                    runs = CInt(Math.Pow(2, factorCount))
            End Select

            Dim dt As New DataTable()
            dt.Columns.Add("Run", GetType(Integer))
            For i As Integer = 1 To factorCount
                dt.Columns.Add("F" & i, GetType(Integer))
            Next
            dt.Columns.Add("Response", GetType(String))

            Dim genPB4() As Integer = {1, 1, -1}
            Dim genPB8() As Integer = {1, 1, 1, -1, 1, -1, -1}

            For run As Integer = 1 To runs
                Dim dr = dt.NewRow()
                dr("Run") = run
                Select Case designType
                    Case "FullFactorial"
                        For f As Integer = 1 To factorCount
                            Dim bit = (run - 1) >> (f - 1) And 1
                            dr("F" & f) = If(bit = 0, -1, 1)
                        Next
                    Case "Fractional"
                        If factorCount <= 1 Then
                            dr("F1") = If(run = 1, -1, 1)
                        Else
                            For f As Integer = 1 To factorCount - 1
                                Dim bit = (run - 1) >> (f - 1) And 1
                                dr("F" & f) = If(bit = 0, -1, 1)
                            Next
                            Dim prod = 1
                            For f As Integer = 1 To factorCount - 1
                                prod *= CInt(dr("F" & f))
                            Next
                            dr("F" & factorCount) = prod
                        End If
                    Case "PlackettBurman"
                        Dim n = runs
                        Dim gen() As Integer
                        If n = 4 Then gen = genPB4 Else gen = genPB8
                        If run = n Then
                            For f As Integer = 1 To factorCount
                                dr("F" & f) = -1
                            Next
                        Else
                            For f As Integer = 1 To factorCount
                                Dim idx = (f - 1 + run - 1) Mod (n - 1)
                                If idx < gen.Length Then dr("F" & f) = gen(idx) Else dr("F" & f) = -1
                            Next
                        End If
                End Select
                dr("Response") = ""
                dt.Rows.Add(dr)
            Next

            DesignDataTable = dt
            Dim names(factorCount - 1) As String
            For i As Integer = 1 To factorCount
                names(i - 1) = GetFactorName(i)
            Next
            FactorNameList = names
            pnlResults.Visible = False

            RebuildDesignMatrix()

            Dim designLabel = GetDesignTypeLabel(designType)
            litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>" & designLabel & " design generated: " & runs & " runs with " & factorCount & " factors.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"

        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error generating design: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnCalculateEffects_Click(sender As Object, e As EventArgs)
        Try
            CollectResponses()
            Dim dt = DesignDataTable
            If dt Is Nothing Then
                litMessage.Text = "<div class='alert alert-warning'>Please generate a design first.</div>"
                Return
            End If

            Dim factorCount = dt.Columns.Count - 2
            Dim runs = dt.Rows.Count
            Dim responses(runs - 1) As Double

            For i As Integer = 0 To runs - 1
                Dim val As Double
                If Not Double.TryParse(dt.Rows(i)("Response").ToString(), val) Then
                    litMessage.Text = "<div class='alert alert-warning'>Invalid or missing response value in run " & (i + 1) & ". Please enter numeric values for all runs.</div>"
                    Return
                End If
                responses(i) = val
            Next

            Dim terms As New List(Of String)()
            For i As Integer = 1 To factorCount
                terms.Add("F" & i)
            Next
            For i As Integer = 1 To factorCount - 1
                For j As Integer = i + 1 To factorCount
                    terms.Add("F" & i & "F" & j)
                Next
            Next
            For i As Integer = 1 To factorCount - 2
                For j As Integer = i + 1 To factorCount - 1
                    For k As Integer = j + 1 To factorCount
                        terms.Add("F" & i & "F" & j & "F" & k)
                    Next
                Next
            Next

            Dim design(runs - 1, factorCount - 1) As Integer
            For i As Integer = 0 To runs - 1
                For j As Integer = 1 To factorCount
                    design(i, j - 1) = CInt(dt.Rows(i)("F" & j))
                Next
            Next

            Dim effectVals As New Dictionary(Of String, Double)()
            Dim ssVals As New Dictionary(Of String, Double)()

            For Each term In terms
                Dim contrast As Double = 0
                For i As Integer = 0 To runs - 1
                    Dim sign = 1
                    Dim chars = term.Replace("F", "").ToCharArray()
                    For Each c In chars
                        Dim fi As Integer = Integer.Parse(c.ToString()) - 1
                        sign *= design(i, fi)
                    Next
                    contrast += sign * responses(i)
                Next
                Dim effect = contrast / (runs / 2.0)
                Dim ss = (contrast * contrast) / runs
                effectVals(term) = effect
                ssVals(term) = ss
            Next

            Dim errorSS As Double = 0
            Dim errorDF As Integer = 0
            For Each term In terms
                Dim level = term.Replace("F", "").Length
                If level >= 3 Then
                    errorSS += ssVals(term)
                    errorDF += 1
                End If
            Next

            Dim highOrderTerms As New List(Of String)()
            For Each term In terms
                Dim level = term.Replace("F", "").Length
                If level >= 3 Then highOrderTerms.Add(term)
            Next
            If errorDF = 0 AndAlso factorCount >= 2 Then
                For Each term In terms
                    Dim level = term.Replace("F", "").Length
                    If level = 2 Then
                        errorSS += ssVals(term)
                        errorDF += 1
                        highOrderTerms.Add(term)
                    End If
                Next
            End If

            Dim modelSS As Double = 0
            Dim modelDF As Integer = 0
            Dim anovaSB As New StringBuilder()

            anovaSB.Append("<table class='table table-bordered table-sm mb-0' style='font-size:0.85rem;'>")
            anovaSB.Append("<thead class='table-dark'><tr><th>Source</th><th class='text-end'>SS</th><th class='text-center'>df</th><th class='text-end'>MS</th><th class='text-end'>F</th><th class='text-end'>p-value</th></tr></thead><tbody>")

            Dim errorMS As Double = 0
            If errorDF > 0 Then errorMS = errorSS / errorDF

            For Each term In terms
                If highOrderTerms.Contains(term) Then Continue For
                Dim termName = FormatTermName(term)
                Dim ss = ssVals(term)
                Dim effect = effectVals(term)
                modelSS += ss
                modelDF += 1
                Dim ms = ss
                Dim fVal As Double = 0
                Dim pVal As String = "-"
                If errorMS > 0 AndAlso errorDF > 0 Then
                    fVal = ms / errorMS
                    pVal = FDistributionPValue(fVal, 1, errorDF).ToString("F4")
                End If
                anovaSB.AppendFormat("<tr><td>{0}</td><td class='text-end'>{1:F4}</td><td class='text-center'>1</td><td class='text-end'>{2:F4}</td><td class='text-end'>{3:F4}</td><td class='text-end'>{4}</td></tr>", termName, ss, ms, fVal, pVal)
            Next

            anovaSB.AppendFormat("<tr class='table-secondary'><td>Error</td><td class='text-end'>{0:F4}</td><td class='text-center'>{1}</td><td class='text-end'>{2:F4}</td><td class='text-end'>-</td><td class='text-end'>-</td></tr>", errorSS, errorDF, errorMS)
            Dim totalSS = modelSS + errorSS
            Dim totalDF = runs - 1
            anovaSB.AppendFormat("<tr class='table-light fw-bold'><td>Total</td><td class='text-end'>{0:F4}</td><td class='text-center'>{1}</td><td class='text-end'>-</td><td class='text-end'>-</td><td class='text-end'>-</td></tr>", totalSS, totalDF)
            anovaSB.Append("</tbody></table>")
            litANOVA.Text = anovaSB.ToString()
            StoredANOVA = anovaSB.ToString()

            Dim effSB As New StringBuilder()
            effSB.Append("<table class='table table-bordered table-sm mb-0' style='font-size:0.85rem;'>")
            effSB.Append("<thead class='table-dark'><tr><th>Term</th><th class='text-end'>Effect</th><th class='text-end'>Coefficient</th></tr></thead><tbody>")
            For Each term In terms
                Dim termName = FormatTermName(term)
                Dim effect = effectVals(term)
                Dim coeff = effect / 2.0
                effSB.AppendFormat("<tr><td>{0}</td><td class='text-end'>{1:F4}</td><td class='text-end'>{2:F4}</td></tr>", termName, effect, coeff)
            Next
            effSB.Append("</tbody></table>")
            litEffects.Text = effSB.ToString()
            StoredEffects = effSB.ToString()

            Dim sorted = terms.Select(Function(t) New With {Key .Term = t, .Effect = effectVals(t), .Abs = Math.Abs(effectVals(t))}).OrderByDescending(Function(x) x.Abs).ToList()
            Dim maxAbs = If(sorted.Count > 0 AndAlso sorted.First().Abs > 0, sorted.First().Abs, 1.0)

            Dim parSB As New StringBuilder()
            parSB.Append("<table class='table table-bordered table-sm mb-0' style='font-size:0.85rem;'>")
            parSB.Append("<thead class='table-dark'><tr><th>Term</th><th class='text-end'>Effect</th><th>Bar</th></tr></thead><tbody>")
            For Each item In sorted
                Dim termName = FormatTermName(item.Term)
                Dim pct = item.Abs / maxAbs * 100
                Dim barColor = If(item.Effect >= 0, "bg-primary", "bg-danger")
                parSB.AppendFormat("<tr><td>{0}</td><td class='text-end'>{1:F4}</td><td><div class='progress' style='height:22px;'><div class='progress-bar {2}' role='progressbar' style='width:{3:F0}%' aria-valuenow='{3:F0}' aria-valuemin='0' aria-valuemax='100'><small class='px-2'>{1:F4}</small></div></div></td></tr>", termName, item.Effect, barColor, pct)
            Next
            parSB.Append("</tbody></table>")
            litPareto.Text = parSB.ToString()

            pnlResults.Visible = True
            litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Effects calculated successfully.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"

        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error calculating effects: " & ex.Message & "</div>"
        End Try
    End Sub

    Private Function FormatTermName(term As String) As String
        Dim names = FactorNameList
        If names Is Nothing OrElse names.Length = 0 Then Return term
        Dim idxs = term.Replace("F", "").ToCharArray()
        Dim parts(idxs.Length - 1) As String
        For pi As Integer = 0 To idxs.Length - 1
            Dim fi As Integer
            If Integer.TryParse(idxs(pi).ToString(), fi) AndAlso fi >= 1 AndAlso fi <= names.Length Then
                parts(pi) = names(fi - 1)
            Else
                parts(pi) = idxs(pi).ToString()
            End If
        Next
        Return String.Join(" x ", parts)
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then
            litMessage.Text = "<div class='alert alert-warning'>Please select a project.</div>"
            Return
        End If
        If DesignDataTable Is Nothing Then
            litMessage.Text = "<div class='alert alert-warning'>Please generate a design first.</div>"
            Return
        End If

        Try
            Dim factorCount = CInt(ddlFactorCount.SelectedValue)
            Dim runs = DesignDataTable.Rows.Count

            Dim factorsJson = SerializeFactorsJson()
            Dim designJson = SerializeDesignMatrixJson()
            Dim responsesJson = SerializeResponsesJson()
            Dim anovaJson = SerializeANOVAJson()

            Using cmd As New MySqlCommand(
                "INSERT INTO DOE_Experiments (TenantId, ProjectId, ExperimentName, ResponseName, DesignType, FactorCount, Runs, FactorsJson, DesignMatrixJson, ResponsesJson, ANOVAJson, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @ExperimentName, @ResponseName, @DesignType, @FactorCount, @Runs, @FactorsJson, @DesignMatrixJson, @ResponsesJson, @ANOVAJson, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", pid)
                cmd.Parameters.AddWithValue("@ExperimentName", IIf(txtExperimentName.Text.Trim() = "", DBNull.Value, CObj(txtExperimentName.Text.Trim())))
                cmd.Parameters.AddWithValue("@ResponseName", txtResponseName.Text.Trim())
                cmd.Parameters.AddWithValue("@DesignType", ddlDesignType.SelectedValue)
                cmd.Parameters.AddWithValue("@FactorCount", factorCount)
                cmd.Parameters.AddWithValue("@Runs", runs)
                cmd.Parameters.AddWithValue("@FactorsJson", factorsJson)
                cmd.Parameters.AddWithValue("@DesignMatrixJson", designJson)
                cmd.Parameters.AddWithValue("@ResponsesJson", responsesJson)
                cmd.Parameters.AddWithValue("@ANOVAJson", anovaJson)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using

            LogAudit("Create", "DOE", Nothing, "Saved DOE experiment: " & txtExperimentName.Text)
            litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Experiment saved successfully.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
            LoadHistory()

        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error saving: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "LoadRecord" Then
            Using cmd As New MySqlCommand(
                "SELECT * FROM DOE_Experiments WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    Dim r = dt.Rows(0)
                    txtExperimentName.Text = If(r("ExperimentName") IsNot DBNull.Value, r("ExperimentName").ToString(), "")
                    txtResponseName.Text = If(r("ResponseName") IsNot DBNull.Value, r("ResponseName").ToString(), "Response")
                    ddlDesignType.SelectedValue = If(r("DesignType") IsNot DBNull.Value, r("DesignType").ToString(), "FullFactorial")
                    ddlFactorCount.SelectedValue = r("FactorCount").ToString()
                    ShowFactorRows()

                    Dim facJson = If(r("FactorsJson") IsNot DBNull.Value, r("FactorsJson").ToString(), "")
                    If facJson <> "" Then LoadFactorsFromJson(facJson)

                    Dim desJson = If(r("DesignMatrixJson") IsNot DBNull.Value, r("DesignMatrixJson").ToString(), "")
                    Dim resJson = If(r("ResponsesJson") IsNot DBNull.Value, r("ResponsesJson").ToString(), "")
                    Dim anovaDb = If(r("ANOVAJson") IsNot DBNull.Value, r("ANOVAJson").ToString(), "")

                    If desJson <> "" Then
                        LoadDesignFromJson(desJson, resJson)
                        RebuildDesignMatrix()
                    End If

                    If anovaDb <> "" Then
                        Dim partsAnova = anovaDb.Split(New String() {"|||"}, StringSplitOptions.None)
                        If partsAnova.Length >= 2 Then
                            litANOVA.Text = partsAnova(0)
                            litEffects.Text = partsAnova(1)
                            StoredANOVA = partsAnova(0)
                            StoredEffects = partsAnova(1)
                            If partsAnova.Length >= 3 Then
                                litPareto.Text = partsAnova(2)
                            End If
                            pnlResults.Visible = True
                        End If
                    End If

                    litMessage.Text = "<div class='alert alert-info'>Experiment loaded. Review and recalculate if needed.</div>"
                End If
            End Using
        ElseIf e.CommandName = "DeleteRecord" Then
            Using cmd As New MySqlCommand(
                "DELETE FROM DOE_Experiments WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
                LogAudit("Delete", "DOE", id, "Deleted DOE experiment")
                LoadHistory()
            End Using
        End If
    End Sub

    Private Sub LoadFactorsFromJson(json As String)
        Try
            Dim parts = json.Split("|"c)
            For i As Integer = 0 To parts.Length - 1
                Dim idx = i + 1
                Dim subParts = parts(i).Split(","c)
                If subParts.Length >= 3 Then
                    Dim txtName As TextBox = CType(Me.FindControl("txtFacName_" & idx), TextBox)
                    Dim txtLow As TextBox = CType(Me.FindControl("txtFacLow_" & idx), TextBox)
                    Dim txtHigh As TextBox = CType(Me.FindControl("txtFacHigh_" & idx), TextBox)
                    If txtName IsNot Nothing Then txtName.Text = subParts(0)
                    If txtLow IsNot Nothing Then txtLow.Text = subParts(1)
                    If txtHigh IsNot Nothing Then txtHigh.Text = subParts(2)
                End If
            Next
        Catch
        End Try
    End Sub

    Private Sub LoadDesignFromJson(desJson As String, resJson As String)
        Try
            Dim dt As New DataTable()
            dt.Columns.Add("Run", GetType(Integer))
            Dim rows = desJson.Split("|"c)
            Dim factorCount = 0
            If rows.Length > 0 Then
                Dim firstParts = rows(0).Split(","c)
                factorCount = firstParts.Length - 1
            End If
            For i As Integer = 1 To factorCount
                dt.Columns.Add("F" & i, GetType(Integer))
            Next
            dt.Columns.Add("Response", GetType(String))

            Dim responses = If(resJson <> "", resJson.Split("|"c), Nothing)

            For i As Integer = 0 To rows.Length - 1
                If rows(i).Trim() = "" Then Continue For
                Dim parts = rows(i).Split(","c)
                Dim dr = dt.NewRow()
                dr("Run") = CInt(parts(0))
                For j As Integer = 1 To Math.Min(parts.Length - 1, factorCount)
                    Dim val As Integer
                    If Integer.TryParse(parts(j), val) Then
                        dr("F" & j) = val
                    End If
                Next
                If responses IsNot Nothing AndAlso i < responses.Length Then
                    dr("Response") = responses(i)
                Else
                    dr("Response") = ""
                End If
                dt.Rows.Add(dr)
            Next

            DesignDataTable = dt

            Dim names(factorCount - 1) As String
            For i As Integer = 1 To factorCount
                names(i - 1) = GetFactorName(i)
            Next
            FactorNameList = names

        Catch
        End Try
    End Sub

    Private Function SerializeFactorsJson() As String
        Dim sb As New StringBuilder()
        Dim count = CInt(ddlFactorCount.SelectedValue)
        For i As Integer = 1 To count
            If sb.Length > 0 Then sb.Append("|")
            sb.Append(GetFactorName(i))
            sb.Append(",")
            sb.Append(GetFactorLow(i).ToString("F4"))
            sb.Append(",")
            sb.Append(GetFactorHigh(i).ToString("F4"))
        Next
        Return sb.ToString()
    End Function

    Private Function SerializeDesignMatrixJson() As String
        Dim dt = DesignDataTable
        If dt Is Nothing Then Return ""
        Dim sb As New StringBuilder()
        For Each dr As DataRow In dt.Rows
            If sb.Length > 0 Then sb.Append("|")
            sb.Append(dr("Run").ToString())
            For i As Integer = 1 To dt.Columns.Count - 2
                sb.Append(",")
                sb.Append(CInt(dr("F" & i)).ToString())
            Next
        Next
        Return sb.ToString()
    End Function

    Private Function SerializeResponsesJson() As String
        Dim dt = DesignDataTable
        If dt Is Nothing Then Return ""
        Dim sb As New StringBuilder()
        For Each dr As DataRow In dt.Rows
            If sb.Length > 0 Then sb.Append("|")
            sb.Append(dr("Response").ToString())
        Next
        Return sb.ToString()
    End Function

    Private Function SerializeANOVAJson() As String
        Dim sb As New StringBuilder()
        sb.Append(If(StoredANOVA, ""))
        sb.Append("|||")
        sb.Append(If(StoredEffects, ""))
        sb.Append("|||")
        sb.Append(litPareto.Text)
        Return sb.ToString()
    End Function

    Private Function FDistributionPValue(f As Double, df1 As Integer, df2 As Integer) As Double
        If f <= 0 Then Return 1.0
        If df1 <= 0 Or df2 <= 0 Then Return 1.0
        Dim a = df2 / 2.0
        Dim b = df1 / 2.0
        Dim x = df2 / (df2 + df1 * f)
        Return IncompleteBeta(a, b, x)
    End Function

    Private Function IncompleteBeta(a As Double, b As Double, x As Double) As Double
        If x < 0 Or x > 1 Then Return 0
        If x = 0 Or x = 1 Then Return x

        If x > (a + 1) / (a + b + 2) Then
            Return 1 - IncompleteBeta(b, a, 1 - x)
        End If

        Dim fpmin = 1.0e-300
        Dim eps = 3.0e-12
        Dim maxIter = 200
        Dim qab = a + b
        Dim qap = a + 1
        Dim qam = a - 1
        Dim c = 1.0
        Dim d = 1.0 - qab * x / qap
        If Math.Abs(d) < fpmin Then d = fpmin
        d = 1.0 / d
        Dim h = d

        For m As Integer = 1 To maxIter
            Dim m2 = 2 * m
            Dim aa = m * (b - m) * x / ((qam + m2) * (a + m2))
            d = 1.0 + aa * d
            If Math.Abs(d) < fpmin Then d = fpmin
            c = 1.0 + aa / c
            If Math.Abs(c) < fpmin Then c = fpmin
            d = 1.0 / d
            h *= d * c

            aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2))
            d = 1.0 + aa * d
            If Math.Abs(d) < fpmin Then d = fpmin
            c = 1.0 + aa / c
            If Math.Abs(c) < fpmin Then c = fpmin
            d = 1.0 / d
            Dim del = d * c
            h *= del

            If Math.Abs(del - 1.0) < eps Then Exit For
        Next

        Dim logBeta = LogGamma(a) + LogGamma(b) - LogGamma(a + b)
        Dim factor = a * Math.Log(x) + b * Math.Log(1 - x) - logBeta
        Dim result = h * Math.Exp(factor)

        If result < 0 Then Return 0
        If result > 1 Then Return 1
        Return result
    End Function

    Private Function LogGamma(x As Double) As Double
        Dim cof() As Double = {76.18009172947146, -86.50532032941677, 24.01409824083091, -1.231739572450155, 0.1208650973866179e-2, -0.5395239384953e-5}
        Dim ser = 1.000000000190015
        Dim y = x
        Dim tmp = x + 5.5
        tmp = (x + 0.5) * Math.Log(tmp) - tmp
        For j As Integer = 0 To 5
            y += 1
            ser += cof(j) / y
        Next
        Return tmp + Math.Log(2.5066282746310005 * ser / x)
    End Function
End Class
