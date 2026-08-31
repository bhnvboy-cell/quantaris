<%@ Page Title="Process Capability" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="ProcessCapability.aspx.vb" Inherits="Tools_ProcessCapability" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-bar-chart-steps me-2 text-primary"></i>Process Capability (Cp, Cpk, Ppk)</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Process Capability</li>
                </ol>
            </nav>
        </div>
    </div>

    <!-- Project Selector -->
    <div class="row g-3 mb-4">
        <div class="col-md-5">
            <label class="form-label small fw-semibold">Select Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true"
                OnSelectedIndexChanged="ddlProject_SelectedIndexChanged">
                <asp:ListItem Text="-- Select Project --" Value="" />
            </asp:DropDownList>
        </div>
        <div class="col-md-3">
            <label class="form-label small fw-semibold">Select Metric (optional)</label>
            <asp:DropDownList ID="ddlMetric" runat="server" CssClass="form-select" AutoPostBack="true"
                OnSelectedIndexChanged="ddlMetric_SelectedIndexChanged">
                <asp:ListItem Text="-- Manual Entry --" Value="" />
            </asp:DropDownList>
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100"
                OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise me-1"></i>Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlCapability" runat="server" Visible="false">
        <div class="row g-4">
            <!-- Input Form -->
            <div class="col-lg-5">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-sliders me-2"></i>Specification Limits</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="row g-3">
                            <div class="col-md-6">
                                <label class="form-label small fw-semibold">Upper Spec Limit (USL)</label>
                                <asp:TextBox ID="txtUSL" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. 10.5" />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label small fw-semibold">Lower Spec Limit (LSL)</label>
                                <asp:TextBox ID="txtLSL" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. 8.5" />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label small fw-semibold">Target</label>
                                <asp:TextBox ID="txtTarget" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. 9.5" />
                            </div>
                            <div class="col-md-6">
                                <label class="form-label small fw-semibold">Metric Name</label>
                                <asp:TextBox ID="txtMetricName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Diameter" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">
                                    Data Points <span class="text-muted fw-normal">(comma or space separated)</span>
                                </label>
                                <asp:TextBox ID="txtDataPoints" runat="server" CssClass="form-control font-monospace"
                                    TextMode="MultiLine" Rows="6" placeholder="e.g. 9.2, 9.5, 9.1, 9.8, 9.3, 9.6, 9.4, 9.7, 9.0, 9.5" />
                            </div>
                            <div class="col-12 d-flex gap-2">
                                <asp:LinkButton ID="btnCalculate" runat="server" CssClass="btn btn-primary btn-sm"
                                    OnClick="btnCalculate_Click">
                                    <i class="bi bi-calculator me-1"></i>Calculate
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-success btn-sm"
                                    OnClick="btnSave_Click">
                                    <i class="bi bi-floppy me-1"></i>Save Analysis
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnClear" runat="server" CssClass="btn btn-outline-secondary btn-sm"
                                    OnClick="btnClear_Click">
                                    <i class="bi bi-x-circle me-1"></i>Clear
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Results Panel -->
            <div class="col-lg-7">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-graph-up me-2"></i>Capability Results</h6>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlResults" runat="server" Visible="false">
                            <div class="row g-3">
                                <!-- Summary Stats -->
                                <div class="col-md-6">
                                    <div class="card bg-light border-0">
                                        <div class="card-body py-3">
                                            <h6 class="small text-muted text-uppercase mb-3">Summary Statistics</h6>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small">Sample Size (n):</span>
                                                <span class="fw-bold"><asp:Literal ID="litSampleSize" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small">Mean:</span>
                                                <span class="fw-bold"><asp:Literal ID="litMean" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small">Std Dev (within):</span>
                                                <span class="fw-bold"><asp:Literal ID="litStdDev" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small">Std Dev (overall):</span>
                                                <span class="fw-bold"><asp:Literal ID="litStdDevOverall" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small">Min:</span>
                                                <span class="fw-bold"><asp:Literal ID="litMin" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between">
                                                <span class="small">Max:</span>
                                                <span class="fw-bold"><asp:Literal ID="litMax" runat="server" /></span>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Capability Indices -->
                                <div class="col-md-6">
                                    <div class="card bg-light border-0">
                                        <div class="card-body py-3">
                                            <h6 class="small text-muted text-uppercase mb-3">Capability Indices</h6>
                                            <div class="d-flex justify-content-between align-items-center mb-2">
                                                <span class="small fw-semibold">Cp:</span>
                                                <asp:Literal ID="litCp" runat="server" />
                                            </div>
                                            <div class="d-flex justify-content-between align-items-center mb-2">
                                                <span class="small fw-semibold">Cpk:</span>
                                                <asp:Literal ID="litCpk" runat="server" />
                                            </div>
                                            <div class="d-flex justify-content-between align-items-center mb-2">
                                                <span class="small fw-semibold">Ppk:</span>
                                                <asp:Literal ID="litPpk" runat="server" />
                                            </div>
                                            <hr class="my-2" />
                                            <div class="d-flex justify-content-between align-items-center mb-1">
                                                <span class="small">Sigma Level:</span>
                                                <span class="fw-bold"><asp:Literal ID="litSigmaLevel" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between align-items-center">
                                                <span class="small">DPMO:</span>
                                                <span class="fw-bold"><asp:Literal ID="litDPMO" runat="server" /></span>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <!-- Interpretation -->
                                <div class="col-12">
                                    <div class="alert alert-info d-flex align-items-center mb-0 py-2" role="alert">
                                        <i class="bi bi-info-circle me-2 fs-5"></i>
                                        <span><asp:Literal ID="litInterpretation" runat="server" /></span>
                                    </div>
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlNoResults" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-calculator fs-1 d-block mb-2"></i>
                            <span>Enter specification limits and data points, then click <strong>Calculate</strong>.</span>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <!-- History -->
        <div class="card shadow-sm mt-4">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>Saved Analyses</h6>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover align-middle mb-0 small">
                        <thead class="table-dark">
                            <tr>
                                <th>Date</th>
                                <th>Metric</th>
                                <th>USL</th>
                                <th>LSL</th>
                                <th>Mean</th>
                                <th class="text-center">Cp</th>
                                <th class="text-center">Cpk</th>
                                <th class="text-center">Ppk</th>
                                <th class="text-center">Sigma</th>
                                <th class="text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd, yyyy") %></td>
                                        <td class="fw-semibold"><%# Eval("MetricName") %></td>
                                        <td><%# Eval("USL") %></td>
                                        <td><%# Eval("LSL") %></td>
                                        <td><%# Eval("Mean") %></td>
                                        <td class="text-center">
                                            <span class="badge <%# GetCapBadge(If(IsDBNull(Eval("Cp")), 0.0, CDbl(Eval("Cp")))) %> rounded-pill px-2"><%# Eval("Cp") %></span>
                                        </td>
                                        <td class="text-center">
                                            <span class="badge <%# GetCapBadge(If(IsDBNull(Eval("Cpk")), 0.0, CDbl(Eval("Cpk")))) %> rounded-pill px-2"><%# Eval("Cpk") %></span>
                                        </td>
                                        <td class="text-center">
                                            <span class="badge <%# GetCapBadge(If(IsDBNull(Eval("Ppk")), 0.0, CDbl(Eval("Ppk")))) %> rounded-pill px-2"><%# Eval("Ppk") %></span>
                                        </td>
                                        <td class="text-center"><%# Eval("SigmaLevel") %></td>
                                        <td class="text-center">
                                            <asp:LinkButton ID="btnLoad" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                CommandName="LoadRecord" CommandArgument='<%# Eval("Id") %>'>
                                                <i class="bi bi-arrow-counterclockwise"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                CommandName="DeleteRecord" CommandArgument='<%# Eval("Id") %>'
                                                OnClientClick="return confirm('Delete this capability analysis?');">
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
        <i class="bi bi-info-circle me-2"></i>Please select a project to perform process capability analysis.
    </asp:Panel>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="Scripts" runat="server">
    <script>
        // Live preview data point count
        document.addEventListener('DOMContentLoaded', function () {
            var txt = document.getElementById('<%= txtDataPoints.ClientID %>');
            if (txt) {
                txt.addEventListener('input', function () {
                    var val = this.value.trim();
                    if (val) {
                        var pts = val.split(/[,\s]+/).filter(function (p) { return p.length > 0 && !isNaN(parseFloat(p)); });
                        // Optionally show count via title
                        this.title = pts.length + ' data points';
                    } else {
                        this.title = '';
                    }
                });
            }
        });
    </script>
</asp:Content>
