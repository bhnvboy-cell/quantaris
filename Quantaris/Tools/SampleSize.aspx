<%@ Page Title="Sample Size Calculator" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="SampleSize.aspx.vb" Inherits="Tools_SampleSize" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .ss-card { border-left: 3px solid #0d6efd; }
        .ss-result { font-size: 2.5rem; font-weight: 700; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-calculator me-2 text-primary"></i>Sample Size Calculator</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Sample Size</li>
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
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100" OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise"></i> Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlSS" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-4">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-sliders me-2"></i>Parameters</h6></div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Calculation Type</label>
                            <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true"
                                OnSelectedIndexChanged="ddlType_SelectedIndexChanged">
                                <asp:ListItem Value="Continuous">Continuous Data (Mean)</asp:ListItem>
                                <asp:ListItem Value="Proportion">Proportion / Attribute</asp:ListItem>
                                <asp:ListItem Value="Sigma">Sigma / Std Dev Known</asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <asp:Panel ID="pnlContinuous" runat="server">
                            <div class="mb-2">
                                <label class="form-label small fw-semibold">Estimated Std Dev (σ)</label>
                                <asp:TextBox ID="txtStdDev" runat="server" CssClass="form-control form-control-sm" Text="1.0" />
                            </div>
                            <div class="mb-2">
                                <label class="form-label small fw-semibold">Margin of Error (d)</label>
                                <asp:TextBox ID="txtMargin" runat="server" CssClass="form-control form-control-sm" Text="0.5" />
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlProportion" runat="server" Visible="false">
                            <div class="mb-2">
                                <label class="form-label small fw-semibold">Estimated Proportion (p)</label>
                                <asp:TextBox ID="txtProportion" runat="server" CssClass="form-control form-control-sm" Text="0.5" />
                                <div class="small text-muted">Use 0.5 for maximum sample size</div>
                            </div>
                            <div class="mb-2">
                                <label class="form-label small fw-semibold">Margin of Error</label>
                                <asp:TextBox ID="txtPropMargin" runat="server" CssClass="form-control form-control-sm" Text="0.05" />
                            </div>
                        </asp:Panel>

                        <asp:Panel ID="pnlSigma" runat="server" Visible="false">
                            <div class="mb-2">
                                <label class="form-label small fw-semibold">Known Sigma</label>
                                <asp:TextBox ID="txtKnownSigma" runat="server" CssClass="form-control form-control-sm" Text="1.0" />
                            </div>
                            <div class="mb-2">
                                <label class="form-label small fw-semibold">Target Half-Width</label>
                                <asp:TextBox ID="txtHalfWidth" runat="server" CssClass="form-control form-control-sm" Text="0.3" />
                            </div>
                        </asp:Panel>

                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Confidence Level</label>
                            <asp:DropDownList ID="ddlConf" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Value="1.645">90%</asp:ListItem>
                                <asp:ListItem Value="1.96" Selected="True">95%</asp:ListItem>
                                <asp:ListItem Value="2.326">98%</asp:ListItem>
                                <asp:ListItem Value="2.576">99%</asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Population Size <span class="text-muted">(optional, for finite correction)</span></label>
                            <asp:TextBox ID="txtPopulation" runat="server" CssClass="form-control form-control-sm" placeholder="Leave blank if infinite" />
                        </div>

                        <div class="d-flex gap-2">
                            <asp:LinkButton ID="btnCalculate" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnCalculate_Click">
                                <i class="bi bi-calculator me-1"></i>Calculate
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-outline-success btn-sm" OnClick="btnSave_Click" Visible="false">
                                <i class="bi bi-floppy me-1"></i>Save
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>

                <div class="card shadow-sm mt-4">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>History</h6></div>
                    <div class="card-body p-0">
                        <table class="table table-hover align-middle mb-0 small">
                            <thead class="table-dark">
                                <tr><th>Date</th><th>Type</th><th class="text-center">n</th><th class="text-center">Actions</th></tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd") %></td>
                                            <td><%# Eval("CalcType") %></td>
                                            <td class="text-center fw-bold"><%# Eval("Result") %></td>
                                            <td class="text-center">
                                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                    CommandName="DeleteCalc" CommandArgument='<%# Eval("Id") %>'
                                                    OnClientClick="return confirm('Delete?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <div class="col-lg-8">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-graph-up me-2"></i>Result</h6></div>
                    <div class="card-body">
                        <asp:Panel ID="pnlResult" runat="server" Visible="false">
                            <div class="text-center">
                                <div class="text-muted small text-uppercase">Recommended Sample Size</div>
                                <div class="ss-result"><asp:Literal ID="litResult" runat="server" /></div>
                                <div class="small text-muted">observations</div>
                            </div>
                            <div class="row g-2 mt-4">
                                <div class="col-4"><div class="card ss-card py-2 px-3"><small>Confidence</small><strong><asp:Literal ID="litConfLevel" runat="server" /></strong></div></div>
                                <div class="col-4"><div class="card ss-card py-2 px-3"><small>Margin of Error</small><strong><asp:Literal ID="litMarginDisplay" runat="server" /></strong></div></div>
                                <div class="col-4"><div class="card ss-card py-2 px-3"><small>Formula</small><strong><asp:Literal ID="litFormula" runat="server" /></strong></div></div>
                            </div>
                            <div class="alert alert-info mt-3 py-2 small">
                                <i class="bi bi-info-circle me-1"></i><asp:Literal ID="litInterpretationSS" runat="server" />
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlNoResult" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-calculator fs-1 d-block mb-2"></i>
                            <span>Set parameters and calculate sample size.</span>
                        </asp:Panel>
                    </div>
                </div>

                <div class="card shadow-sm mt-4">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-lightbulb me-2"></i>Guidelines</h6></div>
                    <div class="card-body small">
                        <ul class="mb-0">
                            <li><strong>Continuous Data:</strong> n = (Z²σ²) / d² — use for measurement data</li>
                            <li><strong>Proportion:</strong> n = (Z² p(1-p)) / d² — use for defect rates, survey responses</li>
                            <li><strong>Finite Population Correction:</strong> n_adj = n / (1 + (n-1)/N)</li>
                            <li><strong>Rule of thumb:</strong> Minimum 30 for Central Limit Theorem validity</li>
                            <li><strong>Common defaults:</strong> 95% confidence (Z=1.96), margin 0.05 for proportions</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project.
    </asp:Panel>
</asp:Content>
