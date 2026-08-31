<%@ Page Title="Projects" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="Projects_Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="ox-page-header">
        <div>
            <h1><i class="bi bi-folder2-open me-2" style="color:var(--ox-accent)"></i>Six Sigma Projects</h1>
            <div class="ox-breadcrumb">
                <a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a> / Projects
            </div>
        </div>
        <div class="d-flex gap-2">
            <asp:LinkButton ID="btnExportCsv" runat="server" CssClass="ox-btn ox-btn-outline" OnClick="btnExportCsv_Click">
                <i class="bi bi-download"></i> Export CSV
            </asp:LinkButton>
            <a href='<%= ResolveUrl("~/Projects/Create.aspx?tenant=" & CurrentTenantCode) %>' class="ox-btn ox-btn-primary">
                <i class="bi bi-plus-lg"></i> New Project
            </a>
        </div>
    </div>

    <!-- Filters -->
    <div class="ox-card mb-3">
        <div class="d-flex gap-3 align-items-end flex-wrap">
            <div>
                <label class="ox-label">Status</label>
                <asp:DropDownList ID="ddlFilter" runat="server" CssClass="ox-select" style="width:160px;" AutoPostBack="true" OnSelectedIndexChanged="ddlFilter_SelectedIndexChanged">
                    <asp:ListItem Text="All" Value="" />
                    <asp:ListItem Text="Not Started" Value="NotStarted" />
                    <asp:ListItem Text="In Progress" Value="InProgress" />
                    <asp:ListItem Text="Complete" Value="Complete" />
                    <asp:ListItem Text="On Hold" Value="OnHold" />
                </asp:DropDownList>
            </div>
            <div>
                <label class="ox-label">Search</label>
                <asp:TextBox ID="txtSearch" runat="server" CssClass="ox-input" style="width:220px;" placeholder="Project name or code..." />
            </div>
            <div>
                <asp:LinkButton ID="btnSearch" runat="server" CssClass="ox-btn ox-btn-outline" OnClick="btnSearch_Click">
                    <i class="bi bi-search"></i> Search
                </asp:LinkButton>
            </div>
        </div>
    </div>

    <!-- Project Tiles -->
    <div class="ox-tile-grid">
        <asp:Repeater ID="rptProjects" runat="server">
            <ItemTemplate>
                <div class="ox-tile">
                    <div class="ox-tile-top">
                        <div class="ox-tile-icon <%# GetStatusIconClass(Eval("Status")) %>">
                            <i class="bi <%# GetStatusIcon(Eval("Status")) %>"></i>
                        </div>
                        <span class="ox-badge <%# GetStatusBadgeClass(Eval("Status")) %>"><%# Eval("Status") %></span>
                    </div>
                    <div class="ox-tile-title"><%# Eval("ProjectName") %></div>
                    <div class="ox-tile-subtitle"><%# Eval("Phase") %></div>
                    <div class="ox-tile-meta">
                        <span class="ox-tile-meta-item"><i class="bi bi-upc-scan"></i> <%# Eval("ProjectCode") %></span>
                        <span class="ox-tile-meta-item"><i class="bi bi-person"></i> <%# Eval("OwnerName") %></span>
                        <span class="ox-tile-meta-item"><i class="bi bi-calendar"></i> <%# Eval("CreatedOn", "{0:MMM dd, yyyy}") %></span>
                    </div>
                    <div class="ox-tile-progress">
                        <div class="ox-tile-progress-label">
                            <span>DMAIC Progress</span>
                            <span><%# Eval("StepsCompleted") %>%</span>
                        </div>
                        <div class="ox-progress" style="height:4px;">
                            <div class="ox-progress-bar" style="width:<%# Eval("StepsCompleted") %>%"></div>
                        </div>
                    </div>
                    <div class="ox-tile-actions">
                        <a href='<%# ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Eval("Id") & "&tenant=" & CurrentTenantCode) %>' class="ox-btn ox-btn-outline">
                            <i class="bi bi-kanban"></i> DMAIC
                        </a>
                        <a href='<%# ResolveUrl("~/Projects/Edit.aspx?projectId=" & Eval("Id") & "&tenant=" & CurrentTenantCode) %>' class="ox-btn ox-btn-outline">
                            <i class="bi bi-pencil"></i> Edit
                        </a>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <asp:Label ID="lblEmpty" runat="server" style="display:block;text-align:center;padding:40px;color:var(--ox-text-muted);font-size:13px;" Visible="false">
        <i class="bi bi-inbox" style="font-size:32px;display:block;margin-bottom:8px;"></i>No projects found.
    </asp:Label>
</asp:Content>
