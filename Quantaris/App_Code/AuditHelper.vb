Imports MySql.Data.MySqlClient
Imports System.Configuration

Public NotInheritable Class AuditHelper
    Private Sub New()
    End Sub

    Public Shared Sub Log(action As String, entityType As String, entityId As Integer?, description As String, Optional oldValue As String = Nothing, Optional newValue As String = Nothing)
        Try
            Dim tenantId = TenantResolver.ResolveTenantId()
            Dim userId = GetCurrentUserId()
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
                    cmd.Parameters.AddWithValue("@Description", description)
                    cmd.Parameters.AddWithValue("@IpAddress", HttpContext.Current.Request.UserHostAddress)
                    cmd.ExecuteNonQuery()
                End Using
            End Using
        Catch ex As Exception
            System.Diagnostics.Debug.WriteLine("Audit log error: " & ex.Message)
        End Try
    End Sub

    Private Shared Function GetCurrentUserId() As Integer
        Dim username = AuthHelper.GetCurrentUsername()
        Dim tenantId = TenantResolver.ResolveTenantId()
        If String.IsNullOrEmpty(username) Then Return 0
        Try
            Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
                conn.Open()
                Using cmd As New MySqlCommand("SELECT Id FROM Users WHERE Username = @Username AND TenantId = @TenantId", conn)
                    cmd.Parameters.AddWithValue("@Username", username)
                    cmd.Parameters.AddWithValue("@TenantId", tenantId)
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing Then Return CInt(result)
                End Using
            End Using
        Catch
        End Try
        Return 0
    End Function
End Class
