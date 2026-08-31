Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Linq

Public Partial Class Tools_AIRootCause
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlMain.Visible = True
                pnlNoProject.Visible = False
                LoadPatterns()
                LoadHistory()
            Else
                pnlMain.Visible = False
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

    Private Sub LoadPatterns()
        Dim sql As String = "SELECT p.Id, p.SymptomKeyword, p.RootCause, p.Probability, p.OccurrenceCount, p.Category, m.ModelName " &
            "FROM AI_RootCausePatterns p JOIN AI_RootCauseModels m ON p.ModelId = m.Id " &
            "WHERE m.TenantId = @TenantId"
        If txtFilterPattern.Text.Trim() <> "" Then
            sql &= " AND (p.SymptomKeyword LIKE @Filter OR p.RootCause LIKE @Filter OR p.Category LIKE @Filter)"
        End If
        sql &= " ORDER BY p.OccurrenceCount DESC"

        Using cmd As New MySqlCommand(sql)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            If txtFilterPattern.Text.Trim() <> "" Then
                cmd.Parameters.AddWithValue("@Filter", "%" & txtFilterPattern.Text.Trim() & "%")
            End If
            rptPatterns.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptPatterns.DataBind()
        End Using
    End Sub

    Private Sub LoadHistory()
        Dim projectId = GetSelectedProjectId()
        If projectId = 0 Then Exit Sub

        Using cmd As New MySqlCommand(
            "SELECT * FROM AI_RootCausePredictions WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", projectId)
            rptHistory.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptHistory.DataBind()
        End Using
    End Sub

    Private Function Tokenize(text As String) As List(Of String)
        Dim result As New List(Of String)
        Dim parts = text.Split(New Char() {","c, " "c, ";"c, "."c, vbCr, vbLf, vbTab}, StringSplitOptions.RemoveEmptyEntries)
        For Each p In parts
            Dim trimmed = p.Trim().ToLower()
            If trimmed.Length > 1 Then
                result.Add(trimmed)
            End If
        Next
        Return result
    End Function

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlMain.Visible = True
            pnlNoProject.Visible = False
            pnlResults.Visible = False
            LoadPatterns()
            LoadHistory()
        Else
            pnlMain.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnPredict_Click(sender As Object, e As EventArgs)
        Dim projectId = GetSelectedProjectId()
        If projectId = 0 Then
            litMessage.Text = "<div class='alert alert-warning'>Please select a project.</div>"
            Return
        End If

        Dim symptoms = txtSymptoms.Text.Trim()
        If String.IsNullOrEmpty(symptoms) Then
            litMessage.Text = "<div class='alert alert-warning'>Please describe the symptoms.</div>"
            Return
        End If

        Try
            Dim tokens = Tokenize(symptoms)
            If tokens.Count = 0 Then
                litMessage.Text = "<div class='alert alert-warning'>Could not extract meaningful keywords from symptoms.</div>"
                Return
            End If

            Dim dtPatterns As DataTable
            Using cmd As New MySqlCommand(
                "SELECT p.Id, p.SymptomKeyword, p.RootCause, p.Probability, p.OccurrenceCount, p.Category " &
                "FROM AI_RootCausePatterns p JOIN AI_RootCauseModels m ON p.ModelId = m.Id " &
                "WHERE m.TenantId = @TenantId AND m.IsActive = TRUE")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                dtPatterns = DbHelper.ExecuteDataTable(cmd)
            End Using

            Dim matchedRows As New List(Of DataRow)
            For Each r As DataRow In dtPatterns.Rows
                Dim keyword = r("SymptomKeyword").ToString().ToLower()
                For Each t In tokens
                    If keyword.Contains(t) OrElse t.Contains(keyword) Then
                        matchedRows.Add(r)
                        Exit For
                    End If
                Next
            Next

            Dim scores As New Dictionary(Of String, Decimal)
            For Each r In matchedRows
                Dim cause = r("RootCause").ToString()
                Dim prob = CDec(r("Probability"))
                Dim count = CInt(r("OccurrenceCount"))
                If Not scores.ContainsKey(cause) Then
                    scores(cause) = 0
                End If
                scores(cause) += prob * count
            Next

            pnlFeedback.Visible = False
            pnlActualRootCause.Visible = False
            litFeedbackMessage.Text = ""

            If scores.Count = 0 Then
                litPredictedRootCause.Text = "No matching patterns found."
                litConfidence.Text = "<span class='badge bg-secondary'>0%</span>"
                rptMatchedPatterns.DataSource = Nothing
                rptMatchedPatterns.DataBind()
                pnlResults.Visible = True
                Return
            End If

            Dim totalScore = scores.Values.Sum()
            Dim topKvp = scores.OrderByDescending(Function(kvp) kvp.Value).First()
            Dim confidencePct = topKvp.Value / totalScore * 100

            litPredictedRootCause.Text = topKvp.Key
            Dim badgeClass = "bg-success"
            If confidencePct < 50 Then badgeClass = "bg-danger"
            If confidencePct >= 50 AndAlso confidencePct < 75 Then badgeClass = "bg-warning text-dark"
            litConfidence.Text = "<span class='badge " & badgeClass & " fs-6 rounded-pill px-3'>" & confidencePct.ToString("F1") & "%</span>"

            Dim matchedDt As New DataTable()
            matchedDt.Columns.Add("SymptomKeyword", GetType(String))
            matchedDt.Columns.Add("RootCause", GetType(String))
            matchedDt.Columns.Add("Probability", GetType(String))
            matchedDt.Columns.Add("OccurrenceCount", GetType(Integer))
            For Each r In matchedRows
                matchedDt.Rows.Add(r("SymptomKeyword").ToString(), r("RootCause").ToString(),
                    CDec(r("Probability")).ToString("F4"), CInt(r("OccurrenceCount")))
            Next
            rptMatchedPatterns.DataSource = matchedDt
            rptMatchedPatterns.DataBind()

            Dim modelId As Object
            Using cmd As New MySqlCommand(
                "SELECT Id FROM AI_RootCauseModels WHERE TenantId = @TenantId AND IsActive = TRUE LIMIT 1")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                modelId = DbHelper.ExecuteScalar(cmd)
            End Using

            Using cmd As New MySqlCommand(
                "INSERT INTO AI_RootCausePredictions (TenantId, ProjectId, Symptoms, PredictedRootCause, Confidence, ModelId, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @Symptoms, @PredictedRootCause, @Confidence, @ModelId, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", projectId)
                cmd.Parameters.AddWithValue("@Symptoms", symptoms)
                cmd.Parameters.AddWithValue("@PredictedRootCause", topKvp.Key)
                cmd.Parameters.AddWithValue("@Confidence", CDec(confidencePct / 100))
                If modelId IsNot Nothing Then
                    cmd.Parameters.AddWithValue("@ModelId", CInt(modelId))
                Else
                    cmd.Parameters.AddWithValue("@ModelId", DBNull.Value)
                End If
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
                Dim predId = CInt(cmd.LastInsertedId)
                ViewState("PredictionId") = predId
            End Using

            pnlResults.Visible = True
            pnlFeedback.Visible = True
            litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Prediction complete.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"

        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnFeedbackYes_Click(sender As Object, e As EventArgs)
        Dim predId = CType(ViewState("PredictionId"), Integer?)
        If Not predId.HasValue Then
            litFeedbackMessage.Text = "<div class='text-danger small'>No prediction to evaluate.</div>"
            Return
        End If

        Try
            Using cmd As New MySqlCommand(
                "UPDATE AI_RootCausePredictions SET WasAccurate = TRUE WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", predId.Value)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using

            Using cmd As New MySqlCommand(
                "SELECT Symptoms, PredictedRootCause FROM AI_RootCausePredictions WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", predId.Value)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    Dim predictedCause = dt.Rows(0)("PredictedRootCause").ToString()
                    Using upd As New MySqlCommand(
                        "UPDATE AI_RootCausePatterns SET OccurrenceCount = OccurrenceCount + 1 " &
                        "WHERE Id IN (SELECT p.Id FROM AI_RootCausePatterns p " &
                        "JOIN AI_RootCauseModels m ON p.ModelId = m.Id " &
                        "WHERE m.TenantId = @TenantId AND p.RootCause = @RootCause AND m.IsActive = TRUE)")
                        upd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                        upd.Parameters.AddWithValue("@RootCause", predictedCause)
                        DbHelper.ExecuteNonQuery(upd)
                    End Using
                End If
            End Using

            LogAudit("Update", "AIPrediction", predId.Value, "Marked prediction as accurate")
            litFeedbackMessage.Text = "<div class='text-success small'><i class='bi bi-check-circle me-1'></i>Thank you for your feedback!</div>"
            btnFeedbackYes.Visible = False
            btnFeedbackNo.Visible = False

        Catch ex As Exception
            litFeedbackMessage.Text = "<div class='text-danger small'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnFeedbackNo_Click(sender As Object, e As EventArgs)
        Dim predId = CType(ViewState("PredictionId"), Integer?)
        If Not predId.HasValue Then
            litFeedbackMessage.Text = "<div class='text-danger small'>No prediction to evaluate.</div>"
            Return
        End If

        pnlActualRootCause.Visible = True
        litFeedbackMessage.Text = "<div class='text-muted small'>Please enter the actual root cause and submit.</div>"
    End Sub

    Protected Sub btnSubmitFeedback_Click(sender As Object, e As EventArgs)
        Dim predId = CType(ViewState("PredictionId"), Integer?)
        If Not predId.HasValue Then
            litFeedbackMessage.Text = "<div class='text-danger small'>No prediction to evaluate.</div>"
            Return
        End If

        Dim actualCause = txtActualRootCause.Text.Trim()
        If String.IsNullOrEmpty(actualCause) Then
            litFeedbackMessage.Text = "<div class='text-danger small'>Please enter the actual root cause.</div>"
            Return
        End If

        Try
            Using cmd As New MySqlCommand(
                "UPDATE AI_RootCausePredictions SET WasAccurate = FALSE, ActualRootCause = @ActualRootCause WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", predId.Value)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ActualRootCause", actualCause)
                DbHelper.ExecuteNonQuery(cmd)
            End Using

            LogAudit("Update", "AIPrediction", predId.Value, "Marked prediction as inaccurate. Actual: " & actualCause)
            litFeedbackMessage.Text = "<div class='text-success small'><i class='bi bi-check-circle me-1'></i>Thank you for your feedback!</div>"
            btnFeedbackYes.Visible = False
            btnFeedbackNo.Visible = False
            pnlActualRootCause.Visible = False

        Catch ex As Exception
            litFeedbackMessage.Text = "<div class='text-danger small'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnAddPattern_Click(sender As Object, e As EventArgs)
        Dim keyword = txtPatternKeyword.Text.Trim()
        Dim rootCause = txtPatternRootCause.Text.Trim()
        Dim category = ddlPatternCategory.SelectedValue

        If String.IsNullOrEmpty(keyword) OrElse String.IsNullOrEmpty(rootCause) Then
            litPatternMessage.Text = "<div class='alert alert-warning'>Keyword and Root Cause are required.</div>"
            Return
        End If

        Try
            Dim modelId As Object
            Using cmd As New MySqlCommand(
                "SELECT Id FROM AI_RootCauseModels WHERE TenantId = @TenantId AND IsActive = TRUE LIMIT 1")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                modelId = DbHelper.ExecuteScalar(cmd)
            End Using

            If modelId Is Nothing Then
                Using cmd As New MySqlCommand(
                    "INSERT INTO AI_RootCauseModels (TenantId, ModelName, ModelType, CreatedByUserId) " &
                    "VALUES (@TenantId, 'Default Bayesian Model', 'Bayesian', @CreatedByUserId)")
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                    DbHelper.ExecuteNonQuery(cmd)
                    modelId = CInt(cmd.LastInsertedId)
                End Using
            End If

            Using cmd As New MySqlCommand(
                "INSERT INTO AI_RootCausePatterns (ModelId, SymptomKeyword, RootCause, Probability, OccurrenceCount, Category) " &
                "VALUES (@ModelId, @SymptomKeyword, @RootCause, @Probability, @OccurrenceCount, @Category)")
                cmd.Parameters.AddWithValue("@ModelId", CInt(modelId))
                cmd.Parameters.AddWithValue("@SymptomKeyword", keyword.ToLower())
                cmd.Parameters.AddWithValue("@RootCause", rootCause)
                cmd.Parameters.AddWithValue("@Probability", 0.5)
                cmd.Parameters.AddWithValue("@OccurrenceCount", 1)
                If category = "" Then
                    cmd.Parameters.AddWithValue("@Category", DBNull.Value)
                Else
                    cmd.Parameters.AddWithValue("@Category", category)
                End If
                DbHelper.ExecuteNonQuery(cmd)
            End Using

            LogAudit("Create", "AIPattern", Nothing, "Added pattern: " & keyword & " -> " & rootCause)
            litPatternMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Pattern added successfully.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
            txtPatternKeyword.Text = ""
            txtPatternRootCause.Text = ""
            ddlPatternCategory.SelectedIndex = 0
            LoadPatterns()

        Catch ex As Exception
            litPatternMessage.Text = "<div class='alert alert-danger'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnFilterPattern_Click(sender As Object, e As EventArgs)
        LoadPatterns()
    End Sub

    Protected Sub btnClearPatternFilter_Click(sender As Object, e As EventArgs)
        txtFilterPattern.Text = ""
        LoadPatterns()
    End Sub

    Protected Function GetConfidenceBadge(confidence As Object) As String
        If confidence Is Nothing OrElse confidence Is DBNull.Value Then
            Return "<span class='badge bg-secondary'>-</span>"
        End If
        Dim val = CDec(confidence) * 100
        Dim badgeClass = "bg-success"
        If val < 50 Then badgeClass = "bg-danger"
        If val >= 50 AndAlso val < 75 Then badgeClass = "bg-warning text-dark"
        Return "<span class='badge " & badgeClass & " rounded-pill'>" & val.ToString("F1") & "%</span>"
    End Function

    Protected Function GetAccuracyIcon(wasAccurate As Object) As String
        If wasAccurate Is Nothing OrElse wasAccurate Is DBNull.Value Then
            Return "<span class='text-muted'><i class='bi bi-dash-circle'></i></span>"
        End If
        If CBool(wasAccurate) Then
            Return "<span class='text-success'><i class='bi bi-check-circle-fill'></i></span>"
        Else
            Return "<span class='text-danger'><i class='bi bi-x-circle-fill'></i></span>"
        End If
    End Function
End Class
