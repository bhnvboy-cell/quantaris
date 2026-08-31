Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Projects_Default
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            BindProjects()
        End If
    End Sub

    Private Sub BindProjects()
        Dim status As String = Nothing
        If ddlFilter.SelectedValue <> "" Then
            status = ddlFilter.SelectedValue
        End If
        Dim dt As DataTable = ProjectHelper.GetProjects(CurrentTenantId, status)
        rptProjects.DataSource = dt
        rptProjects.DataBind()
        lblEmpty.Visible = dt.Rows.Count = 0
    End Sub

    Protected Sub ddlFilter_SelectedIndexChanged(sender As Object, e As EventArgs)
        BindProjects()
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs)
        BindProjects()
    End Sub

    Protected Function GetStatusBadgeClass(status As Object) As String
        If status Is Nothing OrElse status Is DBNull.Value Then
            Return "ox-badge-info"
        End If
        Select Case status.ToString().ToLower()
            Case "inprogress" : Return "ox-badge-warning"
            Case "complete" : Return "ox-badge-success"
            Case "onhold" : Return "ox-badge-danger"
            Case Else : Return "ox-badge-info"
        End Select
    End Function

    Protected Function GetStatusIconClass(status As Object) As String
        If status Is Nothing OrElse status Is DBNull.Value Then Return "blue"
        Select Case status.ToString().ToLower()
            Case "inprogress" : Return "teal"
            Case "complete" : Return "purple"
            Case "onhold" : Return "orange"
            Case Else : Return "blue"
        End Select
    End Function

    Protected Function GetStatusIcon(status As Object) As String
        If status Is Nothing OrElse status Is DBNull.Value Then Return "bi-folder"
        Select Case status.ToString().ToLower()
            Case "inprogress" : Return "bi-arrow-repeat"
            Case "complete" : Return "bi-check-circle"
            Case "onhold" : Return "bi-pause-circle"
            Case Else : Return "bi-folder-plus"
        End Select
    End Function

    Protected Sub btnExportCsv_Click(sender As Object, e As EventArgs)
        Dim cmd As New MySqlCommand("SELECT ProjectCode, ProjectName, Description, Status, Phase, Priority, FinancialImpact, StartDate, TargetEndDate, Champion FROM QuantarisProjects WHERE TenantId = @TenantId ORDER BY CreatedOn DESC")
        cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
        Dim dt = DbHelper.ExecuteDataTable(cmd)
        ExportHelper.ExportToCsv(dt, "Projects")
    End Sub
End Class
