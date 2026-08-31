<%@ Page Title="Critical-to-Quality (CTQ) Requirements" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Define_CTQ.aspx.vb" Inherits="DMAIC_Define_Define_CTQ" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-bullseye me-2"></i>Critical-to-Quality (CTQ) Requirements</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Define/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Define</a></li>
                    <li class="breadcrumb-item active">CTQ</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />

    <%-- SECTION 1: CTQ Entry Form --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-plus-circle me-2"></i>CTQ Requirement</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">CTQ Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtCTQName" runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvCTQName" runat="server" ControlToValidate="txtCTQName"
                        ErrorMessage="CTQ name is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Category</label>
                    <asp:TextBox ID="txtCategory" runat="server" CssClass="form-control" MaxLength="200" />
                </div>
                <div class="col-12">
                    <label class="form-label">Requirement</label>
                    <asp:TextBox ID="txtRequirement" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>
                <div class="col-12">
                    <label class="form-label">Specification</label>
                    <asp:TextBox ID="txtSpecification" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Target Value</label>
                    <asp:TextBox ID="txtTarget" runat="server" CssClass="form-control" MaxLength="100" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Upper Limit</label>
                    <asp:TextBox ID="txtUpperLimit" runat="server" CssClass="form-control" MaxLength="100" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Lower Limit</label>
                    <asp:TextBox ID="txtLowerLimit" runat="server" CssClass="form-control" MaxLength="100" />
                </div>
                <div class="col-12">
                    <div class="form-check">
                        <asp:CheckBox ID="chkCritical" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label" for="chkCritical">Is Critical</label>
                    </div>
                </div>
            </div>
            <div class="mt-3">
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary"
                    OnClick="btnSave_Click"><i class='bi bi-check-lg'></i> Save</asp:LinkButton>
                <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary ms-2"
                    Text="Cancel" OnClick="btnCancel_Click" Visible="false" />
            </div>
        </div>
    </div>

    <%-- SECTION 2: CTQ Requirements Table --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-table me-2"></i>CTQ Requirements</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litRecordCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>CTQ Name</th>
                            <th>Category</th>
                            <th>Requirement</th>
                            <th>Specification</th>
                            <th>Target</th>
                            <th>Upper</th>
                            <th>Lower</th>
                            <th>Critical</th>
                            <th class="text-center" style="width:90px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptCTQ" runat="server" OnItemCommand="rptCTQ_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("CTQName") %></td>
                                    <td><%# Eval("Category") %></td>
                                    <td><%# Eval("Requirement") %></td>
                                    <td><%# Eval("Specification") %></td>
                                    <td><%# Eval("TargetValue") %></td>
                                    <td><%# Eval("UpperLimit") %></td>
                                    <td><%# Eval("LowerLimit") %></td>
                                    <td class="text-center">
                                        <asp:Literal ID="litCritical" runat="server"
                                            Visible='<%# If(IsDBNull(Eval("IsCritical")), False, CBool(Eval("IsCritical"))) %>'><i class="bi bi-check-lg text-success"></i></asp:Literal>
                                    </td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this CTQ requirement?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- SECTION 3: Navigation --%>
    <div class="d-flex justify-content-between">
        <a href='<%= ResolveUrl("~/DMAIC/Define/Define_VOC.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to VOC
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Define/Define_Charter.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Project Charter <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
