<%@ Page Title="Step 13: Institutionalize the Solutions" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Control_Institutionalize.aspx.vb" Inherits="DMAIC_Control_Control_Institutionalize" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-info me-2">13</span>Institutionalize the Solutions</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Control/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Control</a></li>
                    <li class="breadcrumb-item active">Institutionalize</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfControlPlanEditId" runat="server" Value="0" />
    <asp:HiddenField ID="hfChartEditId" runat="server" Value="0" />

    <div class="row g-4">
        <div class="col-lg-6">
            <%-- Card 1: Visual Management Board --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-clipboard-data me-2"></i>Visual Management Board</h6>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-12">
                            <label class="form-label">Description</label>
                            <asp:TextBox ID="txtVMBDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Describe the visual management board layout and content" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label">Location</label>
                            <asp:TextBox ID="txtVMBLocation" runat="server" CssClass="form-control" MaxLength="200" placeholder="e.g. Production area, team room" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label">Update Frequency</label>
                            <asp:DropDownList ID="ddlVMBFrequency" runat="server" CssClass="form-select">
                                <asp:ListItem Text="-- Select --" Value="" />
                                <asp:ListItem Text="Daily" Value="Daily" />
                                <asp:ListItem Text="Weekly" Value="Weekly" />
                                <asp:ListItem Text="Bi-Weekly" Value="BiWeekly" />
                                <asp:ListItem Text="Monthly" Value="Monthly" />
                                <asp:ListItem Text="Quarterly" Value="Quarterly" />
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="mt-3">
                        <asp:LinkButton ID="btnSaveVMB" runat="server" CssClass="btn btn-primary btn-sm"
                            OnClick="btnSaveVMB_Click"><i class='bi bi-save me-1'></i>Save VMB</asp:LinkButton>
                        <asp:Label ID="lblVMBSaved" runat="server" CssClass="text-success small ms-2" Visible="false"><i class="bi bi-check-circle"></i> Saved</asp:Label>
                    </div>
                </div>
            </div>

            <%-- Card 2: Team Huddles --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-people me-2"></i>Team Huddles</h6>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label">Schedule</label>
                            <asp:TextBox ID="txtHuddleSchedule" runat="server" CssClass="form-control" MaxLength="200" placeholder="e.g. Daily at 9:00 AM" />
                        </div>
                        <div class="col-md-4">
                            <label class="form-label">Location</label>
                            <asp:TextBox ID="txtHuddleLocation" runat="server" CssClass="form-control" MaxLength="200" />
                        </div>
                        <div class="col-md-2">
                            <label class="form-label">Duration (mins)</label>
                            <asp:TextBox ID="txtHuddleDuration" runat="server" CssClass="form-control" MaxLength="5" placeholder="15" />
                            <asp:RangeValidator ID="rvHuddleDuration" runat="server" ControlToValidate="txtHuddleDuration"
                                MinimumValue="1" MaximumValue="480" Type="Integer" ErrorMessage="Invalid"
                                CssClass="text-danger small" Display="Dynamic" />
                        </div>
                        <div class="col-12">
                            <label class="form-label">Participants</label>
                            <asp:TextBox ID="txtHuddleParticipants" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" placeholder="List participants or roles" />
                        </div>
                        <div class="col-12">
                            <label class="form-label">Agenda</label>
                            <asp:TextBox ID="txtHuddleAgenda" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Standard huddle agenda items" />
                        </div>
                    </div>
                    <div class="mt-3">
                        <asp:LinkButton ID="btnSaveHuddle" runat="server" CssClass="btn btn-primary btn-sm"
                            OnClick="btnSaveHuddle_Click"><i class='bi bi-save me-1'></i>Save Huddle</asp:LinkButton>
                        <asp:Label ID="lblHuddleSaved" runat="server" CssClass="text-success small ms-2" Visible="false"><i class="bi bi-check-circle"></i> Saved</asp:Label>
                    </div>
                </div>
            </div>

            <%-- Card 3: Control Plan --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white d-flex justify-content-between align-items-center">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-sliders me-2"></i>Control Plan</h6>
                    <span class="badge bg-secondary"><asp:Literal ID="litControlPlanCount" runat="server" /></span>
                </div>
                <div class="card-body">
                    <div class="row g-2 mb-3">
                        <div class="col-md-6">
                            <label class="form-label small">Process Step <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtCPProcessStep" runat="server" CssClass="form-control form-control-sm" MaxLength="200" />
                            <asp:RequiredFieldValidator ID="rfvCPProcessStep" runat="server" ControlToValidate="txtCPProcessStep"
                                ErrorMessage="Required" CssClass="text-danger small" Display="Dynamic" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">Sample Size</label>
                            <asp:TextBox ID="txtCPSampleSize" runat="server" CssClass="form-control form-control-sm" MaxLength="10" />
                            <asp:RangeValidator ID="rvCPSampleSize" runat="server" ControlToValidate="txtCPSampleSize"
                                MinimumValue="1" MaximumValue="999999" Type="Integer" ErrorMessage="Invalid"
                                CssClass="text-danger small" Display="Dynamic" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">Frequency</label>
                            <asp:DropDownList ID="ddlCPFrequency" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="-- Select --" Value="" />
                                <asp:ListItem Text="Hourly" Value="Hourly" />
                                <asp:ListItem Text="Daily" Value="Daily" />
                                <asp:ListItem Text="Weekly" Value="Weekly" />
                                <asp:ListItem Text="Monthly" Value="Monthly" />
                                <asp:ListItem Text="Per Batch" Value="PerBatch" />
                                <asp:ListItem Text="Continuous" Value="Continuous" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-12">
                            <label class="form-label small">Specification</label>
                            <asp:TextBox ID="txtCPSpecification" runat="server" CssClass="form-control form-control-sm" MaxLength="500" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">Control Method</label>
                            <asp:TextBox ID="txtCPControlMethod" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">Reaction Plan</label>
                            <asp:TextBox ID="txtCPReactionPlan" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label small">Responsible</label>
                            <asp:TextBox ID="txtCPResponsible" runat="server" CssClass="form-control form-control-sm" MaxLength="200" />
                        </div>
                    </div>
                    <div class="d-flex gap-2">
                        <asp:LinkButton ID="btnSaveCP" runat="server" CssClass="btn btn-primary btn-sm"
                            OnClick="btnSaveCP_Click"><i class='bi bi-plus-lg me-1'></i>Add Entry</asp:LinkButton>
                        <asp:Button ID="btnCancelCP" runat="server" CssClass="btn btn-outline-secondary btn-sm"
                            Text="Cancel" OnClick="btnCancelCP_Click" Visible="false" />
                    </div>
                </div>
                <div class="card-footer bg-white p-0">
                    <div class="table-responsive">
                        <table class="table table-hover table-striped mb-0 small">
                            <thead class="table-light">
                                <tr>
                                    <th>Step</th>
                                    <th>Method</th>
                                    <th>Spec</th>
                                    <th>Freq</th>
                                    <th>Responsible</th>
                                    <th class="text-center" style="width:80px;">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptControlPlans" runat="server" OnItemCommand="rptControlPlans_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="fw-semibold"><%# Eval("ProcessStep") %></td>
                                            <td><%# Eval("ControlMethod") %></td>
                                            <td><%# Eval("Specification") %></td>
                                            <td><%# Eval("Frequency") %></td>
                                            <td><%# Eval("Responsible") %></td>
                                            <td class="text-center">
                                                <asp:LinkButton ID="btnEditCP" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                                    CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                                <asp:LinkButton ID="btnDeleteCP" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                    CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                                    OnClientClick="return confirm('Delete this control plan entry?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-6">
            <%-- Card 4: SOP References --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-file-earmark-text me-2"></i>SOP References</h6>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label">Standard Operating Procedure Documents</label>
                        <asp:TextBox ID="txtSOPReferences" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5"
                            placeholder="List SOP document IDs, links, or references&#10;e.g. SOP-001: Process Control, SOP-002: Quality Check" />
                    </div>
                    <div>
                        <asp:LinkButton ID="btnSaveSOP" runat="server" CssClass="btn btn-primary btn-sm"
                            OnClick="btnSaveSOP_Click"><i class='bi bi-save me-1'></i>Save SOP References</asp:LinkButton>
                        <asp:Label ID="lblSOPSaved" runat="server" CssClass="text-success small ms-2" Visible="false"><i class="bi bi-check-circle"></i> Saved</asp:Label>
                    </div>
                </div>
            </div>

            <%-- Card 5: Go Gemba --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-eye me-2"></i>Go Gemba</h6>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label">Observation Date</label>
                            <asp:TextBox ID="txtGembaDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label">Area</label>
                            <asp:TextBox ID="txtGembaArea" runat="server" CssClass="form-control" MaxLength="200" placeholder="e.g. Production line, warehouse" />
                        </div>
                        <div class="col-12">
                            <label class="form-label">Observations</label>
                            <asp:TextBox ID="txtGembaObservations" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                        </div>
                        <div class="col-12">
                            <label class="form-label">Action Items</label>
                            <asp:TextBox ID="txtGembaActions" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" placeholder="Follow-up actions from the Gemba walk" />
                        </div>
                    </div>
                    <div class="mt-3">
                        <asp:LinkButton ID="btnSaveGemba" runat="server" CssClass="btn btn-primary btn-sm"
                            OnClick="btnSaveGemba_Click"><i class='bi bi-save me-1'></i>Save Gemba</asp:LinkButton>
                        <asp:Label ID="lblGembaSaved" runat="server" CssClass="text-success small ms-2" Visible="false"><i class="bi bi-check-circle"></i> Saved</asp:Label>
                    </div>
                </div>
            </div>

            <%-- Card 6: Control Charts --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-graph-up me-2"></i>Control Charts</h6>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-4">
                            <label class="form-label">Chart Type</label>
                            <asp:DropDownList ID="ddlChartType" runat="server" CssClass="form-select">
                                <asp:ListItem Text="-- Select --" Value="" />
                                <asp:ListItem Text="I-MR" Value="IMR" />
                                <asp:ListItem Text="X̄-R" Value="XR" />
                                <asp:ListItem Text="X̄-S" Value="XS" />
                                <asp:ListItem Text="p Chart" Value="p" />
                                <asp:ListItem Text="c Chart" Value="c" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-12">
                            <label class="form-label">Data Points (one value per line)</label>
                            <asp:TextBox ID="txtChartDataPoints" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4"
                                placeholder="Enter numeric values, one per line&#10;e.g.&#10;12.5&#10;13.1&#10;11.8" />
                            <asp:RegularExpressionValidator ID="revChartData" runat="server" ControlToValidate="txtChartDataPoints"
                                ValidationExpression="^[\d\s\.,\-]+$" ErrorMessage="Enter numeric values only"
                                CssClass="text-danger small" Display="Dynamic" />
                        </div>
                    </div>

                    <div class="row g-2 mt-2" id="divChartSummary" runat="server" visible="false">
                        <div class="col-md-4">
                            <label class="form-label small">Mean (CL)</label>
                            <asp:TextBox ID="txtChartCL" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small">UCL</label>
                            <asp:TextBox ID="txtChartUCL" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="col-md-4">
                            <label class="form-label small">LCL</label>
                            <asp:TextBox ID="txtChartLCL" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">N</label>
                            <asp:TextBox ID="txtChartN" runat="server" CssClass="form-control form-control-sm" Enabled="false" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">Mean</label>
                            <asp:TextBox ID="txtChartMean" runat="server" CssClass="form-control form-control-sm" Enabled="false" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">Std Dev</label>
                            <asp:TextBox ID="txtChartStdDev" runat="server" CssClass="form-control form-control-sm" Enabled="false" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small">Range</label>
                            <asp:TextBox ID="txtChartRange" runat="server" CssClass="form-control form-control-sm" Enabled="false" />
                        </div>
                    </div>

                    <div class="mt-3">
                        <asp:LinkButton ID="btnCalcChart" runat="server" CssClass="btn btn-outline-secondary btn-sm me-2"
                            OnClick="btnCalcChart_Click"><i class='bi bi-calculator me-1'></i>Calculate Stats</asp:LinkButton>
                        <asp:LinkButton ID="btnSaveChart" runat="server" CssClass="btn btn-primary btn-sm"
                            OnClick="btnSaveChart_Click"><i class='bi bi-save me-1'></i>Save Control Chart</asp:LinkButton>
                        <asp:Label ID="lblChartSaved" runat="server" CssClass="text-success small ms-2" Visible="false"><i class="bi bi-check-circle"></i> Saved</asp:Label>
                    </div>
                </div>
                <div class="card-footer bg-white p-0">
                    <div class="table-responsive">
                        <table class="table table-hover table-striped mb-0 small">
                            <thead class="table-light">
                                <tr>
                                    <th>Type</th>
                                    <th>Data Points</th>
                                    <th>CL</th>
                                    <th>UCL</th>
                                    <th>LCL</th>
                                    <th class="text-center" style="width:60px;">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptCharts" runat="server" OnItemCommand="rptCharts_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><span class="badge bg-info"><%# Eval("ChartTypeName") %></span></td>
                                            <td><%# Eval("DataPointCount") %> pts</td>
                                            <td><%# Eval("CenterLine", "{0:N3}") %></td>
                                            <td><%# Eval("UCL", "{0:N3}") %></td>
                                            <td><%# Eval("LCL", "{0:N3}") %></td>
                                            <td class="text-center">
                                                <asp:LinkButton ID="btnDeleteChart" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                    CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                                    OnClientClick="return confirm('Delete this control chart?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between mt-2 mb-4">
        <a href='<%= ResolveUrl("~/DMAIC/Control/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Control
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Control/Control_BestPractices.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Replicate Best Practices <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
