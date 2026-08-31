Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Analyze_Analyze_StatValidation
    Inherits BasePage

    Private _projectId As Integer

    Private Const STAT_TEST_CATEGORY As String = "StatTest"

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadTestHistory()
            UpdateStepStatus()
        End If
    End Sub

    Protected Sub ddlTestType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlTestType.SelectedIndexChanged
        pnlParameters.Visible = Not String.IsNullOrEmpty(ddlTestType.SelectedValue)
        pnlNoTest.Visible = Not pnlParameters.Visible
    End Sub

    Private Sub LoadTestHistory()
        Using cmd As New MySqlCommand(
            "SELECT * FROM RootCauseAnalysis " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND Category LIKE @CategoryPrefix ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@CategoryPrefix", STAT_TEST_CATEGORY + "%")
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptTests.DataSource = dt
            rptTests.DataBind()
            litTestCount.Text = dt.Rows.Count & " test(s)"
        End Using
    End Sub

    Protected Sub btnAutoSig_Click(sender As Object, e As EventArgs) Handles btnAutoSig.Click
        Dim pVal As Double
        Dim alpha As Double
        If Double.TryParse(txtPValue.Text.Trim(), pVal) AndAlso Double.TryParse(txtAlpha.Text.Trim(), alpha) Then
            ddlSignificance.SelectedValue = If(pVal <= alpha, "Significant", "Not Significant")
        End If
    End Sub

    Protected Sub btnSaveTest_Click(sender As Object, e As EventArgs) Handles btnSaveTest.Click
        If String.IsNullOrEmpty(ddlTestType.SelectedValue) Then Return

        Dim testType As String = ddlTestType.SelectedValue
        Dim description As String = txtTestDescription.Text.Trim()
        Dim editId As Integer = CInt(hfEditId.Value)

        Dim paramsDict As New Dictionary(Of String, String)()
        paramsDict("SampleSize") = txtSampleSize.Text.Trim()
        paramsDict("SampleMean") = txtSampleMean.Text.Trim()
        paramsDict("StdDev") = txtStdDev.Text.Trim()
        paramsDict("HypothesizedMean") = txtHypothesizedMean.Text.Trim()
        paramsDict("Sample2Mean") = txtSample2Mean.Text.Trim()
        paramsDict("Sample2StdDev") = txtSample2StdDev.Text.Trim()
        paramsDict("Sample2Size") = txtSample2Size.Text.Trim()
        paramsDict("DF") = txtDF.Text.Trim()
        paramsDict("ExpectedProp") = txtExpectedProp.Text.Trim()
        Dim paramsJson As String = SerializeParams(paramsDict)

        Dim testStatistic As String = txtTestStatistic.Text.Trim()
        Dim pValue As String = txtPValue.Text.Trim()
        Dim alpha As String = txtAlpha.Text.Trim()
        Dim significance As String = ddlSignificance.SelectedValue
        Dim conclusion As String = txtConclusion.Text.Trim()

        Dim effectData As String = String.Join("|", {pValue, significance, alpha, paramsJson})
        Dim fiveWhyData As String = testStatistic

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE RootCauseAnalysis SET Category = @Category, Cause = @Cause, Effect = @Effect, " &
                "FiveWhy_Chain = @FiveWhy_Chain, Sequence = @Sequence " &
                "WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@Category", STAT_TEST_CATEGORY & "_" & testType)
                cmd.Parameters.AddWithValue("@Cause", description)
                cmd.Parameters.AddWithValue("@Effect", effectData)
                cmd.Parameters.AddWithValue("@FiveWhy_Chain", fiveWhyData)
                cmd.Parameters.AddWithValue("@Sequence", If(String.IsNullOrWhiteSpace(conclusion), CObj(DBNull.Value), CInt(0)))
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "StatTest", editId, "Updated statistical test")
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Category, Cause, Effect, FiveWhy_Chain, Sequence, IsFishbone, CreatedByUserId, CreatedOn) " &
                "VALUES (@TenantId, @ProjectId, @Category, @Cause, @Effect, @FiveWhy_Chain, @Sequence, FALSE, @CreatedByUserId, CURRENT_TIMESTAMP)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@Category", STAT_TEST_CATEGORY & "_" & testType)
                cmd.Parameters.AddWithValue("@Cause", description)
                cmd.Parameters.AddWithValue("@Effect", effectData)
                cmd.Parameters.AddWithValue("@FiveWhy_Chain", fiveWhyData)
                cmd.Parameters.AddWithValue("@Sequence", If(String.IsNullOrWhiteSpace(conclusion), CObj(DBNull.Value), CInt(0)))
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "StatTest", _projectId, "Saved statistical test: " & testType)
        End If

        ClearForm()
        LoadTestHistory()
    End Sub

    Protected Sub btnClearTest_Click(sender As Object, e As EventArgs) Handles btnClearTest.Click
        ClearForm()
    End Sub

    Protected Sub rptTests_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM RootCauseAnalysis WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "StatTest", id, "Deleted statistical test")
            LoadTestHistory()
        ElseIf e.CommandName = "Edit" Then
            LoadTestForEdit(id)
        End If
    End Sub

    Private Sub LoadTestForEdit(id As Integer)
        Using cmd As New MySqlCommand(
            "SELECT * FROM RootCauseAnalysis WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfEditId.Value = id.ToString()

                Dim category As String = row("Category").ToString()
                Dim testType As String = category
                If category.StartsWith(STAT_TEST_CATEGORY & "_") Then
                    testType = category.Substring((STAT_TEST_CATEGORY & "_").Length)
                End If
                If ddlTestType.Items.FindByValue(testType) IsNot Nothing Then
                    ddlTestType.SelectedValue = testType
                End If
                ddlTestType_SelectedIndexChanged(Nothing, Nothing)
                txtTestDescription.Text = row("Cause").ToString()
                txtTestStatistic.Text = row("FiveWhy_Chain").ToString()

                Dim effect As String = row("Effect").ToString()
                Dim parts As String() = effect.Split("|"c)
                If parts.Length >= 3 Then
                    txtPValue.Text = parts(0)
                    If ddlSignificance.Items.FindByValue(parts(1)) IsNot Nothing Then
                        ddlSignificance.SelectedValue = parts(1)
                    End If
                    txtAlpha.Text = parts(2)
                    If parts.Length >= 4 Then
                        DeserializeParams(parts(3))
                    End If
                End If

                txtConclusion.Text = row("Sequence").ToString()
                btnSaveTest.Text = "Update Test"
            End If
        End Using
    End Sub

    Private Sub ClearForm()
        ddlTestType.SelectedIndex = 0
        txtTestDescription.Text = ""
        txtSampleSize.Text = ""
        txtSampleMean.Text = ""
        txtStdDev.Text = ""
        txtHypothesizedMean.Text = ""
        txtSample2Mean.Text = ""
        txtSample2StdDev.Text = ""
        txtSample2Size.Text = ""
        txtDF.Text = ""
        txtExpectedProp.Text = ""
        txtTestStatistic.Text = ""
        txtPValue.Text = ""
        txtAlpha.Text = "0.05"
        ddlSignificance.SelectedIndex = 0
        txtConclusion.Text = ""
        hfEditId.Value = "0"
        btnSaveTest.Text = "Save Test"
        pnlParameters.Visible = False
        pnlNoTest.Visible = True
    End Sub

    Private Function SerializeParams(dict As Dictionary(Of String, String)) As String
        Dim parts As New List(Of String)()
        For Each kv In dict
            parts.Add(kv.Key & "=" & kv.Value)
        Next
        Return String.Join(";"c, parts)
    End Function

    Private Sub DeserializeParams(data As String)
        For Each part As String In data.Split(";"c)
            Dim eqIdx As Integer = part.IndexOf("="c)
            If eqIdx > 0 Then
                Dim key As String = part.Substring(0, eqIdx)
                Dim val As String = part.Substring(eqIdx + 1)
                Select Case key
                    Case "SampleSize" : txtSampleSize.Text = val
                    Case "SampleMean" : txtSampleMean.Text = val
                    Case "StdDev" : txtStdDev.Text = val
                    Case "HypothesizedMean" : txtHypothesizedMean.Text = val
                    Case "Sample2Mean" : txtSample2Mean.Text = val
                    Case "Sample2StdDev" : txtSample2StdDev.Text = val
                    Case "Sample2Size" : txtSample2Size.Text = val
                    Case "DF" : txtDF.Text = val
                    Case "ExpectedProp" : txtExpectedProp.Text = val
                End Select
            End If
        Next
    End Sub

    Public Function GetSignificance(effect As Object) As String
        If effect Is Nothing OrElse effect Is DBNull.Value Then Return String.Empty
        Dim parts As String() = effect.ToString().Split("|"c)
        If parts.Length >= 2 Then Return parts(1)
        Return String.Empty
    End Function

    Public Function GetSigBadge(effect As Object) As String
        Dim sig As String = GetSignificance(effect)
        Select Case sig.ToLower()
            Case "significant" : Return "danger"
            Case "not significant" : Return "success"
            Case Else : Return "secondary"
        End Select
    End Function

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 9, "In Progress")
    End Sub
End Class
