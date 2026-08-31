Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Text

Public Partial Class Tools_ProjectGantt
    Inherits BasePage

    Private _projectId As Integer
    Private Const PX_PER_DAY As Integer = 6

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt", "User")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlGantt.Visible = True
                pnlNoProject.Visible = False
                LoadGantt()
            Else
                pnlGantt.Visible = False
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

    Private Sub LoadGantt()
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then Exit Sub

        Dim dtTasks As DataTable
        Dim dtMilestones As DataTable

        Using cmd As New MySqlCommand(
            "SELECT * FROM GanttTasks WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY SortOrder, StartDate")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            dtTasks = DbHelper.ExecuteDataTable(cmd)
        End Using

        Using cmd As New MySqlCommand(
            "SELECT * FROM GanttMilestones WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY SortOrder, MilestoneDate")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", pid)
            dtMilestones = DbHelper.ExecuteDataTable(cmd)
        End Using

        litGantt.Text = RenderGanttHtml(dtTasks, dtMilestones)
        rptTasks.DataSource = dtTasks
        rptTasks.DataBind()
        litTaskCount.Text = dtTasks.Rows.Count & " tasks"
    End Sub

    Private Function RenderGanttHtml(tasks As DataTable, milestones As DataTable) As String
        Dim sb As New StringBuilder()
        Dim today = DateTime.Today

        ' Determine date range
        Dim minDate As Date = today.AddDays(-14)
        Dim maxDate As Date = today.AddDays(60)

        For Each r As DataRow In tasks.Rows
            Dim sd = CDate(r("StartDate"))
            Dim ed = CDate(r("EndDate"))
            If sd < minDate Then minDate = sd
            If ed > maxDate Then maxDate = ed
        Next

        For Each r As DataRow In milestones.Rows
            Dim md = CDate(r("MilestoneDate"))
            If md < minDate Then minDate = md
            If md > maxDate Then maxDate = md
        Next

        ' Add padding
        minDate = minDate.AddDays(-7)
        maxDate = maxDate.AddDays(14)

        Dim totalDays = (maxDate - minDate).Days
        Dim totalWidth = totalDays * PX_PER_DAY

        ' Group tasks by phase
        Dim phases = {"Define", "Measure", "Analyze", "Improve", "Control"}
        Dim phaseColors = {"#0d6efd", "#198754", "#ffc107", "#fd7e14", "#6f42c1"}

        sb.Append("<div class='gantt-tasklist'>")
        sb.Append("<div class='gantt-tasklist-header'><span>Task</span><span>Dates</span></div>")

        ' Phase headers + task rows (left panel)
        Dim timelineRows As New StringBuilder()
        timelineRows.Append("<div style='min-width:" & totalWidth & "px;position:relative;'>")

        ' Week headers
        timelineRows.Append("<div class='gantt-header' style='min-width:" & totalWidth & "px;'>")
        Dim weekStart = minDate
        While weekStart <= maxDate
            Dim weekEnd = weekStart.AddDays(6)
            Dim wStart = CInt(Math.Max(0.0, (weekStart - minDate).TotalDays))
            Dim rEndVal = (weekEnd - minDate).TotalDays
            Dim tDaysDbl = CDbl(totalDays)
            Dim wEndVal = CInt(IIf(rEndVal < tDaysDbl, rEndVal, tDaysDbl))
            Dim wWidth = (wEndVal - wStart) * PX_PER_DAY
            If wWidth < 40 Then wWidth = 40
            Dim isWeekend = (weekStart.DayOfWeek = DayOfWeek.Saturday OrElse weekStart.DayOfWeek = DayOfWeek.Sunday)
            timelineRows.Append("<div class='gantt-week" & If(isWeekend, " weekend", "") & "' style='width:" & wWidth & "px;'>")
            timelineRows.Append(weekStart.ToString("MMM dd"))
            timelineRows.Append("</div>")
            weekStart = weekStart.AddDays(7)
        End While
        timelineRows.Append("</div>")

        ' Today line position
        Dim todayOffset = CInt((today - minDate).TotalDays) * PX_PER_DAY

        Dim rowIndex = 0

        For Each phase In phases
            Dim phaseTasks = tasks.Select("Phase = '" & phase & "'")
            If phaseTasks.Length = 0 Then Continue For

            ' Phase header row
            Dim colorIndex = Array.IndexOf(phases, phase)
            Dim phaseColor = phaseColors(colorIndex)

            sb.Append("<div class='gantt-phase-header' style='border-left:3px solid " & phaseColor & ";'>")
            sb.Append("<span class='phase-text-" & phase & "'>")
            sb.Append("<i class='bi bi-circle-fill me-1' style='font-size:8px;color:" & phaseColor & ";'></i>")
            sb.Append(phase & " (" & phaseTasks.Length & " tasks)")
            sb.Append("</span></div>")

            For Each task As DataRow In phaseTasks
                Dim taskName = task("TaskName").ToString()
                Dim sd = CDate(task("StartDate"))
                Dim ed = CDate(task("EndDate"))
                Dim progress = CInt(task("Progress"))
                Dim status = task("Status").ToString()
                Dim duration = CInt(task("Duration"))

                Dim leftOffset = CInt(Math.Max(0.0, (sd - minDate).TotalDays)) * PX_PER_DAY
                Dim barWidth = CInt(Math.Max(4.0, (ed - sd).TotalDays)) * PX_PER_DAY

                sb.Append("<div class='gantt-row gantt-row-header'>")
                sb.Append("<div style='flex:1;overflow:hidden;'>")
                sb.Append("<div class='task-name' title='" & HttpUtility.HtmlEncode(taskName) & "'>" & HttpUtility.HtmlEncode(taskName) & "</div>")
                sb.Append("<div class='task-dates'>" & sd.ToString("MMM dd") & " - " & ed.ToString("MMM dd") & " (" & duration & "d)</div>")
                sb.Append("</div>")
                sb.Append("<span class='badge bg-" & If(progress >= 100, "success", If(progress > 0, "primary", "secondary")) & " rounded-pill' style='font-size:9px;'>" & progress & "%</span>")
                sb.Append("</div>")

                ' Timeline row
                timelineRows.Append("<div class='gantt-row'>")
                timelineRows.Append("<div class='gantt-row-body' style='min-height:36px;'>")
                timelineRows.Append("<div class='gantt-bar phase-" & phase & "' style='left:" & leftOffset & "px;width:" & barWidth & "px;' title='" & HttpUtility.HtmlEncode(taskName) & " (" & progress & "%)'>")
                If barWidth > 40 Then
                    timelineRows.Append("<span class='bar-label'>" & HttpUtility.HtmlEncode(taskName) & "</span>")
                End If
                If progress > 0 Then
                    Dim progWidth = CInt((progress / 100) * barWidth)
                    timelineRows.Append("<div class='gantt-progress' style='width:" & progWidth & "px;background:rgba(255,255,255,0.4);'></div>")
                End If
                timelineRows.Append("</div>")

                ' Today line
                If todayOffset > 0 AndAlso todayOffset < totalWidth Then
                    timelineRows.Append("<div class='gantt-today' style='left:" & todayOffset & "px;'></div>")
                End If

                timelineRows.Append("</div></div>")

                rowIndex += 1
            Next
        Next

        ' Milestones
        If milestones.Rows.Count > 0 Then
            sb.Append("<div class='gantt-phase-header' style='border-left:3px solid #6f42c1;'>")
            sb.Append("<span style='color:#6f42c1;'><i class='bi bi-star me-1'></i>Milestones</span></div>")

            For Each m As DataRow In milestones.Rows
                Dim mName = m("MilestoneName").ToString()
                Dim mDate = CDate(m("MilestoneDate"))
                Dim mPhase = m("Phase").ToString()
                Dim isDone = CBool(m("IsCompleted"))

                sb.Append("<div class='gantt-row gantt-row-header gantt-row-milestone'>")
                sb.Append("<div style='flex:1;overflow:hidden;'>")
                sb.Append("<div class='task-name'><i class='bi bi-star" & If(isDone, "-fill", "") & " me-1' style='color:#6f42c1;'></i>" & HttpUtility.HtmlEncode(mName) & "</div>")
                sb.Append("<div class='task-dates'>" & mDate.ToString("MMM dd, yyyy") & If(mPhase <> "General", " Â· " & mPhase, "") & "</div>")
                sb.Append("</div>")
                sb.Append("<span class='badge " & If(isDone, "bg-success", "bg-outline-secondary") & "' style='font-size:9px;'>" & If(isDone, "Done", "Upcoming") & "</span>")
                sb.Append("</div>")

                ' Milestone on timeline
                Dim mLeft = CInt(Math.Max(0, (mDate - minDate).TotalDays)) * PX_PER_DAY
                timelineRows.Append("<div class='gantt-row'>")
                timelineRows.Append("<div class='gantt-row-body' style='min-height:36px;'>")
                Dim mColor = "#6f42c1"
                timelineRows.Append("<div class='gantt-milestone' style='left:" & (mLeft - 6) & "px;background:" & mColor & ";border-color:" & mColor & ";' title='" & HttpUtility.HtmlEncode(mName) & " (" & mDate.ToString("MMM dd, yyyy") & ")'></div>")

                If todayOffset > 0 AndAlso todayOffset < totalWidth Then
                    timelineRows.Append("<div class='gantt-today' style='left:" & todayOffset & "px;'></div>")
                End If
                timelineRows.Append("</div></div>")
            Next
        End If

        timelineRows.Append("</div>")

        sb.Append("</div>") ' end tasklist
        sb.Append("<div class='gantt-timeline'>")
        sb.Append(timelineRows.ToString())
        sb.Append("</div>")

        Return sb.ToString()
    End Function

    Protected Function GetPriorityBadge(priority As String) As String
        Select Case priority.ToLower()
            Case "critical" : Return "bg-danger"
            Case "high" : Return "bg-warning text-dark"
            Case "medium" : Return "bg-info text-dark"
            Case "low" : Return "bg-secondary"
            Case Else : Return "bg-secondary"
        End Select
    End Function

    Protected Function GetStatusBadge(status As String) As String
        Select Case status.ToLower()
            Case "complete" : Return "bg-success"
            Case "in progress" : Return "bg-primary"
            Case "on hold" : Return "bg-warning text-dark"
            Case "not started" : Return "bg-secondary"
            Case Else : Return "bg-secondary"
        End Select
    End Function

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlGantt.Visible = True
            pnlNoProject.Visible = False
            LoadGantt()
        Else
            pnlGantt.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        if ddlProject.SelectedValue <> "" Then LoadGantt()
    End Sub

    Protected Sub btnSaveTask_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then
            litTaskMessage.Text = "<div class='alert alert-warning py-1 small'>No project selected.</div>"
            Return
        End If
        Try
            Dim editId = CType(ViewState("EditTaskId"), Integer?)
            Dim taskName = txtTaskName.Text.Trim()
            If taskName = "" Then
                litTaskMessage.Text = "<div class='alert alert-warning py-1 small'>Task name is required.</div>"
                Return
            End If

            Dim startDate As Date
            Dim endDate As Date
            If Not Date.TryParse(txtStartDate.Text.Trim(), startDate) Then startDate = DateTime.Today
            If Not Date.TryParse(txtEndDate.Text.Trim(), endDate) Then endDate = startDate.AddDays(7)
            If endDate < startDate Then endDate = startDate.AddDays(1)

            Dim progress = 0
            Integer.TryParse(txtProgress.Text.Trim(), progress)
            If progress < 0 Then progress = 0
            If progress > 100 Then progress = 100

            Dim depId? As Integer = Nothing
            Dim depParsed As Integer
            If Integer.TryParse(txtDependency.Text.Trim(), depParsed) Then depId = depParsed

            If editId.HasValue Then
                Using cmd As New MySqlCommand(
                    "UPDATE GanttTasks SET TaskName=@Name, Description=@Desc, Phase=@Phase, " &
                    "StartDate=@Start, EndDate=@End, Progress=@Progress, Priority=@Priority, " &
                    "Status=@Status, DependencyId=@DepId WHERE Id=@Id AND TenantId=@TenantId")
                    cmd.Parameters.AddWithValue("@Id", editId.Value)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@Name", taskName)
                    cmd.Parameters.AddWithValue("@Desc", IIf(txtTaskDesc.Text.Trim() = "", DBNull.Value, CObj(txtTaskDesc.Text.Trim())))
                    cmd.Parameters.AddWithValue("@Phase", ddlPhase.SelectedValue)
                    cmd.Parameters.AddWithValue("@Start", startDate)
                    cmd.Parameters.AddWithValue("@End", endDate)
                    cmd.Parameters.AddWithValue("@Progress", progress)
                    cmd.Parameters.AddWithValue("@Priority", ddlPriority.SelectedValue)
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue)
                    cmd.Parameters.AddWithValue("@DepId", If(depId.HasValue, CObj(depId.Value), DBNull.Value))
                    DbHelper.ExecuteNonQuery(cmd)
                    LogAudit("Update", "GanttTask", editId.Value, "Updated task: " & taskName)
                End Using
            Else
                ' Get next sort order
                Dim nextSort = 0
                Using cmd As New MySqlCommand(
                    "SELECT COALESCE(MAX(SortOrder), -1) + 1 FROM GanttTasks WHERE TenantId=@TenantId AND ProjectId=@ProjectId")
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", pid)
                    Dim result = DbHelper.ExecuteScalar(cmd)
                    If result IsNot Nothing Then Integer.TryParse(result.ToString(), nextSort)
                End Using

                Using cmd As New MySqlCommand(
                    "INSERT INTO GanttTasks (TenantId, ProjectId, TaskName, Description, Phase, StartDate, EndDate, " &
                    "Progress, Priority, Status, DependencyId, SortOrder, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @Name, @Desc, @Phase, @Start, @End, " &
                    "@Progress, @Priority, @Status, @DepId, @SortOrder, @UserId)")
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", pid)
                    cmd.Parameters.AddWithValue("@Name", taskName)
                    cmd.Parameters.AddWithValue("@Desc", IIf(txtTaskDesc.Text.Trim() = "", DBNull.Value, CObj(txtTaskDesc.Text.Trim())))
                    cmd.Parameters.AddWithValue("@Phase", ddlPhase.SelectedValue)
                    cmd.Parameters.AddWithValue("@Start", startDate)
                    cmd.Parameters.AddWithValue("@End", endDate)
                    cmd.Parameters.AddWithValue("@Progress", progress)
                    cmd.Parameters.AddWithValue("@Priority", ddlPriority.SelectedValue)
                    cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue)
                    cmd.Parameters.AddWithValue("@DepId", If(depId.HasValue, CObj(depId.Value), DBNull.Value))
                    cmd.Parameters.AddWithValue("@SortOrder", nextSort)
                    cmd.Parameters.AddWithValue("@UserId", CurrentUserId)
                    DbHelper.ExecuteNonQuery(cmd)
                    LogAudit("Create", "GanttTask", Nothing, "Created task: " & taskName)
                End Using
            End If

            litTaskMessage.Text = "<div class='alert alert-success py-1 small'>Task saved.</div>"
            ClearTaskForm()
            LoadGantt()

        Catch ex As Exception
            litTaskMessage.Text = "<div class='alert alert-danger py-1 small'>" & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnSaveMilestone_Click(sender As Object, e As EventArgs)
        Dim pid = GetSelectedProjectId()
        If pid = 0 Then
            litMilestoneMessage.Text = "<div class='alert alert-warning py-1 small'>No project selected.</div>"
            Return
        End If
        Try
            Dim mName = txtMilestoneName.Text.Trim()
            If mName = "" Then
                litMilestoneMessage.Text = "<div class='alert alert-warning py-1 small'>Milestone name is required.</div>"
                Return
            End If
            Dim mDate As Date
            If Not Date.TryParse(txtMilestoneDate.Text.Trim(), mDate) Then mDate = DateTime.Today

            Using cmd As New MySqlCommand(
                "INSERT INTO GanttMilestones (TenantId, ProjectId, MilestoneName, MilestoneDate, Phase, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @Name, @Date, @Phase, @UserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", pid)
                cmd.Parameters.AddWithValue("@Name", mName)
                cmd.Parameters.AddWithValue("@Date", mDate)
                cmd.Parameters.AddWithValue("@Phase", ddlMilestonePhase.SelectedValue)
                cmd.Parameters.AddWithValue("@UserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
                LogAudit("Create", "GanttMilestone", Nothing, "Created milestone: " & mName)
            End Using

            litMilestoneMessage.Text = "<div class='alert alert-success py-1 small'>Milestone saved.</div>"
            txtMilestoneName.Text = ""
            txtMilestoneDate.Text = ""
            LoadGantt()

        Catch ex As Exception
            litMilestoneMessage.Text = "<div class='alert alert-danger py-1 small'>" & ex.Message & "</div>"
        End Try
    End Sub

    Private Sub ClearTaskForm()
        txtTaskName.Text = ""
        txtTaskDesc.Text = ""
        txtStartDate.Text = ""
        txtEndDate.Text = ""
        txtProgress.Text = "0"
        txtDependency.Text = ""
        ddlPhase.SelectedIndex = 0
        ddlPriority.SelectedIndex = 1
        ddlStatus.SelectedIndex = 0
        hfEditTaskId.Value = ""
        ViewState("EditTaskId") = Nothing
        litModalTitle.Text = "Add Task"
    End Sub

    Protected Sub rptTasks_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id = CInt(e.CommandArgument)
        If e.CommandName = "EditTask" Then
            Using cmd As New MySqlCommand(
                "SELECT * FROM GanttTasks WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    Dim r = dt.Rows(0)
                    txtTaskName.Text = r("TaskName").ToString()
                    txtTaskDesc.Text = If(r("Description") IsNot DBNull.Value, r("Description").ToString(), "")
                    txtStartDate.Text = CDate(r("StartDate")).ToString("yyyy-MM-dd")
                    txtEndDate.Text = CDate(r("EndDate")).ToString("yyyy-MM-dd")
                    txtProgress.Text = r("Progress").ToString()
                    ddlPhase.SelectedValue = r("Phase").ToString()
                    ddlPriority.SelectedValue = r("Priority").ToString()
                    ddlStatus.SelectedValue = r("Status").ToString()
                    txtDependency.Text = If(r("DependencyId") IsNot DBNull.Value, r("DependencyId").ToString(), "")
                    hfEditTaskId.Value = id.ToString()
                    ViewState("EditTaskId") = id
                    litModalTitle.Text = "Edit Task"
                    litTaskMessage.Text = ""
                    ScriptManager.RegisterStartupScript(Me, Me.GetType(), "showModal", _
                        "var modal = new bootstrap.Modal(document.getElementById('taskModal'));" & _
                        "modal.show();", True)
                End If
            End Using
        ElseIf e.CommandName = "DeleteTask" Then
            Using cmd As New MySqlCommand(
                "DELETE FROM GanttTasks WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
                LogAudit("Delete", "GanttTask", id, "Deleted task")
                LoadGantt()
            End Using
        End If
    End Sub
End Class
