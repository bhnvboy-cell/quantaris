Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Text
Imports System.Text.RegularExpressions

Public Partial Class Tools_QFD
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlQFD.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlQFD.Visible = False
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
            "SELECT * FROM QFDModels WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataSource = dt
            rptHistory.DataBind()
        End Using
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlQFD.Visible = True
            pnlNoProject.Visible = False
            pnlMatrix.Visible = False
            pnlNoMatrix.Visible = True
            LoadHistory()
        Else
            pnlQFD.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then LoadHistory()
    End Sub

    Protected Sub btnBuild_Click(sender As Object, e As EventArgs)
        Dim custLines = txtCustomerReqs.Text.Trim().Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
        Dim techLines = txtTechReqs.Text.Trim().Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)

        If custLines.Length = 0 OrElse techLines.Length = 0 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter both customer and technical requirements.</div>"
            Return
        End If

        Dim custNames As New List(Of String)
        Dim custWeights As New List(Of Double)
        Dim techNames As New List(Of String)

        For Each line In custLines
            Dim m = Regex.Match(line.Trim(), "^(.*?)(?:\s*\((\d+(?:\.\d+)?)\))?\s*$")
            custNames.Add(m.Groups(1).Value.Trim())
            Dim w As Double = 1
            If m.Groups(2).Success Then Double.TryParse(m.Groups(2).Value, w)
            custWeights.Add(Math.Max(1, Math.Min(5, w)))
        Next

        For Each line In techLines
            techNames.Add(line.Trim())
        Next

        Dim nc = custNames.Count
        Dim nt = techNames.Count

        ViewState("CustCount") = nc
        ViewState("TechCount") = nt
        ViewState("CustNames") = String.Join("|", custNames.ToArray())
        ViewState("CustWeights") = String.Join(",", custWeights.Select(Function(w) w.ToString("F1")).ToArray())
        ViewState("TechNames") = String.Join("|", techNames.ToArray())

        litMatrix.Text = RenderMatrix(nc, nt, custNames, custWeights, techNames, Nothing, Nothing)
        pnlMatrix.Visible = True
        pnlNoMatrix.Visible = False
        btnSave.Visible = True
        litMessage.Text = ""
    End Sub

    Private Function RenderMatrix(nc As Integer, nt As Integer, custNames As List(Of String),
                                    custWeights As List(Of Double), techNames As List(Of String),
                                    ratings As Dictionary(Of String, Integer),
                                    correlations As Dictionary(Of String, String)) As String
        Dim sb As New StringBuilder()
        sb.Append("<table class='table table-bordered qfd-matrix'>")
        sb.Append("<thead>")

        ' Roof (correlation) row
        sb.Append("<tr class='qfd-roof'><td colspan='2' rowspan='2' class='qfd-corner'>House of Quality</td>")
        For j = 0 To nt - 1
            sb.Append("<td class='p-0' style='width:50px;'>")
            ' Correlations between tech reqs
            For k = 0 To j - 1
                Dim key = k & "-" & j
                Dim corr = ""
                If correlations IsNot Nothing AndAlso correlations.ContainsKey(key) Then
                    corr = correlations(key)
                End If
                Dim corrLabel = ""
                Select Case corr
                    Case "++" : corrLabel = "âŠ•"
                    Case "+" : corrLabel = "+"
                    Case "-" : corrLabel = "âˆ’"
                    Case "--" : corrLabel = "âŠ–"
                End Select
                sb.Append("<div class='small' style='margin-bottom:2px;'>")
                sb.Append("<select name='corr_" & key & "' class='form-select form-select-sm' style='font-size:9px;padding:1px;height:auto;'>")
                sb.Append("<option value='' " & If(corr = "", "selected", "") & ">&nbsp;</option>")
                sb.Append("<option value='++' " & If(corr = "++", "selected", "") & ">âŠ• Strong+</option>")
                sb.Append("<option value='+' " & If(corr = "+", "selected", "") & ">+ Positive</option>")
                sb.Append("<option value='-' " & If(corr = "-", "selected", "") & ">âˆ’ Negative</option>")
                sb.Append("<option value='--' " & If(corr = "--", "selected", "") & ">âŠ– Strong-</option>")
                sb.Append("</select>")
                sb.Append("</div>")
            Next
            sb.Append("</td>")
        Next
        sb.Append("</tr>")

        ' Tech requirement headers
        sb.Append("<tr class='qfd-roof'>")
        For j = 0 To nt - 1
            sb.Append("<td class='qfd-col-label'>" & HttpUtility.HtmlEncode(techNames(j)) & "</td>")
        Next
        sb.Append("</tr>")
        sb.Append("</thead><tbody>")

        ' Customer requirements with rating inputs
        For i = 0 To nc - 1
            sb.Append("<tr>")
            sb.Append("<td class='qfd-row-label'>" & HttpUtility.HtmlEncode(custNames(i)) & "</td>")
            sb.Append("<td style='min-width:40px;'><span class='badge bg-secondary'>" & custWeights(i).ToString("F0") & "</span></td>")
            For j = 0 To nt - 1
                Dim key = i & "-" & j
                Dim val = ""
                If ratings IsNot Nothing AndAlso ratings.ContainsKey(key) Then
                    val = ratings(key).ToString()
                End If
                Dim ratingClass = ""
                If val <> "" Then
                    Select Case CInt(val)
                        Case 1 : ratingClass = "qfd-weak"
                        Case 3 : ratingClass = "qfd-moderate"
                        Case 9 : ratingClass = "qfd-strong"
                    End Select
                End If
                sb.Append("<td class='" & ratingClass & "'>")
                sb.Append("<select name='rate_" & key & "' class='form-select form-select-sm' style='font-size:9px;padding:1px;height:auto;'>")
                sb.Append("<option value='0' " & If(val = "" OrElse val = "0", "selected", "") & ">0</option>")
                sb.Append("<option value='1' " & If(val = "1", "selected", "") & ">1 Weak</option>")
                sb.Append("<option value='3' " & If(val = "3", "selected", "") & ">3 Mod</option>")
                sb.Append("<option value='9' " & If(val = "9", "selected", "") & ">9 Strong</option>")
                sb.Append("</select>")
                sb.Append("</td>")
            Next
            sb.Append("</tr>")
        Next

        ' Importance row
        sb.Append("<tr class='fw-bold'>")
        sb.Append("<td class='text-end' colspan='2'>Importance</td>")
        For j = 0 To nt - 1
            Dim imp = 0.0
            For i = 0 To nc - 1
                Dim key = i & "-" & j
                Dim val = 0
                If ratings IsNot Nothing AndAlso ratings.ContainsKey(key) Then
                    val = ratings(key)
                End If
                imp += custWeights(i) * val
            Next
            sb.Append("<td>" & imp.ToString("F0") & "</td>")
        Next
        sb.Append("</tr>")

        ' Relative importance (%)
        Dim totalImp = 0.0
        For j = 0 To nt - 1
            Dim imp = 0.0
            For i = 0 To nc - 1
                Dim key = i & "-" & j
                Dim val = 0
                If ratings IsNot Nothing AndAlso ratings.ContainsKey(key) Then
                    val = ratings(key)
                End If
                imp += custWeights(i) * val
            Next
            totalImp += imp
        Next

        sb.Append("<tr class='fw-bold'>")
        sb.Append("<td class='text-end' colspan='2'>Rel. Importance</td>")
        For j = 0 To nt - 1
            Dim imp = 0.0
            For i = 0 To nc - 1
                Dim key = i & "-" & j
                Dim val = 0
                If ratings IsNot Nothing AndAlso ratings.ContainsKey(key) Then
                    val = ratings(key)
                End If
                imp += custWeights(i) * val
            Next
            Dim pct = If(totalImp > 0, imp / totalImp * 100, 0)
            sb.Append("<td>" & pct.ToString("F1") & "%</td>")
        Next
        sb.Append("</tr>")

        sb.Append("</tbody></table>")
        Return sb.ToString()
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Return
        Dim modelName = txtModelName.Text.Trim()
        If modelName = "" Then modelName = "QFD - " & DateTime.Today.ToString("MMM dd, yyyy")

        Dim nc = CInt(ViewState("CustCount"))
        Dim nt = CInt(ViewState("TechCount"))
        Dim custNames = TryCast(ViewState("CustNames"), String).Split("|"c).ToList()
        Dim custWeights = TryCast(ViewState("CustWeights"), String).Split(","c).Select(Function(s) CDbl(s)).ToList()
        Dim techNames = TryCast(ViewState("TechNames"), String).Split("|"c).ToList()

        ' Collect ratings from form
        Dim ratings As New Dictionary(Of String, Integer)
        Dim correlations As New Dictionary(Of String, String)

        For i = 0 To nc - 1
            For j = 0 To nt - 1
                Dim key = i & "-" & j
                Dim val = Request.Form("rate_" & key)
                If val IsNot Nothing AndAlso val <> "" Then
                    Dim r As Integer
                    Integer.TryParse(val, r)
                    If r > 0 Then ratings(key) = r
                End If
            Next
        Next

        For j = 0 To nt - 1
            For k = 0 To j - 1
                Dim key = k & "-" & j
                Dim val = Request.Form("corr_" & key)
                If val IsNot Nothing AndAlso val <> "" Then
                    correlations(key) = val
                End If
            Next
        Next

        Dim conn = DbHelper.GetConnection()
        Try
            conn.Open()
            Using tx = conn.BeginTransaction()
                Using cmd As New MySqlCommand(
                    "INSERT INTO QFDModels (TenantId, ProjectId, ModelName, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @Name, @UID)", conn, tx)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", pid)
                    cmd.Parameters.AddWithValue("@Name", modelName)
                    cmd.Parameters.AddWithValue("@UID", CurrentUserId)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "SELECT LAST_INSERT_ID()"
                    cmd.Parameters.Clear()
                    Dim modelId = CInt(cmd.ExecuteScalar())

                    For i = 0 To nc - 1
                        Using icmd As New MySqlCommand(
                            "INSERT INTO QFDRequirements (ModelId, ReqType, ReqIndex, ReqName, Weight) " &
                            "VALUES (@ModelId, 'Customer', @Idx, @Name, @Wt)", conn, tx)
                            icmd.Parameters.AddWithValue("@ModelId", modelId)
                            icmd.Parameters.AddWithValue("@Idx", i)
                            icmd.Parameters.AddWithValue("@Name", custNames(i))
                            icmd.Parameters.AddWithValue("@Wt", custWeights(i))
                            icmd.ExecuteNonQuery()
                        End Using
                    Next

                    For j = 0 To nt - 1
                        Using icmd As New MySqlCommand(
                            "INSERT INTO QFDRequirements (ModelId, ReqType, ReqIndex, ReqName, Weight) " &
                            "VALUES (@ModelId, 'Technical', @Idx, @Name, 1.0)", conn, tx)
                            icmd.Parameters.AddWithValue("@ModelId", modelId)
                            icmd.Parameters.AddWithValue("@Idx", j)
                            icmd.Parameters.AddWithValue("@Name", techNames(j))
                            icmd.ExecuteNonQuery()
                        End Using
                    Next

                    ' Get req IDs
                    Dim custIds As New List(Of Integer)
                    Dim techIds As New List(Of Integer)
                    Using qcmd As New MySqlCommand(
                        "SELECT Id, ReqIndex, ReqType FROM QFDRequirements WHERE ModelId = @ModelId ORDER BY ReqType, ReqIndex", conn, tx)
                        qcmd.Parameters.AddWithValue("@ModelId", modelId)
                        Using rdr = qcmd.ExecuteReader()
                            While rdr.Read()
                                If rdr("ReqType").ToString() = "Customer" Then
                                    custIds.Add(CInt(rdr("Id")))
                                Else
                                    techIds.Add(CInt(rdr("Id")))
                                End If
                            End While
                        End Using
                    End Using

                    For Each kv In ratings
                        Dim parts = kv.Key.Split("-"c)
                        Dim ci = CInt(parts(0))
                        Dim tj = CInt(parts(1))
                        If ci < custIds.Count AndAlso tj < techIds.Count Then
                            Using mcmd As New MySqlCommand(
                                "INSERT INTO QFDMatrix (ModelId, CustomerReqId, TechnicalReqId, Rating) " &
                                "VALUES (@ModelId, @CR, @TR, @R)", conn, tx)
                                mcmd.Parameters.AddWithValue("@ModelId", modelId)
                                mcmd.Parameters.AddWithValue("@CR", custIds(ci))
                                mcmd.Parameters.AddWithValue("@TR", techIds(tj))
                                mcmd.Parameters.AddWithValue("@R", kv.Value)
                                mcmd.ExecuteNonQuery()
                            End Using
                        End If
                    Next

                    For Each kv In correlations
                        Dim parts = kv.Key.Split("-"c)
                        Dim t1 = CInt(parts(0))
                        Dim t2 = CInt(parts(1))
                        If t1 < techIds.Count AndAlso t2 < techIds.Count Then
                            Using ccmd As New MySqlCommand(
                                "INSERT INTO QFDCorrelations (ModelId, TechReq1Id, TechReq2Id, Correlation) " &
                                "VALUES (@ModelId, @T1, @T2, @C)", conn, tx)
                                ccmd.Parameters.AddWithValue("@ModelId", modelId)
                                ccmd.Parameters.AddWithValue("@T1", techIds(t1))
                                ccmd.Parameters.AddWithValue("@T2", techIds(t2))
                                ccmd.Parameters.AddWithValue("@C", kv.Value)
                                ccmd.ExecuteNonQuery()
                            End Using
                        End If
                    Next

                    tx.Commit()
                End Using
            End Using
            LogAudit("Create", "QFDModel", Nothing, "Saved QFD: " & modelName)
            litMessage.Text = "<div class='alert alert-success py-1 small'>QFD saved.</div>"
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
            Using cmd As New MySqlCommand("DELETE FROM QFDModels WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "QFDModel", id, "Deleted QFD model")
            LoadHistory()
        End If
    End Sub
End Class
