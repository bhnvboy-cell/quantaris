<%@ Page Title="Benefits Tracking" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="BenefitsTracking.aspx.vb" Inherits="Tools_BenefitsTracking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .ben-bar { height: 12px; border-radius: 3px; transition: width 0.3s; }
        .ben-card { border-left: 3px solid; }
        .ben-card.savings { border-left-color: #198754; }
        .ben-card.revenue { border-left-color: #0d6efd; }
        .ben-card.cost { border-left-color: #dc3545; }
        .ben-card.quality { border-left-color: #ffc107; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-cash-stack me-2 text-primary"></i>Benefits Tracking</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Benefits</li>
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
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnExportCsv" runat="server" CssClass="btn btn-outline-success btn-sm rounded-pill px-3" OnClick="btnExportCsv_Click">
                <i class="bi bi-download me-1"></i>Export CSV
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlBenefits" runat="server" Visible="false">
        <!-- Summary Cards -->
        <asp:Panel ID="pnlSummary" runat="server" Visible="false">
            <div class="row g-3 mb-4">
                <div class="col-3"><div class="card ben-card savings py-2 px-3"><small class="text-muted">Total Savings</small><h4 class="mb-0"><asp:Literal ID="litTotalSavings" runat="server" /></h4></div></div>
                <div class="col-3"><div class="card ben-card revenue py-2 px-3"><small class="text-muted">Total Revenue</small><h4 class="mb-0"><asp:Literal ID="litTotalRevenue" runat="server" /></h4></div></div>
                <div class="col-3"><div class="card ben-card cost py-2 px-3"><small class="text-muted">Cost Avoided</small><h4 class="mb-0"><asp:Literal ID="litCostAvoided" runat="server" /></h4></div></div>
                <div class="col-3"><div class="card ben-card quality py-2 px-3"><small class="text-muted">Quality Impact</small><h4 class="mb-0"><asp:Literal ID="litQualityImpact" runat="server" /></h4></div></div>
            </div>
        </asp:Panel>

        <div class="row g-4">
            <div class="col-lg-4">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-plus-circle me-2"></i>Add Benefit</h6></div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Benefit Name</label>
                            <asp:TextBox ID="txtBenefitName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Defect reduction savings" />
                        </div>
                        <div class="row g-2 mb-2">
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Type</label>
                                <asp:DropDownList ID="ddlBenefitType" runat="server" CssClass="form-select form-select-sm">
                                    <asp:ListItem Value="Savings">Cost Savings</asp:ListItem>
                                    <asp:ListItem Value="Revenue">Revenue Increase</asp:ListItem>
                                    <asp:ListItem Value="CostAvoidance">Cost Avoidance</asp:ListItem>
                                    <asp:ListItem Value="Quality">Quality Impact</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Category</label>
                                <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select form-select-sm">
                                    <asp:ListItem Value="Direct">Direct</asp:ListItem>
                                    <asp:ListItem Value="Indirect">Indirect</asp:ListItem>
                                    <asp:ListItem Value="Strategic">Strategic</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="row g-2 mb-2">
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Amount</label>
                                <asp:TextBox ID="txtAmount" runat="server" CssClass="form-control form-control-sm" TextMode="Number" />
                            </div>
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Currency</label>
                                <asp:DropDownList ID="ddlCurrency" runat="server" CssClass="form-select form-select-sm">
                                    <asp:ListItem Value="USD">USD</asp:ListItem>
                                    <asp:ListItem Value="EUR">EUR</asp:ListItem>
                                    <asp:ListItem Value="GBP">GBP</asp:ListItem>
                                    <asp:ListItem Value="INR">INR</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="row g-2 mb-2">
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Start Month</label>
                                <asp:TextBox ID="txtStartMonth" runat="server" CssClass="form-control form-control-sm" TextMode="Month" />
                            </div>
                            <div class="col-6">
                                <label class="form-label small fw-semibold">End Month</label>
                                <asp:TextBox ID="txtEndMonth" runat="server" CssClass="form-control form-control-sm" TextMode="Month" />
                            </div>
                        </div>
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Status</label>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Value="Planned">Planned</asp:ListItem>
                                <asp:ListItem Value="InProgress">In Progress</asp:ListItem>
                                <asp:ListItem Value="Achieved">Achieved</asp:ListItem>
                                <asp:ListItem Value="AtRisk">At Risk</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Notes</label>
                            <asp:TextBox ID="txtNotes" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                        </div>
                        <asp:LinkButton ID="btnAddBenefit" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnAddBenefit_Click">
                            <i class="bi bi-plus-lg me-1"></i>Add Benefit
                        </asp:LinkButton>
                    </div>
                </div>
            </div>

            <div class="col-lg-8">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-list-check me-2"></i>Benefits Register</h6></div>
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <table class="table table-hover align-middle mb-0 small">
                                <thead class="table-dark">
                                    <tr><th>Name</th><th>Type</th><th class="text-end">Amount</th><th>Period</th><th>Status</th><th class="text-center">Actions</th></tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptBenefits" runat="server" OnItemCommand="rptBenefits_ItemCommand">
                                        <ItemTemplate>
                                            <tr>
                                                <td class="fw-semibold"><%# Eval("BenefitName") %></td>
                                                <td><span class="badge bg-secondary"><%# Eval("BenefitType") %></span></td>
                                                <td class="text-end"><%# FormatCurrency(Eval("Amount")) %></td>
                                                <td class="text-nowrap"><%# If(Eval("StartMonth") IsNot DBNull.Value, CDate(Eval("StartMonth")).ToString("MMM yy"), "?") %> - <%# If(Eval("EndMonth") IsNot DBNull.Value, CDate(Eval("EndMonth")).ToString("MMM yy"), "?") %></td>
                                                <td><span class="badge <%# GetStatusBadge(Eval("Status").ToString()) %>"><%# Eval("Status") %></span></td>
                                                <td class="text-center">
                                                    <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                        CommandName="DeleteBenefit" CommandArgument='<%# Eval("Id") %>'
                                                        OnClientClick="return confirm('Delete this benefit?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>

                <!-- Benefits by Month Chart -->
                <asp:Panel ID="pnlChart" runat="server" Visible="false">
                    <div class="card shadow-sm mt-4">
                        <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-bar-chart me-2"></i>Benefits Timeline</h6></div>
                        <div class="card-body">
                            <asp:Literal ID="litChart" runat="server" />
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project.
    </asp:Panel>
</asp:Content>
