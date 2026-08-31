<%@ Page Title="Project Gantt Chart" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="ProjectGantt.aspx.vb" Inherits="Tools_ProjectGantt" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .gantt-wrapper { display: flex; border: 1px solid #dee2e6; border-radius: 6px; overflow: hidden; }
        .gantt-tasklist { min-width: 280px; max-width: 280px; border-right: 1px solid #dee2e6; background: #f8f9fa; }
        .gantt-timeline { flex: 1; overflow-x: auto; position: relative; background: #fff; }
        .gantt-header { display: flex; border-bottom: 1px solid #dee2e6; position: sticky; top: 0; z-index: 2; background: #fff; }
        .gantt-week { flex: 1; min-width: 40px; text-align: center; font-size: 10px; padding: 4px 0; border-right: 1px solid #eee; color: #6c757d; }
        .gantt-week.weekend { background: #f8f9fa; }
        .gantt-row { display: flex; align-items: center; border-bottom: 1px solid #f0f0f0; min-height: 36px; }
        .gantt-row-header { padding: 4px 8px; font-size: 12px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
        .gantt-row-header .task-name { font-weight: 500; }
        .gantt-row-header .task-dates { font-size: 10px; color: #6c757d; }
        .gantt-row-body { position: relative; flex: 1; min-height: 36px; }
        .gantt-bar { position: absolute; height: 22px; top: 7px; border-radius: 4px; cursor: pointer; transition: opacity 0.2s; min-width: 4px; }
        .gantt-bar:hover { opacity: 0.85; }
        .gantt-bar .bar-label { position: absolute; left: 6px; top: 2px; font-size: 10px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 90%; color: #fff; font-weight: 500; }
        .gantt-milestone { position: absolute; width: 12px; height: 12px; top: 12px; transform: rotate(45deg); border: 2px solid; z-index: 3; cursor: pointer; }
        .gantt-milestone:hover { transform: rotate(45deg) scale(1.2); }
        .gantt-progress { position: absolute; left: 0; top: 0; height: 100%; border-radius: 4px 0 0 4px; opacity: 0.3; }
        .gantt-today { position: absolute; top: 0; bottom: 0; width: 2px; background: #dc3545; z-index: 4; }
        .gantt-today::before { content: 'Today'; position: absolute; top: -14px; left: -12px; font-size: 9px; color: #dc3545; font-weight: 600; }

        .phase-Define { background: #0d6efd; border-color: #0d6efd; }
        .phase-Measure { background: #198754; border-color: #198754; }
        .phase-Analyze { background: #ffc107; border-color: #ffc107; color: #000; }
        .phase-Improve { background: #fd7e14; border-color: #fd7e14; }
        .phase-Control { background: #6f42c1; border-color: #6f42c1; }

        .phase-text-Define { color: #0d6efd; }
        .phase-text-Measure { color: #198754; }
        .phase-text-Analyze { color: #cc9a06; }
        .phase-text-Improve { color: #fd7e14; }
        .phase-text-Control { color: #6f42c1; }

        .gantt-phase-header { display: flex; align-items: center; padding: 6px 8px; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.05em; border-bottom: 1px solid #dee2e6; background: #e9ecef; }
        .gantt-row-milestone .gantt-row-header { color: #6f42c1; font-style: italic; }
        .gantt-tasklist-header { padding: 8px; font-size: 12px; font-weight: 600; border-bottom: 1px solid #dee2e6; background: #e9ecef; display: flex; justify-content: space-between; }
        .task-form-card { max-height: 600px; overflow-y: auto; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <div>
            <h4 class="mb-1"><i class="bi bi-calendar-check me-2 text-primary"></i>Project Gantt Chart</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Gantt Chart</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-3 mb-3">
        <div class="col-md-4">
            <label class="form-label small fw-semibold">Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true"
                OnSelectedIndexChanged="ddlProject_SelectedIndexChanged" />
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100"
                OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise"></i> Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlGantt" runat="server" Visible="false">
        <!-- Add Task Button -->
        <div class="d-flex gap-2 mb-3">
            <button type="button" class="btn btn-primary btn-sm" data-bs-toggle="modal" data-bs-target="#taskModal">
                <i class="bi bi-plus-lg me-1"></i>Add Task
            </button>
            <button type="button" class="btn btn-outline-primary btn-sm" data-bs-toggle="modal" data-bs-target="#milestoneModal">
                <i class="bi bi-star me-1"></i>Add Milestone
            </button>
        </div>

        <!-- Gantt Chart -->
        <div class="gantt-wrapper shadow-sm mb-4" id="ganttChart">
            <asp:Literal ID="litGantt" runat="server" />
        </div>

        <!-- Today marker script -->
        <script>
            function scrollToToday() {
                var marker = document.querySelector('.gantt-today');
                if (marker) { marker.scrollIntoView({ behavior: 'smooth', inline: 'center' }); }
            }
            document.addEventListener('DOMContentLoaded', function () {
                setTimeout(scrollToToday, 300);
            });
        </script>

        <!-- Task List (non-Gantt table view) -->
        <div class="card shadow-sm">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                <h6 class="mb-0"><i class="bi bi-list-task me-2"></i>All Tasks</h6>
                <span class="badge bg-light text-dark"><asp:Literal ID="litTaskCount" runat="server" /></span>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover align-middle mb-0 small">
                        <thead class="table-dark">
                            <tr>
                                <th>Task</th>
                                <th>Phase</th>
                                <th>Start</th>
                                <th>End</th>
                                <th>Duration</th>
                                <th style="width:150px;">Progress</th>
                                <th>Priority</th>
                                <th>Status</th>
                                <th class="text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptTasks" runat="server" OnItemCommand="rptTasks_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="fw-semibold"><%# Eval("TaskName") %></td>
                                        <td><span class="phase-text-%# Eval("Phase") %>"><%# Eval("Phase") %></span></td>
                                        <td class="text-nowrap"><%# CDate(Eval("StartDate")).ToString("MMM dd") %></td>
                                        <td class="text-nowrap"><%# CDate(Eval("EndDate")).ToString("MMM dd") %></td>
                                        <td><%# Eval("Duration") %>d</td>
                                        <td>
                                            <div class="progress" style="height:8px;">
                                                <div class="progress-bar" role="progressbar"
                                                    style="width:<%# Eval("Progress") %>%"
                                                    aria-valuenow='<%# Eval("Progress") %>' aria-valuemin="0" aria-valuemax="100" />
                                            </div>
                                            <span class="small text-muted"><%# Eval("Progress") %>%</span>
                                        </td>
                                        <td><span class="badge <%# GetPriorityBadge(Eval("Priority").ToString()) %>"><%# Eval("Priority") %></span></td>
                                        <td><span class="badge <%# GetStatusBadge(Eval("Status").ToString()) %>"><%# Eval("Status") %></span></td>
                                        <td class="text-center">
                                            <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                CommandName="EditTask" CommandArgument='<%# Eval("Id") %>'>
                                                <i class="bi bi-pencil"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                CommandName="DeleteTask" CommandArgument='<%# Eval("Id") %>'
                                                OnClientClick="return confirm('Delete this task?');">
                                                <i class="bi bi-trash"></i>
                                            </asp:LinkButton>
                                        </td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project to view the Gantt chart.
    </asp:Panel>

    <!-- Task Modal -->
    <div class="modal fade" id="taskModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header bg-dark text-white">
                    <h6 class="modal-title"><i class="bi bi-plus-circle me-2"></i><asp:Literal ID="litModalTitle" runat="server" Text="Add Task" /></h6>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:Literal ID="litTaskMessage" runat="server" />
                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label small fw-semibold">Task Name</label>
                            <asp:TextBox ID="txtTaskName" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="col-12">
                            <label class="form-label small fw-semibold">Description</label>
                            <asp:TextBox ID="txtTaskDesc" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small fw-semibold">Phase</label>
                            <asp:DropDownList ID="ddlPhase" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Define" Value="Define" />
                                <asp:ListItem Text="Measure" Value="Measure" />
                                <asp:ListItem Text="Analyze" Value="Analyze" />
                                <asp:ListItem Text="Improve" Value="Improve" />
                                <asp:ListItem Text="Control" Value="Control" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small fw-semibold">Priority</label>
                            <asp:DropDownList ID="ddlPriority" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Low" Value="Low" />
                                <asp:ListItem Text="Medium" Value="Medium" />
                                <asp:ListItem Text="High" Value="High" />
                                <asp:ListItem Text="Critical" Value="Critical" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small fw-semibold">Status</label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Not Started" Value="Not Started" />
                                <asp:ListItem Text="In Progress" Value="In Progress" />
                                <asp:ListItem Text="Complete" Value="Complete" />
                                <asp:ListItem Text="On Hold" Value="On Hold" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Start Date</label>
                            <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">End Date</label>
                            <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Progress %</label>
                            <asp:TextBox ID="txtProgress" runat="server" CssClass="form-control form-control-sm" TextMode="Number" placeholder="0-100" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Depends On (Task ID)</label>
                            <asp:TextBox ID="txtDependency" runat="server" CssClass="form-control form-control-sm" placeholder="Optional" />
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <asp:HiddenField ID="hfEditTaskId" runat="server" />
                    <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                    <asp:LinkButton ID="btnSaveTask" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnSaveTask_Click">
                        <i class="bi bi-floppy me-1"></i>Save Task
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </div>

    <!-- Milestone Modal -->
    <div class="modal fade" id="milestoneModal" tabindex="-1">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header bg-dark text-white">
                    <h6 class="modal-title"><i class="bi bi-star me-2"></i>Add Milestone</h6>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                </div>
                <div class="modal-body">
                    <asp:Literal ID="litMilestoneMessage" runat="server" />
                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label small fw-semibold">Milestone Name</label>
                            <asp:TextBox ID="txtMilestoneName" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Date</label>
                            <asp:TextBox ID="txtMilestoneDate" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Phase</label>
                            <asp:DropDownList ID="ddlMilestonePhase" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Define" Value="Define" />
                                <asp:ListItem Text="Measure" Value="Measure" />
                                <asp:ListItem Text="Analyze" Value="Analyze" />
                                <asp:ListItem Text="Improve" Value="Improve" />
                                <asp:ListItem Text="Control" Value="Control" />
                                <asp:ListItem Text="General" Value="General" />
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-outline-secondary btn-sm" data-bs-dismiss="modal">Cancel</button>
                    <asp:LinkButton ID="btnSaveMilestone" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnSaveMilestone_Click">
                        <i class="bi bi-floppy me-1"></i>Save Milestone
                    </asp:LinkButton>
                </div>
            </div>
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="Scripts" runat="server">
    <script>
        // Open edit modal when EditTask command is triggered
        var taskModalEl = document.getElementById('taskModal');
        if (taskModalEl) {
            taskModalEl.addEventListener('hidden.bs.modal', function () {
                // Clear messages on close
            });
        }
    </script>
</asp:Content>
