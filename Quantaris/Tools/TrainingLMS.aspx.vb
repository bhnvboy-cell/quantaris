Imports System.Data
Imports MySql.Data.MySqlClient

Public Partial Class Tools_TrainingLMS
    Inherits BasePage

    Private _activeQuizId As Integer
    Private _activeCourseId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt", "User")
        If Not IsPostBack Then
            ViewState("QuizMode") = False
            ViewState("QuizPassed") = False
            LoadData()
            LoadUsers()
            LoadManageCourses()
        End If
        If CBool(ViewState("QuizMode")) Then
            pnlLMS.Visible = False
            pnlQuiz.Visible = True
            If Not IsPostBack Then LoadQuiz()
        End If
    End Sub

    Private Sub LoadData()
        Using cmd As New MySqlCommand("SELECT COUNT(*) FROM TrainingCourses WHERE TenantId = @TenantId AND IsActive = true")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            litCourseCount.Text = DbHelper.ExecuteScalar(cmd).ToString()
        End Using
        Using cmd As New MySqlCommand("SELECT COUNT(*) FROM QuizAttempts a JOIN TrainingQuizzes q ON a.QuizId = q.Id JOIN TrainingCourses c ON q.CourseId = c.Id WHERE a.TenantId = @TenantId AND a.UserId = @UserId AND a.Passed = true")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@UserId", CurrentUserId)
            litEnrollCount.Text = DbHelper.ExecuteScalar(cmd).ToString()
        End Using
        Using cmd As New MySqlCommand("SELECT COUNT(*) FROM TrainingCertifications WHERE TenantId = @TenantId AND UserId = @UserId AND IsActive = true")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@UserId", CurrentUserId)
            litCertCount.Text = DbHelper.ExecuteScalar(cmd).ToString()
        End Using
        Using cmd As New MySqlCommand("SELECT CASE WHEN COUNT(*) > 0 THEN ROUND(SUM(CASE WHEN a.Passed = true THEN 1 ELSE 0 END) * 100 / COUNT(*), 1) ELSE 0 END FROM QuizAttempts a WHERE a.TenantId = @TenantId AND a.UserId = @UserId")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@UserId", CurrentUserId)
            litCompletionRate.Text = DbHelper.ExecuteScalar(cmd).ToString() & "%"
        End Using
        Using cmd As New MySqlCommand("SELECT c.* FROM TrainingCourses c WHERE c.TenantId = @TenantId AND c.IsActive = true ORDER BY c.BeltLevel, c.CourseName")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptCourses.DataSource = dt
            rptCourses.DataBind()
            divNoCourses.Visible = dt.Rows.Count = 0
        End Using
        Using cmd As New MySqlCommand("SELECT a.*, q.QuizName, c.CourseName FROM QuizAttempts a JOIN TrainingQuizzes q ON a.QuizId = q.Id JOIN TrainingCourses c ON q.CourseId = c.Id WHERE a.TenantId = @TenantId AND a.UserId = @UserId ORDER BY a.CompletedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@UserId", CurrentUserId)
            rptAttempts.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptAttempts.DataBind()
        End Using
        Using cmd As New MySqlCommand("SELECT cert.*, CONCAT_WS(' ', u.FirstName, u.LastName) AS UserName FROM TrainingCertifications cert JOIN Users u ON cert.UserId = u.Id WHERE cert.TenantId = @TenantId AND cert.UserId = @UserId ORDER BY cert.IssueDate DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@UserId", CurrentUserId)
            rptCertifications.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptCertifications.DataBind()
        End Using
    End Sub

    Private Sub LoadUsers()
        Using cmd As New MySqlCommand("SELECT Id, CONCAT_WS(' ', FirstName, LastName) AS FullName FROM Users WHERE TenantId = @TenantId ORDER BY FirstName")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            ddlUser.DataSource = dt
            ddlUser.DataTextField = "FullName"
            ddlUser.DataValueField = "Id"
            ddlUser.DataBind()
            ddlUser.Items.Insert(0, New ListItem("-- Select User --", ""))
        End Using
    End Sub

    Private Sub LoadManageCourses()
        Using cmd As New MySqlCommand("SELECT Id, CONCAT_WS(' - ', CourseCode, CourseName) AS Name FROM TrainingCourses WHERE TenantId = @TenantId AND IsActive = true ORDER BY BeltLevel, CourseName")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            ddlMgrCourse.DataSource = dt
            ddlMgrCourse.DataTextField = "Name"
            ddlMgrCourse.DataValueField = "Id"
            ddlMgrCourse.DataBind()
            ddlMgrCourse.Items.Insert(0, New ListItem("-- Select Course --", ""))
        End Using
        ddlMgrQuiz.Items.Clear()
        ddlMgrQuiz.Items.Insert(0, New ListItem("-- Select Quiz --", ""))
        pnlQuestions.Visible = False
    End Sub

    Protected Sub ddlMgrCourse_Changed(sender As Object, e As EventArgs)
        ddlMgrQuiz.Items.Clear()
        ddlMgrQuiz.Items.Insert(0, New ListItem("-- Select Quiz --", ""))
        If ddlMgrCourse.SelectedValue <> "" Then
            Dim cid As Integer = CInt(ddlMgrCourse.SelectedValue)
            Using cmd As New MySqlCommand("SELECT Id, QuizName FROM TrainingQuizzes WHERE CourseId = @Cid ORDER BY QuizName")
                cmd.Parameters.AddWithValue("@Cid", cid)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                ddlMgrQuiz.DataSource = dt
                ddlMgrQuiz.DataTextField = "QuizName"
                ddlMgrQuiz.DataValueField = "Id"
                ddlMgrQuiz.DataBind()
                ddlMgrQuiz.Items.Insert(0, New ListItem("-- Select Quiz --", ""))
            End Using
        End If
        pnlQuestions.Visible = False
    End Sub

    Protected Sub ddlMgrQuiz_Changed(sender As Object, e As EventArgs)
        If ddlMgrQuiz.SelectedValue <> "" Then
            Dim qid As Integer = CInt(ddlMgrQuiz.SelectedValue)
            Using cmd As New MySqlCommand("SELECT * FROM QuizQuestions WHERE QuizId = @Qid ORDER BY QuestionOrder")
                cmd.Parameters.AddWithValue("@Qid", qid)
                rptMgrQuestions.DataSource = DbHelper.ExecuteDataTable(cmd)
                rptMgrQuestions.DataBind()
            End Using
            pnlQuestions.Visible = True
        Else
            pnlQuestions.Visible = False
        End If
    End Sub

    Protected Function GetBeltClass(belt As String) As String
        Select Case belt.ToLower()
            Case "white" : Return "white-belt"
            Case "yellow" : Return "yellow-belt"
            Case "green" : Return "green-belt"
            Case "black" : Return "black-belt"
            Case "master" : Return "master-black-belt"
            Case Else : Return ""
        End Select
    End Function

    Protected Function GetBeltBadge(belt As String) As String
        Select Case belt.ToLower()
            Case "white" : Return "bg-secondary"
            Case "yellow" : Return "bg-warning text-dark"
            Case "green" : Return "bg-success"
            Case "black" : Return "bg-dark"
            Case "master" : Return "bg-danger"
            Case Else : Return "bg-primary"
        End Select
    End Function

    Protected Function GetQuizStatus(courseId As Object) As String
        Dim cid As Integer = CInt(courseId)
        Dim qid As Object = Nothing
        Using cmd As New MySqlCommand("SELECT Id FROM TrainingQuizzes WHERE CourseId = @Cid AND IsActive = true LIMIT 1")
            cmd.Parameters.AddWithValue("@Cid", cid)
            qid = DbHelper.ExecuteScalar(cmd)
        End Using
        If qid Is Nothing Then Return "<span class='small text-muted'>No quiz available</span>"
        Dim quizId As Integer = CInt(qid)
        Dim passed As Boolean = False
        Using cmd As New MySqlCommand("SELECT COUNT(1) FROM QuizAttempts WHERE QuizId = @Qid AND UserId = @Uid AND Passed = true")
            cmd.Parameters.AddWithValue("@Qid", quizId)
            cmd.Parameters.AddWithValue("@Uid", CurrentUserId)
            passed = CInt(DbHelper.ExecuteScalar(cmd)) > 0
        End Using
        If passed Then Return "<span class='small text-success fw-bold'><i class='bi bi-check-circle me-1'></i>Certified</span>"
        Dim attempts As Integer = 0
        Using cmd As New MySqlCommand("SELECT COUNT(1) FROM QuizAttempts WHERE QuizId = @Qid AND UserId = @Uid")
            cmd.Parameters.AddWithValue("@Qid", quizId)
            cmd.Parameters.AddWithValue("@Uid", CurrentUserId)
            attempts = CInt(DbHelper.ExecuteScalar(cmd))
        End Using
        If attempts >= 2 Then Return "<span class='small text-danger'><i class='bi bi-x-circle me-1'></i>No attempts remaining</span>"
        Return "<span class='small text-primary'>" & (2 - attempts) & " attempt(s) remaining</span>"
    End Function

    Protected Function HasCertification(courseId As Object) As Boolean
        Dim cid As Integer = CInt(courseId)
        Dim qid As Object = Nothing
        Using cmd As New MySqlCommand("SELECT Id FROM TrainingQuizzes WHERE CourseId = @Cid AND IsActive = true LIMIT 1")
            cmd.Parameters.AddWithValue("@Cid", cid)
            qid = DbHelper.ExecuteScalar(cmd)
        End Using
        If qid Is Nothing Then Return False
        Using cmd As New MySqlCommand("SELECT COUNT(1) FROM QuizAttempts WHERE QuizId = @Qid AND UserId = @Uid AND Passed = true")
            cmd.Parameters.AddWithValue("@Qid", CInt(qid))
            cmd.Parameters.AddWithValue("@Uid", CurrentUserId)
            Return CInt(DbHelper.ExecuteScalar(cmd)) > 0
        End Using
    End Function

    Protected Sub CourseCommand(sender As Object, e As CommandEventArgs)
        If e.CommandName = "TakeQuiz" Then
            _activeCourseId = CInt(e.CommandArgument)
            StartQuiz()
        End If
    End Sub

    Private Sub StartQuiz()
        Dim cid As Integer = _activeCourseId
        Dim qid As Object = Nothing
        Using cmd As New MySqlCommand("SELECT Id FROM TrainingQuizzes WHERE CourseId = @Cid AND IsActive = true LIMIT 1")
            cmd.Parameters.AddWithValue("@Cid", cid)
            qid = DbHelper.ExecuteScalar(cmd)
        End Using
        If qid Is Nothing Then Return
        _activeQuizId = CInt(qid)
        ViewState("QuizMode") = True
        ViewState("ActiveQuizId") = _activeQuizId
        ViewState("ActiveCourseId") = cid

        Dim attempts As Integer = 0
        Using cmd As New MySqlCommand("SELECT COUNT(1) FROM QuizAttempts WHERE QuizId = @Qid AND UserId = @Uid")
            cmd.Parameters.AddWithValue("@Qid", _activeQuizId)
            cmd.Parameters.AddWithValue("@Uid", CurrentUserId)
            attempts = CInt(DbHelper.ExecuteScalar(cmd))
        End Using
        If attempts >= 2 Then
            litMessage.Text = "<div class='alert alert-danger py-1 small'>You have used all 2 attempts for this quiz.</div>"
            ViewState("QuizMode") = False
            Return
        End If
        ViewState("AttemptNumber") = attempts + 1

        pnlLMS.Visible = False
        pnlQuiz.Visible = True
        LoadQuiz()
    End Sub

    Private Sub LoadQuiz()
        Using cmd As New MySqlCommand("SELECT q.QuizName, c.CourseName FROM TrainingQuizzes q JOIN TrainingCourses c ON q.CourseId = c.Id WHERE q.Id = @Qid")
            cmd.Parameters.AddWithValue("@Qid", ViewState("ActiveQuizId"))
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                litQuizTitle.Text = Server.HtmlEncode(dt.Rows(0)("CourseName").ToString()) & " - " & Server.HtmlEncode(dt.Rows(0)("QuizName").ToString())
            End If
        End Using
        litAttemptNum.Text = ViewState("AttemptNumber").ToString()
        Using cmd As New MySqlCommand("SELECT * FROM QuizQuestions WHERE QuizId = @Qid ORDER BY QuestionOrder")
            cmd.Parameters.AddWithValue("@Qid", ViewState("ActiveQuizId"))
            rptQuestions.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptQuestions.DataBind()
        End Using
        pnlQuizQuestions.Visible = True
        pnlQuizResults.Visible = False
        breadcrumbLabel.InnerText = "Taking Quiz"
    End Sub

    Protected Sub btnSubmitQuiz_Click(sender As Object, e As EventArgs)
        Dim qid As Integer = CInt(ViewState("ActiveQuizId"))
        Dim questions As DataTable
        Using cmd As New MySqlCommand("SELECT * FROM QuizQuestions WHERE QuizId = @Qid ORDER BY QuestionOrder")
            cmd.Parameters.AddWithValue("@Qid", qid)
            questions = DbHelper.ExecuteDataTable(cmd)
        End Using
        Dim total As Integer = questions.Rows.Count
        Dim correct As Integer = 0
        Dim results As New DataTable()
        results.Columns.Add("QuestionText", GetType(String))
        results.Columns.Add("OptionA", GetType(String))
        results.Columns.Add("OptionB", GetType(String))
        results.Columns.Add("OptionC", GetType(String))
        results.Columns.Add("OptionD", GetType(String))
        results.Columns.Add("CorrectAnswer", GetType(String))
        results.Columns.Add("UserAnswer", GetType(String))
        results.Columns.Add("IsCorrect", GetType(Boolean))

        For Each row In questions.Rows
            Dim questionId As Integer = CInt(row("Id"))
            Dim userAnswer As String = Request.Form("q_" & questionId)
            Dim isCorrect As Boolean = (userAnswer IsNot Nothing AndAlso userAnswer = row("CorrectAnswer").ToString())
            If isCorrect Then correct += 1
            Dim dr = results.NewRow()
            dr("QuestionText") = row("QuestionText")
            dr("OptionA") = row("OptionA")
            dr("OptionB") = row("OptionB")
            dr("OptionC") = row("OptionC")
            dr("OptionD") = row("OptionD")
            dr("CorrectAnswer") = row("CorrectAnswer")
            dr("UserAnswer") = If(userAnswer IsNot Nothing, userAnswer, "")
            dr("IsCorrect") = isCorrect
            results.Rows.Add(dr)
        Next

        Dim score As Double = Math.Round(CDbl(correct) / CDbl(total) * 100, 1)
        Dim passed As Boolean = score >= 85.0
        Dim attemptNum As Integer = CInt(ViewState("AttemptNumber"))

        Using cmd As New MySqlCommand("INSERT INTO QuizAttempts (QuizId, UserId, TenantId, AttemptNumber, Score, TotalQuestions, CorrectAnswers, CompletedOn, Passed) VALUES (@Qid, @Uid, @Tid, @Attempt, @Score, @Total, @Correct, CURRENT_TIMESTAMP, @Passed)")
            cmd.Parameters.AddWithValue("@Qid", qid)
            cmd.Parameters.AddWithValue("@Uid", CurrentUserId)
            cmd.Parameters.AddWithValue("@Tid", CurrentTenantId)
            cmd.Parameters.AddWithValue("@Attempt", attemptNum)
            cmd.Parameters.AddWithValue("@Score", score)
            cmd.Parameters.AddWithValue("@Total", total)
            cmd.Parameters.AddWithValue("@Correct", correct)
            cmd.Parameters.AddWithValue("@Passed", passed)
            DbHelper.ExecuteNonQuery(cmd)
        End Using

        If passed Then
            AutoIssueCertification(CInt(ViewState("ActiveCourseId")))
            litResultText.Text = "Congratulations! You passed the quiz!"
            pnlResultsReview.Visible = True
        Else
            Dim remaining As Integer = 2 - attemptNum
            If remaining > 0 Then
                litResultText.Text = "You scored " & score & "%. You need 85% to pass. You have " & remaining & " attempt(s) remaining."
                btnRetryQuiz.Visible = True
            Else
                litResultText.Text = "You scored " & score & "%. You have used all 2 attempts. Please contact an administrator."
                btnRetryQuiz.Visible = False
            End If
        End If

        litResultScore.Text = score & "%"
        litCorrectCount.Text = correct.ToString()
        litTotalCount.Text = total.ToString()
        ViewState("QuizPassed") = passed

        pnlQuizQuestions.Visible = False
        pnlQuizResults.Visible = True
        rptResults.DataSource = results
        rptResults.DataBind()
        LogAudit("QuizSubmit", "TrainingQuiz", qid, "Score: " & score & "% (" & correct & "/" & total & ") Attempt " & attemptNum)
    End Sub

    Private Sub AutoIssueCertification(courseId As Integer)
        Dim courseName As String = ""
        Dim beltLevel As String = ""
        Using cmd As New MySqlCommand("SELECT CourseName, BeltLevel FROM TrainingCourses WHERE Id = @Id")
            cmd.Parameters.AddWithValue("@Id", courseId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                courseName = dt.Rows(0)("CourseName").ToString()
                beltLevel = dt.Rows(0)("BeltLevel").ToString()
            End If
        End Using
        Using cmd As New MySqlCommand("SELECT COUNT(1) FROM TrainingCertifications WHERE TenantId = @Tid AND UserId = @Uid AND BeltLevel = @Belt AND IsActive = true")
            cmd.Parameters.AddWithValue("@Tid", CurrentTenantId)
            cmd.Parameters.AddWithValue("@Uid", CurrentUserId)
            cmd.Parameters.AddWithValue("@Belt", beltLevel)
            If CInt(DbHelper.ExecuteScalar(cmd)) > 0 Then Return
        End Using
        Dim certName As String = beltLevel & " Belt - " & courseName
        Using cmd As New MySqlCommand("INSERT INTO TrainingCertifications (TenantId, UserId, CertificationName, BeltLevel, IssueDate, IssuedBy, IsActive) VALUES (@Tid, @Uid, @Name, @Belt, CURRENT_DATE, @By, true)")
            cmd.Parameters.AddWithValue("@Tid", CurrentTenantId)
            cmd.Parameters.AddWithValue("@Uid", CurrentUserId)
            cmd.Parameters.AddWithValue("@Name", certName)
            cmd.Parameters.AddWithValue("@Belt", beltLevel)
            cmd.Parameters.AddWithValue("@By", "Quantaris LMS (Auto)")
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Certification", "TrainingCertification", Nothing, "Auto-issued " & beltLevel & " Belt certification for user " & CurrentUserId)
    End Sub

    Protected Sub btnRetryQuiz_Click(sender As Object, e As EventArgs)
        StartQuiz()
    End Sub

    Protected Sub btnBackToCourses_Click(sender As Object, e As EventArgs)
        ViewState("QuizMode") = False
        pnlQuiz.Visible = False
        pnlLMS.Visible = True
        breadcrumbLabel.InnerText = "Training LMS"
        LoadData()
    End Sub

    Protected Sub btnAddCourse_Click(sender As Object, e As EventArgs)
        Dim code = txtCourseCode.Text.Trim()
        Dim name = txtCourseName.Text.Trim()
        If code = "" OrElse name = "" Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter course code and name.</div>" : Return
        End If
        Dim duration As Double
        Double.TryParse(txtDuration.Text.Trim(), duration)
        Using cmd As New MySqlCommand("INSERT INTO TrainingCourses (TenantId, CourseCode, CourseName, BeltLevel, Description, DurationHours) VALUES (@TenantId, @Code, @Name, @Belt, @Desc, @Dur)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@Code", code)
            cmd.Parameters.AddWithValue("@Name", name)
            cmd.Parameters.AddWithValue("@Belt", ddlBelt.SelectedValue)
            cmd.Parameters.AddWithValue("@Desc", txtCourseDesc.Text.Trim())
            cmd.Parameters.AddWithValue("@Dur", duration)
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Create", "TrainingCourse", Nothing, "Added course: " & name)
        litMessage.Text = "<div class='alert alert-success py-1 small'>Course added.</div>"
        LoadData()
        LoadManageCourses()
    End Sub

    Protected Sub btnIssueCert_Click(sender As Object, e As EventArgs)
        If ddlUser.SelectedValue = "" Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Select a user.</div>" : Return
        End If
        Dim certName = txtCertName.Text.Trim()
        If certName = "" Then
            litMessage.Text = "<div class='alert alert-warning py-1 small'>Enter certification name.</div>" : Return
        End If
        Dim issueDate As DateTime
        If Not DateTime.TryParse(txtIssueDate.Text.Trim(), issueDate) Then issueDate = DateTime.Today
        Dim expiryDate As DateTime?
        Dim exp As DateTime
        If DateTime.TryParse(txtExpiry.Text.Trim(), exp) Then expiryDate = exp
        Using cmd As New MySqlCommand("INSERT INTO TrainingCertifications (TenantId, UserId, CertificationName, BeltLevel, IssueDate, ExpiryDate, IssuedBy) VALUES (@TenantId, @User, @Name, @Belt, @Issue, @Expiry, @By)")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@User", CInt(ddlUser.SelectedValue))
            cmd.Parameters.AddWithValue("@Name", certName)
            cmd.Parameters.AddWithValue("@Belt", ddlCertBelt.SelectedValue)
            cmd.Parameters.AddWithValue("@Issue", issueDate)
            cmd.Parameters.AddWithValue("@Expiry", If(expiryDate.HasValue, CObj(expiryDate.Value), DBNull.Value))
            cmd.Parameters.AddWithValue("@By", txtIssuedBy.Text.Trim())
            DbHelper.ExecuteNonQuery(cmd)
        End Using
        LogAudit("Create", "TrainingCertification", Nothing, "Issued cert: " & certName)
        litMessage.Text = "<div class='alert alert-success py-1 small'>Certification issued.</div>"
        LoadData()
    End Sub
End Class
