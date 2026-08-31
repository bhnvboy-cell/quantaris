Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Tools_COPQ
    Inherits BasePage

    Private _totalCOPQ As Decimal

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt", "User")
        If Not IsPostBack Then
            txtEntryDate.Text = DateTime.Today.ToString("yyyy-MM-dd")
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlCOPQ.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlCOPQ.Visible = False
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
            "SELECT * FROM COPQ WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY EntryDate DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            rptHistory.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataBind()
        End Using
    End Sub

    Private Function GetDecimal(value As String) As Decimal
        Dim result As Decimal
        Decimal.TryParse(value.Trim(), result)
        Return result
    End Function

    Private Sub UpdateSummary(appraisal As Decimal, prevention As Decimal, internalF As Decimal, externalF As Decimal, revenue As Decimal?)
        Dim total = appraisal + prevention + internalF + externalF
        Dim failureTotal As Decimal = 0
        _totalCOPQ = total

        litAppraisal.Text = String.Format("{0:N2}", appraisal)
        litPrevention.Text = String.Format("{0:N2}", prevention)
        litInternal.Text = String.Format("{0:N2}", internalF)
        litExternal.Text = String.Format("{0:N2}", externalF)
        litTotal.Text = String.Format("{0:N2}", total)

        If revenue.HasValue AndAlso revenue > 0 Then
            litCOPQRevenue.Text = String.Format("{0:P2}", total / revenue.Value)
        Else
            litCOPQRevenue.Text = "N/A (no revenue)"
        End If

        If total > 0 Then
            failureTotal = internalF + externalF
            litFailureRatio.Text = String.Format("{0:P2}", failureTotal / total)
            Dim goodTotal = appraisal + prevention
            litGoodRatio.Text = String.Format("{0:P2}", goodTotal / total)
        Else
            litFailureRatio.Text = "N/A"
            litGoodRatio.Text = "N/A"
        End If

        ' COPQ Level assessment
        Dim level As String
        Dim levelClass As String
        If total <= 0 Then
            level = "No Data"
            levelClass = "bg-secondary"
        ElseIf failureTotal / total < 0.3 Then
            level = "Excellent"
            levelClass = "bg-success"
        ElseIf failureTotal / total < 0.5 Then
            level = "Moderate"
            levelClass = "bg-warning text-dark"
        Else
            level = "Critical"
            levelClass = "bg-danger"
        End If
        litLevel.Text = "<span class='badge " & levelClass & " fs-6 rounded-pill px-3 py-1'>" & level & "</span>"

        ' Interpretation
        If total > 0 Then
            Dim failurePct = (failureTotal / total) * 100
            If failurePct >= 70 Then
                litInterpretation.Text = "<strong>High Failure Cost Ratio:</strong> " & String.Format("{0:F1}%", failurePct) &
                    " of COPQ comes from failures. Investigate root causes and strengthen prevention/appraisal."
            ElseIf failurePct >= 40 Then
                litInterpretation.Text = "<strong>Moderate Balance:</strong> " & String.Format("{0:F1}%", failurePct) &
                    " of COPQ is from failures. Consider increasing prevention investment."
            Else
                litInterpretation.Text = "<strong>Good Balance:</strong> Only " & String.Format("{0:F1}%", failurePct) &
                    " of COPQ is from failures. Well-managed quality system."
            End If
        Else
            litInterpretation.Text = "Enter cost data to see analysis."
        End If

        ' Bar chart
        Dim maxVal = Math.Max(total, 1)
        Dim pctAppr = CInt((appraisal / maxVal) * 100)
        Dim pctPrev = CInt((prevention / maxVal) * 100)
        Dim pctInt = CInt((internalF / maxVal) * 100)
        Dim pctExt = CInt((externalF / maxVal) * 100)

        barAppr.Style("width") = pctAppr & "%"
        barPrev.Style("width") = pctPrev & "%"
        barInt.Style("width") = pctInt & "%"
        barExt.Style("width") = pctExt & "%"

        litChartAppr.Text = String.Format("{0:N2}", appraisal)
        litChartPrev.Text = String.Format("{0:N2}", prevention)
        litChartInt.Text = String.Format("{0:N2}", internalF)
        litChartExt.Text = String.Format("{0:N2}", externalF)

        pnlSummary.Visible = True
        pnlNoSummary.Visible = False
        pnlChart.Visible = True
        pnlNoChart.Visible = False
    End Sub

    Protected Function FormatCurrency(value As Object) As String
        If value Is Nothing OrElse value Is DBNull.Value Then Return "$0.00"
        Dim v As Decimal
        Decimal.TryParse(value.ToString(), v)
        Return String.Format("{0:N2}", v)
    End Function

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlCOPQ.Visible = True
            pnlNoProject.Visible = False
            pnlSummary.Visible = False
            pnlNoSummary.Visible = True
            pnlChart.Visible = False
            pnlNoChart.Visible = True
            LoadHistory()
        Else
            pnlCOPQ.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadHistory()
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then
            litMessage.Text = "<div class='alert alert-warning'>Please select a project.</div>"
            Return
        End If
        Try
            Dim appraisal = GetDecimal(txtAppraisal.Text)
            Dim prevention = GetDecimal(txtPrevention.Text)
            Dim internalF = GetDecimal(txtInternalFailure.Text)
            Dim externalF = GetDecimal(txtExternalFailure.Text)
            Dim revText = txtRevenue.Text.Trim()
            Dim revenue? As Decimal = Nothing
            Dim revVal As Decimal
            If Decimal.TryParse(revText, revVal) AndAlso revVal > 0 Then revenue = revVal

            Dim entryDate As Date
            If Not Date.TryParse(txtEntryDate.Text.Trim(), entryDate) Then entryDate = DateTime.Today

            Using cmd As New MySqlCommand(
                "INSERT INTO COPQ (TenantId, ProjectId, EntryDate, AppraisalCosts, PreventionCosts, " &
                "InternalFailureCosts, ExternalFailureCosts, Revenue, Notes, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @EntryDate, @Appraisal, @Prevention, " &
                "@InternalFailure, @ExternalFailure, @Revenue, @Notes, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", pid)
                cmd.Parameters.AddWithValue("@EntryDate", entryDate)
                cmd.Parameters.AddWithValue("@Appraisal", appraisal)
                cmd.Parameters.AddWithValue("@Prevention", prevention)
                cmd.Parameters.AddWithValue("@InternalFailure", internalF)
                cmd.Parameters.AddWithValue("@ExternalFailure", externalF)
                cmd.Parameters.AddWithValue("@Revenue", If(revenue.HasValue, CObj(revenue.Value), DBNull.Value))
                cmd.Parameters.AddWithValue("@Notes", IIf(txtNotes.Text.Trim() = "", DBNull.Value, CObj(txtNotes.Text.Trim())))
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)

                LogAudit("Create", "COPQ", Nothing, "Saved COPQ entry for project " & pid)
                litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>COPQ entry saved.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
                ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('COPQ record saved successfully', 'success');", True)

                UpdateSummary(appraisal, prevention, internalF, externalF, revenue)
                LoadHistory()
            End Using

        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error saving: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnClear_Click(sender As Object, e As EventArgs)
        txtAppraisal.Text = ""
        txtPrevention.Text = ""
        txtInternalFailure.Text = ""
        txtExternalFailure.Text = ""
        txtRevenue.Text = ""
        txtNotes.Text = ""
        txtEntryDate.Text = DateTime.Today.ToString("yyyy-MM-dd")
        pnlSummary.Visible = False
        pnlNoSummary.Visible = True
        pnlChart.Visible = False
        pnlNoChart.Visible = True
        litMessage.Text = ""
    End Sub

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "LoadRecord" Then
            Using cmd As New MySqlCommand(
                "SELECT * FROM COPQ WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    Dim r = dt.Rows(0)
                    txtEntryDate.Text = CDate(r("EntryDate")).ToString("yyyy-MM-dd")
                    txtAppraisal.Text = r("AppraisalCosts").ToString()
                    txtPrevention.Text = r("PreventionCosts").ToString()
                    txtInternalFailure.Text = r("InternalFailureCosts").ToString()
                    txtExternalFailure.Text = r("ExternalFailureCosts").ToString()
                    txtRevenue.Text = If(r("Revenue") IsNot DBNull.Value, r("Revenue").ToString(), "")
                    txtNotes.Text = If(r("Notes") IsNot DBNull.Value, r("Notes").ToString(), "")

                    Dim appraisal = CDec(r("AppraisalCosts"))
                    Dim prevention = CDec(r("PreventionCosts"))
                    Dim internalF = CDec(r("InternalFailureCosts"))
                    Dim externalF = CDec(r("ExternalFailureCosts"))
                    Dim revenue? As Decimal = Nothing
                    If r("Revenue") IsNot DBNull.Value Then revenue = CDec(r("Revenue"))

                    UpdateSummary(appraisal, prevention, internalF, externalF, revenue)
                End If
            End Using
        ElseIf e.CommandName = "DeleteRecord" Then
            Using cmd As New MySqlCommand(
                "DELETE FROM COPQ WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
                LogAudit("Delete", "COPQ", id, "Deleted COPQ entry")
                LoadHistory()
                btnClear_Click(Nothing, Nothing)
                ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('COPQ record deleted', 'info');", True)
            End Using
        End If
    End Sub

    Protected Sub btnExportCsv_Click(sender As Object, e As EventArgs)
        Dim projectId = CInt(Request.QueryString("projectId"))
        Dim cmd As New MySqlCommand("SELECT EntryDate, AppraisalCosts, PreventionCosts, InternalFailureCosts, ExternalFailureCosts, TotalCOPQ, Revenue, COPQPercentage, Notes FROM COPQ WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY EntryDate")
        cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
        cmd.Parameters.AddWithValue("@ProjectId", projectId)
        Dim dt = DbHelper.ExecuteDataTable(cmd)
        ExportHelper.ExportToCsv(dt, "COPQ_Project" & projectId)
    End Sub
End Class
