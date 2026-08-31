<%@ Page Language="VB" AutoEventWireup="false" CodeFile="AccessDenied.aspx.vb" Inherits="Account_AccessDenied" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Access Denied - Quantaris</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <style>
        body { background-color: #f8f9fa; display: flex; align-items: center; justify-content: center; min-height: 100vh; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="row justify-content-center">
                <div class="col-md-6 text-center">
                    <div class="card border-0 shadow-sm">
                        <div class="card-body p-5">
                            <i class="bi bi-shield-exclamation text-danger" style="font-size: 4rem;"></i>
                            <h2 class="mt-3 fw-bold text-danger">Access Denied</h2>
                            <p class="text-muted mb-4">You do not have permission to access this page.</p>
                            <a href='<%= ResolveUrl("~/Default.aspx?tenant=" & Request.QueryString("tenant")) %>' class="btn btn-primary">
                                <i class="bi bi-house me-1"></i>Return to Dashboard
                            </a>
                        </div>
                    </div>
                    <p class="text-muted small mt-3">&copy; <%= DateTime.Now.Year %> Quantaris v1.0</p>
                </div>
            </div>
        </div>
    </form>
</body>
</html>
