Imports System.Web.Security
Imports System.Web
Imports MySql.Data.MySqlClient
Imports System.Configuration
Imports System.Security.Cryptography
Imports System.Text

Public NotInheritable Class AuthHelper
    Private Sub New()
    End Sub

    Public Shared Function ValidateUser(username As String, password As String, tenantId As Integer) As Integer
        Dim userId As Integer = 0
        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand(
                "SELECT Id, PasswordHash, PasswordSalt FROM Users WHERE Username = @Username AND TenantId = @TenantId AND IsActive = TRUE", conn)
                cmd.Parameters.AddWithValue("@Username", username)
                cmd.Parameters.AddWithValue("@TenantId", tenantId)
                Using rdr As MySqlDataReader = cmd.ExecuteReader()
                    If rdr.Read() Then
                        Dim storedHash = rdr("PasswordHash").ToString()
                        Dim storedSalt = rdr("PasswordSalt").ToString()
                        If VerifyPassword(password, storedHash, storedSalt) Then
                            userId = CInt(rdr("Id"))
                        End If
                    End If
                End Using
            End Using
        End Using
        Return userId
    End Function

    Public Shared Function CreateUser(username As String, email As String, password As String, firstName As String, lastName As String, role As String, tenantId As Integer, createdByUserId As Integer) As Integer
        Dim salt = GenerateSalt()
        Dim hash = HashPassword(password, salt)
        Dim userId As Integer = 0

        Using conn As New MySqlConnection(ConfigurationManager.ConnectionStrings("QuantarisDb").ConnectionString)
            conn.Open()
            Using cmd As New MySqlCommand(
                "INSERT INTO Users (TenantId, Username, Email, PasswordHash, PasswordSalt, FirstName, LastName, Role, IsActive, CreatedOn, CreatedByUserId) " &
                "VALUES (@TenantId, @Username, @Email, @PasswordHash, @PasswordSalt, @FirstName, @LastName, @Role, TRUE, CURRENT_TIMESTAMP, @CreatedByUserId)", conn)
                cmd.Parameters.AddWithValue("@TenantId", tenantId)
                cmd.Parameters.AddWithValue("@Username", username)
                cmd.Parameters.AddWithValue("@Email", email)
                cmd.Parameters.AddWithValue("@PasswordHash", hash)
                cmd.Parameters.AddWithValue("@PasswordSalt", salt)
                cmd.Parameters.AddWithValue("@FirstName", firstName)
                cmd.Parameters.AddWithValue("@LastName", lastName)
                cmd.Parameters.AddWithValue("@Role", role)
                cmd.Parameters.AddWithValue("@CreatedByUserId", createdByUserId)
                cmd.ExecuteNonQuery()
                cmd.CommandText = "SELECT LAST_INSERT_ID()"
                cmd.Parameters.Clear()
                userId = CInt(cmd.ExecuteScalar())
            End Using
        End Using
        Return userId
    End Function

    Public Shared Function SignIn(username As String, tenantCode As String, rememberMe As Boolean) As Boolean
        Try
            Dim ticket As New FormsAuthenticationTicket(
                1,
                username & "|" & tenantCode,
                DateTime.Now,
                DateTime.Now.Add(FormsAuthentication.Timeout),
                rememberMe,
                tenantCode,
                FormsAuthentication.FormsCookiePath)

            Dim encTicket As String = FormsAuthentication.Encrypt(ticket)
            Dim cookie As New HttpCookie(FormsAuthentication.FormsCookieName, encTicket)
            cookie.HttpOnly = True
            cookie.Secure = FormsAuthentication.RequireSSL
            If rememberMe Then
                cookie.Expires = ticket.Expiration
            End If
            HttpContext.Current.Response.Cookies.Add(cookie)
            Return True
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Shared Sub SignOut()
        FormsAuthentication.SignOut()
    End Sub

    Public Shared Function GetCurrentUsername() As String
        If HttpContext.Current.User.Identity.IsAuthenticated Then
            Dim parts = HttpContext.Current.User.Identity.Name.Split("|"c)
            If parts.Length > 0 Then Return parts(0)
        End If
        Return String.Empty
    End Function

    Public Shared Function GetCurrentTenantCode() As String
        If HttpContext.Current.User.Identity.IsAuthenticated Then
            Dim parts = HttpContext.Current.User.Identity.Name.Split("|"c)
            If parts.Length > 1 Then Return parts(1)
        End If
        Return TenantResolver.ResolveTenantCode()
    End Function

    Private Shared Function GenerateSalt() As String
        Dim rng As New RNGCryptoServiceProvider()
        Dim saltBytes(15) As Byte
        rng.GetBytes(saltBytes)
        Return Convert.ToBase64String(saltBytes)
    End Function

    Private Shared Function HashPassword(password As String, salt As String) As String
        Dim saltBytes = Convert.FromBase64String(salt)
        Using pbkdf2 As New Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256)
            Dim hash = pbkdf2.GetBytes(32)
            Return "pbkdf2:" & Convert.ToBase64String(hash)
        End Using
    End Function

    Private Shared Function VerifyPassword(password As String, storedHash As String, storedSalt As String) As Boolean
        If storedHash.StartsWith("pbkdf2:") Then
            Dim expectedHash = storedHash.Substring(7)
            Dim saltBytes = Convert.FromBase64String(storedSalt)
            Using pbkdf2 As New Rfc2898DeriveBytes(password, saltBytes, 100000, HashAlgorithmName.SHA256)
                Dim hash = Convert.ToBase64String(pbkdf2.GetBytes(32))
                Return hash = expectedHash
            End Using
        Else
            Dim alg As SHA256 = SHA256.Create()
            Dim combined = password & storedSalt
            Dim bytes = Encoding.UTF8.GetBytes(combined)
            Dim hash = alg.ComputeHash(bytes)
            Return Convert.ToBase64String(hash) = storedHash
        End If
    End Function
End Class
