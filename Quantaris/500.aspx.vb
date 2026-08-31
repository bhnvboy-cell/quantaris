Imports System.Web

Public Partial Class Error500
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        Response.StatusCode = 500
        Response.TrySkipIisCustomErrors = True
    End Sub
End Class
