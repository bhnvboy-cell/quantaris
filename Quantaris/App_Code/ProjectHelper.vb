Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public NotInheritable Class ProjectHelper
    Private Sub New()
    End Sub

    Public Shared Function GetProjects(tenantId As Integer, Optional status As String = Nothing) As DataTable
        Dim sql As String = "SELECT p.*, " &
            "COALESCE(NULLIF(CONCAT_WS(' ', u.FirstName, u.LastName), ''), 'Unassigned') AS OwnerName, " &
            "(SELECT COUNT(*) FROM DMAICSteps WHERE ProjectId = p.Id AND Status = 'Complete') AS StepsCompleted " &
            "FROM QuantarisProjects p " &
            "LEFT JOIN Users u ON p.ProjectOwnerId = u.Id " &
            "WHERE p.TenantId = @TenantId"

        If Not String.IsNullOrEmpty(status) Then
            sql &= " AND p.Status = @Status"
        End If
        sql &= " ORDER BY p.CreatedOn DESC"

        Using cmd As New MySqlCommand(sql)
            cmd.Parameters.AddWithValue("@TenantId", tenantId)
            If Not String.IsNullOrEmpty(status) Then
                cmd.Parameters.AddWithValue("@Status", status)
            End If
            Return DbHelper.ExecuteDataTable(cmd)
        End Using
    End Function

    Public Shared Function GetProject(projectId As Integer, tenantId As Integer) As DataRow
        Dim cmd As New MySqlCommand(
            "SELECT p.*, COALESCE(NULLIF(CONCAT_WS(' ', u.FirstName, u.LastName), ''), 'Unassigned') AS OwnerName " &
            "FROM QuantarisProjects p LEFT JOIN Users u ON p.ProjectOwnerId = u.Id " &
            "WHERE p.Id = @ProjectId AND p.TenantId = @TenantId")
        cmd.Parameters.AddWithValue("@ProjectId", projectId)
        cmd.Parameters.AddWithValue("@TenantId", tenantId)
        Dim dt = DbHelper.ExecuteDataTable(cmd)
        If dt.Rows.Count > 0 Then Return dt.Rows(0)
        Return Nothing
    End Function

    Public Shared Function CreateProject(tenantId As Integer, projectCode As String, projectName As String, description As String, ownerId As Integer, createdByUserId As Integer) As Integer
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand(
                "INSERT INTO QuantarisProjects (TenantId, ProjectCode, ProjectName, Description, Status, Phase, ProjectOwnerId, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @ProjectCode, @ProjectName, @Description, 'NotStarted', 'Define', @ProjectOwnerId, CURRENT_TIMESTAMP, @CreatedByUserId)", conn)
                cmd.Parameters.AddWithValue("@TenantId", tenantId)
                cmd.Parameters.AddWithValue("@ProjectCode", projectCode)
                cmd.Parameters.AddWithValue("@ProjectName", projectName)
                cmd.Parameters.AddWithValue("@Description", description)
                cmd.Parameters.AddWithValue("@ProjectOwnerId", If(ownerId > 0, ownerId, CObj(DBNull.Value)))
                cmd.Parameters.AddWithValue("@CreatedByUserId", createdByUserId)
                cmd.ExecuteNonQuery()
                cmd.CommandText = "SELECT LAST_INSERT_ID()"
                cmd.Parameters.Clear()
                Return CInt(cmd.ExecuteScalar())
            End Using
        End Using
    End Function

    Public Shared Sub UpdateProjectPhase(projectId As Integer, phase As String)
        Dim cmd As New MySqlCommand(
            "UPDATE QuantarisProjects SET Phase = @Phase WHERE Id = @ProjectId")
        cmd.Parameters.AddWithValue("@Phase", phase)
        cmd.Parameters.AddWithValue("@ProjectId", projectId)
        DbHelper.ExecuteNonQuery(cmd)
    End Sub

    Public Shared Sub UpdateProject(projectId As Integer, tenantId As Integer, projectCode As String, projectName As String, description As String, ownerId As Integer)
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand(
                "UPDATE QuantarisProjects SET ProjectCode = @ProjectCode, ProjectName = @ProjectName, Description = @Description, ProjectOwnerId = @ProjectOwnerId WHERE Id = @ProjectId AND TenantId = @TenantId", conn)
                cmd.Parameters.AddWithValue("@ProjectId", projectId)
                cmd.Parameters.AddWithValue("@TenantId", tenantId)
                cmd.Parameters.AddWithValue("@ProjectCode", projectCode)
                cmd.Parameters.AddWithValue("@ProjectName", projectName)
                cmd.Parameters.AddWithValue("@Description", description)
                cmd.Parameters.AddWithValue("@ProjectOwnerId", If(ownerId > 0, ownerId, CObj(DBNull.Value)))
                cmd.ExecuteNonQuery()
            End Using
        End Using
    End Sub
End Class
