Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Define_Define_CTQ
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadCTQRecords()
        End If
    End Sub

    Private Sub LoadCTQRecords()
        Using cmd As New MySqlCommand("SELECT * FROM CTQ_Requirements " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptCTQ.DataSource = dt
            rptCTQ.DataBind()
            litRecordCount.Text = dt.Rows.Count & " record(s)"
        End Using
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not Page.IsValid Then Return
        Dim editId As Integer = CInt(hfEditId.Value)

        If editId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE CTQ_Requirements SET CTQName = @CTQName, Category = @Category, Requirement = @Requirement, " &
                "Specification = @Specification, TargetValue = @TargetValue, UpperLimit = @UpperLimit, LowerLimit = @LowerLimit, IsCritical = @IsCritical " &
                "WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", editId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@CTQName", txtCTQName.Text.Trim())
                cmd.Parameters.AddWithValue("@Category", txtCategory.Text.Trim())
                cmd.Parameters.AddWithValue("@Requirement", txtRequirement.Text.Trim())
                cmd.Parameters.AddWithValue("@Specification", txtSpecification.Text.Trim())
                cmd.Parameters.AddWithValue("@TargetValue", txtTarget.Text.Trim())
                cmd.Parameters.AddWithValue("@UpperLimit", txtUpperLimit.Text.Trim())
                cmd.Parameters.AddWithValue("@LowerLimit", txtLowerLimit.Text.Trim())
                cmd.Parameters.AddWithValue("@IsCritical", chkCritical.Checked)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "CTQ", editId, "Updated CTQ " & txtCTQName.Text)
        Else
            Using cmd As New MySqlCommand(
                "INSERT INTO CTQ_Requirements (TenantId, ProjectId, CTQName, Category, Requirement, Specification, TargetValue, UpperLimit, LowerLimit, IsCritical, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @CTQName, @Category, @Requirement, @Specification, @TargetValue, @UpperLimit, @LowerLimit, @IsCritical, CURRENT_TIMESTAMP, @CreatedByUserId)")
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@CTQName", txtCTQName.Text.Trim())
                cmd.Parameters.AddWithValue("@Category", txtCategory.Text.Trim())
                cmd.Parameters.AddWithValue("@Requirement", txtRequirement.Text.Trim())
                cmd.Parameters.AddWithValue("@Specification", txtSpecification.Text.Trim())
                cmd.Parameters.AddWithValue("@TargetValue", txtTarget.Text.Trim())
                cmd.Parameters.AddWithValue("@UpperLimit", txtUpperLimit.Text.Trim())
                cmd.Parameters.AddWithValue("@LowerLimit", txtLowerLimit.Text.Trim())
                cmd.Parameters.AddWithValue("@IsCritical", chkCritical.Checked)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Create", "CTQ", _projectId, "Added CTQ " & txtCTQName.Text)
        End If

        ClearForm()
        LoadCTQRecords()
    End Sub

    Protected Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click
        ClearForm()
    End Sub

    Protected Sub rptCTQ_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        Dim id As Integer = CInt(e.CommandArgument)
        If e.CommandName = "Delete" Then
            Using cmd As New MySqlCommand("DELETE FROM CTQ_Requirements WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "CTQ", id, "Deleted CTQ record")
            LoadCTQRecords()
        ElseIf e.CommandName = "Edit" Then
            LoadCTQForEdit(id)
        End If
    End Sub

    Private Sub LoadCTQForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM CTQ_Requirements WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfEditId.Value = id.ToString()
                txtCTQName.Text = row("CTQName").ToString()
                txtCategory.Text = row("Category").ToString()
                txtRequirement.Text = row("Requirement").ToString()
                txtSpecification.Text = row("Specification").ToString()
                txtTarget.Text = row("TargetValue").ToString()
                txtUpperLimit.Text = row("UpperLimit").ToString()
                txtLowerLimit.Text = row("LowerLimit").ToString()
                chkCritical.Checked = CBool(row("IsCritical"))
                btnCancel.Visible = True
            End If
        End Using
    End Sub

    Private Sub ClearForm()
        txtCTQName.Text = ""
        txtCategory.Text = ""
        txtRequirement.Text = ""
        txtSpecification.Text = ""
        txtTarget.Text = ""
        txtUpperLimit.Text = ""
        txtLowerLimit.Text = ""
        chkCritical.Checked = False
        hfEditId.Value = "0"
        btnCancel.Visible = False
    End Sub
End Class
