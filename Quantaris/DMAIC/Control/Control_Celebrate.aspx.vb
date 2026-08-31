Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Web.Script.Serialization

Public Partial Class DMAIC_Control_Control_Celebrate
    Inherits BasePage

    Private _projectId As Integer
    Private _json As New JavaScriptSerializer()

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadEntries()
            LoadPIRSummary()
            CheckProjectCompletion()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 15, "In Progress")
    End Sub

    Private Sub LoadEntries()
        Using cmd As New MySqlCommand(
            "SELECT * FROM Celebrations WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)

            ' Add a display-friendly type column for badges
            For Each row As DataRow In dt.Rows
                Select Case row("CelebrationType").ToString()
                    Case "LessonLearned"
                        row("CelebrationType") = "<span class='badge bg-info'>Lesson Learned</span>"
                    Case "Recognition"
                        row("CelebrationType") = "<span class='badge bg-warning text-dark'>Recognition</span>"
                    Case "Celebration"
                        row("CelebrationType") = "<span class='badge bg-success'>Celebration</span>"
                End Select
            Next

            rptEntries.DataSource = dt
            rptEntries.DataBind()
            litCount.Text = dt.Rows.Count & " entr(ies)"
        End Using
    End Sub

    Protected Sub rptEntries_ItemDataBound(sender As Object, e As RepeaterItemEventArgs) Handles rptEntries.ItemDataBound
        If e.Item.ItemType = ListItemType.Item OrElse e.Item.ItemType = ListItemType.AlternatingItem Then
            Dim row = DirectCast(e.Item.DataItem, DataRowView)
            Dim litBadge = DirectCast(e.Item.FindControl("litTypeBadge"), Literal)
            If litBadge IsNot Nothing Then
                Select Case row("CelebrationType").ToString()
                    Case "LessonLearned"
                        litBadge.Text = "<span class='badge bg-info'>Lesson Learned</span>"
                    Case "Recognition"
                        litBadge.Text = "<span class='badge bg-warning text-dark'>Recognition</span>"
                    Case "Celebration"
                        litBadge.Text = "<span class='badge bg-success'>Celebration</span>"
                    Case Else
                        litBadge.Text = "<span class='badge bg-secondary'>" & row("CelebrationType") & "</span>"
                End Select
            End If
        End If
    End Sub

    Private Function ParseDate(value As String) As Object
        If String.IsNullOrWhiteSpace(value) Then Return DBNull.Value
        Dim result As Date
        If Date.TryParse(value.Trim(), result) Then Return result
        Return DBNull.Value
    End Function

    Private Sub ClearForm()
        ddlType.SelectedIndex = 0
        txtTitle.Text = ""
        txtDescription.Text = ""
        txtParticipants.Text = ""
        txtRecognitionDate.Text = ""
        hfEditId.Value = "0"
        btnCancel.Visible = False
        btnSave.Text = "Save Entry"
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not Page.IsValid Then Return
        Dim editId As Integer = CInt(hfEditId.Value)

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE Celebrations SET CelebrationType = @CelebrationType, Title = @Title, " &
                "Description = @Description, Participants = @Participants, RecognitionDate = @RecognitionDate " &
                "WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@CelebrationType", ddlType.SelectedValue)
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim())
                cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(txtDescription.Text), CObj(DBNull.Value), txtDescription.Text.Trim()))
                cmd.Parameters.AddWithValue("@Participants", If(String.IsNullOrWhiteSpace(txtParticipants.Text), CObj(DBNull.Value), txtParticipants.Text.Trim()))
                cmd.Parameters.AddWithValue("@RecognitionDate", ParseDate(txtRecognitionDate.Text))
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "Celebration", editId, "Updated celebration entry: " & txtTitle.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO Celebrations (TenantId, ProjectId, CelebrationType, Title, Description, Participants, " &
                "RecognitionDate, CreatedByUserId, CreatedOn) " &
                "VALUES (@TenantId, @ProjectId, @CelebrationType, @Title, @Description, @Participants, " &
                "@RecognitionDate, @CreatedByUserId, CURRENT_TIMESTAMP)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@CelebrationType", ddlType.SelectedValue)
                cmd.Parameters.AddWithValue("@Title", txtTitle.Text.Trim())
                cmd.Parameters.AddWithValue("@Description", If(String.IsNullOrWhiteSpace(txtDescription.Text), CObj(DBNull.Value), txtDescription.Text.Trim()))
                cmd.Parameters.AddWithValue("@Participants", If(String.IsNullOrWhiteSpace(txtParticipants.Text), CObj(DBNull.Value), txtParticipants.Text.Trim()))
                cmd.Parameters.AddWithValue("@RecognitionDate", ParseDate(txtRecognitionDate.Text))
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "Celebration", _projectId, "Added celebration entry: " & txtTitle.Text)
        End If

        ClearForm()
        LoadEntries()
        CheckProjectCompletion()
    End Sub

    Protected Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        ClearForm()
    End Sub

    Protected Sub rptEntries_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM Celebrations WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "Celebration", id, "Deleted celebration entry")
            LoadEntries()
            CheckProjectCompletion()
        ElseIf e.CommandName = "Edit" Then
            LoadEntryForEdit(id)
        End If
    End Sub

    Private Sub LoadEntryForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM Celebrations WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfEditId.Value = id.ToString()
                If ddlType.Items.FindByValue(row("CelebrationType").ToString()) IsNot Nothing Then
                    ddlType.SelectedValue = row("CelebrationType").ToString()
                End If
                txtTitle.Text = row("Title").ToString()
                txtDescription.Text = row("Description").ToString()
                txtParticipants.Text = row("Participants").ToString()
                txtRecognitionDate.Text = If(row("RecognitionDate") IsNot DBNull.Value, CDate(row("RecognitionDate")).ToString("yyyy-MM-dd"), "")
                btnCancel.Visible = True
                btnSave.Text = "Update Entry"
            End If
        End Using
    End Sub

    Private Sub LoadPIRSummary()
        Using cmd As New MySqlCommand(
            "SELECT ControlMethod FROM ControlPlans WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND ProcessStep = 'META_PIR'")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim result = DbHelper.ExecuteScalar(cmd)
            If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                Try
                    Dim data = _json.Deserialize(Of Dictionary(Of String, String))(result.ToString())
                    If data.ContainsKey("Summary") Then
                        txtPIRSummary.Text = data("Summary")
                    End If
                Catch
                End Try
            End If
        End Using
    End Sub

    Protected Sub btnSavePIR_Click(sender As Object, e As EventArgs) Handles btnSavePIR.Click
        Dim data As New Dictionary(Of String, String)()
        data("Summary") = txtPIRSummary.Text.Trim()
        Dim jsonData = _json.Serialize(data)

        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using chkCmd As New MySqlCommand("SELECT COUNT(1) FROM ControlPlans WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND ProcessStep = 'META_PIR'", conn)
                chkCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                chkCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                Dim exists As Boolean = CInt(chkCmd.ExecuteScalar()) > 0
                If exists Then
                    Using updCmd As New MySqlCommand("UPDATE ControlPlans SET ControlMethod = @ControlMethod WHERE TenantId = @TenantId AND ProjectId = @ProjectId AND ProcessStep = 'META_PIR'", conn)
                        updCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                        updCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                        updCmd.Parameters.AddWithValue("@ControlMethod", jsonData)
                        updCmd.ExecuteNonQuery()
                    End Using
                Else
                    Using insCmd As New MySqlCommand("INSERT INTO ControlPlans (TenantId, ProjectId, ProcessStep, ControlMethod, CreatedByUserId, CreatedOn) VALUES (@TenantId, @ProjectId, 'META_PIR', @ControlMethod, @CreatedByUserId, CURRENT_TIMESTAMP)", conn)
                        insCmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                        insCmd.Parameters.AddWithValue("@ProjectId", _projectId)
                        insCmd.Parameters.AddWithValue("@ControlMethod", jsonData)
                        insCmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                        insCmd.ExecuteNonQuery()
                    End Using
                End If
            End Using
        End Using
        LogAudit("Update", "PIRSummary", _projectId, "Saved post-implementation review summary")
        lblPIRSaved.Visible = True
    End Sub

    Private Sub CheckProjectCompletion()
        Using cmd As New MySqlCommand(
            "SELECT COUNT(*) FROM DMAICSteps WHERE ProjectId = @ProjectId AND Status = 'Complete'")
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim completeCount As Integer = CInt(DbHelper.ExecuteScalar(cmd))

            If completeCount >= 15 Then
                pnlAllStepsComplete.Visible = True
                pnlStepsRemaining.Visible = False
            Else
                pnlAllStepsComplete.Visible = False
                pnlStepsRemaining.Visible = True
                Dim remaining = 15 - completeCount
                litStepsRemaining.Text = "<div class='alert alert-warning mb-0'><strong>" & remaining &
                    " step(s) remaining</strong> to mark the project as complete.</div>"
            End If
        End Using
    End Sub

    Protected Sub btnCompleteProject_Click(sender As Object, e As EventArgs) Handles btnCompleteProject.Click
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand(
                "UPDATE QuantarisProjects SET Status = 'Complete', Phase = 'Control', ActualEndDate = CURRENT_TIMESTAMP " &
                "WHERE Id = @ProjectId AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.ExecuteNonQuery()
            End Using
        End Using
        LogAudit("Complete", "Project", _projectId, "Project marked as complete")
        pnlAllStepsComplete.Visible = False
        pnlStepsRemaining.Visible = True
        litStepsRemaining.Text = "<div class='alert alert-success'><i class='bi bi-check-circle me-2'></i>Project has been marked as <strong>Complete</strong>!</div>"
    End Sub
End Class
