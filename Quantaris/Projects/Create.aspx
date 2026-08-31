<%@ Page Title="Create Project" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Create.aspx.vb" Inherits="Projects_Create" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-plus-circle me-2"></i>Create New Project</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item active">Create</li>
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
                    <asp:TextBox ID="txtCode" runat="server" CssClass="form-control" placeholder="e.g. SIX-2025-001" MaxLength="50" />
                    <asp:RequiredFieldValidator ID="rfvCode" runat="server" ControlToValidate="txtCode" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Project code is required" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-semibold">Project Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtName" runat="server" CssClass="form-control" placeholder="e.g. Reduce Defects in Assembly Line" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName" CssClass="text-danger small" Display="Dynamic" ErrorMessage="Project name is required" />
                </div>
                <div class="col-12">
                    <label class="form-label fw-semibold">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" placeholder="Brief description of the project scope and objectives" />
                </div>
                <div class="col-md-4">
                    <label class="form-label fw-semibold">Priority</label>
                    <asp:DropDownList ID="ddlPriority" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Medium" Value="Medium" Selected="True" />
                        <asp:ListItem Text="Low" Value="Low" />
                        <asp:ListItem Text="High" Value="High" />
                        <asp:ListItem Text="Critical" Value="Critical" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-4">
                    <label class="form-label fw-semibold">Start Date</label>
                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-4">
                    <label class="form-label fw-semibold">Target End Date</label>
                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-6">
                    <label class="form-label fw-semibold">Project Owner</label>
                    <asp:DropDownList ID="ddlOwner" runat="server" CssClass="form-select" />
                </div>
            </div>

            <hr class="my-4" />
            <div class="d-flex gap-2">
                <asp:Button ID="btnSave" runat="server" CssClass="btn btn-primary px-4" Text="Save Project" OnClick="btnSave_Click" />
                <a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">Cancel</a>
            </div>
        </div>
    </div>
</asp:Content>
