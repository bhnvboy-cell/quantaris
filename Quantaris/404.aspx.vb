Imports System.Web

Public Partial Class Error404
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Response.StatusCode = 404
        Response.TrySkipIisCustomErrors = True
    End Sub
End Class
