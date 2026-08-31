<%@ Page Title="COPQ Calculator" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="COPQ.aspx.vb" Inherits="Tools_COPQ" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-currency-dollar me-2 text-danger"></i>Cost of Poor Quality (COPQ)</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">COPQ Calculator</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-md-5">
            <label class="form-label small fw-semibold">Select Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true"
                OnSelectedIndexChanged="ddlProject_SelectedIndexChanged">
                <asp:ListItem Text="-- Select Project --" Value="" />
            </asp:DropDownList>
        </div>
        <div class="col-md-3">
            <label class="form-label small fw-semibold">Entry Date</label>
            <asp:TextBox ID="txtEntryDate" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100"
                OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise me-1"></i>Refresh
            </asp:LinkButton>
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnExportCsv" runat="server" CssClass="btn btn-outline-success btn-sm rounded-pill px-3" OnClick="btnExportCsv_Click">
                <i class="bi bi-download me-1"></i>Export CSV
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlCOPQ" runat="server" Visible="false">
        <div class="row g-4">
            <!-- Input Form -->
            <div class="col-lg-5">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-pencil-square me-2"></i>Cost Entry</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="row g-3">
                            <div class="col-12">
                                <label class="form-label small fw-semibold text-success">
                                    <i class="bi bi-check-circle me-1"></i>Appraisal Costs
                                </label>
                                <asp:TextBox ID="txtAppraisal" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="Inspection, testing, auditing..." />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold text-primary">
                                    <i class="bi bi-shield-check me-1"></i>Prevention Costs
                                </label>
                                <asp:TextBox ID="txtPrevention" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="Training, planning, maintenance..." />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold text-warning">
                                    <i class="bi bi-exclamation-triangle me-1"></i>Internal Failure Costs
                                </label>
                                <asp:TextBox ID="txtInternalFailure" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="Rework, scrap, downtime..." />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold text-danger">
                                    <i class="bi bi-bug me-1"></i>External Failure Costs
                                </label>
                                <asp:TextBox ID="txtExternalFailure" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="Warranty, returns, penalties..." />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Revenue <span class="text-muted fw-normal">(optional, for ratio %)</span></label>
                                <asp:TextBox ID="txtRevenue" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="e.g. 1000000" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Notes</label>
                                <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control form-control-sm"
                                    TextMode="MultiLine" Rows="2" placeholder="Optional notes..." />
                            </div>
                            <div class="col-12 d-flex gap-2">
                                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-danger btn-sm"
                                    OnClick="btnSave_Click">
                                    <i class="bi bi-floppy me-1"></i>Save Entry
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

            <!-- Summary Panel -->
            <div class="col-lg-7">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-pie-chart me-2"></i>Cost Summary</h6>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlSummary" runat="server" Visible="false">
                            <div class="row g-3">
                                <!-- Cost Breakdown -->
                                <div class="col-md-6">
                                    <div class="card bg-light border-0">
                                        <div class="card-body py-3">
                                            <h6 class="small text-muted text-uppercase mb-3">Breakdown</h6>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small text-success"><i class="bi bi-check-circle me-1"></i>Appraisal:</span>
                                                <span class="fw-bold"><asp:Literal ID="litAppraisal" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small text-primary"><i class="bi bi-shield-check me-1"></i>Prevention:</span>
                                                <span class="fw-bold"><asp:Literal ID="litPrevention" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small text-warning"><i class="bi bi-exclamation-triangle me-1"></i>Internal Failure:</span>
                                                <span class="fw-bold"><asp:Literal ID="litInternal" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small text-danger"><i class="bi bi-bug me-1"></i>External Failure:</span>
                                                <span class="fw-bold"><asp:Literal ID="litExternal" runat="server" /></span>
                                            </div>
                                            <hr class="my-2" />
                                            <div class="d-flex justify-content-between">
                                                <span class="fw-semibold">Total COPQ:</span>
                                                <span class="fw-bold text-danger fs-5"><asp:Literal ID="litTotal" runat="server" /></span>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                <!-- Ratios -->
                                <div class="col-md-6">
                                    <div class="card bg-light border-0">
                                        <div class="card-body py-3">
                                            <h6 class="small text-muted text-uppercase mb-3">Quality Ratios</h6>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small">COPQ / Revenue:</span>
                                                <span class="fw-bold"><asp:Literal ID="litCOPQRevenue" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small">Failure / Total COPQ:</span>
                                                <span class="fw-bold"><asp:Literal ID="litFailureRatio" runat="server" /></span>
                                            </div>
                                            <div class="d-flex justify-content-between mb-1">
                                                <span class="small">Appraisal + Prevention / Total:</span>
                                                <span class="fw-bold"><asp:Literal ID="litGoodRatio" runat="server" /></span>
                                            </div>
                                            <hr class="my-2" />
                                            <div class="d-flex justify-content-between">
                                                <span class="small">CoPQ Level:</span>
                                                <span class="badge fs-6 rounded-pill px-3 py-1">
                                                    <asp:Literal ID="litLevel" runat="server" />
                                                </span>
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
                        <asp:Panel ID="pnlNoSummary" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-calculator fs-1 d-block mb-2"></i>
                            <span>Enter cost values and click <strong>Save Entry</strong>.</span>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <!-- Trend Chart (bar visualization via CSS) -->
        <div class="card shadow-sm mt-4">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                <h6 class="mb-0"><i class="bi bi-bar-chart-line me-2"></i>Cost Breakdown</h6>
                <span class="small text-white-50">Current entry</span>
            </div>
            <div class="card-body">
                <asp:Panel ID="pnlChart" runat="server" Visible="false">
                    <div class="row g-2 mb-3">
                        <div class="col-12">
                            <div class="d-flex justify-content-between small mb-1">
                                <span class="text-success">Appraisal</span>
                                <span><asp:Literal ID="litChartAppr" runat="server" /></span>
                            </div>
                            <div class="progress" style="height:24px;">
                                <asp:Panel ID="barAppr" runat="server" CssClass="progress-bar bg-success" />
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="d-flex justify-content-between small mb-1">
                                <span class="text-primary">Prevention</span>
                                <span><asp:Literal ID="litChartPrev" runat="server" /></span>
                            </div>
                            <div class="progress" style="height:24px;">
                                <asp:Panel ID="barPrev" runat="server" CssClass="progress-bar bg-primary" />
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="d-flex justify-content-between small mb-1">
                                <span class="text-warning">Internal Failure</span>
                                <span><asp:Literal ID="litChartInt" runat="server" /></span>
                            </div>
                            <div class="progress" style="height:24px;">
                                <asp:Panel ID="barInt" runat="server" CssClass="progress-bar bg-warning" />
                            </div>
                        </div>
                        <div class="col-12">
                            <div class="d-flex justify-content-between small mb-1">
                                <span class="text-danger">External Failure</span>
                                <span><asp:Literal ID="litChartExt" runat="server" /></span>
                            </div>
                            <div class="progress" style="height:24px;">
                                <asp:Panel ID="barExt" runat="server" CssClass="progress-bar bg-danger" />
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlNoChart" runat="server" CssClass="text-center text-muted py-3">
                    <span>Save an entry to see the cost breakdown chart.</span>
                </asp:Panel>
            </div>
        </div>

        <!-- History -->
        <div class="card shadow-sm mt-4">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>COPQ History</h6>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover align-middle mb-0 small">
                        <thead class="table-dark">
                            <tr>
                                <th>Date</th>
                                <th class="text-end">Appraisal</th>
                                <th class="text-end">Prevention</th>
                                <th class="text-end">Int. Failure</th>
                                <th class="text-end">Ext. Failure</th>
                                <th class="text-end">Total COPQ</th>
                                <th>Notes</th>
                                <th class="text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                         <td class="text-nowrap"><%# If(Eval("EntryDate") IsNot DBNull.Value, CDate(Eval("EntryDate")).ToString("MMM dd, yyyy"), "-") %></td>
                                        <td class="text-end"><%# FormatCurrency(Eval("AppraisalCosts")) %></td>
                                        <td class="text-end"><%# FormatCurrency(Eval("PreventionCosts")) %></td>
                                        <td class="text-end"><%# FormatCurrency(Eval("InternalFailureCosts")) %></td>
                                        <td class="text-end"><%# FormatCurrency(Eval("ExternalFailureCosts")) %></td>
                                        <td class="text-end fw-bold"><%# FormatCurrency(Eval("TotalCOPQ")) %></td>
                                        <td class="text-truncate" style="max-width:150px;"><%# Eval("Notes") %></td>
                                        <td class="text-center">
                                            <asp:LinkButton ID="btnLoad" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                CommandName="LoadRecord" CommandArgument='<%# Eval("Id") %>'>
                                                <i class="bi bi-arrow-counterclockwise"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                CommandName="DeleteRecord" CommandArgument='<%# Eval("Id") %>'
                                                OnClientClick="return confirm('Delete this COPQ entry?');">
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
        <i class="bi bi-info-circle me-2"></i>Please select a project to track COPQ.
    </asp:Panel>
</asp:Content>
