<%@ Application Language="VB" %>
<script RunAt="server">
    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        Dim ex = Server.GetLastError()
        If ex IsNot Nothing Then
            System.Diagnostics.Debug.WriteLine("Global Error: " & ex.Message)
            ' Server.ClearError()
            ' Response.Redirect("~/Error.aspx")
        End If
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
    End Sub
</script>
