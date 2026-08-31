Imports MySql.Data.MySqlClient
Imports System.Configuration

Public NotInheritable Class Logger
    Private Sub New()
    End Sub

    Public Shared Sub Log(tenantId As Integer, userId As Integer, action As String, entityType As String, Optional entityId As Integer? = Nothing, Optional description As String = Nothing)
        Try
            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()
                Using cmd As New MySqlCommand(
                    "INSERT INTO AuditLogs (TenantId, UserId, Action, EntityType, EntityId, Description, IpAddress, CreatedOn) " &
                    "VALUES (@TenantId, @UserId, @Action, @EntityType, @EntityId, @Description, @IpAddress, CURRENT_TIMESTAMP)", conn)
                    cmd.Parameters.AddWithValue("@TenantId", tenantId)
                    cmd.Parameters.AddWithValue("@UserId", userId)
                    cmd.Parameters.AddWithValue("@Action", action)
                    cmd.Parameters.AddWithValue("@EntityType", entityType)
                    cmd.Parameters.AddWithValue("@EntityId", If(entityId.HasValue, CObj(entityId.Value), DBNull.Value))
                    cmd.Parameters.AddWithValue("@Description", If(description IsNot Nothing, CObj(description), DBNull.Value))
                    cmd.Parameters.AddWithValue("@IpAddress", HttpContext.Current.Request.UserHostAddress)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            ' Silently fail - logging should never break the app
            System.Diagnostics.Debug.WriteLine("Logger failed: " & ex.Message)
        End Try
    End Sub
End Class
