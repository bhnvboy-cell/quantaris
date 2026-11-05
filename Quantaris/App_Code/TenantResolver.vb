Imports System.Web
Imports MySql.Data.MySqlClient

Public NotInheritable Class TenantResolver
    Private Sub New()
    End Sub

    Public Shared Function ResolveTenantCode() As String
        Dim tenantCode As String = String.Empty

        ' Check authenticated identity ticket first to prevent tenant spoofing
        If HttpContext.Current IsNot Nothing AndAlso
           HttpContext.Current.User IsNot Nothing AndAlso
           HttpContext.Current.User.Identity IsNot Nothing AndAlso
           HttpContext.Current.User.Identity.IsAuthenticated Then
            Dim parts = HttpContext.Current.User.Identity.Name.Split("|"c)
            If parts.Length > 1 AndAlso Not String.IsNullOrEmpty(parts(1)) Then
                Return parts(1).Trim()
            End If
        End If

        ' Check query string next (for unauthenticated pages like Login/Register)
        If Not String.IsNullOrEmpty(HttpContext.Current.Request.QueryString("tenant")) Then
            tenantCode = HttpContext.Current.Request.QueryString("tenant").Trim()
        End If

        ' Check cookie as fallback
        If String.IsNullOrEmpty(tenantCode) AndAlso HttpContext.Current.Request.Cookies("TenantCode") IsNot Nothing Then
            tenantCode = HttpContext.Current.Request.Cookies("TenantCode").Value
        End If

        Return tenantCode
    End Function

    Public Shared Function ResolveTenantId(Optional explicitTenantCode As String = Nothing) As Integer
        Dim tenantCode As String = If(Not String.IsNullOrEmpty(explicitTenantCode), explicitTenantCode.Trim(), ResolveTenantCode())
        If String.IsNullOrEmpty(tenantCode) Then Return 0

        Dim tenantId As Integer = 0
        Dim cacheKey As String = "TenantId_" & tenantCode.ToUpperInvariant()
        ' Cache in HttpContext.Items per tenantCode
        If HttpContext.Current.Items(cacheKey) IsNot Nothing Then
            Return CInt(HttpContext.Current.Items(cacheKey))
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
                    HttpContext.Current.Items(cacheKey) = tenantId
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
