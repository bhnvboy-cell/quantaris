<%@ Page Title="DMAIC Dashboard" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Dashboard.aspx.vb" Inherits="DMAIC_Dashboard" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ox-page-header">
        <div>
            <h1><i class="bi bi-kanban me-2" style="color:var(--ox-accent)"></i><asp:Literal ID="litProjectName" runat="server" /></h1>
            <div class="ox-breadcrumb">
                <a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a> /
                <a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a> /
                <asp:Literal ID="litBreadcrumbProject" runat="server" />
            </div>
        </div>
        <span class="ox-badge ox-badge-success" style="font-size:12px;padding:4px 12px;">
            <asp:Literal ID="litProjectStatus" runat="server" />
        </span>
    </div>

    <!-- DMAIC Progress -->
    <div class="ox-card mb-4">
        <div class="ox-card-header">
            <span class="ox-card-title"><i class="bi bi-kanban me-2"></i>DMAIC Overall Progress</span>
            <span class="ox-badge ox-badge-success"><asp:Literal ID="litProgress" runat="server" /></span>
        </div>
        <div class="ox-progress" style="height:8px;">
            <asp:Panel ID="progressBar" runat="server" CssClass="ox-progress-bar" role="progressbar"
                aria-valuemin="0" aria-valuemax="100">
            </asp:Panel>
        </div>
    </div>

    <div class="row g-3" id="dmaicCards">

        <%-- DEFINE --%>
        <div class="col-lg-4 col-md-6">
            <div class="ox-card h-100" style="border-left:3px solid #4a9eff;">
                <div class="ox-card-header">
                    <div class="d-flex align-items-center gap-2">
                        <div class="ox-kpi-icon blue" style="width:28px;height:28px;font-size:12px;">1</div>
                        <div>
                            <span class="ox-card-title" style="color:#4a9eff;">Define</span>
                            <div style="font-size:10px;color:var(--ox-text-muted);">Understand Customer &amp; Business</div>
                        </div>
                    </div>
                </div>
                <div class="p-0">
                    <asp:Repeater ID="rptDefine" runat="server">
                        <ItemTemplate>
                            <a href='<%# ResolveUrl(GetStepLink(CInt(Eval("StepNumber")))) %>' class="d-flex align-items-center px-3 py-2" style="border-bottom:1px solid var(--ox-border);color:var(--ox-text-primary);text-decoration:none;transition:background 0.15s;" onmouseover="this.style.background='rgba(255,255,255,0.03)'" onmouseout="this.style.background='transparent'">
                                <span class="d-flex align-items-center justify-content-center me-2 rounded-circle" style="width:24px;height:24px;font-size:11px;font-weight:600;flex-shrink:0;
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "background:rgba(0,212,170,0.15);color:#00d4aa;",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "background:rgba(255,140,66,0.15);color:#ff8c42;",
                                             "background:var(--ox-bg-input);color:var(--ox-text-muted);")) %>">
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "<i class='bi bi-check-lg'></i>",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "<i class='bi bi-play-fill'></i>",
                                             Eval("StepNumber").ToString())) %>
                                </span>
                                <span class="flex-grow-1" style="font-size:12px;font-weight:500;"><%# Eval("StepName") %></span>
                                <span class="ox-badge <%# Eval("StatusClass") %>" style="font-size:9px;"><%# Eval("Status") %></span>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <%-- MEASURE --%>
        <div class="col-lg-4 col-md-6">
            <div class="ox-card h-100" style="border-left:3px solid #a78bfa;">
                <div class="ox-card-header">
                    <div class="d-flex align-items-center gap-2">
                        <div class="ox-kpi-icon purple" style="width:28px;height:28px;font-size:12px;">2</div>
                        <div>
                            <span class="ox-card-title" style="color:#a78bfa;">Measure</span>
                            <div style="font-size:10px;color:var(--ox-text-muted);">Data Collection &amp; Baseline</div>
                        </div>
                    </div>
                </div>
                <div class="p-0">
                    <asp:Repeater ID="rptMeasure" runat="server">
                        <ItemTemplate>
                            <a href='<%# ResolveUrl(GetStepLink(CInt(Eval("StepNumber")))) %>' class="d-flex align-items-center px-3 py-2" style="border-bottom:1px solid var(--ox-border);color:var(--ox-text-primary);text-decoration:none;transition:background 0.15s;" onmouseover="this.style.background='rgba(255,255,255,0.03)'" onmouseout="this.style.background='transparent'">
                                <span class="d-flex align-items-center justify-content-center me-2 rounded-circle" style="width:24px;height:24px;font-size:11px;font-weight:600;flex-shrink:0;
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "background:rgba(0,212,170,0.15);color:#00d4aa;",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "background:rgba(255,140,66,0.15);color:#ff8c42;",
                                             "background:var(--ox-bg-input);color:var(--ox-text-muted);")) %>">
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "<i class='bi bi-check-lg'></i>",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "<i class='bi bi-play-fill'></i>",
                                             Eval("StepNumber").ToString())) %>
                                </span>
                                <span class="flex-grow-1" style="font-size:12px;font-weight:500;"><%# Eval("StepName") %></span>
                                <span class="ox-badge <%# Eval("StatusClass") %>" style="font-size:9px;"><%# Eval("Status") %></span>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <%-- ANALYZE --%>
        <div class="col-lg-4 col-md-6">
            <div class="ox-card h-100" style="border-left:3px solid #ff8c42;">
                <div class="ox-card-header">
                    <div class="d-flex align-items-center gap-2">
                        <div class="ox-kpi-icon orange" style="width:28px;height:28px;font-size:12px;">3</div>
                        <div>
                            <span class="ox-card-title" style="color:#ff8c42;">Analyze</span>
                            <div style="font-size:10px;color:var(--ox-text-muted);">Root Cause Analysis</div>
                        </div>
                    </div>
                </div>
                <div class="p-0">
                    <asp:Repeater ID="rptAnalyze" runat="server">
                        <ItemTemplate>
                            <a href='<%# ResolveUrl(GetStepLink(CInt(Eval("StepNumber")))) %>' class="d-flex align-items-center px-3 py-2" style="border-bottom:1px solid var(--ox-border);color:var(--ox-text-primary);text-decoration:none;transition:background 0.15s;" onmouseover="this.style.background='rgba(255,255,255,0.03)'" onmouseout="this.style.background='transparent'">
                                <span class="d-flex align-items-center justify-content-center me-2 rounded-circle" style="width:24px;height:24px;font-size:11px;font-weight:600;flex-shrink:0;
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "background:rgba(0,212,170,0.15);color:#00d4aa;",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "background:rgba(255,140,66,0.15);color:#ff8c42;",
                                             "background:var(--ox-bg-input);color:var(--ox-text-muted);")) %>">
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "<i class='bi bi-check-lg'></i>",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "<i class='bi bi-play-fill'></i>",
                                             Eval("StepNumber").ToString())) %>
                                </span>
                                <span class="flex-grow-1" style="font-size:12px;font-weight:500;"><%# Eval("StepName") %></span>
                                <span class="ox-badge <%# Eval("StatusClass") %>" style="font-size:9px;"><%# Eval("Status") %></span>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <%-- IMPROVE --%>
        <div class="col-lg-4 col-md-6">
            <div class="ox-card h-100" style="border-left:3px solid #00d4aa;">
                <div class="ox-card-header">
                    <div class="d-flex align-items-center gap-2">
                        <div class="ox-kpi-icon teal" style="width:28px;height:28px;font-size:12px;">4</div>
                        <div>
                            <span class="ox-card-title" style="color:#00d4aa;">Improve</span>
                            <div style="font-size:10px;color:var(--ox-text-muted);">Solutions &amp; Validation</div>
                        </div>
                    </div>
                </div>
                <div class="p-0">
                    <asp:Repeater ID="rptImprove" runat="server">
                        <ItemTemplate>
                            <a href='<%# ResolveUrl(GetStepLink(CInt(Eval("StepNumber")))) %>' class="d-flex align-items-center px-3 py-2" style="border-bottom:1px solid var(--ox-border);color:var(--ox-text-primary);text-decoration:none;transition:background 0.15s;" onmouseover="this.style.background='rgba(255,255,255,0.03)'" onmouseout="this.style.background='transparent'">
                                <span class="d-flex align-items-center justify-content-center me-2 rounded-circle" style="width:24px;height:24px;font-size:11px;font-weight:600;flex-shrink:0;
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "background:rgba(0,212,170,0.15);color:#00d4aa;",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "background:rgba(255,140,66,0.15);color:#ff8c42;",
                                             "background:var(--ox-bg-input);color:var(--ox-text-muted);")) %>">
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "<i class='bi bi-check-lg'></i>",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "<i class='bi bi-play-fill'></i>",
                                             Eval("StepNumber").ToString())) %>
                                </span>
                                <span class="flex-grow-1" style="font-size:12px;font-weight:500;"><%# Eval("StepName") %></span>
                                <span class="ox-badge <%# Eval("StatusClass") %>" style="font-size:9px;"><%# Eval("Status") %></span>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

        <%-- CONTROL --%>
        <div class="col-lg-4 col-md-6">
            <div class="ox-card h-100" style="border-left:3px solid #a78bfa;">
                <div class="ox-card-header">
                    <div class="d-flex align-items-center gap-2">
                        <div class="ox-kpi-icon purple" style="width:28px;height:28px;font-size:12px;">5</div>
                        <div>
                            <span class="ox-card-title" style="color:#a78bfa;">Control</span>
                            <div style="font-size:10px;color:var(--ox-text-muted);">Sustain &amp; Celebrate</div>
                        </div>
                    </div>
                </div>
                <div class="p-0">
                    <asp:Repeater ID="rptControl" runat="server">
                        <ItemTemplate>
                            <a href='<%# ResolveUrl(GetStepLink(CInt(Eval("StepNumber")))) %>' class="d-flex align-items-center px-3 py-2" style="border-bottom:1px solid var(--ox-border);color:var(--ox-text-primary);text-decoration:none;transition:background 0.15s;" onmouseover="this.style.background='rgba(255,255,255,0.03)'" onmouseout="this.style.background='transparent'">
                                <span class="d-flex align-items-center justify-content-center me-2 rounded-circle" style="width:24px;height:24px;font-size:11px;font-weight:600;flex-shrink:0;
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "background:rgba(0,212,170,0.15);color:#00d4aa;",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "background:rgba(255,140,66,0.15);color:#ff8c42;",
                                             "background:var(--ox-bg-input);color:var(--ox-text-muted);")) %>">
                                    <%# If(Eval("Status").ToString().ToLower() = "complete", "<i class='bi bi-check-lg'></i>",
                                          If(Eval("Status").ToString().ToLower() = "in progress", "<i class='bi bi-play-fill'></i>",
                                             Eval("StepNumber").ToString())) %>
                                </span>
                                <span class="flex-grow-1" style="font-size:12px;font-weight:500;"><%# Eval("StepName") %></span>
                                <span class="ox-badge <%# Eval("StatusClass") %>" style="font-size:9px;"><%# Eval("Status") %></span>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
