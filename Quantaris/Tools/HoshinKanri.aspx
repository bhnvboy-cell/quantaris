<%@ Page Title="Hoshin Kanri" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="HoshinKanri.aspx.vb" Inherits="Tools_HoshinKanri" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .hk-matrix { font-size: 11px; }
        .hk-matrix th, .hk-matrix td { vertical-align: middle; padding: 6px 8px; }
        .hk-strategic { background: #cfe2ff; }
        .hk-tactical { background: #e2f0e2; }
        .hk-operational { background: #fff3cd; }
        .hk-progress { height: 6px; border-radius: 3px; }
        .hk-progress-bar { height: 100%; border-radius: 3px; transition: width 0.3s; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-diagram-2 me-2 text-primary"></i>Hoshin Kanri - Policy Deployment</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Hoshin Kanri</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-md-4">
            <label class="form-label small fw-semibold">Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true"
                OnSelectedIndexChanged="ddlProject_SelectedIndexChanged" />
        </div>
        <div class="col-md-2">
            <label class="form-label small fw-semibold">Model Name</label>
            <asp:TextBox ID="txtModelName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Hoshin 2026" />
        </div>
        <div class="col-md-2">
            <label class="form-label small fw-semibold">Fiscal Year</label>
            <asp:TextBox ID="txtFiscalYear" runat="server" CssClass="form-control form-control-sm" TextMode="Number" Text="2026" />
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100" OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise"></i> Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlHK" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-3">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-plus-circle me-2"></i>Add Item</h6></div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Level</label>
                            <asp:DropDownList ID="ddlLevel" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Value="Strategic">Strategic Objective</asp:ListItem>
                                <asp:ListItem Value="Tactical">Tactical Initiative</asp:ListItem>
                                <asp:ListItem Value="Operational">Operational Action</asp:ListItem>
                            </asp:DropDownList>
                        </div>
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Item Name</label>
                            <asp:TextBox ID="txtItemName" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                        </div>
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Owner</label>
                            <asp:TextBox ID="txtOwner" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Target Date</label>
                            <asp:TextBox ID="txtTargetDate" runat="server" CssClass="form-control form-control-sm" TextMode="Date" />
                        </div>
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Metrics / KPI</label>
                            <asp:TextBox ID="txtMetrics" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Reduce defects by 50%" />
                        </div>
                        <asp:LinkButton ID="btnAdd" runat="server" CssClass="btn btn-primary btn-sm w-100" OnClick="btnAdd_Click">
                            <i class="bi bi-plus-lg me-1"></i>Add Item
                        </asp:LinkButton>
                    </div>
                </div>

                <div class="card shadow-sm mt-4">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>History</h6></div>
                    <div class="card-body p-0">
                        <table class="table table-hover align-middle mb-0 small">
                            <thead class="table-dark">
                                <tr><th>Model</th><th>Year</th><th class="text-center">Actions</th></tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("ModelName") %></td>
                                            <td><%# Eval("FiscalYear") %></td>
                                            <td class="text-center">
                                                <asp:LinkButton ID="btnLoad" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                    CommandName="LoadModel" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-arrow-counterclockwise"></i></asp:LinkButton>
                                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                    CommandName="DeleteModel" CommandArgument='<%# Eval("Id") %>'
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

            <div class="col-lg-9">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                        <h6 class="mb-0"><i class="bi bi-table me-2"></i>Hoshin Kanri X-Matrix</h6>
                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-outline-light btn-sm" OnClick="btnSave_Click" Visible="false">
                            <i class="bi bi-floppy me-1"></i>Save Model
                        </asp:LinkButton>
                    </div>
                    <div class="card-body p-2 overflow-auto" style="max-height:600px;">
                        <asp:Panel ID="pnlMatrix" runat="server" Visible="false">
                            <asp:Literal ID="litMatrix" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlNoMatrix" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-diagram-2 fs-1 d-block mb-2"></i>
                            <span>Add strategic objectives and tactical initiatives to build the X-Matrix.</span>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project.
    </asp:Panel>
</asp:Content>
