<%@ Page Title="Project Charter" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Define_Charter.aspx.vb" Inherits="DMAIC_Define_Define_Charter" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-file-earmark-text me-2"></i>Project Charter</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Define/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Define</a></li>
                    <li class="breadcrumb-item active">Charter</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfCharterId" runat="server" Value="0" />

    <div class="row g-4">
        <%-- Card 1: Problem & Goal --%>
        <div class="col-md-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-exclamation-triangle me-2"></i>Problem &amp; Goal</h6>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label">Problem Statement</label>
                        <asp:TextBox ID="txtProblemStatement" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
                    </div>
                    <div class="mb-0">
                        <label class="form-label">Goal Statement</label>
                        <asp:TextBox ID="txtGoalStatement" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
                    </div>
                </div>
            </div>
        </div>

        <%-- Card 2: Business Case & Elevator Speech --%>
        <div class="col-md-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-briefcase me-2"></i>Business Case</h6>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label">Business Case</label>
                        <asp:TextBox ID="txtBusinessCase" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
                    </div>
                    <div class="mb-0">
                        <label class="form-label">Elevator Speech</label>
                        <asp:TextBox ID="txtElevatorSpeech" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
                    </div>
                </div>
            </div>
        </div>

        <%-- Card 3: Scope --%>
        <div class="col-md-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-bounding-box-circles me-2"></i>Scope</h6>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label">Included</label>
                        <asp:TextBox ID="txtScopeIncluded" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
                    </div>
                    <div class="mb-0">
                        <label class="form-label">Excluded</label>
                        <asp:TextBox ID="txtScopeExcluded" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4" />
                    </div>
                </div>
            </div>
        </div>

        <%-- Card 4: Timeline --%>
        <div class="col-md-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-calendar-event me-2"></i>Timeline</h6>
                </div>
                <div class="card-body">
                    <div class="mb-0">
                        <label class="form-label">Timeline Details</label>
                        <asp:TextBox ID="txtTimeline" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="10"
                            placeholder="Enter key milestones, start/end dates, phase durations..." />
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Card 5: Stakeholders --%>
    <div class="card shadow-sm mt-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-people me-2"></i>Stakeholders</h6>
        </div>
        <div class="card-body">
            <div class="row g-3 align-items-end mb-3">
                <div class="col-md-3">
                    <label class="form-label">Name</label>
                    <asp:TextBox ID="txtStakeholderName" runat="server" CssClass="form-control" MaxLength="200" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Role</label>
                    <asp:TextBox ID="txtStakeholderRole" runat="server" CssClass="form-control" MaxLength="200" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Interest</label>
                    <asp:DropDownList ID="ddlInterest" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Select --" Value="" />
                        <asp:ListItem Text="High" Value="High" />
                        <asp:ListItem Text="Medium" Value="Medium" />
                        <asp:ListItem Text="Low" Value="Low" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Influence</label>
                    <asp:DropDownList ID="ddlInfluence" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Select --" Value="" />
                        <asp:ListItem Text="High" Value="High" />
                        <asp:ListItem Text="Medium" Value="Medium" />
                        <asp:ListItem Text="Low" Value="Low" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-1">
                    <label class="form-label">ARMI</label>
                    <asp:DropDownList ID="ddlARMI" runat="server" CssClass="form-select">
                        <asp:ListItem Text="" Value="" />
                        <asp:ListItem Text="A" Value="A" />
                        <asp:ListItem Text="R" Value="R" />
                        <asp:ListItem Text="M" Value="M" />
                        <asp:ListItem Text="I" Value="I" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-1">
                    <label class="form-label">RACI</label>
                    <asp:DropDownList ID="ddlRACI" runat="server" CssClass="form-select">
                        <asp:ListItem Text="" Value="" />
                        <asp:ListItem Text="R" Value="R" />
                        <asp:ListItem Text="A" Value="A" />
                        <asp:ListItem Text="C" Value="C" />
                        <asp:ListItem Text="I" Value="I" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-1">
                    <label class="form-label">&nbsp;</label>
                    <asp:LinkButton ID="btnAddStakeholder" runat="server" CssClass="btn btn-primary d-block"
                        OnClick="btnAddStakeholder_Click"><i class="bi bi-plus-lg"></i></asp:LinkButton>
                </div>
            </div>

            <asp:HiddenField ID="hfStakeholderEditId" runat="server" Value="0" />

            <div class="table-responsive">
                <table class="table table-hover table-striped table-sm mb-0">
                    <thead class="table-light">
                        <tr>
                            <th>Name</th>
                            <th>Role</th>
                            <th>Interest</th>
                            <th>Influence</th>
                            <th>ARMI</th>
                            <th>RACI</th>
                            <th class="text-center" style="width:80px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptStakeholders" runat="server" OnItemCommand="rptStakeholders_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("Name") %></td>
                                    <td><%# Eval("Role") %></td>
                                    <td><span class="badge bg-info"><%# Eval("Interest") %></span></td>
                                    <td><span class="badge bg-warning text-dark"><%# Eval("Influence") %></span></td>
                                    <td><%# Eval("ARMI_Type") %></td>
                                    <td><%# Eval("RACI_Type") %></td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEditSH" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDeleteSH" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Remove this stakeholder?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- Save & Navigation --%>
    <div class="d-flex justify-content-between mt-4">
        <a href='<%= ResolveUrl("~/DMAIC/Define/Define_CTQ.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to CTQ
        </a>
        <div>
            <asp:LinkButton ID="btnSaveCharter" runat="server" CssClass="btn btn-primary me-2"
                OnClick="btnSaveCharter_Click"><i class='bi bi-save me-1'></i>Save Charter</asp:LinkButton>
            <a href='<%= ResolveUrl("~/DMAIC/Define/Define_SIPOC.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
                Next: SIPOC <i class="bi bi-arrow-right ms-1"></i>
            </a>
        </div>
    </div>
</asp:Content>
