Imports System.Web.UI
Imports MySql.Data.MySqlClient
Imports System.Configuration

Public MustInherit Class BasePage
    Inherits System.Web.UI.Page

    Private _currentTenantId As Integer
    Private _currentTenantCode As String
    Private _currentUserId As Integer
    Private _pageAuthorized As Boolean = True

    Public ReadOnly Property CurrentTenantId As Integer
        Get
            Return _currentTenantId
        End Get
    End Property

    Public ReadOnly Property CurrentTenantCode As String
        Get
            Return _currentTenantCode
        End Get
    End Property

    Public ReadOnly Property CurrentUserId As Integer
        Get
            Return _currentUserId
        End Get
    End Property

    Protected Overrides Sub OnLoad(e As EventArgs)
        ResolveTenant()
        ResolveUser()
        If Not _pageAuthorized Then
            Response.Redirect("~/Account/Login.aspx?tenant=" & _currentTenantCode, True)
        End If
        MyBase.OnLoad(e)
    End Sub

    Private Sub ResolveTenant()
        ' Try from auth ticket first, then query string
        If HttpContext.Current.User.Identity.IsAuthenticated Then
            Dim parts = HttpContext.Current.User.Identity.Name.Split("|"c)
            If parts.Length > 1 Then
                _currentTenantCode = parts(1)
            End If
        End If

        If String.IsNullOrEmpty(_currentTenantCode) Then
            _currentTenantCode = TenantResolver.ResolveTenantCode()
        End If

        If String.IsNullOrEmpty(_currentTenantCode) Then
            _currentTenantCode = "DEMO"
        End If

        _currentTenantId = TenantResolver.ResolveTenantId(_currentTenantCode)

        ' Ensure tenant cookie is set
        If HttpContext.Current.Request.Cookies("TenantCode") Is Nothing Then
            TenantResolver.SetTenantCookie(_currentTenantCode)
        End If
    End Sub

    Private Sub ResolveUser()
        If HttpContext.Current.User.Identity.IsAuthenticated Then
            Dim username = AuthHelper.GetCurrentUsername()
            _currentUserId = GetUserId(username)
        End If
    End Sub

    Private Function GetUserId(username As String) As Integer
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand("SELECT Id FROM Users WHERE Username = @Username AND TenantId = @TenantId", conn)
                cmd.Parameters.AddWithValue("@Username", username)
                cmd.Parameters.AddWithValue("@TenantId", _currentTenantId)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then Return CInt(result)
            End Using
        End Using
        Return 0
    End Function

    Protected Sub RequireRole(ParamArray roles() As String)
        If Not HttpContext.Current.User.Identity.IsAuthenticated Then
            _pageAuthorized = False
            Return
        End If

        Dim userRole = GetUserRole()
        If roles.Length > 0 AndAlso Not roles.Contains(userRole) Then
            Response.Redirect("~/Account/AccessDenied.aspx?tenant=" & _currentTenantCode, True)
        End If
    End Sub

    Protected Function GetUserRole() As String
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand("SELECT Role FROM Users WHERE Id = @UserId AND TenantId = @TenantId", conn)
                cmd.Parameters.AddWithValue("@UserId", _currentUserId)
                cmd.Parameters.AddWithValue("@TenantId", _currentTenantId)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then Return result.ToString()
            End Using
        End Using
        Return "User"
    End Function

    Protected Sub LogAudit(action As String, entityType As String, Optional entityId As Integer? = Nothing, Optional description As String = Nothing)
        Logger.Log(_currentTenantId, _currentUserId, action, entityType, entityId, description)
    End Sub

    Protected Function GetCurrentUserFullName() As String
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand(
                "SELECT CONCAT_WS(' ', FirstName, LastName) AS FullName FROM Users WHERE Id = @UserId AND TenantId = @TenantId", conn)
                cmd.Parameters.AddWithValue("@UserId", _currentUserId)
                cmd.Parameters.AddWithValue("@TenantId", _currentTenantId)
                Dim result = cmd.ExecuteScalar()
                If result IsNot Nothing Then Return result.ToString()
            End Using
        End Using
        Return String.Empty
    End Function
End Class
