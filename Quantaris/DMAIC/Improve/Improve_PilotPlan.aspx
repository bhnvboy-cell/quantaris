<%@ Page Title="Step 11: Pilot Solution(s)" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Improve_PilotPlan.aspx.vb" Inherits="DMAIC_Improve_Improve_PilotPlan" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-warning text-dark me-2">11</span>Pilot Solution(s)</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Improve/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Improve</a></li>
                    <li class="breadcrumb-item active">Pilot Plan</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />

    <%-- Card 1: Pilot Plan Form --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-airplane me-2"></i>Pilot Plan</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">Plan Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtPlanName" runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvPlanName" runat="server" ControlToValidate="txtPlanName"
                        ErrorMessage="Plan name is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Duration</label>
                    <asp:TextBox ID="txtDuration" runat="server" CssClass="form-control" MaxLength="100" placeholder="e.g. 2 weeks" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Status</label>
                    <asp:DropDownList ID="ddlPilotStatus" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Planned" Value="Planned" />
                        <asp:ListItem Text="In Progress" Value="InProgress" />
                        <asp:ListItem Text="Complete" Value="Complete" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-6">
                    <label class="form-label">Scope</label>
                    <asp:TextBox ID="txtScope" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Start Date</label>
                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">End Date</label>
                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" TextMode="Date" />
                    <asp:CompareValidator ID="cvDateRange" runat="server" ControlToValidate="txtEndDate"
                        ControlToCompare="txtStartDate" Operator="GreaterThanEqual" Type="Date"
                        ErrorMessage="End date must be on or after start date" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-12">
                    <label class="form-label">Metrics</label>
                    <asp:TextBox ID="txtMetrics" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Key metrics to track during pilot" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Poka-Yoke Ideas</label>
                    <asp:TextBox ID="txtPokaYoke" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Mistake-proofing ideas" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Implementation Plan</label>
                    <asp:TextBox ID="txtImplementationPlan" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>
            </div>
            <div class="mt-3">
                <asp:LinkButton ID="btnSavePilot" runat="server" CssClass="btn btn-primary"
                    OnClick="btnSavePilot_Click"><i class='bi bi-save me-1'></i>Save Pilot Plan</asp:LinkButton>
                <asp:Button ID="btnCancelPilot" runat="server" CssClass="btn btn-outline-secondary ms-2"
                    Text="Cancel" OnClick="btnCancelPilot_Click" Visible="false" />
            </div>
        </div>
    </div>

    <%-- Card 2: Existing Pilot Plans --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-list-check me-2"></i>Existing Pilot Plans</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litPilotCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Plan Name</th>
                            <th>Scope</th>
                            <th>Duration</th>
                            <th>Start Date</th>
                            <th>End Date</th>
                            <th>Status</th>
                            <th class="text-center" style="width:90px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptPilots" runat="server" OnItemCommand="rptPilots_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-semibold"><%# Eval("PlanName") %></td>
                                    <td><%# Eval("Scope") %></td>
                                    <td><%# Eval("Duration") %></td>
                                    <td><%# If(Eval("StartDate") IsNot DBNull.Value, CDate(Eval("StartDate")).ToString("yyyy-MM-dd"), "") %></td>
                                    <td><%# If(Eval("EndDate") IsNot DBNull.Value, CDate(Eval("EndDate")).ToString("yyyy-MM-dd"), "") %></td>
                                    <td><span class="badge bg-<%# GetStatusBadge(Eval("Status")) %>"><%# Eval("Status") %></span></td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEditPilot" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDeletePilot" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this pilot plan?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- Card 3: FMEA Link --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-shield-exclamation me-2"></i>FMEA Link - Risk Analysis</h6>
        </div>
        <div class="card-body">
            <p class="text-muted mb-2">Use Failure Mode and Effects Analysis (FMEA) to identify and mitigate risks in your pilot process.</p>
            <a href='<%= ResolveUrl("~/Tools/FMEA.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-primary btn-sm" target="_blank">
                <i class="bi bi-box-arrow-up-right me-1"></i>Open Full FMEA Tool
            </a>
            <hr />
            <h6 class="fw-semibold mb-2">Quick FMEA Entry &amp; Overview</h6>
            <div class="row g-2 align-items-end mb-3">
                <div class="col-md-2">
                    <label class="form-label small">Process Step</label>
                    <asp:TextBox ID="txtFMEAStep" runat="server" CssClass="form-control form-control-sm" MaxLength="200" />
                </div>
                <div class="col-md-2">
                    <label class="form-label small">Failure Mode</label>
                    <asp:TextBox ID="txtFMEAFailure" runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-2">
                    <label class="form-label small">Effect</label>
                    <asp:TextBox ID="txtFMEAEffect" runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-2">
                    <label class="form-label small">Cause</label>
                    <asp:TextBox ID="txtFMEACause" runat="server" CssClass="form-control form-control-sm" />
                </div>
                <div class="col-md-1">
                    <label class="form-label small">S (1-10)</label>
                    <asp:TextBox ID="txtFMEASeverity" runat="server" CssClass="form-control form-control-sm" MaxLength="2" />
                </div>
                <div class="col-md-1">
                    <label class="form-label small">O (1-10)</label>
                    <asp:TextBox ID="txtFMEAOccurrence" runat="server" CssClass="form-control form-control-sm" MaxLength="2" />
                </div>
                <div class="col-md-1">
                    <label class="form-label small">D (1-10)</label>
                    <asp:TextBox ID="txtFMEADetection" runat="server" CssClass="form-control form-control-sm" MaxLength="2" />
                </div>
                <div class="col-md-1">
                    <label class="form-label small">&nbsp;</label>
                    <asp:LinkButton ID="btnAddFMEA" runat="server" CssClass="btn btn-sm btn-primary d-block"
                        OnClick="btnAddFMEA_Click"><i class="bi bi-plus-lg"></i></asp:LinkButton>
                </div>
            </div>

            <%-- Card 4: FMEA Records Table (inline) --%>
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Process Step</th>
                            <th>Failure Mode</th>
                            <th>Effect</th>
                            <th>Cause</th>
                            <th>S</th>
                            <th>O</th>
                            <th>D</th>
                            <th>RPN</th>
                            <th class="text-center" style="width:60px;">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptFMEA" runat="server" OnItemCommand="rptFMEA_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("ProcessStep") %></td>
                                    <td><%# Eval("FailureMode") %></td>
                                    <td><%# Eval("EffectOfFailure") %></td>
                                    <td><%# Eval("CauseOfFailure") %></td>
                                    <td><%# Eval("Severity") %></td>
                                    <td><%# Eval("Occurrence") %></td>
                                    <td><%# Eval("Detection") %></td>
                                    <td><strong><%# Eval("RPN") %></strong></td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnDeleteFMEA" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this FMEA record?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
            <p class="text-muted small mt-2 mb-0">
                <i class="bi bi-info-circle me-1"></i>RPN = Severity &times; Occurrence &times; Detection. Higher RPN values indicate higher risk priority.
            </p>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between mt-4">
        <a href='<%= ResolveUrl("~/DMAIC/Improve/Improve_SolutionGeneration.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Solution Generation
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Improve/Improve_ImpactValidation.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Validate Impact <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
