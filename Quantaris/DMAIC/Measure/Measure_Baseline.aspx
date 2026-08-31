<%@ Page Title="Step 6: Determine Baseline Performance" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Measure_Baseline.aspx.vb" Inherits="DMAIC_Measure_Baseline" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-warning text-dark me-2">6</span>Determine Baseline Performance</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Measure/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Measure</a></li>
                    <li class="breadcrumb-item active">Baseline</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />

    <%-- Section 1: Metric Entry Form --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-graph-up me-2"></i>Baseline Metric Entry</h6>
        </div>
        <div class="card-body">
            <p class="text-muted small mb-3">Record baseline performance metrics for your project. Baseline is the current performance <strong>before</strong> improvements are made.</p>
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Metric Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtMetricName" runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvMetricName" runat="server" ControlToValidate="txtMetricName"
                        ErrorMessage="Metric name is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Data Type</label>
                    <asp:DropDownList ID="ddlDataType" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Select --" Value="" />
                        <asp:ListItem Text="Continuous" Value="Continuous" />
                        <asp:ListItem Text="Discrete" Value="Discrete" />
                        <asp:ListItem Text="Attribute" Value="Attribute" />
                        <asp:ListItem Text="Ordinal" Value="Ordinal" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label class="form-label">Baseline</label>
                    <asp:TextBox ID="txtBaseline" runat="server" CssClass="form-control" MaxLength="18"
                        placeholder="Current value" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Target</label>
                    <asp:TextBox ID="txtTarget" runat="server" CssClass="form-control" MaxLength="18"
                        placeholder="Goal value" />
                </div>
                <div class="col-md-1">
                    <label class="form-label">Current</label>
                    <asp:TextBox ID="txtCurrent" runat="server" CssClass="form-control" MaxLength="18"
                        placeholder="Latest" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Sigma Level</label>
                    <asp:TextBox ID="txtSigmaLevel" runat="server" CssClass="form-control" MaxLength="18"
                        placeholder="e.g. 2.5" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">DPMO</label>
                    <asp:TextBox ID="txtDPMO" runat="server" CssClass="form-control" MaxLength="18"
                        placeholder="Defects Per Million Opportunities" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Cp</label>
                    <asp:TextBox ID="txtCp" runat="server" CssClass="form-control" MaxLength="18"
                        placeholder="Process capability" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Cpk</label>
                    <asp:TextBox ID="txtCpk" runat="server" CssClass="form-control" MaxLength="18"
                        placeholder="Process capability index" />
                </div>
                <div class="col-12">
                    <label class="form-label">Normality Test Result</label>
                    <asp:DropDownList ID="ddlNormality" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Not Tested --" Value="" />
                        <asp:ListItem Text="Normal" Value="Normal" />
                        <asp:ListItem Text="Not Normal" Value="Not Normal" />
                        <asp:ListItem Text="Borderline" Value="Borderline" />
                    </asp:DropDownList>
                </div>
            </div>
            <div class="mt-3">
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary"
                    OnClick="btnSave_Click"><i class='bi bi-check-lg'></i> Save</asp:LinkButton>
                <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary ms-2"
                    Text="Cancel" OnClick="btnCancel_Click" Visible="false" />
            </div>
        </div>
    </div>

    <%-- Section 2: Existing Metrics Table --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-table me-2"></i>Baseline Metrics</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litRecordCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Metric Name</th>
                            <th>Type</th>
                            <th>Baseline</th>
                            <th>Target</th>
                            <th>Current</th>
                            <th>Sigma</th>
                            <th>DPMO</th>
                            <th>Cp</th>
                            <th>Cpk</th>
                            <th>Normality</th>
                            <th class="text-center" style="width:90px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptMetrics" runat="server" OnItemCommand="rptMetrics_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-semibold"><%# Eval("MetricName") %></td>
                                    <td><%# Eval("DataType") %></td>
                                    <td><%# Eval("Baseline", "{0:N2}") %></td>
                                    <td><%# Eval("Target", "{0:N2}") %></td>
                                    <td><%# Eval("CurrentValue", "{0:N2}") %></td>
                                    <td><%# Eval("SigmaLevel", "{0:N2}") %></td>
                                    <td><%# Eval("DPMO", "{0:N0}") %></td>
                                    <td><%# Eval("CapabilityCp", "{0:N2}") %></td>
                                    <td><%# Eval("CapabilityCpk", "{0:N2}") %></td>
                                    <td><%# Eval("NormalityTestResult") %></td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this metric?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between">
        <a href='<%= ResolveUrl("~/DMAIC/Measure/Measure_PlanCollectData.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Plan &amp; Collect Data
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-success">
            <i class="bi bi-check-circle me-1"></i>Complete Measure Phase
        </a>
    </div>
</asp:Content>
