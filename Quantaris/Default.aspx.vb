Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class _Default
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            LoadDashboard()
        End If
    End Sub

    Private Sub LoadDashboard()
        Dim sql As String = ""
        sql = sql & "SELECT COUNT(*) AS TotalProjects, "
        sql = sql & "  SUM(CASE WHEN Status = 'NotStarted' THEN 1 ELSE 0 END) AS NotStarted, "
        sql = sql & "  SUM(CASE WHEN Status = 'InProgress' THEN 1 ELSE 0 END) AS InProgress, "
        sql = sql & "  SUM(CASE WHEN Status = 'Complete' THEN 1 ELSE 0 END) AS Complete, "
        sql = sql & "  SUM(CASE WHEN Status = 'OnHold' THEN 1 ELSE 0 END) AS OnHold, "
        sql = sql & "  COALESCE(SUM(StepsCompleted), 0) AS TotalStepsCompleted, "
        sql = sql & "  COALESCE(SUM(TotalSteps), 0) AS TotalStepsAll "
        sql = sql & "FROM ( "
        sql = sql & "  SELECT p.Status, "
        sql = sql & "    (SELECT COUNT(*) FROM DMAICSteps WHERE ProjectId = p.Id AND Status = 'Complete') AS StepsCompleted, "
        sql = sql & "    15 AS TotalSteps "
        sql = sql & "  FROM QuantarisProjects p "
        sql = sql & "  WHERE p.TenantId = @TenantId "
        sql = sql & ") sub"

        Using cmd As New MySqlCommand(sql)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim r = dt.Rows(0)
                litTotalProjects.Text = r("TotalProjects").ToString()

                Dim n = CInt(r("NotStarted"))
                Dim ip = CInt(r("InProgress"))
                Dim c = CInt(r("Complete"))
                Dim oh = CInt(r("OnHold"))

                litNotStarted.Text = n
                litInProgress.Text = ip
                litComplete.Text = c

                chartN = n
                chartIP = ip
                chartC = c
                chartOH = oh

                Dim totalSteps = CInt(r("TotalStepsAll"))
                Dim doneSteps = CInt(r("TotalStepsCompleted"))
                Dim pct As Integer = If(totalSteps > 0, CInt((doneSteps / totalSteps) * 100), 0)
                progressBar.Style("width") = pct & "%"
                progressBar.Attributes("aria-valuenow") = pct
                litProgress.Text = doneSteps & " / " & totalSteps & " steps (" & pct & "%)"
            End If
        End Using

        sql = ""
        sql = sql & "SELECT p.Id, p.ProjectCode, p.ProjectName, p.Status, p.Phase, p.CreatedOn, "
        sql = sql & "  COALESCE(NULLIF(CONCAT_WS(' ', u.FirstName, u.LastName), ''), 'Unassigned') AS OwnerName, "
        sql = sql & "  (SELECT COUNT(*) FROM DMAICSteps WHERE ProjectId = p.Id AND Status = 'Complete') AS StepsCompleted, "
        sql = sql & "  COALESCE((SELECT ROUND(COUNT(*) * 100 / 15) FROM DMAICSteps WHERE ProjectId = p.Id AND Status = 'Complete'), 0) AS StepsPct "
        sql = sql & "FROM QuantarisProjects p "
        sql = sql & "LEFT JOIN Users u ON p.ProjectOwnerId = u.Id "
        sql = sql & "WHERE p.TenantId = @TenantId "
        sql = sql & "ORDER BY p.CreatedOn DESC "

        Using cmd As New MySqlCommand(sql)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptRecentProjects.DataSource = dt
            rptRecentProjects.DataBind()
            If dt.Rows.Count = 0 Then
                emptyRow.Visible = True
            End If
            chartProjects = dt
        End Using
    End Sub

    Private chartN As Integer = 0
    Private chartIP As Integer = 0
    Private chartC As Integer = 0
    Private chartOH As Integer = 0
    Private chartProjects As DataTable = Nothing

    Protected Function GetNotStarted() As Integer
        Return chartN
    End Function

    Protected Function GetInProgress() As Integer
        Return chartIP
    End Function

    Protected Function GetComplete() As Integer
        Return chartC
    End Function

    Protected Function GetOnHold() As Integer
        Return chartOH
    End Function

    Protected Function GetProjectNames() As String
        If chartProjects Is Nothing OrElse chartProjects.Rows.Count = 0 Then Return ""
        Dim sb As New System.Text.StringBuilder()
        For Each row As DataRow In chartProjects.Rows
            If sb.Length > 0 Then sb.Append(",")
            sb.Append("'").Append(row("ProjectCode").ToString().Replace("'", "")).Append("'")
        Next
        Return sb.ToString()
    End Function

    Protected Function GetProjectPcts() As String
        If chartProjects Is Nothing OrElse chartProjects.Rows.Count = 0 Then Return ""
        Dim sb As New System.Text.StringBuilder()
        For Each row As DataRow In chartProjects.Rows
            If sb.Length > 0 Then sb.Append(",")
            sb.Append(row("StepsPct").ToString())
        Next
        Return sb.ToString()
    End Function

    Protected Function GetPhaseAvg(phase As String) As Integer
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Dim sql = "SELECT ROUND(AVG(CASE WHEN Status = 'Complete' THEN 100 ELSE 0 END)) AS AvgPct " &
                      "FROM DMAICSteps WHERE TenantId = @TenantId AND Phase = @Phase"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@Phase", phase)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso result IsNot DBNull.Value Then Return CInt(result)
            End Using
        End Using
        Return 0
    End Function

    Protected Function GetStatusBadgeClass(status As Object) As String
        Dim s = status.ToString().ToLower()
        Select Case s
            Case "notstarted" : Return "ox-badge-info"
            Case "inprogress" : Return "ox-badge-success"
            Case "complete" : Return "ox-badge-success"
            Case "onhold" : Return "ox-badge-warning"
            Case Else : Return "ox-badge-info"
        End Select
    End Function

    Protected Function GetStatusIconClass(status As Object) As String
        Dim s = status.ToString().ToLower()
        Select Case s
            Case "notstarted" : Return "blue"
            Case "inprogress" : Return "teal"
            Case "complete" : Return "purple"
            Case "onhold" : Return "orange"
            Case Else : Return "blue"
        End Select
    End Function

    Protected Function GetStatusIcon(status As Object) As String
        Dim s = status.ToString().ToLower()
        Select Case s
            Case "notstarted" : Return "bi-folder-plus"
            Case "inprogress" : Return "bi-arrow-repeat"
            Case "complete" : Return "bi-check-circle"
            Case "onhold" : Return "bi-pause-circle"
            Case Else : Return "bi-folder"
        End Select
    End Function
End Class