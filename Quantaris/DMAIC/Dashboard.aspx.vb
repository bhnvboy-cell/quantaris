Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Dashboard
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)

        If Not IsPostBack Then
            LoadProjectInfo()
            LoadProgress()
        End If
    End Sub

    Private Sub LoadProjectInfo()
        Dim cmd As New MySqlCommand(
            "SELECT ProjectName, Status FROM QuantarisProjects WHERE Id = @ProjectId AND TenantId = @TenantId")
        cmd.Parameters.AddWithValue("@ProjectId", _projectId)
        cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
        Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
        If dt.Rows.Count > 0 Then
            Dim projectName = dt.Rows(0)("ProjectName").ToString()
            litProjectName.Text = projectName
            litBreadcrumbProject.Text = projectName
            litProjectStatus.Text = dt.Rows(0)("Status").ToString()
        End If
    End Sub

    Private Sub LoadProgress()
        Dim dt As DataTable = DMAICHelper.GetDMAICProgressData(_projectId)
        Dim dv As DataView = New DataView(dt)

        dv.RowFilter = "Phase = 'Define'"
        rptDefine.DataSource = dv
        rptDefine.DataBind()

        dv.RowFilter = "Phase = 'Measure'"
        rptMeasure.DataSource = dv
        rptMeasure.DataBind()

        dv.RowFilter = "Phase = 'Analyze'"
        rptAnalyze.DataSource = dv
        rptAnalyze.DataBind()

        dv.RowFilter = "Phase = 'Improve'"
        rptImprove.DataSource = dv
        rptImprove.DataBind()

        dv.RowFilter = "Phase = 'Control'"
        rptControl.DataSource = dv
        rptControl.DataBind()

        Dim totalSteps = dt.Rows.Count
        Dim completedSteps = 0
        For Each row As DataRow In dt.Rows
            If row("Status").ToString().ToLower() = "complete" Then
                completedSteps += 1
            End If
        Next

        Dim pct As Integer = If(totalSteps > 0, CInt((completedSteps / totalSteps) * 100), 0)
        progressBar.Style("width") = pct & "%"
        progressBar.Attributes("aria-valuenow") = pct
        litProgress.Text = completedSteps & " / " & totalSteps & " steps (" & pct & "%)"
    End Sub

    Protected Function GetStepLink(stepNumber As Integer) As String
        Dim base As String = "~/DMAIC/"
        Select Case stepNumber
            Case 1 : Return base & "Define/Define_VOC.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 2 : Return base & "Define/Define_Charter.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 3 : Return base & "Define/Define_SIPOC.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 4 : Return base & "Measure/Measure_WhatToMeasure.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 5 : Return base & "Measure/Measure_PlanCollectData.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 6 : Return base & "Measure/Measure_Baseline.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 7 : Return base & "Analyze/Analyze_PerformanceGaps.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 8 : Return base & "Analyze/Analyze_RootCausePrioritization.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 9 : Return base & "Analyze/Analyze_StatValidation.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 10 : Return base & "Improve/Improve_SolutionGeneration.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 11 : Return base & "Improve/Improve_PilotPlan.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 12 : Return base & "Improve/Improve_ImpactValidation.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 13 : Return base & "Control/Control_Institutionalize.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 14 : Return base & "Control/Control_BestPractices.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case 15 : Return base & "Control/Control_Celebrate.aspx?projectId=" & _projectId & "&tenant=" & CurrentTenantCode
            Case Else : Return "#"
        End Select
    End Function
End Class
