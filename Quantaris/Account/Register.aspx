<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Register.aspx.vb" Inherits="Account_Register" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Register - Quantaris</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <style>
        body {
            background: linear-gradient(135deg, #0d6efd 0%, #6610f2 100%);
            min-height: 100vh;
            display: flex;
            align-items: center;
        }
        .register-card {
            max-width: 520px;
            width: 100%;
        }
        .tenant-badge {
            position: absolute;
            top: 1rem;
            right: 1rem;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container d-flex justify-content-center">
            <div class="register-card">
                <div class="card shadow-lg border-0 rounded-4 position-relative">
                    <span class="tenant-badge badge bg-light text-dark"><i class="bi bi-building me-1"></i><asp:Literal ID="litTenantCode" runat="server" /></span>
                    <div class="card-body p-5">
                        <div class="text-center mb-4">
                            <i class="bi bi-person-plus text-primary" style="font-size: 3rem;"></i>
                            <h4 class="mt-2 fw-bold">Create Account</h4>
                            <p class="text-muted small">Register for Quantaris</p>
                        </div>

                        <div id="divError" runat="server" Visible="false" class="alert alert-danger" role="alert"></div>

                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="txtFirstName" class="form-label">First Name</label>
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control" placeholder="First name" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFirstName" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Required" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="txtLastName" class="form-label">Last Name</label>
                                <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control" placeholder="Last name" />
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLastName" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Required" />
                            </div>
                        </div>

                        <div class="mb-3">
                            <label for="txtUsername" class="form-label">Username</label>
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-person"></i></span>
                                <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" placeholder="Choose a username" />
                            </div>
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtUsername" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Username is required." />
                        </div>

                        <div class="mb-3">
                            <label for="txtEmail" class="form-label">Email Address</label>
                            <div class="input-group">
                                <span class="input-group-text"><i class="bi bi-envelope"></i></span>
                                <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" TextMode="Email" placeholder="Enter email" />
                            </div>
                            <asp:RequiredFieldValidator runat="server" ControlToValidate="txtEmail" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Email is required." />
                            <asp:RegularExpressionValidator runat="server" ControlToValidate="txtEmail" CssClass="text-danger small" Display="Dynamic" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" ErrorMessage="Invalid email format." />
                        </div>

                        <div class="row">
                            <div class="col-md-6 mb-3">
                                <label for="txtPassword" class="form-label">Password</label>
                                <div class="input-group">
                                    <span class="input-group-text"><i class="bi bi-lock"></i></span>
                                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Password" />
                                </div>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Required" />
                            </div>
                            <div class="col-md-6 mb-3">
                                <label for="txtConfirmPassword" class="form-label">Confirm Password</label>
                                <div class="input-group">
                                    <span class="input-group-text"><i class="bi bi-lock-fill"></i></span>
                                    <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Confirm password" />
                                </div>
                                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtConfirmPassword" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Required" />
                                <asp:CompareValidator runat="server" ControlToCompare="txtPassword" ControlToValidate="txtConfirmPassword" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Passwords do not match." />
                            </div>
                        </div>

                        <asp:Button ID="btnRegister" runat="server" CssClass="btn btn-primary w-100 fw-semibold py-2" Text="Create Account" OnClick="btnRegister_Click" />

                        <div class="text-center mt-3">
                            <small class="text-muted">Already have an account?</small>
                            <a href="Login.aspx?tenant=<%= Request.QueryString("tenant") %>" class="btn btn-link btn-sm p-0 ms-1">Sign in</a>
                        </div>
                    </div>
                </div>
                <p class="text-center text-white-50 small mt-3">&copy; <%= DateTime.Now.Year %> Quantaris v1.0</p>
            </div>
        </div>
    </form>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
