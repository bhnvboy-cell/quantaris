<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Setup.aspx.vb" Inherits="Setup" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Quantaris - Setup</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body class="bg-light">
    <form id="form1" runat="server">
        <div class="container py-5">
            <div class="card shadow-sm mx-auto" style="max-width:600px;">
                <div class="card-body text-center">
                    <h4 class="mb-4">System Setup</h4>
                    <asp:Label ID="litResult" runat="server" CssClass="d-block mb-3" />
                    <asp:Button ID="btnSetup" runat="server" Text="Run Setup" CssClass="btn btn-primary" OnClick="btnSetup_Click" />
                    <a href="Account/Login.aspx?tenant=DEMO" class="btn btn-outline-secondary ms-2">Go to Login</a>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
