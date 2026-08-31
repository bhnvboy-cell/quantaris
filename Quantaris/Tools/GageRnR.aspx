<%@ Page Title="Gage R&amp;R" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="GageRnR.aspx.vb" Inherits="Tools_GageRnR" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .grr-table { font-size: 12px; }
        .grr-table th { white-space: nowrap; font-size: 11px; vertical-align: middle; }
        .grr-table input { width: 60px; text-align: center; font-size: 12px; padding: 2px 4px; }
        .grr-table .part-cell { font-weight: 500; background: #f8f9fa; }
        .grr-pass { background: #d4edda; }
        .grr-warn { background: #fff3cd; }
        .grr-fail { background: #f8d7da; }
        .result-card .display-6 { font-size: 2.2rem; font-weight: 700; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-rulers me-2 text-primary"></i>Gage R&amp;R (Measurement System Analysis)</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Gage R&amp;R</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-md-5">
            <label class="form-label small fw-semibold">Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true"
                OnSelectedIndexChanged="ddlProject_SelectedIndexChanged" />
        </div>
        <div class="col-md-3">
            <label class="form-label small fw-semibold">Study Name</label>
            <asp:TextBox ID="txtStudyName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Diameter Gage R&R" />
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100" OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise"></i> Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlGRR" runat="server" Visible="false">
        <div class="row g-4">
            <!-- Left: Setup -->
            <div class="col-lg-4">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-gear me-2"></i>Study Setup</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="row g-3">
                            <div class="col-4">
                                <label class="form-label small fw-semibold">Parts</label>
                                <asp:TextBox ID="txtPartCount" runat="server" CssClass="form-control form-control-sm" TextMode="Number"
                                    Text="5" min="2" max="20" />
                            </div>
                            <div class="col-4">
                                <label class="form-label small fw-semibold">Appraisers</label>
                                <asp:TextBox ID="txtAppraiserCount" runat="server" CssClass="form-control form-control-sm" TextMode="Number"
                                    Text="2" min="2" max="10" />
                            </div>
                            <div class="col-4">
                                <label class="form-label small fw-semibold">Trials</label>
                                <asp:TextBox ID="txtTrialCount" runat="server" CssClass="form-control form-control-sm" TextMode="Number"
                                    Text="2" min="2" max="5" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Part Names <span class="text-muted fw-normal">(comma separated)</span></label>
                                <asp:TextBox ID="txtPartNames" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="Part-1, Part-2, Part-3..." Rows="2" TextMode="MultiLine" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Appraiser Names</label>
                                <asp:TextBox ID="txtAppraiserNames" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="A, B, C..." />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">USL / Tolerance (optional)</label>
                                <div class="row g-2">
                                    <div class="col-6">
                                        <asp:TextBox ID="txtUSL" runat="server" CssClass="form-control form-control-sm" placeholder="USL" />
                                    </div>
                                    <div class="col-6">
                                        <asp:TextBox ID="txtLSL" runat="server" CssClass="form-control form-control-sm" placeholder="LSL" />
                                    </div>
                                </div>
                            </div>
                            <div class="col-12 d-flex gap-2">
                                <asp:LinkButton ID="btnSetup" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnSetup_Click">
                                    <i class="bi bi-table me-1"></i>Setup Grid
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnClear" runat="server" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClear_Click">
                                    <i class="bi bi-x-circle me-1"></i>Clear
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Right: Data Entry Grid -->
            <div class="col-lg-8">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white d-flex justify-content-between">
                        <h6 class="mb-0"><i class="bi bi-grid me-2"></i>Measurement Data</h6>
                        <div>
                            <asp:LinkButton ID="btnCalculate" runat="server" CssClass="btn btn-success btn-sm me-1"
                                OnClick="btnCalculate_Click" Visible="false">
                                <i class="bi bi-calculator me-1"></i>Calculate
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-outline-light btn-sm"
                                OnClick="btnSave_Click" Visible="false">
                                <i class="bi bi-floppy me-1"></i>Save
                            </asp:LinkButton>
                        </div>
                    </div>
                    <div class="card-body p-2 overflow-auto" style="max-height:500px;">
                        <asp:Panel ID="pnlGrid" runat="server" Visible="false">
                            <asp:Literal ID="litGrid" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlNoGrid" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-grid-3x3 fs-1 d-block mb-2"></i>
                            <span>Set study parameters and click <strong>Setup Grid</strong> to begin.</span>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <!-- Results -->
        <asp:Panel ID="pnlResults" runat="server" Visible="false">
            <div class="card shadow-sm mt-4">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-clipboard-data me-2"></i>Gage R&amp;R Results</h6>
                </div>
                <div class="card-body">
                    <div class="row g-4">
                        <!-- Main GRR metric -->
                        <div class="col-md-3">
                            <div class="card text-center border-0 grr-pass">
                                <div class="card-body py-3">
                                    <div class="small text-muted text-uppercase">%GRR</div>
                                    <div class="display-6"><asp:Literal ID="litPctGRR" runat="server" /></div>
                                    <div class="small"><asp:Literal ID="litGRRAssessment" runat="server" /></div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="card text-center border-0 bg-light">
                                <div class="card-body py-3">
                                    <div class="small text-muted text-uppercase">ndc</div>
                                    <div class="display-6"><asp:Literal ID="litNDC" runat="server" /></div>
                                    <div class="small">Distinct Categories</div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="card text-center border-0 bg-light">
                                <div class="card-body py-3">
                                    <div class="small text-muted text-uppercase">Total Variation</div>
                                    <div class="display-6"><asp:Literal ID="litTV" runat="server" /></div>
                                    <div class="small">Std Dev</div>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-3">
                            <div class="card text-center border-0 bg-light">
                                <div class="card-body py-3">
                                    <div class="small text-muted text-uppercase">Study Var</div>
                                    <div class="display-6"><asp:Literal ID="litStudyVar" runat="server" /></div>
                                    <div class="small">5.15 * TV</div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Variance Components Table -->
                    <div class="table-responsive mt-4">
                        <table class="table table-bordered table-sm small grr-table">
                            <thead class="table-dark">
                                <tr>
                                    <th>Source</th>
                                    <th class="text-end">Variance</th>
                                    <th class="text-end">Std Dev</th>
                                    <th class="text-end">Study Var (5.15)</th>
                                    <th class="text-end">%Contribution</th>
                                    <th class="text-end">%Study Var</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr><td>Total Gage R&amp;R</td>
                                    <td class="text-end"><asp:Literal ID="litVarGRR" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSD_GRR" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSV_GRR" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litPctContGRR" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litPctSV_GRR" runat="server" /></td>
                                </tr>
                                <tr><td>&nbsp;&nbsp;Repeatability (EV)</td>
                                    <td class="text-end"><asp:Literal ID="litVarEV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSD_EV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSV_EV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litPctContEV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litPctSV_EV" runat="server" /></td>
                                </tr>
                                <tr><td>&nbsp;&nbsp;Reproducibility (AV)</td>
                                    <td class="text-end"><asp:Literal ID="litVarAV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSD_AV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSV_AV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litPctContAV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litPctSV_AV" runat="server" /></td>
                                </tr>
                                <tr><td>Part-to-Part (PV)</td>
                                    <td class="text-end"><asp:Literal ID="litVarPV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSD_PV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSV_PV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litPctContPV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litPctSV_PV" runat="server" /></td>
                                </tr>
                                <tr class="fw-bold"><td>Total Variation (TV)</td>
                                    <td class="text-end"><asp:Literal ID="litVarTV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSD_TV" runat="server" /></td>
                                    <td class="text-end"><asp:Literal ID="litSV_TV" runat="server" /></td>
                                    <td class="text-end">100.00%</td>
                                    <td class="text-end">100.00%</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <!-- Interpretation -->
                    <div class="alert alert-info d-flex align-items-center mb-0 py-2" role="alert">
                        <i class="bi bi-info-circle me-2 fs-5"></i>
                        <span><asp:Literal ID="litInterpretation" runat="server" /></span>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <!-- History -->
        <div class="card shadow-sm mt-4">
            <div class="card-header bg-dark text-white">
                <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>Study History</h6>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover align-middle mb-0 small">
                        <thead class="table-dark">
                            <tr>
                                <th>Date</th>
                                <th>Study Name</th>
                                <th class="text-center">Parts</th>
                                <th class="text-center">Appraisers</th>
                                <th class="text-center">Trials</th>
                                <th class="text-center">%GRR</th>
                                <th class="text-center">ndc</th>
                                <th>Conclusion</th>
                                <th class="text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd, HH:mm") %></td>
                                        <td class="fw-semibold"><%# Eval("StudyName") %></td>
                                        <td class="text-center"><%# Eval("PartCount") %></td>
                                        <td class="text-center"><%# Eval("AppraiserCount") %></td>
                                        <td class="text-center"><%# Eval("TrialCount") %></td>
                                        <td class="text-center"><span class="badge <%# GetGRRBadge(Eval("Conclusion").ToString()) %>"><%# GetGRRValue(Eval("ResultsJson")) %></span></td>
                                        <td class="text-center"><%# GetNDCValue(Eval("ResultsJson")) %></td>
                                        <td><span class="badge <%# GetGRRBadge(Eval("Conclusion").ToString()) %>"><%# Eval("Conclusion") %></span></td>
                                        <td class="text-center">
                                            <asp:LinkButton ID="btnLoad" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                CommandName="LoadStudy" CommandArgument='<%# Eval("Id") %>'>
                                                <i class="bi bi-arrow-counterclockwise"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                CommandName="DeleteStudy" CommandArgument='<%# Eval("Id") %>'
                                                OnClientClick="return confirm('Delete this Gage R&R study?');">
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
        <i class="bi bi-info-circle me-2"></i>Please select a project.
    </asp:Panel>
</asp:Content>
