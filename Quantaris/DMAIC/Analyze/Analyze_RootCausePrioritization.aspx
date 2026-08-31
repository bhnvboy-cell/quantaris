<%@ Page Title="Step 8: Root Cause Prioritization" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Analyze_RootCausePrioritization.aspx.vb" Inherits="DMAIC_Analyze_Analyze_RootCausePrioritization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-bar-chart-steps me-2"></i>Step 8: Ascertain Critical Root Causes</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Analyze/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Analyze</a></li>
                    <li class="breadcrumb-item active">Root Cause Prioritization</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />

    <%-- Card 1: Pareto Chart Data --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-bar-chart-fill me-2"></i>Pareto Chart Data</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litParetoCount" runat="server" /></span>
        </div>
        <div class="card-body">
            <div class="row g-2 align-items-end mb-3">
                <div class="col-md-5">
                    <label class="form-label">Cause</label>
                    <asp:TextBox ID="txtParetoCause" runat="server" CssClass="form-control" MaxLength="200" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Frequency</label>
                    <asp:TextBox ID="txtParetoFrequency" runat="server" CssClass="form-control" TextMode="Number" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">%</label>
                    <asp:TextBox ID="txtParetoPercent" runat="server" CssClass="form-control" TextMode="Number" step="0.01" placeholder="0.00" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">&nbsp;</label>
                    <asp:LinkButton ID="btnAddPareto" runat="server" CssClass="btn btn-primary d-block w-100"
                        OnClick="btnAddPareto_Click"><i class='bi bi-plus-lg'></i></asp:LinkButton>
                </div>
            </div>
            <div class="table-responsive">
                <table class="table table-hover table-striped table-sm mb-0">
                    <thead class="table-light">
                        <tr>
                            <th>Cause</th>
                            <th class="text-end">Frequency</th>
                            <th class="text-end">%</th>
                            <th class="text-end">Cumulative %</th>
                            <th class="text-center" style="width:60px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptPareto" runat="server" OnItemCommand="rptPareto_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("Cause") %></td>
                                    <td class="text-end"><%# Eval("Frequency") %></td>
                                    <td class="text-end"><%# Eval("Percentage", "{0:N2}") %></td>
                                    <td class="text-end"><%# Eval("CumulativePercent", "{0:N2}") %></td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnDeletePareto" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Remove this cause?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- Card 2: Multi-Voting Results --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-hand-thumbs-up me-2"></i>Multi-Voting Results</h6>
        </div>
        <div class="card-body">
            <div class="mb-3">
                <label class="form-label">Voting results (one entry per line: Cause - Votes)</label>
                <asp:TextBox ID="txtMultiVoting" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="6"
                    placeholder="Cause A - 5&#10;Cause B - 3&#10;Cause C - 1" />
            </div>
            <asp:LinkButton ID="btnSaveMultiVoting" runat="server" CssClass="btn btn-primary"
                OnClick="btnSaveMultiVoting_Click"><i class='bi bi-save me-1'></i>Save Multi-Voting</asp:LinkButton>
        </div>
    </div>

    <%-- Card 3: Control-Impact Matrix --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-grid-3x3-gap-fill me-2"></i>Control-Impact Matrix</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litMatrixCount" runat="server" /></span>
        </div>
        <div class="card-body">
            <div class="row g-2 align-items-end mb-3">
                <div class="col-md-4">
                    <label class="form-label">Root Cause</label>
                    <asp:TextBox ID="txtMatrixCause" runat="server" CssClass="form-control" MaxLength="200" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Control</label>
                    <asp:DropDownList ID="ddlMatrixControl" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Low" Value="Low" />
                        <asp:ListItem Text="Medium" Value="Medium" />
                        <asp:ListItem Text="High" Value="High" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Impact</label>
                    <asp:DropDownList ID="ddlMatrixImpact" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Low" Value="Low" />
                        <asp:ListItem Text="Medium" Value="Medium" />
                        <asp:ListItem Text="High" Value="High" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Priority</label>
                    <asp:TextBox ID="txtMatrixPriority" runat="server" CssClass="form-control" TextMode="Number" placeholder="1-10" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">&nbsp;</label>
                    <asp:LinkButton ID="btnAddMatrix" runat="server" CssClass="btn btn-primary d-block w-100"
                        OnClick="btnAddMatrix_Click"><i class='bi bi-plus-lg'></i></asp:LinkButton>
                </div>
            </div>
            <div class="table-responsive">
                <table class="table table-hover table-striped table-sm mb-0">
                    <thead class="table-light">
                        <tr>
                            <th>Root Cause</th>
                            <th>Control</th>
                            <th>Impact</th>
                            <th class="text-end">Priority Score</th>
                            <th class="text-center" style="width:60px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptMatrix" runat="server" OnItemCommand="rptMatrix_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("Cause") %></td>
                                    <td><span class="badge bg-<%# GetControlBadge(Eval("Control")) %>"><%# Eval("Control") %></span></td>
                                    <td><span class="badge bg-<%# GetImpactBadge(Eval("Impact")) %>"><%# Eval("Impact") %></span></td>
                                    <td class="text-end"><%# Eval("PriorityScore") %></td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnDeleteMatrix" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Remove this matrix entry?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between mt-4">
        <a href='<%= ResolveUrl("~/DMAIC/Analyze/Analyze_PerformanceGaps.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Performance Gaps
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Analyze/Analyze_StatValidation.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Validate Root Causes <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
