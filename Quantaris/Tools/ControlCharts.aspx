<%@ Page Title="Control Charts" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="ControlCharts.aspx.vb" Inherits="Tools_ControlCharts" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href='<%= ResolveUrl("~/Content/spreadsheet.css") %>' rel="stylesheet" />
    <script src='<%= ResolveUrl("~/Content/spreadsheet.js") %>'></script>
    <style>
        .cc-chart { position: relative; height: 300px; border-bottom: 1px solid #dee2e6; margin-bottom: 20px; }
        .cc-canvas { position: relative; width: 100%; height: 100%; }
        .cc-line { position: absolute; left: 0; right: 0; border-top: 1px dashed; }
        .cc-line.cl { border-top: 2px solid #198754; }
        .cc-line.ucl { border-top: 2px solid #dc3545; }
        .cc-line.lcl { border-top: 2px solid #dc3545; }
        .cc-point { position: absolute; width: 8px; height: 8px; border-radius: 50%; background: #0d6efd; transform: translate(-50%, -50%); cursor: pointer; }
        .cc-point.oc { background: #dc3545; width: 10px; height: 10px; border: 2px solid #dc3545; background: transparent; }
        .cc-point:hover { transform: translate(-50%, -50%) scale(1.5); }
        .cc-yaxis { position: absolute; left: 0; top: 0; bottom: 0; width: 60px; font-size: 10px; color: #6c757d; }
        .cc-yaxis span { position: absolute; right: 4px; }
        .cc-xaxis { font-size: 10px; color: #6c757d; text-align: center; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-graph-up me-2 text-primary"></i>Control Charts</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Control Charts</li>
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
            <label class="form-label small fw-semibold">Chart Type</label>
            <asp:DropDownList ID="ddlChartType" runat="server" CssClass="form-select form-select-sm">
                <asp:ListItem Value="IMR">I-MR (Individuals)</asp:ListItem>
                <asp:ListItem Value="XbarR">X-bar &amp; R</asp:ListItem>
                <asp:ListItem Value="P">p (Proportion)</asp:ListItem>
                <asp:ListItem Value="U">u (Defects/Unit)</asp:ListItem>
                <asp:ListItem Value="C">c (Defects)</asp:ListItem>
            </asp:DropDownList>
        </div>
        <div class="col-md-2">
            <label class="form-label small fw-semibold">Chart Name</label>
            <asp:TextBox ID="txtChartName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Diameter" />
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100" OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise"></i> Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlCC" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-4">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-pencil-square me-2"></i>Data Entry</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />

                        <div id="spreadsheetContainer"></div>

                        <asp:HiddenField ID="hfSpreadsheetData" runat="server" />
                        <asp:HiddenField ID="hfLoadData" runat="server" />

                        <div class="mb-2 mt-3">
                            <asp:LinkButton ID="btnCalculate" runat="server" CssClass="btn btn-success btn-sm w-100"
                                OnClientClick="return serializeSpreadsheet();"
                                OnClick="btnCalculate_Click">
                                <i class="bi bi-calculator me-1"></i>Calculate Limits
                            </asp:LinkButton>
                        </div>

                        <div class="mt-3 d-flex gap-2">
                            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-outline-success btn-sm" OnClientClick="return serializeSpreadsheet();"
                                OnClick="btnSave_Click" Visible="false">
                                <i class="bi bi-floppy me-1"></i>Save Chart
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnClear" runat="server" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClear_Click">
                                <i class="bi bi-eraser me-1"></i>Clear
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>

                <div class="card shadow-sm mt-4">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>Chart History</h6>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <table class="table table-hover align-middle mb-0 small">
                                <thead class="table-dark">
                                    <tr>
                                        <th>Date</th>
                                        <th>Chart</th>
                                        <th>Type</th>
                                        <th class="text-center">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                        <ItemTemplate>
                                            <tr>
                                                <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd") %></td>
                                                <td class="fw-semibold"><%# Eval("ChartName") %></td>
                                                <td><%# Eval("ChartType") %></td>
                                                <td class="text-center">
                                                    <asp:LinkButton ID="btnLoad" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                        CommandName="LoadChart" CommandArgument='<%# Eval("Id") %>'>
                                                        <i class="bi bi-arrow-counterclockwise"></i>
                                                    </asp:LinkButton>
                                                    <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                        CommandName="DeleteChart" CommandArgument='<%# Eval("Id") %>'
                                                        OnClientClick="return confirm('Delete this chart?');">
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
            </div>

            <div class="col-lg-8">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-graph-up me-2"></i>Chart Display</h6>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlChart" runat="server" Visible="false">
                            <asp:Literal ID="litChartInfo" runat="server" />
                            <div class="cc-chart" id="chartContainer">
                                <asp:Literal ID="litChart" runat="server" />
                            </div>
                            <div class="small text-muted mt-2">
                                <span class="d-inline-block me-3"><span style="border-top:2px solid #198754;display:inline-block;width:20px;margin-right:4px;"></span>Center Line</span>
                                <span class="d-inline-block me-3"><span style="border-top:2px solid #dc3545;display:inline-block;width:20px;margin-right:4px;"></span>UCL / LCL</span>
                                <span class="d-inline-block me-3"><span style="display:inline-block;width:8px;height:8px;border-radius:50%;background:#0d6efd;margin-right:4px;"></span>In Control</span>
                                <span class="d-inline-block"><span style="display:inline-block;width:10px;height:10px;border:2px solid #dc3545;border-radius:50%;margin-right:4px;"></span>Out of Control</span>
                            </div>

                            <asp:Panel ID="pnlRChart" runat="server" Visible="false">
                                <h6 class="mt-4">R Chart</h6>
                                <div class="cc-chart" id="rChartContainer">
                                    <asp:Literal ID="litRChart" runat="server" />
                                </div>
                            </asp:Panel>
                        </asp:Panel>
                        <asp:Panel ID="pnlNoChart" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-graph-up fs-1 d-block mb-2"></i>
                            <span>Enter sample data and calculate control limits.</span>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project.
    </asp:Panel>

    <script type="text/javascript">
        (function () {
            var containerId = 'spreadsheetContainer';
            var chartType = '<%= ddlChartType.SelectedValue %>';

            function getColumnsForChartType(ct) {
                switch (ct) {
                    case 'IMR':
                        return [{ name: 'C1', label: 'Measurement' }];
                    case 'XbarR':
                        return [
                            { name: 'C1', label: 'Subgroup 1' },
                            { name: 'C2', label: 'Subgroup 2' }
                        ];
                    case 'P':
                    case 'U':
                        return [
                            { name: 'C1', label: 'Defects/Count' },
                            { name: 'C2', label: 'Sample Size (n)' }
                        ];
                    case 'C':
                        return [{ name: 'C1', label: 'Defects' }];
                    default:
                        return [{ name: 'C1', label: 'Measurement' }];
                }
            }

            var cols = getColumnsForChartType(chartType);
            Spreadsheet.render(containerId, { columns: cols, rows: 50 });

            var ddl = document.getElementById('<%= ddlChartType.ClientID %>');
            if (ddl) {
                ddl.addEventListener('change', function () {
                    var newCols = getColumnsForChartType(ddl.value);
                    Spreadsheet.setData(containerId, { columns: newCols, rows: 50 });
                });
            }

            var loadData = document.getElementById('<%= hfLoadData.ClientID %>');
            if (loadData && loadData.value) {
                try {
                    var data = JSON.parse(loadData.value);
                    Spreadsheet.setData(containerId, data);
                    loadData.value = '';
                } catch (e) { }
            }
        })();

        function serializeSpreadsheet() {
            var data = Spreadsheet.getData('spreadsheetContainer');
            var hf = document.getElementById('<%= hfSpreadsheetData.ClientID %>');
            if (hf) hf.value = JSON.stringify(data);
            return true;
        }

        function loadSpreadsheetData(jsonStr) {
            try {
                var data = JSON.parse(jsonStr);
                Spreadsheet.setData('spreadsheetContainer', data);
            } catch (e) {
                console.error('Failed to load spreadsheet data:', e);
            }
        }
    </script>
</asp:Content>
