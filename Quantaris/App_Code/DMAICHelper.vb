Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Data

Public NotInheritable Class DMAICHelper
    Private Sub New()
    End Sub

    Public Shared Function GetStepStatus(projectId As Integer, stepNumber As Integer) As DataRow
        Dim cmd As New MySqlCommand(
            "SELECT * FROM DMAICSteps WHERE ProjectId = @ProjectId AND StepNumber = @StepNumber")
        cmd.Parameters.AddWithValue("@ProjectId", projectId)
        cmd.Parameters.AddWithValue("@StepNumber", stepNumber)
        Dim dt As DataTable = DbHelper.ExecuteDataTable(cmd)
        dt.TableName = "DMAICSteps"
        If dt.Rows.Count > 0 Then Return dt.Rows(0)
        Return Nothing
    End Function

    Public Shared Function GetAllSteps(projectId As Integer) As DataTable
        Dim cmd As New MySqlCommand(
            "SELECT * FROM DMAICSteps WHERE ProjectId = @ProjectId ORDER BY StepNumber")
        cmd.Parameters.AddWithValue("@ProjectId", projectId)
        Return DbHelper.ExecuteDataTable(cmd)
    End Function

    Public Shared Sub UpdateStepStatus(projectId As Integer, stepNumber As Integer, status As String, Optional completedOn As DateTime? = Nothing)
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Dim sql As String = "INSERT INTO DMAICSteps (TenantId, ProjectId, Phase, StepNumber, StepName, Status, CompletedOn, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectId, @Phase, @StepNumber, @StepName, @Status, @CompletedOn, CURRENT_TIMESTAMP, @CreatedByUserId) " &
                "ON DUPLICATE KEY UPDATE Status = @Status, CompletedOn = @CompletedOn"
            Using cmd As New MySqlCommand(sql, conn)
                cmd.Parameters.AddWithValue("@ProjectId", projectId)
                cmd.Parameters.AddWithValue("@StepNumber", stepNumber)
                cmd.Parameters.AddWithValue("@Status", status)
                cmd.Parameters.AddWithValue("@CompletedOn", If(completedOn.HasValue, CObj(completedOn.Value), DBNull.Value))
                cmd.Parameters.AddWithValue("@Phase", GetPhaseForStep(stepNumber))
                cmd.Parameters.AddWithValue("@StepName", GetStepName(stepNumber))
                cmd.Parameters.AddWithValue("@TenantId", GetTenantIdForProject(projectId))
                cmd.Parameters.AddWithValue("@CreatedByUserId", GetCurrentUserId())
                cmd.ExecuteNonQuery()
            End Using
        End Using
        AuditHelper.Log("Update", "DMAICStep", projectId, "Step " & stepNumber & " status changed to " & status)
    End Sub

    Private Shared Function GetPhaseForStep(stepNumber As Integer) As String
        Select Case stepNumber
            Case 1 To 3 : Return "Define"
            Case 4 To 6 : Return "Measure"
            Case 7 To 9 : Return "Analyze"
            Case 10 To 12 : Return "Improve"
            Case 13 To 15 : Return "Control"
            Case Else : Return "Unknown"
        End Select
    End Function

    Public Shared Function GetStepName(stepNumber As Integer) As String
        Select Case stepNumber
            Case 1 : Return "Understand Customer & Business Requirements"
            Case 2 : Return "Complete Project Charter"
            Case 3 : Return "Complete High-Level As-Is Process Map (SIPOC)"
            Case 4 : Return "Identify What to Measure"
            Case 5 : Return "Plan and Collect Data"
            Case 6 : Return "Determine Baseline Performance"
            Case 7 : Return "Identify Performance Gaps"
            Case 8 : Return "Ascertain Critical Root Causes"
            Case 9 : Return "Validate Root Causes"
            Case 10 : Return "Generate, Prioritize and Select Solutions"
            Case 11 : Return "Pilot Solution(s)"
            Case 12 : Return "Validate Impact of Solutions"
            Case 13 : Return "Institutionalize the Solutions"
            Case 14 : Return "Replicate and Share Best Practices"
            Case 15 : Return "Celebrate and Recognize Success"
            Case Else : Return "Unknown Step"
        End Select
    End Function

    Private Shared Function GetTenantIdForProject(projectId As Integer) As Integer
        Dim cmd As New MySqlCommand(
            "SELECT TenantId FROM QuantarisProjects WHERE Id = @ProjectId")
        cmd.Parameters.AddWithValue("@ProjectId", projectId)
        Dim dt = DbHelper.ExecuteDataTable(cmd)
        If dt.Rows.Count > 0 Then Return CInt(dt.Rows(0)("TenantId"))
        Return 0
    End Function

    Private Shared Function GetCurrentUserId() As Integer
        Dim username = AuthHelper.GetCurrentUsername()
        Dim tenantCode = AuthHelper.GetCurrentTenantCode()
        Dim tenantId = TenantResolver.ResolveTenantId()
        If String.IsNullOrEmpty(username) Then Return 0
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand("SELECT Id FROM Users WHERE Username = @Username AND TenantId = @TenantId", conn)
                cmd.Parameters.AddWithValue("@Username", username)
                cmd.Parameters.AddWithValue("@TenantId", tenantId)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then Return CInt(result)
            End Using
        End Using
        Return 0
    End Function

    Public Shared Function GetDMAICProgressData(projectId As Integer) As DataTable
        Dim dt As New DataTable()
        dt.Columns.Add("StepNumber", GetType(Integer))
        dt.Columns.Add("StepName", GetType(String))
        dt.Columns.Add("Phase", GetType(String))
        dt.Columns.Add("Status", GetType(String))
        dt.Columns.Add("StatusClass", GetType(String))

        For i As Integer = 1 To 15
            Dim row = dt.NewRow()
            row("StepNumber") = i
            row("StepName") = GetStepName(i)
            row("Phase") = GetPhaseForStep(i)
            row("Status") = "Not Started"
            row("StatusClass") = "ox-badge-info"
            dt.Rows.Add(row)
        Next

        ' Fill in actual status from DB
        Dim actualSteps = GetAllSteps(projectId)
        For Each actual In actualSteps.Rows
            Dim stepNum = CInt(actual("StepNumber"))
            Dim matchingRow = dt.Select("StepNumber = " & stepNum)
            If matchingRow.Length > 0 Then
                Dim status = actual("Status").ToString()
                matchingRow(0)("Status") = status
                Select Case status.ToLower()
                    Case "in progress" : matchingRow(0)("StatusClass") = "ox-badge-warning"
                    Case "complete" : matchingRow(0)("StatusClass") = "ox-badge-success"
                    Case Else : matchingRow(0)("StatusClass") = "ox-badge-info"
                End Select
            End If
        Next

        Return dt
    End Function
End Class
