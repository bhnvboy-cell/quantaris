Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class Tools_WorkflowDesigner
    Inherits BasePage

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        RequireRole("Owner", "Admin")
        If Not IsPostBack Then
            LoadRoles()
            LoadWorkflows()
            LoadActiveInstances()
            LoadPendingApprovals()
        End If
    End Sub

    Private Sub LoadRoles()
        Using cmd As New MySqlCommand(
            "SELECT DISTINCT Role FROM Users WHERE TenantId = @TenantId ORDER BY Role")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            ddlAssignedRole.DataSource = dt
            ddlAssignedRole.DataTextField = "Role"
            ddlAssignedRole.DataValueField = "Role"
            ddlAssignedRole.DataBind()
            ddlAssignedRole.Items.Insert(0, New ListItem("-- Select --", ""))
            ddlRequiredRole.DataSource = dt.Copy()
            ddlRequiredRole.DataTextField = "Role"
            ddlRequiredRole.DataValueField = "Role"
            ddlRequiredRole.DataBind()
            ddlRequiredRole.Items.Insert(0, New ListItem("-- Select --", ""))
        End Using
    End Sub

    Private Sub LoadWorkflows()
        Using cmd As New MySqlCommand(
            "SELECT Id, WorkflowName, EntityType FROM WorkflowDefinitions WHERE TenantId = @TenantId AND IsActive = TRUE ORDER BY WorkflowName")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt = DbHelper.ExecuteDataTable(cmd)
            ddlWorkflowDef.DataSource = dt
            ddlWorkflowDef.DataTextField = "WorkflowName"
            ddlWorkflowDef.DataValueField = "Id"
            ddlWorkflowDef.DataBind()
            ddlWorkflowDef.Items.Insert(0, New ListItem("-- Select Workflow --", ""))
        End Using
    End Sub

    Private Sub LoadActiveInstances()
        Dim wfId As Integer = 0
        If ddlWorkflowDef.SelectedValue <> "" Then
            Integer.TryParse(ddlWorkflowDef.SelectedValue, wfId)
        End If
        Dim sql As String = "SELECT wi.Id, wi.EntityType, wi.EntityId, ws.StateName AS CurrentState, " &
            "COALESCE(CONCAT_WS(' ', u.FirstName, u.LastName), '') AS AssignedUser, wi.CreatedOn " &
            "FROM WorkflowInstances wi " &
            "JOIN WorkflowStates ws ON wi.CurrentStateId = ws.Id " &
            "LEFT JOIN Users u ON wi.AssignedUserId = u.Id " &
            "WHERE wi.TenantId = @TenantId AND wi.Status = 'Active'"
        If wfId > 0 Then
            sql &= " AND wi.WorkflowId = @WorkflowId"
        End If
        sql &= " ORDER BY wi.CreatedOn DESC"
        Using cmd As New MySqlCommand(sql)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            If wfId > 0 Then
                cmd.Parameters.AddWithValue("@WorkflowId", wfId)
            End If
            rptActiveInstances.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptActiveInstances.DataBind()
        End Using
    End Sub

    Private Sub LoadPendingApprovals()
        Dim sql As String = "SELECT wa.Id, wi.EntityType, wi.EntityId, ws.StateName AS CurrentState, " &
            "wt.TransitionName, wa.CreatedOn " &
            "FROM WorkflowApprovals wa " &
            "JOIN WorkflowInstances wi ON wa.InstanceId = wi.Id " &
            "JOIN WorkflowStates ws ON wi.CurrentStateId = ws.Id " &
            "JOIN WorkflowTransitions wt ON wa.TransitionId = wt.Id " &
            "WHERE wi.AssignedUserId = @UserId AND wa.Decision = 'Pending' " &
            "ORDER BY wa.CreatedOn DESC"
        Using cmd As New MySqlCommand(sql)
            cmd.Parameters.AddWithValue("@UserId", CurrentUserId)
            rptPendingApprovals.DataSource = DbHelper.ExecuteDataTable(cmd)
            rptPendingApprovals.DataBind()
        End Using
    End Sub

    Private Function GetStatesTable() As DataTable
        Dim dt = CType(ViewState("States"), DataTable)
        If dt Is Nothing Then
            dt = New DataTable()
            dt.Columns.Add("RowIndex", GetType(Integer))
            dt.Columns.Add("StateName", GetType(String))
            dt.Columns.Add("StateOrder", GetType(Integer))
            dt.Columns.Add("IsInitial", GetType(Boolean))
            dt.Columns.Add("IsFinal", GetType(Boolean))
            dt.Columns.Add("AssignedRole", GetType(String))
            dt.PrimaryKey = New DataColumn() {dt.Columns("RowIndex")}
            ViewState("States") = dt
        End If
        Return dt
    End Function

    Private Function GetTransitionsTable() As DataTable
        Dim dt = CType(ViewState("Transitions"), DataTable)
        If dt Is Nothing Then
            dt = New DataTable()
            dt.Columns.Add("RowIndex", GetType(Integer))
            dt.Columns.Add("FromStateName", GetType(String))
            dt.Columns.Add("ToStateName", GetType(String))
            dt.Columns.Add("TransitionName", GetType(String))
            dt.Columns.Add("RequiredRole", GetType(String))
            dt.PrimaryKey = New DataColumn() {dt.Columns("RowIndex")}
            ViewState("Transitions") = dt
        End If
        Return dt
    End Function

    Private Sub BindStates()
        rptStates.DataSource = GetStatesTable()
        rptStates.DataBind()
        PopulateTransitionStateDropdowns()
    End Sub

    Private Sub BindTransitions()
        rptTransitions.DataSource = GetTransitionsTable()
        rptTransitions.DataBind()
    End Sub

    Private Sub PopulateTransitionStateDropdowns()
        Dim dt = GetStatesTable()
        ddlFromState.Items.Clear()
        ddlFromState.Items.Add(New ListItem("-- Select --", ""))
        ddlToState.Items.Clear()
        ddlToState.Items.Add(New ListItem("-- Select --", ""))
        For Each r As DataRow In dt.Rows
            Dim name = r("StateName").ToString()
            Dim idx = r("RowIndex").ToString()
            ddlFromState.Items.Add(New ListItem(name, idx))
            ddlToState.Items.Add(New ListItem(name, idx))
        Next
    End Sub

    Protected Sub btnAddState_Click(sender As Object, e As EventArgs)
        Dim name = txtStateName.Text.Trim()
        If String.IsNullOrEmpty(name) Then
            litMessage.Text = "<div class='alert alert-warning'>Please enter a state name.</div>"
            Return
        End If
        Dim order As Integer
        Integer.TryParse(txtStateOrder.Text.Trim(), order)
        Dim dt = GetStatesTable()
        Dim nextIdx = 1
        If dt.Rows.Count > 0 Then
            nextIdx = CInt(dt.Compute("MAX(RowIndex)", "")) + 1
        End If
        Dim role = ddlAssignedRole.SelectedValue
        Dim row = dt.NewRow()
        row("RowIndex") = nextIdx
        row("StateName") = name
        row("StateOrder") = If(order = 0, dt.Rows.Count + 1, order)
        row("IsInitial") = chkIsInitial.Checked
        row("IsFinal") = chkIsFinal.Checked
        row("AssignedRole") = If(role = "", DBNull.Value, CObj(role))
        dt.Rows.Add(row)
        txtStateName.Text = ""
        txtStateOrder.Text = ""
        chkIsInitial.Checked = False
        chkIsFinal.Checked = False
        ddlAssignedRole.SelectedIndex = 0
        litMessage.Text = ""
        BindStates()
        BindTransitions()
    End Sub

    Protected Sub btnAddTransition_Click(sender As Object, e As EventArgs)
        If ddlFromState.SelectedValue = "" OrElse ddlToState.SelectedValue = "" Then
            litMessage.Text = "<div class='alert alert-warning'>Please select From and To states.</div>"
            Return
        End If
        Dim tName = txtTransitionName.Text.Trim()
        If String.IsNullOrEmpty(tName) Then
            litMessage.Text = "<div class='alert alert-warning'>Please enter a transition name.</div>"
            Return
        End If
        Dim statesDt = GetStatesTable()
        Dim fromRow = statesDt.Rows.Find(CInt(ddlFromState.SelectedValue))
        Dim toRow = statesDt.Rows.Find(CInt(ddlToState.SelectedValue))
        If fromRow Is Nothing OrElse toRow Is Nothing Then
            litMessage.Text = "<div class='alert alert-warning'>Invalid state selection.</div>"
            Return
        End If
        Dim dt = GetTransitionsTable()
        Dim nextIdx = 1
        If dt.Rows.Count > 0 Then
            nextIdx = CInt(dt.Compute("MAX(RowIndex)", "")) + 1
        End If
        Dim role = ddlRequiredRole.SelectedValue
        Dim row = dt.NewRow()
        row("RowIndex") = nextIdx
        row("FromStateName") = fromRow("StateName").ToString()
        row("ToStateName") = toRow("StateName").ToString()
        row("TransitionName") = tName
        row("RequiredRole") = If(role = "", DBNull.Value, CObj(role))
        dt.Rows.Add(row)
        txtTransitionName.Text = ""
        ddlRequiredRole.SelectedIndex = 0
        litMessage.Text = ""
        BindTransitions()
    End Sub

    Protected Sub btnSaveWorkflow_Click(sender As Object, e As EventArgs)
        Dim wfName = txtWorkflowName.Text.Trim()
        If String.IsNullOrEmpty(wfName) Then
            litMessage.Text = "<div class='alert alert-warning'>Please enter a workflow name.</div>"
            Return
        End If
        Dim statesDt = GetStatesTable()
        Dim transDt = GetTransitionsTable()
        If statesDt.Rows.Count = 0 Then
            litMessage.Text = "<div class='alert alert-warning'>Please add at least one state.</div>"
            Return
        End If
        If Not statesDt.AsEnumerable().Any(Function(r) CBool(r("IsInitial"))) Then
            litMessage.Text = "<div class='alert alert-warning'>Please mark at least one state as Initial.</div>"
            Return
        End If
        Try
            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()
                Using tx = conn.BeginTransaction()
                    Using cmd As New MySqlCommand(
                        "INSERT INTO WorkflowDefinitions (TenantId, WorkflowName, EntityType, Description, CreatedByUserId) " &
                        "VALUES (@TenantId, @WorkflowName, @EntityType, @Description, @CreatedByUserId)", conn, tx)
                        cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                        cmd.Parameters.AddWithValue("@WorkflowName", wfName)
                        cmd.Parameters.AddWithValue("@EntityType", ddlEntityType.SelectedValue)
                        cmd.Parameters.AddWithValue("@Description", If(txtDescription.Text.Trim() = "", DBNull.Value, CObj(txtDescription.Text.Trim())))
                        cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                        cmd.ExecuteNonQuery()
                        cmd.CommandText = "SELECT LAST_INSERT_ID()"
                        cmd.Parameters.Clear()
                        Dim workflowId = CInt(cmd.ExecuteScalar())
                        Dim stateIdMap As New Dictionary(Of Integer, Integer)()
                        For Each r As DataRow In statesDt.Rows
                            Using cmdState As New MySqlCommand(
                                "INSERT INTO WorkflowStates (WorkflowId, StateName, StateOrder, IsInitial, IsFinal, AssignedRole) " &
                                "VALUES (@WorkflowId, @StateName, @StateOrder, @IsInitial, @IsFinal, @AssignedRole)", conn, tx)
                                cmdState.Parameters.AddWithValue("@WorkflowId", workflowId)
                                cmdState.Parameters.AddWithValue("@StateName", r("StateName").ToString())
                                cmdState.Parameters.AddWithValue("@StateOrder", CInt(r("StateOrder")))
                                cmdState.Parameters.AddWithValue("@IsInitial", CBool(r("IsInitial")))
                                cmdState.Parameters.AddWithValue("@IsFinal", CBool(r("IsFinal")))
                                cmdState.Parameters.AddWithValue("@AssignedRole", If(r("AssignedRole") Is DBNull.Value, DBNull.Value, CObj(r("AssignedRole").ToString())))
                                cmdState.ExecuteNonQuery()
                                cmdState.CommandText = "SELECT LAST_INSERT_ID()"
                                cmdState.Parameters.Clear()
                                Dim stateId = CInt(cmdState.ExecuteScalar())
                                stateIdMap(CInt(r("RowIndex"))) = stateId
                            End Using
                        Next
                        For Each r As DataRow In transDt.Rows
                            Dim fromStateIdx = 0
                            Dim toStateIdx = 0
                            For Each sr As DataRow In statesDt.Rows
                                If sr("StateName").ToString() = r("FromStateName").ToString() Then
                                    fromStateIdx = CInt(sr("RowIndex"))
                                End If
                                If sr("StateName").ToString() = r("ToStateName").ToString() Then
                                    toStateIdx = CInt(sr("RowIndex"))
                                End If
                            Next
                            If fromStateIdx > 0 AndAlso toStateIdx > 0 AndAlso stateIdMap.ContainsKey(fromStateIdx) AndAlso stateIdMap.ContainsKey(toStateIdx) Then
                                Using cmdTrans As New MySqlCommand(
                                    "INSERT INTO WorkflowTransitions (WorkflowId, FromStateId, ToStateId, TransitionName, RequiredRole) " &
                                    "VALUES (@WorkflowId, @FromStateId, @ToStateId, @TransitionName, @RequiredRole)", conn, tx)
                                    cmdTrans.Parameters.AddWithValue("@WorkflowId", workflowId)
                                    cmdTrans.Parameters.AddWithValue("@FromStateId", stateIdMap(fromStateIdx))
                                    cmdTrans.Parameters.AddWithValue("@ToStateId", stateIdMap(toStateIdx))
                                    cmdTrans.Parameters.AddWithValue("@TransitionName", r("TransitionName").ToString())
                                    cmdTrans.Parameters.AddWithValue("@RequiredRole", If(r("RequiredRole") Is DBNull.Value, DBNull.Value, CObj(r("RequiredRole").ToString())))
                                    cmdTrans.ExecuteNonQuery()
                                End Using
                            End If
                        Next
                        tx.Commit()
                        LogAudit("Create", "WorkflowDefinition", workflowId, "Created workflow: " & wfName)
                    End Using
                End Using
            End Using
            litMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Workflow saved successfully.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
            ClearDesigner()
            LoadWorkflows()
        Catch ex As Exception
            litMessage.Text = "<div class='alert alert-danger'>Error: " & ex.Message & "</div>"
        End Try
    End Sub

    Private Sub ClearDesigner()
        txtWorkflowName.Text = ""
        txtDescription.Text = ""
        ddlEntityType.SelectedIndex = 0
        ViewState("States") = Nothing
        ViewState("Transitions") = Nothing
        BindStates()
        BindTransitions()
    End Sub

    Protected Sub btnClearDesigner_Click(sender As Object, e As EventArgs)
        ClearDesigner()
        litMessage.Text = ""
    End Sub

    Protected Sub ddlWorkflowDef_SelectedIndexChanged(sender As Object, e As EventArgs)
        pnlApprovalHistory.Visible = False
        LoadActiveInstances()
    End Sub

    Protected Sub rptActiveInstances_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "ViewHistory" Then
            Dim instanceId = CInt(e.CommandArgument)
            Dim sql As String = "SELECT fs.StateName AS FromState, ts.StateName AS ToState, " &
                "wt.TransitionName, wa.Decision, wa.Comments, " &
                "COALESCE(CONCAT_WS(' ', u.FirstName, u.LastName), '') AS ApprovedByName, wa.DecidedOn " &
                "FROM WorkflowApprovals wa " &
                "JOIN WorkflowTransitions wt ON wa.TransitionId = wt.Id " &
                "JOIN WorkflowStates fs ON wa.FromStateId = fs.Id " &
                "JOIN WorkflowStates ts ON wa.ToStateId = ts.Id " &
                "LEFT JOIN Users u ON wa.ApprovedByUserId = u.Id " &
                "WHERE wa.InstanceId = @InstanceId ORDER BY wa.CreatedOn DESC"
            Using cmd As New MySqlCommand(sql)
                cmd.Parameters.AddWithValue("@InstanceId", instanceId)
                rptApprovalHistory.DataSource = DbHelper.ExecuteDataTable(cmd)
                rptApprovalHistory.DataBind()
            End Using
            pnlApprovalHistory.Visible = True
        End If
    End Sub

    Protected Sub rptPendingApprovals_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim approvalId = CInt(e.CommandArgument)
        Dim decision = If(e.CommandName = "Approve", "Approved", "Rejected")
        Try
            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()
                Using tx = conn.BeginTransaction()
                    Using cmd As New MySqlCommand(
                        "UPDATE WorkflowApprovals SET Decision = @Decision, ApprovedByUserId = @ApprovedByUserId, " &
                        "Comments = @Comments, DecidedOn = CURRENT_TIMESTAMP WHERE Id = @Id AND Decision = 'Pending'", conn, tx)
                        cmd.Parameters.AddWithValue("@Decision", decision)
                        cmd.Parameters.AddWithValue("@ApprovedByUserId", CurrentUserId)
                        cmd.Parameters.AddWithValue("@Comments", If(txtApprovalComments.Text.Trim() = "", DBNull.Value, CObj(txtApprovalComments.Text.Trim())))
                        cmd.Parameters.AddWithValue("@Id", approvalId)
                        Dim affected = cmd.ExecuteNonQuery()
                        If affected > 0 AndAlso decision = "Approved" Then
                            Dim toStateId As Integer?
                            Dim instanceId As Integer?
                            Using cmdGet As New MySqlCommand(
                                "SELECT wa.InstanceId, wa.ToStateId FROM WorkflowApprovals wa WHERE wa.Id = @Id", conn, tx)
                                cmdGet.Parameters.AddWithValue("@Id", approvalId)
                                Using rdr = cmdGet.ExecuteReader()
                                    If rdr.Read() Then
                                        instanceId = CInt(rdr("InstanceId"))
                                        toStateId = CInt(rdr("ToStateId"))
                                    End If
                                End Using
                            End Using
                            If instanceId.HasValue AndAlso toStateId.HasValue Then
                                Using cmdUpdate As New MySqlCommand(
                                    "UPDATE WorkflowInstances SET CurrentStateId = @CurrentStateId WHERE Id = @Id", conn, tx)
                                    cmdUpdate.Parameters.AddWithValue("@CurrentStateId", toStateId.Value)
                                    cmdUpdate.Parameters.AddWithValue("@Id", instanceId.Value)
                                    cmdUpdate.ExecuteNonQuery()
                                End Using
                            End If
                        End If
                    End Using
                    tx.Commit()
                    LogAudit(decision, "WorkflowApproval", approvalId, decision & " approval for instance")
                End Using
            End Using
            txtApprovalComments.Text = ""
            litApprovalMessage.Text = "<div class='alert alert-success alert-dismissible fade show'>Approval " & decision.ToLower() & " successfully.<button type='button' class='btn-close' data-bs-dismiss='alert'></button></div>"
            LoadPendingApprovals()
            LoadActiveInstances()
        Catch ex As Exception
            litApprovalMessage.Text = "<div class='alert alert-danger'>Error: " & ex.Message & "</div>"
        End Try
    End Sub
End Class
