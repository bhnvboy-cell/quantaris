Imports System.Data
Imports MySql.Data.MySqlClient

Public Partial Class Tools_SampleSize
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlSS.Visible = True
                pnlNoProject.Visible = False
                LoadHistory()
            Else
                pnlSS.Visible = False
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
            "SELECT * FROM SampleSizeCalculations WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataSource = dt
            rptHistory.DataBind()
        End Using
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlSS.Visible = True
            pnlNoProject.Visible = False
            pnlResult.Visible = False
            pnlNoResult.Visible = True
            LoadHistory()
        Else
            pnlSS.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then LoadHistory()
    End Sub

    Protected Sub ddlType_SelectedIndexChanged(sender As Object, e As EventArgs)
        pnlContinuous.Visible = (ddlType.SelectedValue = "Continuous")
        pnlProportion.Visible = (ddlType.SelectedValue = "Proportion")
        pnlSigma.Visible = (ddlType.SelectedValue = "Sigma")
    End Sub

    Protected Sub btnCalculate_Click(sender As Object, e As EventArgs)
        Dim calcType = ddlType.SelectedValue
        Dim zStr = ddlConf.SelectedValue
        Dim z As Double
        Double.TryParse(zStr, z)

        Dim n0 As Integer = 0
        Dim paramDesc As String = ""
        Dim formulaName As String = ""

        Select Case calcType
            Case "Continuous"
                Dim sigma, d As Double
                If Not Double.TryParse(txtStdDev.Text.Trim(), sigma) OrElse sigma <= 0 Then
                    litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter valid std dev (> 0).</div>" : Return
                End If
                If Not Double.TryParse(txtMargin.Text.Trim(), d) OrElse d <= 0 Then
                    litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter valid margin (> 0).</div>" : Return
                End If
                n0 = CInt(Math.Ceiling((z ^ 2 * sigma ^ 2) / (d ^ 2)))
                paramDesc = "Ïƒ = " & sigma & ", d = " & d
                formulaName = "n = ZÂ²ÏƒÂ²/dÂ²"

            Case "Proportion"
                Dim p, d As Double
                If Not Double.TryParse(txtProportion.Text.Trim(), p) OrElse p <= 0 OrElse p >= 1 Then
                    litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter valid proportion (0-1).</div>" : Return
                End If
                If Not Double.TryParse(txtPropMargin.Text.Trim(), d) OrElse d <= 0 OrElse d >= 1 Then
                    litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter valid margin (0-1).</div>" : Return
                End If
                n0 = CInt(Math.Ceiling((z ^ 2 * p * (1 - p)) / (d ^ 2)))
                paramDesc = "p = " & p & ", d = " & d
                formulaName = "n = ZÂ²p(1-p)/dÂ²"

            Case "Sigma"
                Dim sigma, hw As Double
                If Not Double.TryParse(txtKnownSigma.Text.Trim(), sigma) OrElse sigma <= 0 Then
                    litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter valid sigma (> 0).</div>" : Return
                End If
                If Not Double.TryParse(txtHalfWidth.Text.Trim(), hw) OrElse hw <= 0 Then
                    litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter valid half-width (> 0).</div>" : Return
                End If
                n0 = CInt(Math.Ceiling((z ^ 2 * sigma ^ 2) / (hw ^ 2)))
                paramDesc = "Ïƒ = " & sigma & ", hw = " & hw
                formulaName = "n = ZÂ²ÏƒÂ²/hwÂ²"
        End Select

        ' Finite population correction
        Dim popStr = txtPopulation.Text.Trim()
        Dim pop As Integer = 0
        Dim hasPop = Integer.TryParse(popStr, pop) AndAlso pop > 0

        Dim nFinal = n0
        If hasPop AndAlso pop > 0 Then
            nFinal = CInt(Math.Ceiling(CDbl(n0) / (1.0 + CDbl(n0 - 1) / pop)))
            formulaName &= " (FPC applied)"
        End If

        If nFinal < 1 Then nFinal = 1

        litResult.Text = nFinal.ToString()
        litConfLevel.Text = ddlConf.SelectedItem.Text
        litMarginDisplay.Text = paramDesc
        litFormula.Text = formulaName

        Dim interp As String
        If calcType = "Proportion" Then
            interp = "To estimate the population proportion with " & ddlConf.SelectedItem.Text &
                " confidence and the specified margin of error, you need at least <strong>" & nFinal &
                "</strong> observations."
        Else
            interp = "To estimate the population mean with " & ddlConf.SelectedItem.Text &
                " confidence and the specified precision, you need at least <strong>" & nFinal &
                "</strong> observations."
        End If
        If hasPop Then
            interp &= " Finite population correction applied (N = " & pop & ")."
        End If
        litInterpretationSS.Text = interp

        ViewState("CalcType") = calcType
        ViewState("Result") = nFinal
        ViewState("Params") = paramDesc & "|" & formulaName

        pnlResult.Visible = True
        pnlNoResult.Visible = False
        btnSave.Visible = True
        litMessage.Text = ""
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Return
        Dim calcType = TryCast(ViewState("CalcType"), String)
        Dim result = CInt(ViewState("Result"))
        Dim params = TryCast(ViewState("Params"), String)

        Using cmd As New MySqlCommand(
            "INSERT INTO SampleSizeCalculations (TenantId, ProjectId, CalcName, CalcType, Parameters, Result, CreatedByUserId) " &
            "VALUES (@TenantId, @ProjectId, @Name, @Type, @Params, @Result, @UID)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            cmd.Parameters.AddWithValue("@Name", calcType & " - " & DateTime.Today.ToString("MMM dd"))
            cmd.Parameters.AddWithValue("@Type", calcType)
            cmd.Parameters.AddWithValue("@Params", params)
            cmd.Parameters.AddWithValue("@Result", result)
            cmd.Parameters.AddWithValue("@UID", CurrentUserId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Create", "SampleSizeCalc", Nothing, "Saved sample size: n=" & result)
        litMessage.Text = "<div class='alert alert-success py-1 small'>Saved.</div>"
        LoadHistory()
    End Sub

    Protected Sub rptHistory_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "DeleteCalc" Then
            Using cmd As New MySqlCommand("DELETE FROM SampleSizeCalculations WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LoadHistory()
        End If
    End Sub
End Class
