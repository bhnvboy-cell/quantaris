Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Define_Define_VOC
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadVOCRecords()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadVOCRecords()
        Using cmd As New MySqlCommand("SELECT v.*, CONCAT_WS(' ', u.FirstName, u.LastName) AS CreatedByName " &
            "FROM VOC_Records v LEFT JOIN Users u ON v.CreatedByUserId = u.Id " &
            "WHERE v.TenantId = @TenantId AND v.ProjectId = @ProjectId ORDER BY v.CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            rptVOC.DataSource = dt
            rptVOC.DataBind()
            litRecordCount.Text = dt.Rows.Count & " record(s)"
        End Using
    End Sub

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        If Not Page.IsValid Then Return
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand(
                "INSERT INTO VOC_Records (TenantId, ProjectId, CustomerName, VoiceStatement, AffinityGrouping, TranslatedNeed, Requirement, KanoClassification, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @CustomerName, @VoiceStatement, @AffinityGrouping, @TranslatedNeed, @Requirement, @KanoClassification, CURRENT_TIMESTAMP, @CreatedByUserId)", conn)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                cmd.Parameters.AddWithValue("@CustomerName", txtCustomerName.Text.Trim())
                cmd.Parameters.AddWithValue("@VoiceStatement", txtVoice.Text.Trim())
                cmd.Parameters.AddWithValue("@AffinityGrouping", txtAffinity.Text.Trim())
                cmd.Parameters.AddWithValue("@TranslatedNeed", txtNeed.Text.Trim())
                cmd.Parameters.AddWithValue("@Requirement", txtRequirement.Text.Trim())
                cmd.Parameters.AddWithValue("@KanoClassification", ddlKano.SelectedValue)
                cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                cmd.ExecuteNonQuery()
            End Using
        End Using

        LogAudit("Create", "VOC", _projectId, "Added VOC record for " & txtCustomerName.Text)
        ClearForm()
        LoadVOCRecords()
    End Sub

    Protected Sub btnSaveNew_Click(sender As Object, e As EventArgs) Handles btnSaveNew.Click
        btnSave_Click(sender, e)
    End Sub

    Protected Sub rptVOC_ItemCommand(source As Object, e As RepeaterCommandEventArgs)
        If e.CommandName = "Delete" Then
            Dim id As Integer = CInt(e.CommandArgument)
            Using cmd As New MySqlCommand("DELETE FROM VOC_Records WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", id)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Delete", "VOC", id, "Deleted VOC record")
            LoadVOCRecords()
        ElseIf e.CommandName = "Edit" Then
            Dim id As Integer = CInt(e.CommandArgument)
            LoadVOCForEdit(id)
        End If
    End Sub

    Private Sub LoadVOCForEdit(id As Integer)
        Using cmd As New MySqlCommand("SELECT * FROM VOC_Records WHERE Id = @Id AND TenantId = @TenantId")
            cmd.Parameters.AddWithValue("@Id", id)
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                txtCustomerName.Text = row("CustomerName").ToString()
                txtVoice.Text = row("VoiceStatement").ToString()
                txtAffinity.Text = row("AffinityGrouping").ToString()
                txtNeed.Text = row("TranslatedNeed").ToString()
                txtRequirement.Text = row("Requirement").ToString()
                If ddlKano.Items.FindByValue(row("KanoClassification").ToString()) IsNot Nothing Then
                    ddlKano.SelectedValue = row("KanoClassification").ToString()
                End If
                btnSave.Text = "Update"
                btnSave.OnClientClick = "return confirm('Update this record?');"
            End If
        End Using
    End Sub

    Private Sub ClearForm()
        txtCustomerName.Text = ""
        txtVoice.Text = ""
        txtAffinity.Text = ""
        txtNeed.Text = ""
        txtRequirement.Text = ""
        ddlKano.SelectedIndex = 0
        btnSave.Text = "Save"
        btnSave.OnClientClick = ""
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 1, "In Progress")
    End Sub
End Class
