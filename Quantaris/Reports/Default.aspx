<%@ Page Title="Reports Dashboard" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="Reports_Default" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex flex-wrap justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-file-earmark-bar-graph me-2"></i>Reports Dashboard</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Reports</li>
                </ol>
            </nav>
        </div>
        <div class="d-flex gap-2">
            <button type="button" class="btn btn-outline-dark btn-sm rounded-pill px-3" onclick="window.print()">
                <i class="bi bi-printer me-1"></i>Print
            </button>
            <button type="button" class="btn btn-outline-primary btn-sm rounded-pill px-3" onclick="exportPDF()">
                <i class="bi bi-file-earmark-pdf me-1"></i>Export PDF
            </button>
            <a href='<%= ResolveUrl("~/Reports/Default.aspx?export=csv&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-success btn-sm rounded-pill px-3">
                <i class="bi bi-filetype-csv me-1"></i>Export CSV
            </a>
        </div>
    </div>

    <style media="print">
        .navbar, .sidebar, .footer, nav.breadcrumb, .btn { display: none !important; }
        .main-content { margin-left: 0 !important; padding-top: 0 !important; }
        .card { box-shadow: none !important; border: 1px solid #dee2e6 !important; }
        .card-header { background: #212529 !important; color: #fff !important; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        .table-dark { background: #212529 !important; color: #fff !important; -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        .badge { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        .progress-bar { -webkit-print-color-adjust: exact; print-color-adjust: exact; }
        body { font-size: 12px; }
        .container-fluid { padding: 0 !important; }
    </style>

    <div class="row g-4">
        <div class="col-12">
            <div class="card shadow-sm">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-check2-circle me-2"></i>Project DMAIC Completion Status</h6>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-hover table-bordered align-middle mb-0 small">
                            <thead class="table-dark">
                                <tr>
                                    <th>Project Code</th>
                                    <th>Project Name</th>
                                    <th>Phase</th>
                                    <th>Define %</th>
                                    <th>Measure %</th>
                                    <th>Analyze %</th>
                                    <th>Improve %</th>
                                    <th>Control %</th>
                                    <th>Overall %</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptCompletion" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="fw-semibold"><%# Eval("ProjectCode") %></td>
                                            <td><%# Eval("ProjectName") %></td>
                                            <td><span class="badge bg-info"><%# Eval("Phase") %></span></td>
                                            <td><%# Eval("DefinePct", "{0:N0}") %>%</td>
                                            <td><%# Eval("MeasurePct", "{0:N0}") %>%</td>
                                            <td><%# Eval("AnalyzePct", "{0:N0}") %>%</td>
                                            <td><%# Eval("ImprovePct", "{0:N0}") %>%</td>
                                            <td><%# Eval("ControlPct", "{0:N0}") %>%</td>
                                            <td>
                                                <div class="d-flex align-items-center">
                                                    <div class="progress flex-grow-1" style="height: 8px;">
                                                        <div class="progress-bar bg-success" role="progressbar"
                                                            style='width: <%# Eval("OverallPct") %>%;'
                                                            aria-valuenow='<%# Eval("OverallPct") %>' aria-valuemin="0" aria-valuemax="100">
                                                        </div>
                                                    </div>
                                                    <span class="ms-2 fw-semibold"><%# Eval("OverallPct", "{0:N0}") %>%</span>
                                                </div>
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

        <div class="col-md-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-currency-dollar me-2"></i>Financial Impact Summary</h6>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-hover table-bordered align-middle mb-0 small">
                            <thead class="table-dark">
                                <tr>
                                    <th>Project Code</th>
                                    <th>Project Name</th>
                                    <th>Status</th>
                                    <th class="text-end">Financial Impact</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptFinancial" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="fw-semibold"><%# Eval("ProjectCode") %></td>
                                            <td><%# Eval("ProjectName") %></td>
                                            <td><span class="badge <%# GetStatusBadge(Eval("Status")) %>"><%# Eval("Status") %></span></td>
                                            <td class="text-end"><%# String.Format("{0:N2}", Eval("FinancialImpact")) %></td>
                                        </tr>
                                    </ItemTemplate>
                                    
                                </asp:Repeater>
                            </tbody>
                            <tfoot>
                                <tr class="table-secondary fw-bold">
                                    <td colspan="3" class="text-end">Total Financial Impact:</td>
                                    <td class="text-end"><asp:Literal ID="litTotalImpact" runat="server" /></td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-md-6">
            <div class="card shadow-sm h-100">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-exclamation-triangle me-2"></i>High-RPN FMEA Items (RPN &ge; 100)</h6>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-hover table-bordered align-middle mb-0 small">
                            <thead class="table-dark">
                                <tr>
                                    <th>Project</th>
                                    <th>Process Step</th>
                                    <th>Failure Mode</th>
                                    <th class="text-center">S</th>
                                    <th class="text-center">O</th>
                                    <th class="text-center">D</th>
                                    <th class="text-center">RPN</th>
                                    <th>Recommended Action</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptRPN" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="fw-semibold"><%# Eval("ProjectCode") %> - <%# Eval("ProjectName") %></td>
                                            <td><%# Eval("ProcessStep") %></td>
                                            <td><%# Eval("FailureMode") %></td>
                                            <td class="text-center"><%# Eval("Severity") %></td>
                                            <td class="text-center"><%# Eval("Occurrence") %></td>
                                            <td class="text-center"><%# Eval("Detection") %></td>
                                            <td class="text-center"><span class="badge <%# If(If(IsDBNull(Eval("RPN")), 0, CInt(Eval("RPN"))) >= 200, "bg-danger", "bg-warning text-dark") %>"><%# Eval("RPN") %></span></td>
                                            <td><%# Eval("RecommendedAction") %></td>
                                        </tr>
                                    </ItemTemplate>
                                    
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-12">
            <div class="card shadow-sm">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-star me-2"></i>Best Practices Across Tenants</h6>
                </div>
                <div class="card-body">
                    <div class="table-responsive">
                        <table class="table table-hover table-bordered align-middle mb-0 small">
                            <thead class="table-dark">
                                <tr>
                                    <th>Tenant</th>
                                    <th>Project</th>
                                    <th>Title</th>
                                    <th>Areas</th>
                                    <th>Replicated</th>
                                    <th>Created</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptBestPractices" runat="server">
                                    <ItemTemplate>
                                        <tr>
                                            <td class="fw-semibold"><%# Eval("TenantName") %></td>
                                            <td><%# Eval("ProjectCode") %> - <%# Eval("ProjectName") %></td>
                                            <td><%# Eval("Title") %></td>
                                            <td><%# Eval("Areas") %></td>
                                            <td>
                                                <span class="badge <%# If(If(IsDBNull(Eval("IsReplicated")), False, CBool(Eval("IsReplicated"))), "bg-success", "bg-secondary") %>">
                                                    <i class="bi <%# If(If(IsDBNull(Eval("IsReplicated")), False, CBool(Eval("IsReplicated"))), "bi-check-circle", "bi-clock") %>"></i>
                                                    <%# If(If(IsDBNull(Eval("IsReplicated")), False, CBool(Eval("IsReplicated"))), "Replicated", "Pending") %>
                                                </span>
                                            </td>
                                            <td><%# If(Eval("CreatedOn") IsNot DBNull.Value, CDate(Eval("CreatedOn")).ToString("MMM dd, yyyy"), "-") %></td>
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
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Scripts" runat="server">
    <script>
        function exportPDF() {
            window.print();
        }
        setTimeout(function () { location.reload(); }, 60000);
    </script>
</asp:Content>
