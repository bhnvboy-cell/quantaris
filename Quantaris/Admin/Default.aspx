<%@ Page Title="Admin Dashboard" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="Admin_Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-gear me-2"></i>Admin Dashboard</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Admin</li>
                </ol>
            </nav>
        </div>
    </div>

    <asp:Literal ID="litMessage" runat="server" />

    <div class="row g-4">
        <div class="col-lg-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-building me-2"></i>Tenant Details</h6>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Tenant Code</label>
                            <asp:TextBox ID="txtTenantCode" runat="server" CssClass="form-control form-control-sm" Enabled="false" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Plan Type</label>
                            <asp:TextBox ID="txtPlanType" runat="server" CssClass="form-control form-control-sm" Enabled="false" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Tenant Name</label>
                            <asp:TextBox ID="txtTenantName" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small fw-semibold">Max Projects</label>
                            <asp:TextBox ID="txtMaxProjects" runat="server" CssClass="form-control form-control-sm" Enabled="false" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small fw-semibold">Max Users</label>
                            <asp:TextBox ID="txtMaxUsers" runat="server" CssClass="form-control form-control-sm" Enabled="false" />
                        </div>
                        <div class="col-12">
                            <div class="form-check">
                                <asp:CheckBox ID="chkActive" runat="server" CssClass="form-check-input" />
                                <label class="form-check-label small fw-semibold" for="chkActive">Active</label>
                            </div>
                        </div>
                        <div class="col-12">
                            <asp:LinkButton ID="btnSaveTenant" runat="server" CssClass="btn btn-primary btn-sm">
                                <i class="bi bi-floppy me-1"></i>Save Tenant
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-person-plus me-2"></i>Add New User</h6>
                </div>
                <div class="card-body">
                    <asp:Literal ID="litUserMessage" runat="server" />
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Username</label>
                            <asp:TextBox ID="txtNewUsername" runat="server" CssClass="form-control form-control-sm" />
                            <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtNewUsername" Display="Dynamic" CssClass="text-danger small" ErrorMessage="Required" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Email</label>
                            <asp:TextBox ID="txtNewEmail" runat="server" CssClass="form-control form-control-sm" />
                            <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtNewEmail" Display="Dynamic" CssClass="text-danger small" ErrorMessage="Required" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Password</label>
                            <asp:TextBox ID="txtNewPassword" runat="server" TextMode="Password" CssClass="form-control form-control-sm" />
                            <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtNewPassword" Display="Dynamic" CssClass="text-danger small" ErrorMessage="Required" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Role</label>
                            <asp:DropDownList ID="ddlNewRole" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="User" Value="User" />
                                <asp:ListItem Text="GreenBelt" Value="GreenBelt" />
                                <asp:ListItem Text="BlackBelt" Value="BlackBelt" />
                                <asp:ListItem Text="Admin" Value="Admin" />
                                <asp:ListItem Text="Owner" Value="Owner" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">First Name</label>
                            <asp:TextBox ID="txtNewFirstName" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Last Name</label>
                            <asp:TextBox ID="txtNewLastName" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="col-12">
                            <asp:LinkButton ID="btnAddUser" runat="server" CssClass="btn btn-success btn-sm">
                                <i class="bi bi-person-plus me-1"></i>Create User
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="card shadow-sm mt-4">
        <div class="card-header bg-dark text-white">
            <h6 class="mb-0"><i class="bi bi-people me-2"></i>Users Management</h6>
        </div>
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-hover table-bordered align-middle mb-0 small">
                    <thead class="table-dark">
                        <tr>
                            <th>Username</th>
                            <th>Email</th>
                            <th>Name</th>
                            <th>Role</th>
                            <th>Last Login</th>
                            <th>Active</th>
                            <th class="text-center">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptUsers" runat="server" OnItemCommand="rptUsers_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-semibold"><%# Eval("Username") %></td>
                                    <td><%# Eval("Email") %></td>
                                    <td><%# Eval("FirstName") & " " & Eval("LastName") %></td>
                                    <td>
                                        <asp:DropDownList ID="ddlRoleEdit" runat="server" CssClass="form-select form-select-sm" SelectedValue='<%# Eval("Role") %>' AutoPostBack="false">
                                            <asp:ListItem Text="User" Value="User" />
                                            <asp:ListItem Text="GreenBelt" Value="GreenBelt" />
                                            <asp:ListItem Text="BlackBelt" Value="BlackBelt" />
                                            <asp:ListItem Text="Admin" Value="Admin" />
                                            <asp:ListItem Text="Owner" Value="Owner" />
                                        </asp:DropDownList>
                                    </td>
                                    <td><%# If(Eval("LastLoginOn") IsNot DBNull.Value, CDate(Eval("LastLoginOn")).ToString("MMM dd, yyyy HH:mm"), "-") %></td>
                                    <td>
                                        <span class="badge <%# If(CBool(Eval("IsActive")), "bg-success", "bg-secondary") %>">
                                            <%# If(CBool(Eval("IsActive")), "Active", "Inactive") %>
                                        </span>
                                    </td>
                                    <td class="text-center" style="min-width: 160px;">
                                        <asp:LinkButton ID="btnToggleActive" runat="server" CssClass='btn btn-sm <%# If(CBool(Eval("IsActive")), "btn-warning", "btn-success") %> me-1'
                                            CommandName="ToggleActive" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Toggle active status for this user?');">
                                            <i class="bi <%# If(CBool(Eval("IsActive")), "bi-pause-circle", "bi-play-circle") %>"></i>
                                            <%# If(CBool(Eval("IsActive")), "Deactivate", "Activate") %>
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnUpdateRole" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="UpdateRole" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Update role for this user?');">
                                            <i class="bi bi-arrow-up-circle"></i> Role
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnDeleteUser" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="DeleteUser" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Permanently delete this user?');">
                                            <i class="bi bi-trash"></i>
                                        </asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</asp:Content>
