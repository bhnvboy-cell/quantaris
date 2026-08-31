<%@ Page Title="Regression Analysis" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="Regression.aspx.vb" Inherits="Tools_Regression" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href='<%= ResolveUrl("~/Content/spreadsheet.css") %>' rel="stylesheet" />
    <script src='<%= ResolveUrl("~/Content/spreadsheet.js") %>'></script>
    <style>
        .reg-scatter { position: relative; height: 280px; border-bottom: 1px solid #dee2e6; }
        .reg-point { position: absolute; width: 6px; height: 6px; border-radius: 50%; background: #0d6efd; transform: translate(-50%, -50%); }
        .reg-line { position: absolute; height: 2px; background: #dc3545; transform-origin: 0 0; }
        .resid-bar { position: absolute; width: 4px; background: #198754; transform: translateX(-50%); }
        .resid-bar.neg { background: #dc3545; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-graph-up-arrow me-2 text-primary"></i>Regression Analysis</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Regression</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-md-4">
            <label class="form-label small fw-semibold">Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlProject_SelectedIndexChanged" />
        </div>
        <div class="col-md-3">
            <label class="form-label small fw-semibold">Model Name</label>
            <asp:TextBox ID="txtModelName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Yield Model" />
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100" OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise"></i> Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlReg" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-4">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-input-cursor me-2"></i>Data Entry</h6></div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Response (Y) Name</label>
                            <asp:TextBox ID="txtYName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Yield" />
                        </div>
                        <div class="mb-2">
                            <label class="form-label small fw-semibold">Predictor(s) (X)</label>
                            <asp:TextBox ID="txtXNames" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Temperature, Pressure" />
                            <div class="small text-muted">Comma-separated (matches column headers)</div>
                        </div>

                        <div id="regSpreadsheetContainer"></div>

                        <asp:HiddenField ID="hfSpreadsheetData" runat="server" />
                        <asp:HiddenField ID="hfLoadData" runat="server" />

                        <div class="d-flex gap-2 mt-3">
                            <asp:LinkButton ID="btnRun" runat="server" CssClass="btn btn-primary btn-sm" OnClientClick="return serializeRegSpreadsheet();"
                                OnClick="btnRun_Click">
                                <i class="bi bi-calculator me-1"></i>Run Regression
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-outline-success btn-sm" OnClientClick="return serializeRegSpreadsheet();"
                                OnClick="btnSave_Click" Visible="false">
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
                                <tr><th>Date</th><th>Model</th><th class="text-center">Actions</th></tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd") %></td>
                                            <td><%# Eval("ModelName") %></td>
                                            <td class="text-center">
                                                <asp:LinkButton ID="btnLoad" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                    CommandName="LoadModel" CommandArgument='<%# Eval("Id") %>'>
                                                    <i class="bi bi-arrow-counterclockwise"></i>
                                                </asp:LinkButton>
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

            <div class="col-lg-8">
                <asp:Panel ID="pnlResults" runat="server" Visible="false">
                    <div class="card shadow-sm mb-4">
                        <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-table me-2"></i>Regression Summary</h6></div>
                        <div class="card-body">
                            <div class="row g-2 mb-3">
                                <div class="col-3"><div class="card bg-light py-2 px-3"><small>R²</small><strong><asp:Literal ID="litRSq" runat="server" /></strong></div></div>
                                <div class="col-3"><div class="card bg-light py-2 px-3"><small>Adj. R²</small><strong><asp:Literal ID="litAdjRSq" runat="server" /></strong></div></div>
                                <div class="col-3"><div class="card bg-light py-2 px-3"><small>F-Stat</small><strong><asp:Literal ID="litFStat" runat="server" /></strong></div></div>
                                <div class="col-3"><div class="card bg-light py-2 px-3"><small>Std Error</small><strong><asp:Literal ID="litStdErr" runat="server" /></strong></div></div>
                            </div>
                            <div class="table-responsive">
                                <table class="table table-bordered table-sm small">
                                    <thead class="table-dark">
                                        <tr><th>Predictor</th><th class="text-end">Coefficient</th><th class="text-end">Std Error</th><th class="text-end">t-Stat</th><th class="text-end">p-Value</th></tr>
                                    </thead>
                                    <tbody>
                                        <asp:Literal ID="litCoefficients" runat="server" />
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    </div>

                    <div class="card shadow-sm mb-4">
                        <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-graph-up me-2"></i>Scatter Plot</h6></div>
                        <div class="card-body">
                            <div class="reg-scatter"><asp:Literal ID="litScatter" runat="server" /></div>
                        </div>
                    </div>

                    <div class="card shadow-sm">
                        <div class="card-header bg-dark text-white"><h6 class="mb-0"><i class="bi bi-bar-chart me-2"></i>Residual Plot</h6></div>
                        <div class="card-body">
                            <div class="reg-scatter"><asp:Literal ID="litResiduals" runat="server" /></div>
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlNoResults" runat="server" CssClass="text-center text-muted py-5">
                    <i class="bi bi-graph-up-arrow fs-1 d-block mb-2"></i>
                    <span>Enter data and run regression analysis.</span>
                </asp:Panel>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project.
    </asp:Panel>

    <script type="text/javascript">
        (function () {
            var containerId = 'regSpreadsheetContainer';
            Spreadsheet.render(containerId, {
                columns: [
                    { name: 'C1', label: 'Y' },
                    { name: 'C2', label: 'X1' },
                    { name: 'C3', label: 'X2' }
                ],
                rows: 50
            });

            var loadData = document.getElementById('<%= hfLoadData.ClientID %>');
            if (loadData && loadData.value) {
                try {
                    var data = JSON.parse(loadData.value);
                    Spreadsheet.setData(containerId, data);
                    loadData.value = '';
                } catch (e) { }
            }
        })();

        function serializeRegSpreadsheet() {
            var data = Spreadsheet.getData('regSpreadsheetContainer');
            var hf = document.getElementById('<%= hfSpreadsheetData.ClientID %>');
            if (hf) hf.value = JSON.stringify(data);
            return true;
        }

        function loadRegSpreadsheetData(jsonStr) {
            try {
                var data = JSON.parse(jsonStr);
                Spreadsheet.setData('regSpreadsheetContainer', data);
            } catch (e) {
                console.error('Failed to load regression spreadsheet data:', e);
            }
        }
    </script>
</asp:Content>
