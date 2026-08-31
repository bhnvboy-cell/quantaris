Imports System.Data
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public NotInheritable Class DbHelper
    Private Sub New()
    End Sub

    Private Shared ReadOnly _connString As String = ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString

    Public Shared Function GetConnection() As MySqlConnection
        Return New MySqlConnection(_connString)
    End Function

    Public Shared Function ExecuteNonQuery(cmd As MySqlCommand) As Integer
        Using conn As New MySqlConnection(_connString)
            cmd.Connection = conn
            conn.Open()
            Return cmd.ExecuteNonQuery()
        End Using
    End Function

    Public Shared Function ExecuteScalar(cmd As MySqlCommand) As Object
        Using conn As New MySqlConnection(_connString)
            cmd.Connection = conn
            conn.Open()
            Return cmd.ExecuteScalar()
        End Using
    End Function

    Public Shared Function ExecuteDataTable(cmd As MySqlCommand) As DataTable
        Dim dt As New DataTable()
        Using conn As New MySqlConnection(_connString)
            cmd.Connection = conn
            Using da As New MySqlDataAdapter(cmd)
                da.Fill(dt)
            End Using
        End Using
        Return dt
    End Function

    Public Shared Function ExecuteDataReader(cmd As MySqlCommand) As MySqlDataReader
        Dim conn As New MySqlConnection(_connString)
        cmd.Connection = conn
        conn.Open()
        Return cmd.ExecuteReader(CommandBehavior.CloseConnection)
    End Function

    Public Shared Sub AddParameter(cmd As MySqlCommand, name As String, value As Object)
        If value Is Nothing OrElse value Is DBNull.Value Then
            cmd.Parameters.AddWithValue(name, DBNull.Value)
        Else
            cmd.Parameters.AddWithValue(name, value)
        End If
    End Sub

    Public Shared Function AddTenantAndProjectParams(cmd As MySqlCommand, tenantId As Integer, Optional projectId As Integer? = Nothing) As MySqlCommand
        cmd.Parameters.AddWithValue("@TenantId", tenantId)
        If projectId.HasValue Then
            cmd.Parameters.AddWithValue("@ProjectId", projectId.Value)
        End If
        Return cmd
    End Function
End Class
