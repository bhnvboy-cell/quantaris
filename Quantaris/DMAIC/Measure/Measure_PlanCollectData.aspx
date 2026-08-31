<%@ Page Title="Step 5: Plan and Collect Data" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Measure_PlanCollectData.aspx.vb" Inherits="DMAIC_Measure_PlanCollectData" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-warning text-dark me-2">5</span>Plan and Collect Data</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Measure/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Measure</a></li>
                    <li class="breadcrumb-item active">Plan &amp; Collect Data</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />

    <%-- Section 1: Measurement Plan Entry Form --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-clipboard-data me-2"></i>Measurement Plan Entry</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">Metric Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtMetricName" runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvMetricName" runat="server" ControlToValidate="txtMetricName"
                        ErrorMessage="Metric name is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Data Source</label>
                    <asp:TextBox ID="txtDataSource" runat="server" CssClass="form-control" MaxLength="200"
                        placeholder="System, survey, manual, etc." />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Frequency</label>
                    <asp:DropDownList ID="ddlFrequency" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Select --" Value="" />
                        <asp:ListItem Text="Hourly" Value="Hourly" />
                        <asp:ListItem Text="Daily" Value="Daily" />
                        <asp:ListItem Text="Weekly" Value="Weekly" />
                        <asp:ListItem Text="Monthly" Value="Monthly" />
                        <asp:ListItem Text="Quarterly" Value="Quarterly" />
                        <asp:ListItem Text="Per Batch" Value="Per Batch" />
                        <asp:ListItem Text="Continuous" Value="Continuous" />
                    </asp:DropDownList>
                </div>
                <div class="col-12">
                    <label class="form-label">Operational Definition</label>
                    <asp:TextBox ID="txtOpDefinition" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"
                        placeholder="Clear, precise definition of how the metric is measured..." />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Sampling Plan</label>
                    <asp:TextBox ID="txtSamplingPlan" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"
                        placeholder="Describe the sampling approach..." />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Sample Size</label>
                    <asp:TextBox ID="txtSampleSize" runat="server" CssClass="form-control" MaxLength="10"
                        placeholder="e.g. 30" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Who</label>
                    <asp:TextBox ID="txtWho" runat="server" CssClass="form-control" MaxLength="200"
                        placeholder="Person/team responsible" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">MSA Type</label>
                    <asp:DropDownList ID="ddlMSAType" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Select --" Value="" />
                        <asp:ListItem Text="Gage R&R" Value="Gage R&R" />
                        <asp:ListItem Text="Attribute Agreement" Value="Attribute Agreement" />
                        <asp:ListItem Text="Bias Study" Value="Bias Study" />
                        <asp:ListItem Text="Linearity Study" Value="Linearity Study" />
                        <asp:ListItem Text="Stability Study" Value="Stability Study" />
                        <asp:ListItem Text="Not Applicable" Value="Not Applicable" />
                    </asp:DropDownList>
                </div>
                <div class="col-12">
                    <label class="form-label">MSA Result</label>
                    <asp:TextBox ID="txtMSAResult" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2"
                        placeholder="Describe the MSA result or conclusion..." />
                </div>
                <div class="col-md-4">
                    <div class="form-check mt-2">
                        <asp:CheckBox ID="chkMSAAcceptable" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label" for="chkMSAAcceptable">MSA Acceptable</label>
                    </div>
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

    <%-- Section 2: Existing Measurement Plans Table --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-table me-2"></i>Measurement Plans</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litRecordCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Metric Name</th>
                            <th>Definition</th>
                            <th>Data Source</th>
                            <th>Sampling</th>
                            <th>Size</th>
                            <th>Frequency</th>
                            <th>Who</th>
                            <th>MSA Type</th>
                            <th>MSA OK</th>
                            <th class="text-center" style="width:90px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptPlans" runat="server" OnItemCommand="rptPlans_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-semibold"><%# Eval("MetricName") %></td>
                                    <td><%# Eval("OperationalDefinition") %></td>
                                    <td><%# Eval("DataSource") %></td>
                                    <td><%# Eval("SamplingPlan") %></td>
                                    <td><%# Eval("SampleSize") %></td>
                                    <td><%# Eval("Frequency") %></td>
                                    <td><%# Eval("Who") %></td>
                                    <td><%# Eval("MSA_Type") %></td>
                                    <td class="text-center">
                                        <asp:Literal ID="litMSAOk" runat="server"
                                            Visible='<%# If(IsDBNull(Eval("MSA_Acceptable")), False, CBool(Eval("MSA_Acceptable"))) %>'><i class="bi bi-check-circle-fill text-success"></i></asp:Literal>
                                    </td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this measurement plan?');"><i class="bi bi-trash"></i></asp:LinkButton>
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
        <a href='<%= ResolveUrl("~/DMAIC/Measure/Measure_WhatToMeasure.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to What to Measure
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Measure/Measure_Baseline.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Baseline Performance <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
