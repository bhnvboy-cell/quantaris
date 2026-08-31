Imports System.Web
Imports MySql.Data.MySqlClient

Public NotInheritable Class TenantResolver
    Private Sub New()
    End Sub

    Public Shared Function ResolveTenantCode() As String
        Dim tenantCode As String = String.Empty

        ' Check query string first
        If Not String.IsNullOrEmpty(HttpContext.Current.Request.QueryString("tenant")) Then
            tenantCode = HttpContext.Current.Request.QueryString("tenant").Trim()
        End If

        ' Check cookie as fallback
        If String.IsNullOrEmpty(tenantCode) AndAlso HttpContext.Current.Request.Cookies("TenantCode") IsNot Nothing Then
            tenantCode = HttpContext.Current.Request.Cookies("TenantCode").Value
        End If

        Return tenantCode
    End Function

    Public Shared Function ResolveTenantId() As Integer
        Dim tenantCode As String = ResolveTenantCode()
        If String.IsNullOrEmpty(tenantCode) Then Return 0

        Dim tenantId As Integer = 0
        ' Cache in HttpContext.Items
        If HttpContext.Current.Items("TenantId") IsNot Nothing Then
            Return CInt(HttpContext.Current.Items("TenantId"))
        End If

        Dim connString As String = System.Configuration.ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString
        Using conn As New MySqlConnection(connString)
            conn.Open()
            Using cmd As New MySqlCommand(
                "SELECT Id FROM Tenants WHERE TenantCode = @TenantCode AND IsActive = TRUE", conn)
                cmd.Parameters.AddWithValue("@TenantCode", tenantCode)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then
                    tenantId = CInt(result)
                    HttpContext.Current.Items("TenantId") = tenantId
                End If
            End Using
        End Using
        Return tenantId
    End Function

    Public Shared Sub SetTenantCookie(tenantCode As String)
        Dim cookie As New HttpCookie("TenantCode", tenantCode)
        cookie.Expires = DateTime.Now.AddYears(1)
        cookie.HttpOnly = True
        HttpContext.Current.Response.Cookies.Add(cookie)
    End Sub
End Class
