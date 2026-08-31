<%@ Page Title="Step 7: Identify Performance Gaps" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Analyze_PerformanceGaps.aspx.vb" Inherits="DMAIC_Analyze_Analyze_PerformanceGaps" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-search me-2"></i>Step 7: Identify Performance Gaps</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Analyze/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Analyze</a></li>
                    <li class="breadcrumb-item active">Performance Gaps</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />

    <%-- Card 1: Brainstorming --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-lightbulb me-2"></i>Brainstorming</h6>
        </div>
        <div class="card-body">
            <div class="mb-3">
                <label class="form-label">Free-form brainstorming notes</label>
                <asp:TextBox ID="txtBrainstorming" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"
                    placeholder="Enter any potential causes, ideas, or observations from brainstorming sessions..." />
            </div>
            <asp:LinkButton ID="btnSaveBrainstorming" runat="server" CssClass="btn btn-primary"
                OnClick="btnSaveBrainstorming_Click"><i class='bi bi-save me-1'></i>Save Brainstorming Notes</asp:LinkButton>
        </div>
    </div>

    <%-- Card 2: Fishbone (Ishikawa) Diagram --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-diagram-3 me-2"></i>Fishbone (Ishikawa) Diagram</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litFishboneCount" runat="server" /></span>
        </div>
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-bordered mb-3">
                    <thead class="table-light">
                        <tr>
                            <th style="width:14%;">Machine</th>
                            <th style="width:14%;">Method</th>
                            <th style="width:14%;">Material</th>
                            <th style="width:14%;">Measurement</th>
                            <th style="width:14%;">Environment</th>
                            <th style="width:14%;">Manpower</th>
                            <th style="width:16%;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td><asp:TextBox ID="txtMachine" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="3" placeholder="Causes..." /></td>
                            <td><asp:TextBox ID="txtMethod" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="3" placeholder="Causes..." /></td>
                            <td><asp:TextBox ID="txtMaterial" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="3" placeholder="Causes..." /></td>
                            <td><asp:TextBox ID="txtMeasurement" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="3" placeholder="Causes..." /></td>
                            <td><asp:TextBox ID="txtEnvironment" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="3" placeholder="Causes..." /></td>
                            <td><asp:TextBox ID="txtManpower" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="3" placeholder="Causes..." /></td>
                            <td class="align-middle text-center">
                                <asp:Button ID="btnSaveFishbone" runat="server" CssClass="btn btn-sm btn-primary"
                                    Text="Save" OnClick="btnSaveFishbone_Click" />
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div class="row g-2">
                <div class="col-md-4">
                    <div class="card">
                        <div class="card-header py-1 bg-secondary text-white small fw-semibold">Machine</div>
                        <div class="card-body py-2"><asp:Repeater ID="rptMachine" runat="server"><ItemTemplate><span class="badge bg-dark me-1 mb-1"><%# Eval("Cause") %></span></ItemTemplate></asp:Repeater></div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card">
                        <div class="card-header py-1 bg-secondary text-white small fw-semibold">Method</div>
                        <div class="card-body py-2"><asp:Repeater ID="rptMethod" runat="server"><ItemTemplate><span class="badge bg-dark me-1 mb-1"><%# Eval("Cause") %></span></ItemTemplate></asp:Repeater></div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card">
                        <div class="card-header py-1 bg-secondary text-white small fw-semibold">Material</div>
                        <div class="card-body py-2"><asp:Repeater ID="rptMaterial" runat="server"><ItemTemplate><span class="badge bg-dark me-1 mb-1"><%# Eval("Cause") %></span></ItemTemplate></asp:Repeater></div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card">
                        <div class="card-header py-1 bg-secondary text-white small fw-semibold">Measurement</div>
                        <div class="card-body py-2"><asp:Repeater ID="rptMeasurement" runat="server"><ItemTemplate><span class="badge bg-dark me-1 mb-1"><%# Eval("Cause") %></span></ItemTemplate></asp:Repeater></div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card">
                        <div class="card-header py-1 bg-secondary text-white small fw-semibold">Environment</div>
                        <div class="card-body py-2"><asp:Repeater ID="rptEnvironment" runat="server"><ItemTemplate><span class="badge bg-dark me-1 mb-1"><%# Eval("Cause") %></span></ItemTemplate></asp:Repeater></div>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="card">
                        <div class="card-header py-1 bg-secondary text-white small fw-semibold">Manpower</div>
                        <div class="card-body py-2"><asp:Repeater ID="rptManpower" runat="server"><ItemTemplate><span class="badge bg-dark me-1 mb-1"><%# Eval("Cause") %></span></ItemTemplate></asp:Repeater></div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Card 3: 5-Why Analysis --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-question-circle me-2"></i>5-Why Analysis</h6>
            <div>
                <asp:LinkButton ID="btnAddFiveWhy" runat="server" CssClass="btn btn-sm btn-outline-primary"
                    OnClick="btnAddFiveWhy_Click"><i class='bi bi-plus-lg'></i> New Chain</asp:LinkButton>
                <span class="badge bg-secondary ms-2"><asp:Literal ID="litFiveWhyCount" runat="server" /></span>
            </div>
        </div>
        <div class="card-body p-0">
            <asp:Repeater ID="rptFiveWhy" runat="server" OnItemCommand="rptFiveWhy_ItemCommand">
                <ItemTemplate>
                    <div class="border-bottom p-3">
                        <div class="d-flex justify-content-between align-items-start mb-2">
                            <h6 class="mb-0 fw-semibold">5-Why Chain #<%# Container.ItemIndex + 1 %></h6>
                            <asp:LinkButton ID="btnDeleteFiveWhy" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                OnClientClick="return confirm('Delete this 5-Why chain?');"><i class="bi bi-trash"></i></asp:LinkButton>
                        </div>
                        <div class="row g-2">
                            <div class="col-md-6">
                                <label class="form-label small">Problem Statement</label>
                                <asp:TextBox ID="txtFWProblem" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2"
                                    Text='<%# Eval("Cause") %>' />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label small">Root Cause</label>
                                <asp:TextBox ID="txtFWRootCause" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2"
                                    Text='<%# Eval("Effect") %>' />
                            </div>
                        </div>
                        <div class="row g-1 mt-2">
                            <div class="col"><label class="form-label small">Why 1</label><asp:TextBox ID="txtWhy1" runat="server" CssClass="form-control form-control-sm" Text='<%# GetWhy(Eval("FiveWhy_Chain"), 0) %>' /></div>
                            <div class="col"><label class="form-label small">Why 2</label><asp:TextBox ID="txtWhy2" runat="server" CssClass="form-control form-control-sm" Text='<%# GetWhy(Eval("FiveWhy_Chain"), 1) %>' /></div>
                            <div class="col"><label class="form-label small">Why 3</label><asp:TextBox ID="txtWhy3" runat="server" CssClass="form-control form-control-sm" Text='<%# GetWhy(Eval("FiveWhy_Chain"), 2) %>' /></div>
                            <div class="col"><label class="form-label small">Why 4</label><asp:TextBox ID="txtWhy4" runat="server" CssClass="form-control form-control-sm" Text='<%# GetWhy(Eval("FiveWhy_Chain"), 3) %>' /></div>
                            <div class="col"><label class="form-label small">Why 5</label><asp:TextBox ID="txtWhy5" runat="server" CssClass="form-control form-control-sm" Text='<%# GetWhy(Eval("FiveWhy_Chain"), 4) %>' /></div>
                        </div>
                        <div class="mt-2">
                            <asp:LinkButton ID="btnUpdateFiveWhy" runat="server" CssClass="btn btn-sm btn-primary"
                                CommandName="Update" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-check-lg"></i> Update</asp:LinkButton>
                        </div>
                    </div>
                </ItemTemplate>
                
            </asp:Repeater>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between mt-4">
        <a href='<%= ResolveUrl("~/DMAIC/Analyze/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Analyze
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Analyze/Analyze_RootCausePrioritization.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Root Cause Prioritization <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
