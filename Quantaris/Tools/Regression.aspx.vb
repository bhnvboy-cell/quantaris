Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Web.Script.Serialization

Public Partial Class Tools_Regression
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlReg.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlReg.Visible = False
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
            "SELECT * FROM RegressionModels WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataSource = dt
            rptHistory.DataBind()
        End Using
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlReg.Visible = True
            pnlNoProject.Visible = False
            pnlResults.Visible = False
            pnlNoResults.Visible = True
            LoadHistory()
        Else
            pnlReg.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then LoadHistory()
    End Sub

    Private Function GetSpreadsheetData() As Dictionary(Of String, Object)
        Dim raw = hfSpreadsheetData.Value
        If String.IsNullOrEmpty(raw) Then Return Nothing
        Try
            Dim js As New JavaScriptSerializer()
            Return js.Deserialize(Of Dictionary(Of String, Object))(raw)
        Catch
            Return Nothing
        End Try
    End Function

    Protected Sub btnRun_Click(sender As Object, e As EventArgs)
        Dim yName = txtYName.Text.Trim()
        Dim xNamesRaw = txtXNames.Text.Trim()

        If yName = "" Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter response (Y) name.</div>"
            Return
        End If

        Dim xNames As List(Of String)
        If xNamesRaw <> "" Then
            xNames = xNamesRaw.Split(","c).Select(Function(s) s.Trim()).Where(Function(s) s <> "").ToList()
        Else
            xNames = New List(Of String)()
        End If

        Dim spreadsheetData = GetSpreadsheetData()
        If spreadsheetData Is Nothing Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter data in the spreadsheet.</div>"
            Return
        End If

        Dim rows = TryCast(spreadsheetData("rows"), List(Of Object))
        Dim columns = TryCast(spreadsheetData("columns"), List(Of Object))
        If rows Is Nothing OrElse columns Is Nothing OrElse columns.Count < 2 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Need at least Y and one X column in the spreadsheet.</div>"
            Return
        End If

        Dim dataRows As New List(Of Double())
        For Each row In rows
            Dim rowList = TryCast(row, List(Of Object))
            If rowList IsNot Nothing AndAlso rowList.Count >= 2 Then
                Dim vals(rowList.Count - 1) As Double
                Dim hasData = False
                For c = 0 To rowList.Count - 1
                    Dim cellStr = If(rowList(c) Is Nothing, "", rowList(c).ToString().Trim())
                    If cellStr <> "" Then hasData = True
                    Double.TryParse(cellStr, vals(c))
                Next
                If hasData Then dataRows.Add(vals)
            End If
        Next

        Dim k = columns.Count - 1
        Dim n = dataRows.Count
        If n < k + 2 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Need at least " & (k + 2) & " observations.</div>"
            Return
        End If

        ' Auto-populate xNames if not provided
        If xNames.Count < k Then
            xNames.Clear()
            For c = 1 To columns.Count - 1
                Dim colDict = TryCast(columns(c), Dictionary(Of String, Object))
                If colDict IsNot Nothing AndAlso colDict.ContainsKey("label") Then
                    xNames.Add(colDict("label").ToString())
                Else
                    xNames.Add("X" & c)
                End If
            Next
        End If

        Dim y(n - 1) As Double
        Dim x(n - 1, k) As Double

        For i = 0 To n - 1
            Dim parts = dataRows(i)
            y(i) = parts(0)
            x(i, 0) = 1.0
            For j = 1 To k
                If j < parts.Length Then x(i, j) = parts(j)
            Next
        Next

        ' OLS: b = (X'X)^(-1) X'Y
        Dim p = k + 1
        Dim xtx(p - 1, p - 1) As Double
        Dim xty(p - 1) As Double

        For r = 0 To p - 1
            For c = 0 To p - 1
                Dim sum = 0.0
                For i = 0 To n - 1
                    sum += x(i, r) * x(i, c)
                Next
                xtx(r, c) = sum
            Next
            Dim sumY = 0.0
            For i = 0 To n - 1
                sumY += x(i, r) * y(i)
            Next
            xty(r) = sumY
        Next

        Dim inv(p - 1, p - 1) As Double
        If Not InvertMatrix(xtx, inv, p) Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Matrix is singular. Check for collinear predictors.</div>"
            Return
        End If

        Dim b(p - 1) As Double
        For r = 0 To p - 1
            Dim sum = 0.0
            For c = 0 To p - 1
                sum += inv(r, c) * xty(c)
            Next
            b(r) = sum
        Next

        Dim yHat(n - 1) As Double
        Dim residuals(n - 1) As Double
        Dim sse = 0.0
        Dim sst = 0.0
        Dim yBar = y.Average()

        For i = 0 To n - 1
            Dim pred = 0.0
            For j = 0 To p - 1
                pred += b(j) * x(i, j)
            Next
            yHat(i) = pred
            residuals(i) = y(i) - pred
            sse += residuals(i) ^ 2
            sst += (y(i) - yBar) ^ 2
        Next

        Dim mse = sse / (n - p)
        Dim se = Math.Sqrt(mse)
        Dim rsq = 1.0 - sse / sst
        Dim adjRsq = 1.0 - (sse / (n - p)) / (sst / (n - 1))
        Dim fStat = If(mse > 0, ((sst - sse) / (p - 1)) / mse, 0)

        Dim seCoeff(p - 1) As Double
        Dim tStat(p - 1) As Double

        For j = 0 To p - 1
            seCoeff(j) = Math.Sqrt(mse * inv(j, j))
            tStat(j) = If(seCoeff(j) > 0, b(j) / seCoeff(j), 0)
        Next

        litRSq.Text = rsq.ToString("F4")
        litAdjRSq.Text = adjRsq.ToString("F4")
        litFStat.Text = fStat.ToString("F4")
        litStdErr.Text = se.ToString("F4")

        Dim sb As New StringBuilder()
        sb.Append("<tr><td>Intercept</td><td class='text-end'>" & b(0).ToString("F4") & "</td>")
        sb.Append("<td class='text-end'>" & seCoeff(0).ToString("F4") & "</td>")
        sb.Append("<td class='text-end'>" & tStat(0).ToString("F4") & "</td>")
        sb.Append("<td class='text-end'>" & PValueT(tStat(0), n - p).ToString("F4") & "</td></tr>")

        For j = 1 To k
            Dim xName = If(j - 1 < xNames.Count, xNames(j - 1), "X" & j)
            sb.Append("<tr><td>" & HttpUtility.HtmlEncode(xName) & "</td>")
            sb.Append("<td class='text-end'>" & b(j).ToString("F4") & "</td>")
            sb.Append("<td class='text-end'>" & seCoeff(j).ToString("F4") & "</td>")
            sb.Append("<td class='text-end'>" & tStat(j).ToString("F4") & "</td>")
            sb.Append("<td class='text-end'>" & PValueT(tStat(j), n - p).ToString("F4") & "</td></tr>")
        Next
        litCoefficients.Text = sb.ToString()

        litScatter.Text = RenderScatter(y, yHat)
        litResiduals.Text = RenderResiduals(yHat, residuals)

        ViewState("YName") = yName
        ViewState("XNames") = String.Join(",", xNames)
        ViewState("Coeffs") = String.Join(",", b.Select(Function(v) v.ToString("F6")))
        ViewState("RSq") = rsq.ToString("F6")
        ViewState("FStat") = fStat.ToString("F6")

        pnlResults.Visible = True
        pnlNoResults.Visible = False
        btnSave.Visible = True
        litMessage.Text = "<div class='alert alert-success py-1 small'>Regression complete.</div>"
    End Sub

    Private Function InvertMatrix(ByVal a(,) As Double, ByRef inv(,) As Double, n As Integer) As Boolean
        Dim aug(n - 1, 2 * n - 1) As Double
        For i = 0 To n - 1
            For j = 0 To n - 1
                aug(i, j) = a(i, j)
            Next
            aug(i, n + i) = 1.0
        Next

        For col = 0 To n - 1
            Dim pivot = col
            For row = col + 1 To n - 1
                If Math.Abs(aug(row, col)) > Math.Abs(aug(pivot, col)) Then pivot = row
            Next
            If Math.Abs(aug(pivot, col)) < 1e-12 Then Return False

            For j = 0 To 2 * n - 1
                Dim tmp = aug(col, j)
                aug(col, j) = aug(pivot, j)
                aug(pivot, j) = tmp
            Next

            Dim pivVal = aug(col, col)
            For j = 0 To 2 * n - 1
                aug(col, j) /= pivVal
            Next

            For row = 0 To n - 1
                If row <> col Then
                    Dim factor = aug(row, col)
                    For j = 0 To 2 * n - 1
                        aug(row, j) -= factor * aug(col, j)
                    Next
                End If
            Next
        Next

        For i = 0 To n - 1
            For j = 0 To n - 1
                inv(i, j) = aug(i, n + j)
            Next
        Next
        Return True
    End Function

    Private Function PValueT(t As Double, df As Integer) As Double
        Dim x = df / (df + t * t)
        Return BetaInc(x, df / 2.0, 0.5)
    End Function

    Private Function BetaInc(x As Double, a As Double, b As Double) As Double
        If x < 0 OrElse x > 1 Then Return 0
        Dim bt As Double
        If x = 0 OrElse x = 1 Then
            bt = 0
        Else
            bt = ExpGamma(a + b) - ExpGamma(a) - ExpGamma(b) + a * Math.Log(x) + b * Math.Log(1.0 - x)
            bt = Math.Exp(bt)
        End If
        If x < (a + 1.0) / (a + b + 2.0) Then
            Return bt * BetaCF(x, a, b) / a
        Else
            Return 1.0 - bt * BetaCF(1.0 - x, b, a) / b
        End If
    End Function

    Private Function ExpGamma(z As Double) As Double
        Dim coeff() As Double = {76.18009173, -86.50532033, 24.01409822, -1.231739516, 0.00120858003, -0.00000536382}
        Dim x = z
        Dim y = x
        Dim tmp = x + 5.5
        tmp = (x + 0.5) * Math.Log(tmp) - tmp
        Dim ser = 1.00000000019
        For j = 0 To 5
            y += 1
            ser += coeff(j) / y
        Next
        Return tmp + Math.Log(2.50662827465 * ser / x)
    End Function

    Private Function BetaCF(x As Double, a As Double, b As Double) As Double
        Dim maxIter = 100
        Dim eps = 1e-10
        Dim qab = a + b
        Dim qap = a + 1.0
        Dim qam = a - 1.0
        Dim c = 1.0
        Dim d = 1.0 - qab * x / qap
        If Math.Abs(d) < eps Then d = eps
        d = 1.0 / d
        Dim h = d
        For m = 1 To maxIter
            Dim m2 = 2 * m
            Dim aa = m * (b - m) * x / ((qam + m2) * (a + m2))
            d = 1.0 + aa * d
            If Math.Abs(d) < eps Then d = eps
            c = 1.0 + aa / c
            If Math.Abs(c) < eps Then c = eps
            d = 1.0 / d
            h *= d * c
            aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2))
            d = 1.0 + aa * d
            If Math.Abs(d) < eps Then d = eps
            c = 1.0 + aa / c
            If Math.Abs(c) < eps Then c = eps
            d = 1.0 / d
            Dim del = d * c
            h *= del
            If Math.Abs(del - 1.0) < eps Then Exit For
        Next
        Return h
    End Function

    Private Function RenderScatter(actual As Double(), fitted As Double()) As String
        Dim sb As New StringBuilder()
        Dim n = actual.Length
        Dim yMin = Math.Min(actual.Min(), fitted.Min())
        Dim yMax = Math.Max(actual.Max(), fitted.Max())
        Dim yRange = yMax - yMin
        If yRange = 0 Then yRange = 1
        Dim height = 260
        Dim width = 100

        sb.Append("<div class='cc-yaxis'>")
        For i = 0 To 4
            Dim yVal = yMin + yRange / 4 * i
            Dim yPos = height - (yVal - yMin) / yRange * height
            sb.Append("<span style='bottom:" & yPos.ToString("F0") & "px;'>" & yVal.ToString("F2") & "</span>")
        Next
        sb.Append("</div>")

        sb.Append("<div style='position:absolute;left:65px;right:20px;top:10px;bottom:20px;'>")
        sb.Append("<div class='reg-line' style='left:0;bottom:0;width:100%;' title='Perfect fit'></div>")

        For i = 0 To n - 1
            Dim xPos = (fitted(i) - yMin) / yRange * width
            Dim yPos = (actual(i) - yMin) / yRange * height
            sb.Append("<div class='reg-point' style='left:" & xPos.ToString("F1") & "%;bottom:" & yPos.ToString("F0") & "px;' title='Obs:" & actual(i).ToString("F3") & " Pred:" & fitted(i).ToString("F3") & "'></div>")
        Next

        sb.Append("</div>")
        Return sb.ToString()
    End Function

    Private Function RenderResiduals(fitted As Double(), residuals As Double()) As String
        Dim sb As New StringBuilder()
        Dim n = fitted.Length
        Dim maxResid = Math.Max(Math.Abs(residuals.Min()), Math.Abs(residuals.Max()))
        If maxResid = 0 Then maxResid = 1
        Dim height = 260
        Dim width = 100

        sb.Append("<div style='position:absolute;left:65px;right:20px;top:10px;bottom:20px;'>")

        Dim zeroY = height / 2
        sb.Append("<div style='position:absolute;left:0;right:0;bottom:" & zeroY & "px;border-top:1px dashed #6c757d;'></div>")

        For i = 0 To n - 1
            Dim xPos = CDbl(i) / (n - 1) * width
            Dim rHt = Math.Abs(residuals(i)) / maxResid * (height / 2 - 10)
            Dim yPos = If(residuals(i) >= 0, zeroY - rHt, zeroY)
            Dim cls = If(residuals(i) >= 0, "resid-bar", "resid-bar neg")
            sb.Append("<div class='" & cls & "' style='left:" & xPos.ToString("F1") & "%;bottom:" & yPos.ToString("F0") & "px;height:" & rHt.ToString("F0") & "px;' title='Residual: " & residuals(i).ToString("F4") & "'></div>")
        Next

        sb.Append("</div>")
        Return sb.ToString()
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Return
        Dim modelName = txtModelName.Text.Trim()
        If modelName = "" Then modelName = "Regression - " & DateTime.Today.ToString("MMM dd, yyyy")
        Dim yName = TryCast(ViewState("YName"), String)
        Dim xNames = TryCast(ViewState("XNames"), String)

        Dim spreadsheetData = GetSpreadsheetData()
        If spreadsheetData Is Nothing Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>No data to save.</div>"
            Return
        End If

        Dim rows = TryCast(spreadsheetData("rows"), List(Of Object))
        If rows Is Nothing Then rows = New List(Of Object)()

        Dim conn = DbHelper.GetConnection()
        Try
            conn.Open()
            Using tx = conn.BeginTransaction()
                Using cmd As New MySqlCommand(
                    "INSERT INTO RegressionModels (TenantId, ProjectId, ModelName, ResponseName, PredictorNames, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @Name, @Resp, @Preds, @UID)", conn, tx)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", pid)
                    cmd.Parameters.AddWithValue("@Name", modelName)
                    cmd.Parameters.AddWithValue("@Resp", If(yName, ""))
                    cmd.Parameters.AddWithValue("@Preds", If(xNames, ""))
                    cmd.Parameters.AddWithValue("@UID", CurrentUserId)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "SELECT LAST_INSERT_ID()"
                    cmd.Parameters.Clear()
                    Dim modelId = CInt(cmd.ExecuteScalar())

                    Dim dataIdx = 0
                    For Each row In rows
                        Dim rowList = TryCast(row, List(Of Object))
                        If rowList IsNot Nothing AndAlso rowList.Count >= 2 Then
                            Dim yv As Double
                            Double.TryParse(If(rowList(0) Is Nothing, "", rowList(0).ToString().Trim()), yv)
                            Dim predVals As New List(Of String)()
                            For c = 1 To rowList.Count - 1
                                predVals.Add(If(rowList(c) Is Nothing, "", rowList(c).ToString().Trim()))
                            Next
                            Using dcmd As New MySqlCommand(
                                "INSERT INTO RegressionData (ModelId, DataIndex, ResponseValue, PredictorValues) " &
                                "VALUES (@ModelId, @Idx, @RespVal, @PredVals)", conn, tx)
                                dcmd.Parameters.AddWithValue("@ModelId", modelId)
                                dcmd.Parameters.AddWithValue("@Idx", dataIdx)
                                dcmd.Parameters.AddWithValue("@RespVal", yv)
                                dcmd.Parameters.AddWithValue("@PredVals", String.Join(",", predVals))
                                dcmd.ExecuteNonQuery()
                            End Using
                            dataIdx += 1
                        End If
                    Next
                    tx.Commit()
                End Using
            End Using
            LogAudit("Create", "RegressionModel", Nothing, "Saved regression: " & modelName)
            litMessage.Text = "<div class='alert alert-success py-1 small'>Model saved.</div>"
            LoadHistory()
        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger py-1 small'>Error: " & ex.Message & "</div>"
        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "DeleteModel" Then
            Using cmd As New MySqlCommand("DELETE FROM RegressionModels WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "RegressionModel", id, "Deleted regression model")
            LoadHistory()
        ElseIf e.CommandName = "LoadModel" Then
            LoadModel(id)
        End If
    End Sub

    Private Sub LoadModel(modelId As Integer)
        Dim modelName As String = ""
        Dim respName As String = ""
        Dim predNames As String = ""

        Using cmd As New MySqlCommand(
            "SELECT ModelName, ResponseName, PredictorNames FROM RegressionModels WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", modelId)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                modelName = If(IsDBNull(dt.Rows(0)("ModelName")), "", dt.Rows(0)("ModelName").ToString())
                respName = If(IsDBNull(dt.Rows(0)("ResponseName")), "", dt.Rows(0)("ResponseName").ToString())
                predNames = If(IsDBNull(dt.Rows(0)("PredictorNames")), "", dt.Rows(0)("PredictorNames").ToString())
            End If
        End Using

        txtModelName.Text = modelName
        txtYName.Text = respName
        txtXNames.Text = predNames

        Dim columns As New List(Of Object)()
        columns.Add(New Dictionary(Of String, String) From {{"name", "C1"}, {"label", If(respName, "Y")}})

        Dim preds = predNames.Split(","c).Select(Function(s) s.Trim()).Where(Function(s) s <> "").ToList()
        If preds.Count = 0 Then preds.Add("X1")
        For i = 0 To preds.Count - 1
            columns.Add(New Dictionary(Of String, String) From {{"name", "C" & (i + 2)}, {"label", preds(i)}})
        Next

        Dim dataRows As New List(Of Object)()
        Using cmd As New MySqlCommand(
            "SELECT ResponseValue, PredictorValues FROM RegressionData WHERE ModelId = @ModelId ORDER BY DataIndex")
            cmd.Parameters.AddWithValue("@ModelId", modelId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            For Each row As DataRow In dt.Rows
                Dim respVal = If(IsDBNull(row("ResponseValue")), "", row("ResponseValue").ToString())
                Dim predVals = If(IsDBNull(row("PredictorValues")), "", row("PredictorValues").ToString())
                Dim rowArr As New List(Of Object)()
                rowArr.Add(respVal)
                For Each p In predVals.Split(","c)
                    rowArr.Add(p.Trim())
                Next
                dataRows.Add(rowArr)
            Next
        End Using

        Dim spreadsheetObj As New Dictionary(Of String, Object) From {{"columns", columns}, {"rows", dataRows}}
        Dim js As New JavaScriptSerializer()
        Dim json = js.Serialize(spreadsheetObj)

        ClientScript.RegisterStartupScript(Me.GetType(), "loadRegModel_" & modelId,
            "loadRegSpreadsheetData('" & json.Replace("'", "\'") & "');", True)

        pnlResults.Visible = False
        pnlNoResults.Visible = True
        btnSave.Visible = False
        litMessage.Text = "<div class='alert alert-info py-1 small'>Loaded model: " & HttpUtility.HtmlEncode(modelName) & ". Click Run Regression to view.</div>"
    End Sub
End Class
