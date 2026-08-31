<%@ Page Title="Step 14: Replicate and Share Best Practices" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Control_BestPractices.aspx.vb" Inherits="DMAIC_Control_Control_BestPractices" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-info me-2">14</span>Replicate and Share Best Practices</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Control/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Control</a></li>
                    <li class="breadcrumb-item active">Best Practices</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />

    <%-- Card 1: Best Practice Entry --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-star me-2"></i>Best Practice Entry</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-8">
                    <label class="form-label">Title <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle"
                        ErrorMessage="Title is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-4 d-flex align-items-center">
                    <div class="form-check mt-4">
                        <asp:CheckBox ID="chkIsReplicated" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label">Is Replicated</label>
                    </div>
                </div>
                <div class="col-12">
                    <label class="form-label">Description <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                    <asp:RequiredFieldValidator ID="rfvDescription" runat="server" ControlToValidate="txtDescription"
                        ErrorMessage="Description is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Applicable Areas</label>
                    <asp:TextBox ID="txtApplicableAreas" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"
                        placeholder="Where else this could apply" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Sites</label>
                    <asp:TextBox ID="txtSites" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"
                        placeholder="Which sites" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Teams</label>
                    <asp:TextBox ID="txtTeams" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"
                        placeholder="Which teams" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Replication Date</label>
                    <asp:TextBox ID="txtReplicationDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
            </div>
            <div class="mt-3">
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary"
                    OnClick="btnSave_Click"><i class='bi bi-save me-1'></i>Save Best Practice</asp:LinkButton>
                <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary ms-2"
                    Text="Cancel" OnClick="btnCancel_Click" Visible="false" />
            </div>
        </div>
    </div>

    <%-- Card 2: Best Practices List --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-list-star me-2"></i>Best Practices List</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Title</th>
                            <th>Description</th>
                            <th>Areas</th>
                            <th>Sites</th>
                            <th>Replicated</th>
                            <th class="text-center" style="width:120px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptBestPractices" runat="server" OnItemCommand="rptBestPractices_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-semibold"><%# Eval("Title") %></td>
                                    <td><%# Truncate(Eval("Description"), 80) %></td>
                                    <td><%# Truncate(Eval("ApplicableAreas"), 50) %></td>
                                    <td><%# Truncate(Eval("Sites"), 50) %></td>
                                    <td>
                                        <asp:LinkButton ID="btnToggleReplicated" runat="server" CssClass='<%# IIf(If(IsDBNull(Eval("IsReplicated")), False, CBool(Eval("IsReplicated"))), "badge bg-success text-decoration-none", "badge bg-secondary text-decoration-none") %>'
                                            CommandName="ToggleReplicate" CommandArgument='<%# Eval("Id") %>'>
                                            <%# IIf(If(IsDBNull(Eval("IsReplicated")), False, CBool(Eval("IsReplicated"))), "Replicated", "Not Replicated") %>
                                        </asp:LinkButton>
                                    </td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this best practice?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- Card 3: Across Tenant Sharing --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-share me-2"></i>Across Tenant Sharing</h6>
        </div>
        <div class="card-body">
            <p class="text-muted mb-2">
                Share this project's best practices across your organization. When you mark a best practice as replicated,
                it becomes visible to other teams and sites within your tenant.
            </p>
            <div class="alert alert-info d-flex align-items-center mb-0" role="alert">
                <i class="bi bi-info-circle me-2 fs-5"></i>
                <div>
                    <strong>Organization-wide sharing:</strong> Best practices marked as replicated are included in the
                    organizational knowledge base. Other project teams can view and adopt these practices.
                    <a href='<%= ResolveUrl("~/KnowledgeBase.aspx?tenant=" & CurrentTenantCode) %>' class="alert-link ms-1">View Knowledge Base</a>
                </div>
            </div>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between mt-4 mb-4">
        <a href='<%= ResolveUrl("~/DMAIC/Control/Control_Institutionalize.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back: Institutionalize
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Control/Control_Celebrate.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Celebrate Success <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
