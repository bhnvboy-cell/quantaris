<%@ Page Title="Edit Project" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Edit.aspx.vb" Inherits="Projects_Edit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-pencil me-2"></i>Edit Project</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item active">Edit</li>
                </ol>
            </nav>
        </div>
    </div>

    <asp:Literal ID="litError" runat="server" />

    <div class="card shadow-sm">
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label fw-semibold">Project Code <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtCode" runat="server" CssClass="form-control" MaxLength="50" />
                    <asp:RequiredFieldValidator ID="rfvCode" runat="server" ControlToValidate="txtCode" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Project code is required" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-semibold">Project Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Project name is required" />
                </div>
                <div class="col-12">
                    <label class="form-label fw-semibold">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-semibold">Project Owner</label>
                    <asp:DropDownList ID="ddlOwner" runat="server" CssClass="form-select" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-semibold">Status</label>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Not Started" Value="NotStarted" />
                        <asp:ListItem Text="In Progress" Value="InProgress" />
                        <asp:ListItem Text="Complete" Value="Complete" />
                        <asp:ListItem Text="On Hold" Value="OnHold" />
                    </asp:DropDownList>
                </div>
            </div>

            <hr class="my-4" />
            <div class="d-flex gap-2">
                <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary px-4" Text="Save Changes" OnClick="btnSave_Click" />
                <asp:HyperLink ID="lnkCancel" runat="server" CssClass="btn btn-outline-secondary">Cancel</asp:HyperLink>
            </div>
        </div>
    </div>
</asp:Content>