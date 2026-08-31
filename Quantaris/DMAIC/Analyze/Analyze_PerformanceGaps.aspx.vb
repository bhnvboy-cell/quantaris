Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Analyze_Analyze_PerformanceGaps
    Inherits BasePage

    Private _projectId As Integer

    Private Const BRAINSTORMING_CATEGORY As String = "Brainstorming"
    Private Const BRAINSTORMING_CAUSE As String = "Notes"

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadBrainstorming()
            LoadFishbone()
            LoadFiveWhy()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadBrainstorming()
        Using cmd As New MySqlCommand(
            "SELECT Cause, Effect FROM RootCauseAnalysis " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND IsFishbone = FALSE AND Category = @Category ORDER BY CreatedOn DESC LIMIT 1")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@Category", BRAINSTORMING_CATEGORY)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                txtBrainstorming.Text = dt.Rows(0)("Cause").ToString()
            End If
        End Using
    End Sub

    Private Sub LoadFishbone()
        Using cmd As New MySqlCommand(
            "SELECT Category, Cause FROM RootCauseAnalysis " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND IsFishbone = TRUE ORDER BY Id")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)

            Dim machine As DataTable = dt.Clone()
            Dim method As DataTable = dt.Clone()
            Dim material As DataTable = dt.Clone()
            Dim measurement As DataTable = dt.Clone()
            Dim envt As DataTable = dt.Clone()
            Dim manpower As DataTable = dt.Clone()

            For Each row As DataRow In dt.Rows
                Dim cat As String = row("Category").ToString().ToLower()
                Dim target As DataTable = Nothing
                Select Case cat
                    Case "machine" : target = machine
                    Case "method" : target = method
                    Case "material" : target = material
                    Case "measurement" : target = measurement
                    Case "environment" : target = envt
                    Case "manpower" : target = manpower
                End Select
                If target IsNot Nothing Then target.ImportRow(row)
            Next

            rptMachine.DataSource = machine : rptMachine.DataBind()
            rptMethod.DataSource = method : rptMethod.DataBind()
            rptMaterial.DataSource = material : rptMaterial.DataBind()
            rptMeasurement.DataSource = measurement : rptMeasurement.DataBind()
            rptEnvironment.DataSource = envt : rptEnvironment.DataBind()
            rptManpower.DataSource = manpower : rptManpower.DataBind()
            litFishboneCount.Text = dt.Rows.Count & " cause(s)"
        End Using
    End Sub

    Private Sub LoadFiveWhy()
        Using cmd As New MySqlCommand(
            "SELECT * FROM RootCauseAnalysis " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND IsFishbone = FALSE " &
            "AND Category IS NULL AND Effect IS NOT NULL ORDER BY CreatedOn")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptFiveWhy.DataSource = dt
            rptFiveWhy.DataBind()
            litFiveWhyCount.Text = dt.Rows.Count & " chain(s)"
        End Using
    End Sub

    Protected Sub btnSaveBrainstorming_Click(sender As Object, e As EventArgs) Handles btnSaveBrainstorming.Click
        Dim exists As Boolean
        Using chkCmd As New MySqlCommand("SELECT COUNT(1) FROM RootCauseAnalysis WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND IsFishbone = FALSE AND Category = @Category")
            chkCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            chkCmd.Parameters.AddWithValue("@ProjectId", _projectId)
            chkCmd.Parameters.AddWithValue("@Category", BRAINSTORMING_CATEGORY)
            exists = CInt(DbHelper.ExecuteScalar(chkCmd)) > 0
        End Using
        If exists Then
            Using updCmd As New MySqlCommand("UPDATE RootCauseAnalysis SET Cause = @Cause, CreatedOn = CURRENT_TIMESTAMP WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND IsFishbone = FALSE AND Category = @Category")
                updCmd.Parameters.AddWithValue("@Cause", txtBrainstorming.Text.Trim())
                updCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                updCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                updCmd.Parameters.AddWithValue("@Category", BRAINSTORMING_CATEGORY)
                DbHelper.ExecuteNonQuery(updCmd)
            End Using
        Else
            Using insCmd As New MySqlCommand("INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Category, Cause, IsFishbone, CreatedByUserId, CreatedOn) VALUES (@TenantId, @ProjectId, @Category, @Cause, FALSE, @CreatedByUserId, CURRENT_TIMESTAMP)")
                insCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                insCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                insCmd.Parameters.AddWithValue("@Category", BRAINSTORMING_CATEGORY)
                insCmd.Parameters.AddWithValue("@Cause", txtBrainstorming.Text.Trim())
                insCmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(insCmd)
            End Using
        End If
        LogAudit("Save", "Brainstorming", _projectId, "Saved brainstorming notes")
        LoadBrainstorming()
    End Sub

    Protected Sub btnSaveFishbone_Click(sender As Object, e As EventArgs) Handles btnSaveFishbone.Click
        SaveFishboneCategory("Machine", txtMachine.Text.Trim())
        SaveFishboneCategory("Method", txtMethod.Text.Trim())
        SaveFishboneCategory("Material", txtMaterial.Text.Trim())
        SaveFishboneCategory("Measurement", txtMeasurement.Text.Trim())
        SaveFishboneCategory("Environment", txtEnvironment.Text.Trim())
        SaveFishboneCategory("Manpower", txtManpower.Text.Trim())

        txtMachine.Text = ""
        txtMethod.Text = ""
        txtMaterial.Text = ""
        txtMeasurement.Text = ""
        txtEnvironment.Text = ""
        txtManpower.Text = ""

        LogAudit("Save", "Fishbone", _projectId, "Saved fishbone diagram causes")
        LoadFishbone()
    End Sub

    Private Sub SaveFishboneCategory(category As String, causes As String)
        If String.IsNullOrWhiteSpace(causes) Then Return

        Using cmd As New MySqlCommand("DELETE FROM RootCauseAnalysis " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND IsFishbone = TRUE AND Category = @Category")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@Category", category)
            DbHelper.ExecuteNonQuery(cmd)
        End Using

        For Each causeLine As String In causes.Split({vbCrLf, vbLf, vbCr}, StringSplitOptions.RemoveEmptyEntries)
            Dim trimmed As String = causeLine.Trim()
            If String.IsNullOrWhiteSpace(trimmed) Then Continue For
            Using cmd As New MySqlCommand(
                "INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Category, Cause, IsFishbone, CreatedByUserId, CreatedOn) " &
                "VALUES (@TenantId, @ProjectId, @Category, @Cause, 1, @CreatedByUserId, CURRENT_TIMESTAMP)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@Category", category)
                cmd.Parameters.AddWithValue("@Cause", trimmed)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
        Next
    End Sub

    Protected Sub btnAddFiveWhy_Click(sender As Object, e As EventArgs) Handles btnAddFiveWhy.Click
        Using cmd As New MySqlCommand(
            "INSERT INTO RootCauseAnalysis (TenantId, ProjectId, Cause, Effect, IsFishbone, FiveWhy_Chain, CreatedByUserId, CreatedOn) " &
            "VALUES (@TenantId, @ProjectId, @Cause, @Effect, FALSE, @FiveWhy_Chain, @CreatedByUserId, CURRENT_TIMESTAMP)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            cmd.Parameters.AddWithValue("@Cause", "New Problem Statement")
            cmd.Parameters.AddWithValue("@Effect", "New Root Cause")
            Dim whys As String() = {"", "", "", "", ""}
            cmd.Parameters.AddWithValue("@FiveWhy_Chain", String.Join("|", whys))
            cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Create", "FiveWhy", _projectId, "Added new 5-Why chain")
        LoadFiveWhy()
    End Sub

    Protected Sub rptFiveWhy_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM RootCauseAnalysis WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "FiveWhy", id, "Deleted 5-Why chain")
            LoadFiveWhy()
        ElseIf e.CommandName = "Update" Then
            Dim problem As String = DirectCast(e.Item.FindControl("txtFWProblem"), TextBox).Text.Trim()
            Dim rootCause As String = DirectCast(e.Item.FindControl("txtFWRootCause"), TextBox).Text.Trim()
            Dim why1 As String = DirectCast(e.Item.FindControl("txtWhy1"), TextBox).Text.Trim()
            Dim why2 As String = DirectCast(e.Item.FindControl("txtWhy2"), TextBox).Text.Trim()
            Dim why3 As String = DirectCast(e.Item.FindControl("txtWhy3"), TextBox).Text.Trim()
            Dim why4 As String = DirectCast(e.Item.FindControl("txtWhy4"), TextBox).Text.Trim()
            Dim why5 As String = DirectCast(e.Item.FindControl("txtWhy5"), TextBox).Text.Trim()
            Dim chain As String = String.Join("|", {why1, why2, why3, why4, why5})

            Using cmd As New MySqlCommand(
                "UPDATE RootCauseAnalysis SET Cause = @Cause, Effect = @Effect, FiveWhy_Chain = @FiveWhy_Chain " &
                "WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@Cause", problem)
                cmd.Parameters.AddWithValue("@Effect", rootCause)
                cmd.Parameters.AddWithValue("@FiveWhy_Chain", chain)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "FiveWhy", id, "Updated 5-Why chain")
            LoadFiveWhy()
        End If
    End Sub

    Public Function GetWhy(fiveWhyChain As Object, index As Integer) As String
        If fiveWhyChain Is Nothing OrElse fiveWhyChain Is DBNull.Value Then Return String.Empty
        Dim parts As String() = fiveWhyChain.ToString().Split("|"c)
        If index >= 0 AndAlso index < parts.Length Then Return parts(index)
        Return String.Empty
    End Function

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 7, "In Progress")
    End Sub
End Class
