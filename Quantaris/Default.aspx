<%@ Page Title="Dashboard" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="_Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="ox-page-header">
        <div>
            <h1><i class="bi bi-grid-1x2-fill me-2" style="color:var(--ox-accent)"></i>Dashboard</h1>
            <div class="ox-breadcrumb">
                <a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a>
            </div>
        </div>
        <a href='<%= ResolveUrl("~/Projects/Create.aspx?tenant=" & CurrentTenantCode) %>' class="ox-btn ox-btn-primary">
            <i class="bi bi-plus-lg"></i> New Project
        </a>
    </div>

    <!-- KPI Tiles -->
    <div class="ox-tile-grid mb-4">
        <div class="ox-tile ox-tile-compact accent-blue" onclick="window.location='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'">
            <div class="ox-tile-icon blue"><i class="bi bi-folder2-open"></i></div>
            <div>
                <div class="ox-tile-title"><asp:Literal ID="litTotalProjects" runat="server" /></div>
                <div class="ox-tile-subtitle">Total Projects</div>
            </div>
        </div>
        <div class="ox-tile ox-tile-compact accent-orange">
            <div class="ox-tile-icon orange"><i class="bi bi-pause-circle"></i></div>
            <div>
                <div class="ox-tile-title"><asp:Literal ID="litNotStarted" runat="server" /></div>
                <div class="ox-tile-subtitle">Not Started</div>
            </div>
        </div>
        <div class="ox-tile ox-tile-compact accent-teal">
            <div class="ox-tile-icon teal"><i class="bi bi-arrow-repeat"></i></div>
            <div>
                <div class="ox-tile-title"><asp:Literal ID="litInProgress" runat="server" /></div>
                <div class="ox-tile-subtitle">In Progress</div>
            </div>
        </div>
        <div class="ox-tile ox-tile-compact accent-purple">
            <div class="ox-tile-icon purple"><i class="bi bi-check-circle"></i></div>
            <div>
                <div class="ox-tile-title"><asp:Literal ID="litComplete" runat="server" /></div>
                <div class="ox-tile-subtitle">Completed</div>
            </div>
        </div>
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

    <!-- Section: Recent Projects -->
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h6 style="font-size:13px;font-weight:600;color:var(--ox-text-secondary);text-transform:uppercase;letter-spacing:0.04em;margin:0;">
            <i class="bi bi-folder2 me-2"></i>Recent Projects
        </h6>
        <a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>' class="ox-btn ox-btn-outline" style="font-size:11px;padding:4px 10px;">
            View All <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>

    <!-- Project Tiles -->
    <div class="ox-tile-grid mb-4">
        <asp:Repeater ID="rptRecentProjects" runat="server">
            <ItemTemplate>
                <div class="ox-tile" onclick="window.location='<%# ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Eval("Id") & "&tenant=" & CurrentTenantCode) %>'">
                    <div class="ox-tile-top">
                        <div class="ox-tile-icon <%# GetStatusIconClass(Eval("Status")) %>">
                            <i class="bi <%# GetStatusIcon(Eval("Status")) %>"></i>
                        </div>
                        <span class="ox-badge <%# GetStatusBadgeClass(Eval("Status")) %>"><%# Eval("Status") %></span>
                    </div>
                    <div class="ox-tile-title"><%# Eval("ProjectName") %></div>
                    <div class="ox-tile-meta">
                        <span class="ox-tile-meta-item"><i class="bi bi-upc-scan"></i> <%# Eval("ProjectCode") %></span>
                        <span class="ox-tile-meta-item"><i class="bi bi-person"></i> <%# Eval("OwnerName") %></span>
                        <span class="ox-tile-meta-item"><i class="bi bi-calendar"></i> <%# Eval("CreatedOn", "{0:MMM dd}") %></span>
                    </div>
                    <div class="ox-tile-progress">
                        <div class="ox-tile-progress-label">
                            <span>DMAIC Progress</span>
                            <span><%# Eval("StepsPct") %>%</span>
                        </div>
                        <div class="ox-progress" style="height:4px;">
                            <div class="ox-progress-bar" style="width:<%# Eval("StepsPct") %>%"></div>
                        </div>
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>
    </div>

    <div class="text-center py-3 mb-4" style="color:var(--ox-text-muted);font-size:12px;" id="emptyRow" runat="server" visible="false">
        <i class="bi bi-inbox me-2"></i>No projects yet.
        <a href='<%= ResolveUrl("~/Projects/Create.aspx?tenant=" & CurrentTenantCode) %>' class="ms-2">Create one <i class="bi bi-arrow-right"></i></a>
    </div>

    <!-- Charts Row -->
    <div class="ox-tile-grid">
        <div class="ox-tile" style="cursor:default;">
            <div class="ox-tile-top" style="margin-bottom:8px;">
                <span class="ox-card-title"><i class="bi bi-pie-chart me-2"></i>Status Distribution</span>
            </div>
            <div style="height:200px;">
                <canvas id="statusChart"></canvas>
            </div>
        </div>
        <div class="ox-tile" style="cursor:default;">
            <div class="ox-tile-top" style="margin-bottom:8px;">
                <span class="ox-card-title"><i class="bi bi-bar-chart me-2"></i>DMAIC Completion</span>
            </div>
            <div style="height:200px;">
                <canvas id="dmaicChart"></canvas>
            </div>
        </div>
        <div class="ox-tile" style="cursor:default;">
            <div class="ox-tile-top" style="margin-bottom:8px;">
                <span class="ox-card-title"><i class="bi bi-reception-4 me-2"></i>Phase Progress</span>
            </div>
            <div style="height:200px;">
                <canvas id="phaseChart"></canvas>
            </div>
        </div>
    </div>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Scripts" runat="server">
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.4/dist/chart.umd.min.js"></script>
    <script>
        var oxColors = {
            teal: '#00d4aa', blue: '#4a9eff', orange: '#ff8c42',
            purple: '#a78bfa', red: '#ff4d6a', muted: '#5a5a7a',
            grid: '#2a2a45', text: '#8b8ba3'
        };

        new Chart(document.getElementById('statusChart'), {
            type: 'doughnut',
            data: {
                labels: ['Not Started', 'In Progress', 'Completed', 'On Hold'],
                datasets: [{
                    data: [<%= GetNotStarted() %>, <%= GetInProgress() %>, <%= GetComplete() %>, <%= GetOnHold() %>],
                    backgroundColor: [oxColors.muted, oxColors.teal, oxColors.purple, oxColors.orange],
                    borderWidth: 0
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false, cutout: '65%',
                plugins: { legend: { position: 'bottom', labels: { color: oxColors.text, font: { size: 11 }, usePointStyle: true, padding: 12 } } }
            }
        });

        new Chart(document.getElementById('dmaicChart'), {
            type: 'bar',
            data: {
                labels: [<%= GetProjectNames() %>],
                datasets: [{
                    label: 'DMAIC %',
                    data: [<%= GetProjectPcts() %>],
                    backgroundColor: oxColors.teal,
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false, indexAxis: 'y',
                scales: {
                    x: { max: 100, ticks: { color: oxColors.text, callback: function(v) { return v + '%'; } }, grid: { color: oxColors.grid } },
                    y: { ticks: { color: oxColors.text }, grid: { display: false } }
                },
                plugins: { legend: { display: false } }
            }
        });

        new Chart(document.getElementById('phaseChart'), {
            type: 'radar',
            data: {
                labels: ['Define', 'Measure', 'Analyze', 'Improve', 'Control'],
                datasets: [{
                    label: 'Avg Completion',
                    data: [<%= GetPhaseAvg("Define") %>, <%= GetPhaseAvg("Measure") %>, <%= GetPhaseAvg("Analyze") %>, <%= GetPhaseAvg("Improve") %>, <%= GetPhaseAvg("Control") %>],
                    backgroundColor: 'rgba(0, 212, 170, 0.15)',
                    borderColor: oxColors.teal,
                    pointBackgroundColor: oxColors.teal
                }]
            },
            options: {
                responsive: true, maintainAspectRatio: false,
                scales: { r: { min: 0, max: 100, ticks: { stepSize: 25, color: oxColors.text, backdropColor: 'transparent' }, grid: { color: oxColors.grid }, pointLabels: { color: oxColors.text } } },
                plugins: { legend: { display: false } }
            }
        });
    </script>
</asp:Content>
