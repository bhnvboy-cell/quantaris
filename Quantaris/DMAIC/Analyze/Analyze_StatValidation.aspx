<%@ Page Title="Step 9: Validate Root Causes" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Analyze_StatValidation.aspx.vb" Inherits="DMAIC_Analyze_Analyze_StatValidation" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-calculator me-2"></i>Step 9: Validate Root Causes</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Analyze/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Analyze</a></li>
                    <li class="breadcrumb-item active">Statistical Validation</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />

    <%-- Card 1: Test Selection --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-list-check me-2"></i>Statistical Test Selection</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">Test Type <span class="text-danger">*</span></label>
                    <asp:DropDownList ID="ddlTestType" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlTestType_SelectedIndexChanged">
                        <asp:ListItem Text="-- Select Test --" Value="" />
                        <asp:ListItem Text="1-Sample t" Value="1-Sample t" />
                        <asp:ListItem Text="2-Sample t" Value="2-Sample t" />
                        <asp:ListItem Text="Paired t" Value="Paired t" />
                        <asp:ListItem Text="1-Way ANOVA" Value="1-Way ANOVA" />
                        <asp:ListItem Text="Variance Test" Value="Variance Test" />
                        <asp:ListItem Text="Mann-Whitney" Value="Mann-Whitney" />
                        <asp:ListItem Text="Mood's Median" Value="Mood's Median" />
                        <asp:ListItem Text="Correlation" Value="Correlation" />
                        <asp:ListItem Text="Regression" Value="Regression" />
                        <asp:ListItem Text="Chi-Square" Value="Chi-Square" />
                        <asp:ListItem Text="1-Proportion" Value="1-Proportion" />
                        <asp:ListItem Text="2-Proportion" Value="2-Proportion" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-6">
                    <label class="form-label">Test Description</label>
                    <asp:TextBox ID="txtTestDescription" runat="server" CssClass="form-control" MaxLength="500"
                        placeholder="e.g., Comparing batch A vs batch B cycle time" />
                </div>
            </div>
        </div>
    </div>

    <%-- Card 2: Test Parameters --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-sliders me-2"></i>Test Parameters</h6>
        </div>
        <div class="card-body">
            <asp:Panel ID="pnlParameters" runat="server" Visible="false">
                <div class="row g-3">
                    <div class="col-md-4">
                        <label class="form-label">Sample Size (n)</label>
                        <asp:TextBox ID="txtSampleSize" runat="server" CssClass="form-control" TextMode="Number" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Sample Mean</label>
                        <asp:TextBox ID="txtSampleMean" runat="server" CssClass="form-control" TextMode="Number" step="any" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Standard Deviation</label>
                        <asp:TextBox ID="txtStdDev" runat="server" CssClass="form-control" TextMode="Number" step="any" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Hypothesized Mean / Target</label>
                        <asp:TextBox ID="txtHypothesizedMean" runat="server" CssClass="form-control" TextMode="Number" step="any" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Sample 2 Mean (if applicable)</label>
                        <asp:TextBox ID="txtSample2Mean" runat="server" CssClass="form-control" TextMode="Number" step="any" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Sample 2 Std Dev</label>
                        <asp:TextBox ID="txtSample2StdDev" runat="server" CssClass="form-control" TextMode="Number" step="any" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Sample 2 Size</label>
                        <asp:TextBox ID="txtSample2Size" runat="server" CssClass="form-control" TextMode="Number" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Degrees of Freedom</label>
                        <asp:TextBox ID="txtDF" runat="server" CssClass="form-control" TextMode="Number" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label">Expected Frequency / Proportion</label>
                        <asp:TextBox ID="txtExpectedProp" runat="server" CssClass="form-control" TextMode="Number" step="any" placeholder="0.00" />
                    </div>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlNoTest" runat="server" CssClass="text-muted small">
                Select a test type above to configure parameters.
            </asp:Panel>
        </div>
    </div>

    <%-- Card 3: Test Results --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-clipboard-data me-2"></i>Test Results</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-3">
                    <label class="form-label">Test Statistic</label>
                    <asp:TextBox ID="txtTestStatistic" runat="server" CssClass="form-control" TextMode="Number" step="any" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">P-Value</label>
                    <asp:TextBox ID="txtPValue" runat="server" CssClass="form-control" TextMode="Number" step="0.0001" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Alpha (&#945;)</label>
                    <asp:TextBox ID="txtAlpha" runat="server" CssClass="form-control" TextMode="Number" step="0.01" Text="0.05" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Significance</label>
                    <asp:DropDownList ID="ddlSignificance" runat="server" CssClass="form-select">
                        <asp:ListItem Text="--" Value="" />
                        <asp:ListItem Text="Significant" Value="Significant" />
                        <asp:ListItem Text="Not Significant" Value="Not Significant" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-2 d-flex align-items-end">
                    <asp:Button ID="btnAutoSig" runat="server" CssClass="btn btn-outline-info btn-sm w-100"
                        Text="Auto-Calc" OnClick="btnAutoSig_Click" />
                </div>
                <div class="col-12">
                    <label class="form-label">Conclusion</label>
                    <asp:TextBox ID="txtConclusion" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"
                        placeholder="Interpret the test results..." />
                </div>
            </div>
            <div class="mt-3">
                <asp:LinkButton ID="btnSaveTest" runat="server" CssClass="btn btn-primary"
                    OnClick="btnSaveTest_Click"><i class='bi bi-save me-1'></i>Save Test</asp:LinkButton>
                <asp:Button ID="btnClearTest" runat="server" CssClass="btn btn-outline-secondary ms-2"
                    Text="Clear" OnClick="btnClearTest_Click" />
            </div>
        </div>
    </div>

    <%-- Card 4: Test History --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-clock-history me-2"></i>Test History</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litTestCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Test Type</th>
                            <th>Description</th>
                            <th class="text-end">Statistic</th>
                            <th class="text-end">P-Value</th>
                            <th class="text-center">Significance</th>
                            <th>Conclusion</th>
                            <th>Date</th>
                            <th class="text-center" style="width:90px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptTests" runat="server" OnItemCommand="rptTests_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><span class="badge bg-primary"><%# Eval("Category") %></span></td>
                                    <td><%# Eval("Cause") %></td>
                                    <td class="text-end"><%# Eval("FiveWhy_Chain") %></td>
                                    <td class="text-end"><%# Eval("Effect") %></td>
                                    <td class="text-center">
                                        <span class="badge bg-<%# GetSigBadge(Eval("Effect")) %>"><%# GetSignificance(Eval("Effect")) %></span>
                                    </td>
                                    <td><%# Eval("Sequence") %></td>
                                    <td><%# CDate(Eval("CreatedOn")).ToString("yyyy-MM-dd HH:mm") %></td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEditTest" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDeleteTest" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this test record?');"><i class="bi bi-trash"></i></asp:LinkButton>
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
    <div class="d-flex justify-content-between mt-4">
        <a href='<%= ResolveUrl("~/DMAIC/Analyze/Analyze_RootCausePrioritization.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Prioritization
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Improve/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Improve Phase <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
