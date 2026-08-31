Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Text
Imports System.Collections.Generic
Imports System.Web.Script.Serialization

Public Partial Class Tools_ControlCharts
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlCC.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlCC.Visible = False
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
            "SELECT m.*, (SELECT COUNT(*) FROM ControlChartData WHERE ModelId = m.Id) SampleCount " &
            "FROM ControlChartModels m WHERE m.TenantId = @TenantId AND m.ProjectId = @ProjectId ORDER BY m.CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataSource = dt
            rptHistory.DataBind()
        End Using
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlCC.Visible = True
            pnlNoProject.Visible = False
            pnlChart.Visible = False
            pnlNoChart.Visible = True
            LoadHistory()
        Else
            pnlCC.Visible = False
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

    Protected Sub btnCalculate_Click(sender As Object, e As EventArgs)
        Dim spreadsheetData = GetSpreadsheetData()
        If spreadsheetData Is Nothing Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>No data entered.</div>"
            Return
        End If

        Dim rows = TryCast(spreadsheetData("rows"), List(Of Object))
        Dim columns = TryCast(spreadsheetData("columns"), List(Of Object))

        If rows Is Nothing OrElse rows.Count = 0 OrElse columns Is Nothing OrElse columns.Count = 0 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>No data entered.</div>"
            Return
        End If

        Dim chartType = ddlChartType.SelectedValue
        Dim parsedSamples As New List(Of List(Of Double))
        For Each row In rows
            Dim rowList = TryCast(row, List(Of Object))
            If rowList IsNot Nothing Then
                Dim values As New List(Of Double)
                For Each cell In rowList
                    Dim cellStr = If(cell Is Nothing, "", cell.ToString().Trim())
                    Dim v As Double
                    If Double.TryParse(cellStr, v) Then values.Add(v)
                Next
                If values.Count > 0 Then parsedSamples.Add(values)
            End If
        Next

        If parsedSamples.Count < 2 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter at least 2 rows of data.</div>"
            Return
        End If

        Dim sgSize As Integer = 1

        If chartType = "XbarR" Then
            Dim expectedCols = columns.Count
            For Each s In parsedSamples
                If s.Count < expectedCols Then
                    litMessage.Text = "<div class='alert alert-warning py-1 small'>All X-bar R rows must have the same number of values.</div>"
                    Return
                End If
            Next
            sgSize = columns.Count
        End If

        Dim cl, ucl, lcl As Double
        Dim clR, uclR, lclR As Double
        Dim xValues As New List(Of Double)
        Dim rValues As New List(Of Double)
        Dim ocPoints As New List(Of Integer)
        Dim rOcPoints As New List(Of Integer)

        Select Case chartType
            Case "IMR"
                Dim individuals As New List(Of Double)
                For Each s In parsedSamples
                    individuals.Add(s(0))
                Next
                Dim movingRanges As New List(Of Double)
                For i = 1 To individuals.Count - 1
                    movingRanges.Add(Math.Abs(individuals(i) - individuals(i - 1)))
                Next
                xValues = individuals
                cl = individuals.Average()
                Dim mrBar = If(movingRanges.Count > 0, movingRanges.Average(), 0)
                ucl = cl + 2.66 * mrBar
                lcl = cl - 2.66 * mrBar
                rValues = movingRanges
                clR = mrBar
                uclR = 3.267 * mrBar
                lclR = 0
                sgSize = 1

            Case "XbarR"
                sgSize = columns.Count
                For Each s In parsedSamples
                    xValues.Add(s.Average())
                    rValues.Add(s.Max() - s.Min())
                Next
                cl = xValues.Average()
                Dim rBar = rValues.Average()
                Dim d2 = GetD2(sgSize)
                Dim d3 = GetD3(sgSize)
                Dim d4 = GetD4(sgSize)
                ucl = cl + 3 * rBar / (d2 * Math.Sqrt(sgSize))
                lcl = cl - 3 * rBar / (d2 * Math.Sqrt(sgSize))
                clR = rBar
                uclR = d4 * rBar
                lclR = If(d3 > 0, d3 * rBar, 0.0)

            Case "P"
                Dim n = columns.Count
                For Each s In parsedSamples
                    xValues.Add(s(0) / n)
                Next
                cl = xValues.Average()
                ucl = cl + 3 * Math.Sqrt(cl * (1 - cl) / n)
                lcl = Math.Max(0, cl - 3 * Math.Sqrt(cl * (1 - cl) / n))
                sgSize = n

            Case "U"
                For Each s In parsedSamples
                    Dim defects = s(0)
                    Dim units = If(s.Count > 1, s(1), 1.0)
                    xValues.Add(defects / units)
                Next
                Dim totalDefects = parsedSamples.Sum(Function(s) s(0))
                Dim totalUnits = parsedSamples.Sum(Function(s) If(s.Count > 1, s(1), 1.0))
                cl = totalDefects / totalUnits
                ucl = cl + 3 * Math.Sqrt(cl / (totalUnits / parsedSamples.Count))
                lcl = Math.Max(0, cl - 3 * Math.Sqrt(cl / (totalUnits / parsedSamples.Count)))

            Case "C"
                For Each s In parsedSamples
                    xValues.Add(s(0))
                Next
                cl = xValues.Average()
                ucl = cl + 3 * Math.Sqrt(cl)
                lcl = Math.Max(0, cl - 3 * Math.Sqrt(cl))
        End Select

        For i = 0 To xValues.Count - 1
            If xValues(i) > ucl OrElse xValues(i) < lcl Then
                ocPoints.Add(i)
            End If
        Next

        ViewState("ChartType") = chartType
        ViewState("CL") = cl
        ViewState("UCL") = ucl
        ViewState("LCL") = lcl
        ViewState("CLR") = clR
        ViewState("UCLR") = uclR
        ViewState("LCLR") = lclR
        ViewState("XValues") = String.Join(",", xValues.Select(Function(v) v.ToString("F4")))
        ViewState("RValues") = String.Join(",", rValues.Select(Function(v) v.ToString("F4")))
        ViewState("SGSize") = sgSize

        litChart.Text = RenderSPCChart(xValues, cl, ucl, lcl, ocPoints, chartType)
        litRChart.Text = RenderSPCChart(rValues, clR, uclR, lclR, Nothing, "R")

        litChartInfo.Text = String.Format(
            "<div class='small mb-2'><strong>{0}</strong> | CL: {1:F4} | UCL: {2:F4} | LCL: {3:F4} | Samples: {4}</div>",
            GetChartTypeName(chartType), cl, ucl, lcl, xValues.Count)

        pnlChart.Visible = True
        pnlNoChart.Visible = False
        pnlRChart.Visible = (chartType = "XbarR")
        btnSave.Visible = True
        litMessage.Text = "<div class='alert alert-success py-1 small'>Chart calculated.</div>"
    End Sub

    Private Function RenderSPCChart(values As List(Of Double), cl As Double, ucl As Double, lcl As Double,
                                     ocPoints As List(Of Integer), chartType As String) As String
        If values.Count = 0 OrElse (ucl - lcl) = 0 Then Return "<div class='text-muted small'>No variation.</div>"

        Dim sb As New StringBuilder()
        Dim n = values.Count
        Dim range = ucl - lcl
        Dim padding = range * 0.15
        Dim yMin = lcl - padding
        Dim yMax = ucl + padding
        Dim yRange = yMax - yMin
        Dim chartHeight = 280
        Dim chartWidth = Math.Max(n * 30, 400)
        Dim labelCount = 5

        sb.Append("<div class='cc-yaxis'>")
        For i = 0 To labelCount
            Dim yVal = yMin + yRange / labelCount * i
            Dim yPos = chartHeight - (yVal - yMin) / yRange * chartHeight
            sb.Append("<span style='bottom:" & yPos.ToString("F0") & "px;'>" & yVal.ToString("F2") & "</span>")
        Next
        sb.Append("</div>")

        sb.Append("<div style='position:absolute;left:65px;right:20px;top:0;bottom:20px;'>")

        Dim clY = chartHeight - (cl - yMin) / yRange * chartHeight
        sb.Append("<div class='cc-line cl' style='bottom:" & clY.ToString("F0") & "px;'></div>")

        Dim uclY = chartHeight - (ucl - yMin) / yRange * chartHeight
        sb.Append("<div class='cc-line ucl' style='bottom:" & uclY.ToString("F0") & "px;'></div>")

        Dim lclY = chartHeight - (lcl - yMin) / yRange * chartHeight
        sb.Append("<div class='cc-line lcl' style='bottom:" & lclY.ToString("F0") & "px;'></div>")

        For i = 0 To n - 1
            Dim xPos = CDbl(i) / (n - 1) * 100
            Dim yPos = chartHeight - (values(i) - yMin) / yRange * chartHeight
            Dim isOc = ocPoints IsNot Nothing AndAlso ocPoints.Contains(i)
            Dim cls = If(isOc, "cc-point oc", "cc-point")
            sb.Append("<div class='" & cls & "' style='left:" & xPos.ToString("F1") & "%;bottom:" & yPos.ToString("F0") & "px;'")
            sb.Append(" title='Sample " & (i + 1) & ": " & values(i).ToString("F4") & "'></div>")
        Next

        sb.Append("<div class='cc-xaxis' style='position:absolute;left:0;right:0;bottom:-18px;'>")
        Dim stepVal = Math.Max(1, n / 20)
        For i = 0 To n - 1 Step stepVal
            Dim xPos = CDbl(i) / (n - 1) * 100
            sb.Append("<span style='position:absolute;left:" & xPos.ToString("F1") & "%;transform:translateX(-50%);'>" & (i + 1) & "</span>")
        Next
        sb.Append("</div>")

        sb.Append("</div>")
        Return sb.ToString()
    End Function

    Private Function GetChartTypeName(ct As String) As String
        Select Case ct
            Case "IMR" : Return "I-MR Chart"
            Case "XbarR" : Return "X-bar & R Chart"
            Case "P" : Return "p Chart"
            Case "U" : Return "u Chart"
            Case "C" : Return "c Chart"
            Case Else : Return ct
        End Select
    End Function

    Private Function GetD2(n As Integer) As Double
        Dim d2() As Double = {0, 0, 1.128, 1.693, 2.059, 2.326, 2.534, 2.704, 2.847, 2.970, 3.078}
        If n < d2.Length Then Return d2(n)
        Return 3.078
    End Function

    Private Function GetD3(n As Integer) As Double
        Dim d3() As Double = {0, 0, 0, 0, 0, 0, 0.076, 0.136, 0.184, 0.223, 0.256}
        If n < d3.Length Then Return d3(n)
        Return 0.256
    End Function

    Private Function GetD4(n As Integer) As Double
        Dim d4() As Double = {0, 0, 3.267, 2.574, 2.282, 2.114, 2.004, 1.924, 1.864, 1.816, 1.777}
        If n < d4.Length Then Return d4(n)
        Return 1.777
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Return
        Dim chartName = txtChartName.Text.Trim()
        If chartName = "" Then chartName = "Chart - " & DateTime.Today.ToString("MMM dd, yyyy")
        Dim chartType = ddlChartType.SelectedValue

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
                    "INSERT INTO ControlChartModels (TenantId, ProjectId, ChartName, ChartType, SubgroupSize, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @Name, @Type, @SG, @UID)", conn, tx)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", pid)
                    cmd.Parameters.AddWithValue("@Name", chartName)
                    cmd.Parameters.AddWithValue("@Type", chartType)
                    cmd.Parameters.AddWithValue("@SG", 1)
                    cmd.Parameters.AddWithValue("@UID", CurrentUserId)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "SELECT LAST_INSERT_ID()"
                    cmd.Parameters.Clear()
                    Dim modelId = CInt(cmd.ExecuteScalar())

                    Dim sampleIdx = 0
                    For Each row In rows
                        Dim rowList = TryCast(row, List(Of Object))
                        If rowList IsNot Nothing Then
                            Dim cellValues As New List(Of String)
                            For Each cell In rowList
                                Dim cellStr = If(cell Is Nothing, "", cell.ToString().Trim())
                                If cellStr <> "" Then cellValues.Add(cellStr)
                            Next
                            If cellValues.Count > 0 Then
                                Using scmd As New MySqlCommand(
                                    "INSERT INTO ControlChartData (ModelId, SampleIndex, SampleValues) VALUES (@ModelId, @Idx, @Vals)",
                                    conn, tx)
                                    scmd.Parameters.AddWithValue("@ModelId", modelId)
                                    scmd.Parameters.AddWithValue("@Idx", sampleIdx)
                                    scmd.Parameters.AddWithValue("@Vals", String.Join(",", cellValues))
                                    scmd.ExecuteNonQuery()
                                End Using
                                sampleIdx += 1
                            End If
                        End If
                    Next

                    Using lcmd As New MySqlCommand(
                        "INSERT INTO ControlChartLimits (ModelId, CenterLine, UCL, LCL, CenterLineR, UCLR, LCLR) " &
                        "VALUES (@ModelId, @CL, @UCL, @LCL, @CLR, @UCLR, @LCLR)", conn, tx)
                        lcmd.Parameters.AddWithValue("@ModelId", modelId)
                        lcmd.Parameters.AddWithValue("@CL", CDbl(ViewState("CL")))
                        lcmd.Parameters.AddWithValue("@UCL", CDbl(ViewState("UCL")))
                        lcmd.Parameters.AddWithValue("@LCL", CDbl(ViewState("LCL")))
                        lcmd.Parameters.AddWithValue("@CLR", If(ViewState("CLR") IsNot Nothing, CDbl(ViewState("CLR")), CObj(DBNull.Value)))
                        lcmd.Parameters.AddWithValue("@UCLR", If(ViewState("UCLR") IsNot Nothing, CDbl(ViewState("UCLR")), CObj(DBNull.Value)))
                        lcmd.Parameters.AddWithValue("@LCLR", If(ViewState("LCLR") IsNot Nothing, CDbl(ViewState("LCLR")), CObj(DBNull.Value)))
                        lcmd.ExecuteNonQuery()
                    End Using
                    tx.Commit()
                End Using
            End Using
            LogAudit("Create", "ControlChart", Nothing, "Saved chart: " & chartName)
            litMessage.Text = "<div class='alert alert-success py-1 small'>Chart saved.</div>"
            LoadHistory()
        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger py-1 small'>Error: " & ex.Message & "</div>"
        Finally
            conn.Close()
        End Try
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        pnlChart.Visible = False
        pnlNoChart.Visible = True
        pnlRChart.Visible = False
        btnSave.Visible = False
        litMessage.Text = ""
        hfSpreadsheetData.Value = ""
        ClientScript.RegisterStartupScript(Me.GetType(), "clearSpreadsheet",
            "Spreadsheet.setData('spreadsheetContainer', { columns: [{name:'C1', label:'Measurement'}], rows: 50 });", True)
    End Sub

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "DeleteChart" Then
            Using cmd As New MySqlCommand("DELETE FROM ControlChartModels WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "ControlChart", id, "Deleted chart")
            LoadHistory()
        ElseIf e.CommandName = "LoadChart" Then
            LoadChartData(id)
        End If
    End Sub

    Private Sub LoadChartData(modelId As Integer)
        Dim chartType As String = ""
        Dim chartName As String = ""

        Using cmd As New MySqlCommand(
            "SELECT ChartType, ChartName FROM ControlChartModels WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", modelId)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                chartType = If(IsDBNull(dt.Rows(0)("ChartType")), "", dt.Rows(0)("ChartType").ToString())
                chartName = If(IsDBNull(dt.Rows(0)("ChartName")), "", dt.Rows(0)("ChartName").ToString())
            End If
        End Using

        txtChartName.Text = chartName

        Dim items As New List(Of Dictionary(Of String, Object))

        Using cmd As New MySqlCommand(
            "SELECT SampleValues FROM ControlChartData WHERE ModelId = @ModelId ORDER BY SampleIndex")
            cmd.Parameters.AddWithValue("@ModelId", modelId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            For Each row As DataRow In dt.Rows
                Dim vals = If(IsDBNull(row("SampleValues")), "", row("SampleValues").ToString())
                Dim parts = vals.Split(","c)
                items.Add(New Dictionary(Of String, Object) From {{"values", parts}})
            Next
        End Using

        Dim columns As New List(Of Object)
        Dim dataRows As New List(Of Object)

        Select Case chartType
            Case "IMR", "C"
                columns.Add(New Dictionary(Of String, String) From {{"name", "C1"}, {"label", "Measurement"}})
                For Each item In items
                    Dim vals = DirectCast(item("values"), String())
                    Dim rowArr As New List(Of Object)()
                    rowArr.Add(If(vals.Length > 0, vals(0), ""))
                    dataRows.Add(rowArr)
                Next
            Case "XbarR"
                Dim maxCols = 1
                For Each item In items
                    Dim vals = DirectCast(item("values"), String())
                    If vals.Length > maxCols Then maxCols = vals.Length
                Next
                For c = 0 To maxCols - 1
                    columns.Add(New Dictionary(Of String, String) From {{"name", "C" & (c + 1)}, {"label", "Subgroup " & (c + 1)}})
                Next
                For Each item In items
                    Dim vals = DirectCast(item("values"), String())
                    Dim rowArr As New List(Of Object)()
                    For c = 0 To maxCols - 1
                        rowArr.Add(If(c < vals.Length, vals(c), ""))
                    Next
                    dataRows.Add(rowArr)
                Next
            Case "P", "U"
                columns.Add(New Dictionary(Of String, String) From {{"name", "C1"}, {"label", "Defects/Count"}})
                columns.Add(New Dictionary(Of String, String) From {{"name", "C2"}, {"label", "Sample Size (n)"}})
                For Each item In items
                    Dim vals = DirectCast(item("values"), String())
                    Dim rowArr As New List(Of Object)()
                    rowArr.Add(If(vals.Length > 0, vals(0), ""))
                    rowArr.Add(If(vals.Length > 1, vals(1), ""))
                    dataRows.Add(rowArr)
                Next
        End Select

        Dim spreadsheetObj As New Dictionary(Of String, Object) From {{"columns", columns}, {"rows", dataRows}}
        Dim js As New JavaScriptSerializer()
        Dim json = js.Serialize(spreadsheetObj)

        ClientScript.RegisterStartupScript(Me.GetType(), "loadChart_" & modelId,
            "loadSpreadsheetData('" & json.Replace("'", "\'") & "');", True)

        pnlChart.Visible = False
        pnlNoChart.Visible = True
        btnSave.Visible = False
        litMessage.Text = "<div class='alert alert-info py-1 small'>Loaded chart: " & HttpUtility.HtmlEncode(chartName) & ". Click Calculate Limits to view.</div>"
    End Sub

End Class
