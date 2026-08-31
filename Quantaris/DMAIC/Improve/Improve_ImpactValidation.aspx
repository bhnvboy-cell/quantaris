<%@ Page Title="Step 12: Validate Impact of Solutions" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Improve_ImpactValidation.aspx.vb" Inherits="DMAIC_Improve_Improve_ImpactValidation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-warning text-dark me-2">12</span>Validate Impact of Solutions</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Improve/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Improve</a></li>
                    <li class="breadcrumb-item active">Impact Validation</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfCostBenefitId" runat="server" Value="0" />

    <div class="row g-4">
        <%-- Card 1: Cost-Benefit Analysis --%>
        <div class="col-lg-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-cash-coin me-2"></i>Cost-Benefit Analysis</h6>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label">Implementation Costs ($)</label>
                        <asp:TextBox ID="txtImplCost" runat="server" CssClass="form-control" MaxLength="18" placeholder="Total cost to implement" />
                        <asp:RangeValidator ID="rvImplCost" runat="server" ControlToValidate="txtImplCost"
                            MinimumValue="0" MaximumValue="999999999" Type="Double"
                            ErrorMessage="Enter a valid cost" CssClass="text-danger small" Display="Dynamic" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Annual Savings ($)</label>
                        <asp:TextBox ID="txtAnnualSavings" runat="server" CssClass="form-control" MaxLength="18" placeholder="Expected annual savings" />
                        <asp:RangeValidator ID="rvSavings" runat="server" ControlToValidate="txtAnnualSavings"
                            MinimumValue="0" MaximumValue="999999999" Type="Double"
                            ErrorMessage="Enter valid savings" CssClass="text-danger small" Display="Dynamic" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Payback Period (months)</label>
                        <asp:TextBox ID="txtPaybackPeriod" runat="server" CssClass="form-control" ReadOnly="true"
                            placeholder="Auto-calculated" />
                    </div>
                    <div class="mb-0">
                        <label class="form-label">Non-Financial Benefits</label>
                        <asp:TextBox ID="txtNonFinancialBenefits" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="4"
                            placeholder="Describe qualitative benefits..." />
                    </div>
                    <div class="mt-3">
                        <asp:LinkButton ID="btnSaveCostBenefit" runat="server" CssClass="btn btn-primary"
                            OnClick="btnSaveCostBenefit_Click"><i class='bi bi-save me-1'></i>Save Cost-Benefit</asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>

        <%-- Card 2: Updated Metrics --%>
        <div class="col-lg-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-graph-up-arrow me-2"></i>Updated Metrics</h6>
                </div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <table class="table table-hover table-striped mb-0 small">
                            <thead class="table-light">
                                <tr>
                                    <th>Metric</th>
                                    <th>Baseline</th>
                                    <th>After Value</th>
                                    <th>Improvement</th>
                                    <th>Cp (Before)</th>
                                    <th>Cp (After)</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptMetrics" runat="server" OnItemCommand="rptMetrics_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="fw-semibold"><%# Eval("MetricName") %></td>
                                            <td><%# Eval("Baseline", "{0:N2}") %></td>
                                            <td>
                                                <asp:TextBox ID="txtAfterValue" runat="server" CssClass="form-control form-control-sm"
                                                    Text='<%# If(Eval("CurrentValue") IsNot DBNull.Value, Eval("CurrentValue", "{0:N2}"), "") %>'
                                                    MaxLength="18" style="width:100px;" />
                                                <asp:HiddenField ID="hfMetricId" runat="server" Value='<%# Eval("Id") %>' />
                                            </td>
                                            <td>
                                                <asp:Literal ID="litImprovement" runat="server" />
                                            </td>
                                            <td><%# Eval("CapabilityCp", "{0:N2}") %></td>
                                            <td><%# Eval("CapabilityCpk", "{0:N2}") %></td>
                                        </tr>
                                    </ItemTemplate>
                                    
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                    <div class="p-3 border-top">
                        <asp:LinkButton ID="btnSaveMetrics" runat="server" CssClass="btn btn-primary"
                            OnClick="btnSaveMetrics_Click"><i class='bi bi-save me-1'></i>Update All Metrics</asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Card 3: Updated Capability --%>
    <div class="card shadow-sm mt-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-bar-chart-line me-2"></i>Updated Capability</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-3">
                    <label class="form-label">New Sigma Level</label>
                    <asp:TextBox ID="txtNewSigma" runat="server" CssClass="form-control" MaxLength="18" placeholder="e.g. 4.5" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">New DPMO</label>
                    <asp:TextBox ID="txtNewDPMO" runat="server" CssClass="form-control" MaxLength="18" placeholder="Defects Per Million" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">New Cp</label>
                    <asp:TextBox ID="txtNewCp" runat="server" CssClass="form-control" MaxLength="18" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">New Cpk</label>
                    <asp:TextBox ID="txtNewCpk" runat="server" CssClass="form-control" MaxLength="18" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Normality Test</label>
                    <asp:DropDownList ID="ddlNormality" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Not Tested --" Value="" />
                        <asp:ListItem Text="Normal" Value="Normal" />
                        <asp:ListItem Text="Not Normal" Value="Not Normal" />
                        <asp:ListItem Text="Borderline" Value="Borderline" />
                    </asp:DropDownList>
                </div>
            </div>
            <div class="mt-3">
                <asp:LinkButton ID="btnSaveCapability" runat="server" CssClass="btn btn-primary"
                    OnClick="btnSaveCapability_Click"><i class='bi bi-save me-1'></i>Save Updated Capability</asp:LinkButton>
            </div>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between mt-4">
        <a href='<%= ResolveUrl("~/DMAIC/Improve/Improve_PilotPlan.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Pilot Plan
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-success">
            <i class="bi bi-check-circle me-1"></i>Complete Improve Phase
        </a>
    </div>
</asp:Content>
