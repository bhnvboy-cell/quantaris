Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Text

Public Partial Class Tools_BenefitsTracking
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlBenefits.Visible = True
                pnlNoProject.Visible = False
                LoadBenefits()
            Else
                pnlBenefits.Visible = False
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

    Private Sub LoadBenefits()
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Exit Sub
        Using cmd As New MySqlCommand(
            "SELECT * FROM BenefitsTracking WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptBenefits.DataSource = dt
            rptBenefits.DataBind()

            ' Summary
            Dim savings = dt.AsEnumerable().Where(Function(r) r("BenefitType").ToString() = "Savings").Sum(Function(r) CDec(r("Amount")))
            Dim revenue = dt.AsEnumerable().Where(Function(r) r("BenefitType").ToString() = "Revenue").Sum(Function(r) CDec(r("Amount")))
            Dim costAvoid = dt.AsEnumerable().Where(Function(r) r("BenefitType").ToString() = "CostAvoidance").Sum(Function(r) CDec(r("Amount")))
            Dim quality = dt.AsEnumerable().Where(Function(r) r("BenefitType").ToString() = "Quality").Sum(Function(r) CDec(r("Amount")))

            litTotalSavings.Text = savings.ToString("N0")
            litTotalRevenue.Text = revenue.ToString("N0")
            litCostAvoided.Text = costAvoid.ToString("N0")
            litQualityImpact.Text = quality.ToString("N0")

            pnlSummary.Visible = dt.Rows.Count > 0

            ' Timeline chart
            If dt.Rows.Count > 0 Then
                Dim months As New SortedSet(Of String)()
                For Each r In dt.Rows
                    months.Add(CDate(r("StartMonth")).ToString("yyyy-MM"))
                    months.Add(CDate(r("EndMonth")).ToString("yyyy-MM"))
                Next
                Dim monthList = months.ToList()
                Dim sb As New StringBuilder()
                For Each m In monthList
                    Dim monthlyTotal = 0.0
                    For Each r In dt.Rows
                        Dim start = CDate(r("StartMonth"))
                        Dim [end] = CDate(r("EndMonth"))
                        Dim mDate = DateTime.ParseExact(m & "-01", "yyyy-MM-dd", Nothing)
                        If mDate >= start AndAlso mDate <= [end] Then
                            Dim monthsInRange = CInt(([end].Year - start.Year) * 12 + [end].Month - start.Month) + 1
                            If monthsInRange > 0 Then
                                monthlyTotal += CDec(r("Amount")) / monthsInRange
                            End If
                        End If
                    Next
                Next
                pnlChart.Visible = True
            End If
        End Using
    End Sub

    Private Function FormatCurrency(val As Object) As String
        If val Is Nothing OrElse val Is DBNull.Value Then Return "$0"
        Return "$" & CDec(val).ToString("N0")
    End Function

    Protected Function GetStatusBadge(status As String) As String
        Select Case status
            Case "Achieved" : Return "bg-success"
            Case "InProgress" : Return "bg-primary"
            Case "AtRisk" : Return "bg-danger"
            Case "Planned" : Return "bg-secondary"
            Case Else : Return "bg-secondary"
        End Select
    End Function

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlBenefits.Visible = True
            pnlNoProject.Visible = False
            LoadBenefits()
        Else
            pnlBenefits.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then LoadBenefits()
    End Sub

    Protected Sub btnAddBenefit_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Return
        Dim name = txtBenefitName.Text.Trim()
        If name = "" Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter a benefit name.</div>" : Return
        End If
        Dim amount As Decimal
        If Not Decimal.TryParse(txtAmount.Text.Trim(), amount) OrElse amount <= 0 Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter a valid amount.</div>" : Return
        End If

        Using cmd As New MySqlCommand(
            "INSERT INTO BenefitsTracking (TenantId, ProjectId, BenefitName, BenefitType, Category, Amount, Currency, StartMonth, EndMonth, Status, Notes, CreatedByUserId) " &
            "VALUES (@TenantId, @ProjectId, @Name, @Type, @Cat, @Amt, @Cur, @Start, @End, @Status, @Notes, @UID)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            cmd.Parameters.AddWithValue("@Name", name)
            cmd.Parameters.AddWithValue("@Type", ddlBenefitType.SelectedValue)
            cmd.Parameters.AddWithValue("@Cat", ddlCategory.SelectedValue)
            cmd.Parameters.AddWithValue("@Amt", amount)
            cmd.Parameters.AddWithValue("@Cur", ddlCurrency.SelectedValue)
            cmd.Parameters.AddWithValue("@Start", DateTime.ParseExact(txtStartMonth.Text.Trim() & "-01", "yyyy-MM-dd", Nothing))
            cmd.Parameters.AddWithValue("@End", DateTime.ParseExact(txtEndMonth.Text.Trim() & "-01", "yyyy-MM-dd", Nothing))
            cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue)
            cmd.Parameters.AddWithValue("@Notes", txtNotes.Text.Trim())
            cmd.Parameters.AddWithValue("@UID", CurrentUserId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Create", "Benefit", Nothing, "Added benefit: " & name)
        litMessage.Text = "<div class='alert alert-success py-1 small'>Benefit added.</div>"
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('Benefits record saved successfully', 'success');", True)
        LoadBenefits()
    End Sub

    Protected Sub rptBenefits_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "DeleteBenefit" Then
            Using cmd As New MySqlCommand("DELETE FROM BenefitsTracking WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "Benefit", id, "Deleted benefit")
            LoadBenefits()
            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('Benefit record deleted', 'info');", True)
        End If
    End Sub

    Protected Sub btnExportCsv_Click(sender As Object, e As EventArgs)
        Dim projectId = CInt(Request.QueryString("projectId"))
        Dim cmd As New MySqlCommand("SELECT BenefitName, BenefitType, Category, Amount, Currency, StartMonth, EndMonth, Status, Notes FROM BenefitsTracking WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY BenefitType, BenefitName")
        cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
        cmd.Parameters.AddWithValue("@ProjectId", projectId)
        Dim dt = DbHelper.ExecuteDataTable(cmd)
        ExportHelper.ExportToCsv(dt, "Benefits_Project" & projectId)
    End Sub
End Class
