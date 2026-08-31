Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Tools_FMEA
    Inherits BasePage

    Private _selectedFMEAId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin", "BlackBelt", "GreenBelt")
        If Not IsPostBack Then
            LoadProjects()
            If ddlProject.SelectedValue <> "" Then
                pnlFMEA.Visible = True
                pnlNoProject.Visible = False
                LoadFMEARecords()
            Else
                pnlFMEA.Visible = False
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
        If ddlProject.SelectedValue <> "" Then
            Integer.TryParse(ddlProject.SelectedValue, pid)
        End If
        Return pid
    End Function

    Private Sub LoadFMEARecords()
        Dim projectId = GetSelectedProjectId()
        If projectId = 0 Then Exit Sub

        Dim sql As String = "SELECT * FROM FMEA_Records WHERE TenantId = @TenantId AND ProjectId = @ProjectId"
        If txtFilterStep.Text.Trim() <> "" Then
            sql &= " AND ProcessStep LIKE @Filter"
        End If
        sql &= " ORDER BY RPN DESC"

        Using cmd As New MySqlCommand(sql)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", projectId)
            If txtFilterStep.Text.Trim() <> "" Then
                cmd.Parameters.AddWithValue("@Filter", "%" & txtFilterStep.Text.Trim() & "%")
            End If
            rptFMEARecords.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptFMEARecords.DataBind()
        End Using

        LoadFMEADropdown()
    End Sub

    Private Sub LoadFMEADropdown()
        Dim projectId = GetSelectedProjectId()
        ddlFMEARecord.Items.Clear()
        ddlFMEARecord.Items.Add(New ListItem("-- Select Record --", ""))
        Using cmd As New MySqlCommand(
            "SELECT Id, ProcessStep, FailureMode FROM FMEA_Records WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY ProcessStep")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", projectId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            For Each r As DataRow In dt.Rows
                ddlFMEARecord.Items.Add(New ListItem(
                    r("ProcessStep").ToString() & " - " & r("FailureMode").ToString(),
                    r("Id").ToString()))
            Next
        End Using
    End Sub

    Private Sub ClearEntryForm()
        txtProcessStep.Text = ""
        txtFailureMode.Text = ""
        txtEffect.Text = ""
        txtCause.Text = ""
        txtCurrentControl.Text = ""
        ddlSeverity.SelectedIndex = 0
        ddlOccurrence.SelectedIndex = 0
        ddlDetection.SelectedIndex = 0
        txtRecommendedAction.Text = ""
        txtRPN.Text = "1"
        _selectedFMEAId = 0
        ViewState("EditFMEAId") = Nothing
    End Sub

    Protected Sub ddlProject_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlProject.SelectedValue <> "" Then
            pnlFMEA.Visible = True
            pnlNoProject.Visible = False
            ClearEntryForm()
            pnlAction.Visible = False
            LoadFMEARecords()
        Else
            pnlFMEA.Visible = False
            pnlNoProject.Visible = True
        End If
    End Sub

    Protected Sub btnRefresh_Click(sender As Object, e As EventArgs)
        LoadFMEARecords()
    End Sub

    Protected Sub btnSaveFMEA_Click(sender As Object, e As EventArgs)
        Dim projectId = GetSelectedProjectId()
        If projectId = 0 Then
            litMessage.Text = "<div class='alert alert-danger'>Please select a project.</div>"
            Return
        End If

        Try
            Dim editId = CType(ViewState("EditFMEAId"), Integer?)
            Dim severity = CInt(ddlSeverity.SelectedValue)
            Dim occurrence = CInt(ddlOccurrence.SelectedValue)
            Dim detection = CInt(ddlDetection.SelectedValue)
            Dim rpn = severity * occurrence * detection

            If editId.HasValue Then
                Using cmd As New MySqlCommand(
                    "UPDATE FMEA_Records SET ProcessStep=@ProcessStep, FailureMode=@FailureMode, " &
                    "EffectOfFailure=@Effect, CauseOfFailure=@Cause, CurrentControl=@CurrentControl, " &
                    "Severity=@Severity, Occurrence=@Occurrence, Detection=@Detection, RPN=@RPN, " &
                    "RecommendedAction=@RecommendedAction WHERE Id=@Id AND TenantId=@TenantId")
                    cmd.Parameters.AddWithValue("@ProcessStep", txtProcessStep.Text.Trim())
                    cmd.Parameters.AddWithValue("@FailureMode", txtFailureMode.Text.Trim())
                    cmd.Parameters.AddWithValue("@Effect", txtEffect.Text.Trim())
                    cmd.Parameters.AddWithValue("@Cause", txtCause.Text.Trim())
                    cmd.Parameters.AddWithValue("@CurrentControl", txtCurrentControl.Text.Trim())
                    cmd.Parameters.AddWithValue("@Severity", severity)
                    cmd.Parameters.AddWithValue("@Occurrence", occurrence)
                    cmd.Parameters.AddWithValue("@Detection", detection)
                    cmd.Parameters.AddWithValue("@RPN", rpn)
                    cmd.Parameters.AddWithValue("@RecommendedAction", txtRecommendedAction.Text.Trim())
                    cmd.Parameters.AddWithValue("@Id", editId.Value)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    DbHelper.ExecuteNonQuery(cmd)
                End Using
                LogAudit("Update", "FMEA", editId.Value, "Updated FMEA record")
                AuditHelper.Log("Update", "FMEA", editId.Value, "Updated FMEA: " & txtProcessStep.Text.Trim())
                litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>FMEA record updated.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
                ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('FMEA record updated successfully', 'success');", True)
            Else
                Using cmd As New MySqlCommand(
                    "INSERT INTO FMEA_Records (TenantId, ProjectId, ProcessStep, FailureMode, EffectOfFailure, CauseOfFailure, " &
                    "CurrentControl, Severity, Occurrence, Detection, RPN, RecommendedAction, CreatedOn, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @ProcessStep, @FailureMode, @Effect, @Cause, @CurrentControl, " &
                    "@Severity, @Occurrence, @Detection, @RPN, @RecommendedAction, CURRENT_TIMESTAMP, @CreatedByUserId)")
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", projectId)
                    cmd.Parameters.AddWithValue("@ProcessStep", txtProcessStep.Text.Trim())
                    cmd.Parameters.AddWithValue("@FailureMode", txtFailureMode.Text.Trim())
                    cmd.Parameters.AddWithValue("@Effect", txtEffect.Text.Trim())
                    cmd.Parameters.AddWithValue("@Cause", txtCause.Text.Trim())
                    cmd.Parameters.AddWithValue("@CurrentControl", txtCurrentControl.Text.Trim())
                    cmd.Parameters.AddWithValue("@Severity", severity)
                    cmd.Parameters.AddWithValue("@Occurrence", occurrence)
                    cmd.Parameters.AddWithValue("@Detection", detection)
                    cmd.Parameters.AddWithValue("@RPN", rpn)
                    cmd.Parameters.AddWithValue("@RecommendedAction", txtRecommendedAction.Text.Trim())
                    cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                    DbHelper.ExecuteNonQuery(cmd)
                    Dim newId = CInt(cmd.LastInsertedId)
                    LogAudit("Create", "FMEA", newId, "Created FMEA record: " & txtProcessStep.Text)
                    AuditHelper.Log("Create", "FMEA", newId, "Created FMEA: " & txtProcessStep.Text.Trim())
                End Using
                litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>FMEA record created.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
                ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('FMEA record saved successfully', 'success');", True)
            End If

            ClearEntryForm()
            LoadFMEARecords()
        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub btnClearForm_Click(sender As Object, e As EventArgs)
        ClearEntryForm()
    End Sub

    Protected Sub ddlFMEARecord_SelectedIndexChanged(sender As Object, e As EventArgs)
        If ddlFMEARecord.SelectedValue <> "" Then
            Dim fmeaId = CInt(ddlFMEARecord.SelectedValue)
            Using cmd As New MySqlCommand(
                "SELECT * FROM FMEA_Records WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", fmeaId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    Dim r = dt.Rows(0)
                    txtPrevRPN.Text = r("RPN").ToString()
                    ddlNewSeverity.SelectedValue = r("Severity").ToString()
                    ddlNewOccurrence.SelectedValue = r("Occurrence").ToString()
                    ddlNewDetection.SelectedValue = r("Detection").ToString()
                    txtNewRPN.Text = r("RPN").ToString()
                    txtActionTaken.Text = ""
                    pnlAction.Visible = True
                    ViewState("ActionFMEAId") = fmeaId
                End If
            End Using
        Else
            pnlAction.Visible = False
        End If
    End Sub

    Protected Sub btnUpdateAction_Click(sender As Object, e As EventArgs)
        Dim fmeaId = CType(ViewState("ActionFMEAId"), Integer?)
        If Not fmeaId.HasValue Then
            litActionMessage.Text = "<div class='alert alert-warning'>No FMEA record selected.</div>"
            Return
        End If

        Try
            Dim newSeverity = CInt(ddlNewSeverity.SelectedValue)
            Dim newOccurrence = CInt(ddlNewOccurrence.SelectedValue)
            Dim newDetection = CInt(ddlNewDetection.SelectedValue)
            Dim newRpn = newSeverity * newOccurrence * newDetection

            Using cmd As New MySqlCommand(
                "UPDATE FMEA_Records SET ActionTaken=@ActionTaken, NewSeverity=@NewSeverity, " &
                "NewOccurrence=@NewOccurrence, NewDetection=@NewDetection, NewRPN=@NewRPN, UpdatedOn=CURRENT_TIMESTAMP, UpdatedByUserId=@UpdatedByUserId " &
                "WHERE Id=@Id AND TenantId=@TenantId")
                cmd.Parameters.AddWithValue("@ActionTaken", txtActionTaken.Text.Trim())
                cmd.Parameters.AddWithValue("@NewSeverity", newSeverity)
                cmd.Parameters.AddWithValue("@NewOccurrence", newOccurrence)
                cmd.Parameters.AddWithValue("@NewDetection", newDetection)
                cmd.Parameters.AddWithValue("@NewRPN", newRpn)
                cmd.Parameters.AddWithValue("@Id", fmeaId.Value)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@UpdatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "FMEA", fmeaId.Value, "Updated action tracking for FMEA")
            litActionMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Action updated successfully. New RPN: " & newRpn & "<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('FMEA action updated successfully', 'success');", True)
            LoadFMEARecords()
            pnlAction.Visible = False
        Catch ex As Exception
            litActionMessage.Text = "<div class='alert alert-danger'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Protected Sub rptFMEARecords_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim fmeaId = CInt(e.CommandArgument)
        If e.CommandName = "EditRecord" Then
            Using cmd As New MySqlCommand(
                "SELECT * FROM FMEA_Records WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", fmeaId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                Dim dt = DbHelper.ExecuteDataTable(cmd)
                If dt.Rows.Count > 0 Then
                    Dim r = dt.Rows(0)
                    txtProcessStep.Text = r("ProcessStep").ToString()
                    txtFailureMode.Text = r("FailureMode").ToString()
                    txtEffect.Text = r("EffectOfFailure").ToString()
                    txtCause.Text = r("CauseOfFailure").ToString()
                    txtCurrentControl.Text = r("CurrentControl").ToString()
                    ddlSeverity.SelectedValue = r("Severity").ToString()
                    ddlOccurrence.SelectedValue = r("Occurrence").ToString()
                    ddlDetection.SelectedValue = r("Detection").ToString()
                    Dim rpn = CInt(r("Severity")) * CInt(r("Occurrence")) * CInt(r("Detection"))
                    txtRPN.Text = rpn.ToString()
                    txtRecommendedAction.Text = r("RecommendedAction").ToString()
                    ViewState("EditFMEAId") = fmeaId
                    litMessage.Text = "<div class='alert alert-info'>Editing FMEA record. Click Save to update.</div>"
                End If
            End Using
        ElseIf e.CommandName = "DeleteRecord" Then
            Using cmd As New MySqlCommand("DELETE FROM FMEA_Records WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", fmeaId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "FMEA", fmeaId, "Deleted FMEA record")
            AuditHelper.Log("Delete", "FMEA", fmeaId, "Deleted FMEA record")
            LoadFMEARecords()
            ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('FMEA record deleted', 'info');", True)
        End If
    End Sub

    Protected Sub btnFilter_Click(sender As Object, e As EventArgs)
        LoadFMEARecords()
    End Sub

    Protected Sub btnClearFilter_Click(sender As Object, e As EventArgs)
        txtFilterStep.Text = ""
        LoadFMEARecords()
    End Sub

    Protected Function GetRPNBadge(rpn As Integer) As String
        If rpn >= 200 Then Return "bg-danger"
        If rpn >= 100 Then Return "bg-warning text-dark"
        Return "bg-success"
    End Function

    Protected Sub btnExportCsv_Click(sender As Object, e As EventArgs)
        Dim projectId = CInt(Request.QueryString("projectId"))
        Dim cmd As New MySqlCommand("SELECT f.ProcessStep, f.FailureMode, f.EffectOfFailure, f.CauseOfFailure, f.CurrentControl, f.Severity, f.Occurrence, f.Detection, f.RPN, f.RecommendedAction, f.NewSeverity, f.NewOccurrence, f.NewDetection, f.NewRPN FROM FMEA_Records f WHERE f.TenantId = @TenantId AND f.ProjectId = @ProjectId ORDER BY f.RPN DESC")
        cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
        cmd.Parameters.AddWithValue("@ProjectId", projectId)
        Dim dt = DbHelper.ExecuteDataTable(cmd)
        ExportHelper.ExportToCsv(dt, "FMEA_Project" & projectId)
        ScriptManager.RegisterStartupScript(Me, Me.GetType(), "toast", "showToast('FMEA exported to CSV', 'success');", True)
    End Sub
End Class
