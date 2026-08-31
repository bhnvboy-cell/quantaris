Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public Partial Class DMAIC_Define_Define_SIPOC
    Inherits BasePage

    Private _projectId As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        _projectId = CInt(Request.QueryString("projectId"))
        If _projectId = 0 Then Response.Redirect("~/Projects/Default.aspx?tenant=" & CurrentTenantCode)
        If Not IsPostBack Then
            LoadSIPOC()
            UpdateStepStatus()
        End If
    End Sub

    Private Sub LoadSIPOC()
        Using cmd As New MySqlCommand("SELECT * FROM SIPOC_Maps " &
            "WHERE TenantId = @TenantId AND ProjectId = @ProjectId ORDER BY CreatedOn DESC")
            cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
            cmd.Parameters.AddWithValue("@ProjectId", _projectId)
            Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
            If dt.Rows.Count > 0 Then
                Dim row As DataRow = dt.Rows(0)
                hfSipocId.Value = row("Id").ToString()
                txtSuppliers.Text = row("Suppliers").ToString()
                txtInputs.Text = row("Inputs").ToString()
                txtProcessSteps.Text = row("ProcessSteps").ToString()
                txtOutputs.Text = row("Outputs").ToString()
                txtCustomers.Text = row("Customers").ToString()
            End If
        End Using
        RenderPreview()
    End Sub

    Private Sub RenderPreview()
        litSuppliers.Text = FormatSipocColumn(txtSuppliers.Text)
        litInputs.Text = FormatSipocColumn(txtInputs.Text)
        litProcessSteps.Text = FormatSipocColumn(txtProcessSteps.Text)
        litOutputs.Text = FormatSipocColumn(txtOutputs.Text)
        litCustomers.Text = FormatSipocColumn(txtCustomers.Text)
    End Sub

    Private Shared Function FormatSipocColumn(value As String) As String
        If String.IsNullOrWhiteSpace(value) Then Return "<span class='text-muted fst-italic'>Not defined</span>"
        Dim items = value.Split({vbCrLf, vbLf}, StringSplitOptions.RemoveEmptyEntries)
        Dim sb As New System.Text.StringBuilder()
        sb.Append("<ol class='list-unstyled mb-0 text-start'>")
        For Each item In items
            sb.Append("<li><i class='bi bi-arrow-right-short me-1'></i>" & HttpUtility.HtmlEncode(item.Trim()) & "</li>")
        Next
        sb.Append("</ol>")
        Return sb.ToString()
    End Function

    Protected Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        Dim sipocId As Integer = CInt(hfSipocId.Value)

        If sipocId > 0 Then
            Using cmd As New MySqlCommand(
                "UPDATE SIPOC_Maps SET Suppliers = @Suppliers, Inputs = @Inputs, ProcessSteps = @ProcessSteps, " &
                "Outputs = @Outputs, Customers = @Customers WHERE Id = @Id AND TenantId = @TenantId")
                cmd.Parameters.AddWithValue("@Id", sipocId)
                cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                cmd.Parameters.AddWithValue("@Suppliers", txtSuppliers.Text.Trim())
                cmd.Parameters.AddWithValue("@Inputs", txtInputs.Text.Trim())
                cmd.Parameters.AddWithValue("@ProcessSteps", txtProcessSteps.Text.Trim())
                cmd.Parameters.AddWithValue("@Outputs", txtOutputs.Text.Trim())
                cmd.Parameters.AddWithValue("@Customers", txtCustomers.Text.Trim())
                DbHelper.ExecuteNonQuery(cmd)
            End Using
            LogAudit("Update", "SIPOC", sipocId, "Updated SIPOC map")
        Else
            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()
                Using cmd As New MySqlCommand(
                    "INSERT INTO SIPOC_Maps (TenantId, ProjectId, Suppliers, Inputs, ProcessSteps, Outputs, Customers, CreatedOn, CreatedByUserId) " &
                    "VALUES (@TenantId, @ProjectId, @Suppliers, @Inputs, @ProcessSteps, @Outputs, @Customers, CURRENT_TIMESTAMP, @CreatedByUserId)", conn)
                    cmd.Parameters.AddWithValue("@TenantId", CurrentTenantId)
                    cmd.Parameters.AddWithValue("@ProjectId", _projectId)
                    cmd.Parameters.AddWithValue("@Suppliers", txtSuppliers.Text.Trim())
                    cmd.Parameters.AddWithValue("@Inputs", txtInputs.Text.Trim())
                    cmd.Parameters.AddWithValue("@ProcessSteps", txtProcessSteps.Text.Trim())
                    cmd.Parameters.AddWithValue("@Outputs", txtOutputs.Text.Trim())
                    cmd.Parameters.AddWithValue("@Customers", txtCustomers.Text.Trim())
                    cmd.Parameters.AddWithValue("@CreatedByUserId", CurrentUserId)
                    cmd.ExecuteNonQuery()
                    cmd.CommandText = "SELECT LAST_INSERT_ID()"
                    cmd.Parameters.Clear()
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing Then hfSipocId.Value = CInt(result).ToString()
                End Using
            End Using
            LogAudit("Create", "SIPOC", _projectId, "Created SIPOC map")
        End If

        RenderPreview()
    End Sub

    Private Sub UpdateStepStatus()
        DMAICHelper.UpdateStepStatus(_projectId, 3, "In Progress")
    End Sub
End Class
