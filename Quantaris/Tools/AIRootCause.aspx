<%@ Page Title="AI Root Cause Predictor" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="AIRootCause.aspx.vb" Inherits="Tools_AIRootCause" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-robot me-2 text-info"></i>AI Root Cause Predictor</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">AI Root Cause Predictor</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-3 mb-3">
        <div class="col-md-6">
            <label class="form-label small fw-semibold">Select Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlProject_SelectedIndexChanged">
                <asp:ListItem Text="-- Select Project --" Value="" />
            </asp:DropDownList>
        </div>
    </div>

    <asp:Panel ID="pnlMain" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-6">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-pencil-square me-2"></i>Symptom Input</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="mb-3">
                            <label class="form-label small fw-semibold">Describe the symptoms or problem</label>
                            <asp:TextBox ID="txtSymptoms" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="5" placeholder="e.g. High defect rate, machine vibration, temperature fluctuation..." />
                        </div>
                        <div class="mb-3">
                            <asp:LinkButton ID="btnPredict" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnPredict_Click">
                                <i class="bi bi-robot me-1"></i>Predict Root Cause
                            </asp:LinkButton>
                        </div>
                        <asp:Panel ID="pnlResults" runat="server" Visible="false">
                            <div class="card bg-light border-0 mb-3">
                                <div class="card-body py-3">
                                    <h6 class="small text-muted text-uppercase mb-2">Prediction Result</h6>
                                    <div class="d-flex justify-content-between align-items-center mb-2">
                                        <span class="small fw-semibold">Predicted Root Cause:</span>
                                        <span class="fw-bold fs-5 text-primary"><asp:Literal ID="litPredictedRootCause" runat="server" /></span>
                                    </div>
                                    <div class="d-flex justify-content-between align-items-center mb-2">
                                        <span class="small fw-semibold">Confidence:</span>
                                        <asp:Literal ID="litConfidence" runat="server" />
                                    </div>
                                </div>
                            </div>
                            <div class="mb-3">
                                <h6 class="small text-muted text-uppercase mb-2">Matched Patterns</h6>
                                <div class="table-responsive" style="max-height: 200px;">
                                    <table class="table table-sm table-hover mb-0 small">
                                        <thead class="table-dark">
                                            <tr>
                                                <th>Keyword</th>
                                                <th>Root Cause</th>
                                                <th class="text-center">Probability</th>
                                                <th class="text-center">Count</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <asp:Repeater ID="rptMatchedPatterns" runat="server">
                                                <ItemTemplate>
                                                    <tr>
                                                        <td><%# Eval("SymptomKeyword") %></td>
                                                        <td><%# Eval("RootCause") %></td>
                                                        <td class="text-center"><%# Eval("Probability") %></td>
                                                        <td class="text-center"><%# Eval("OccurrenceCount") %></td>
                                                    </tr>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                        </tbody>
                                    </table>
                                </div>
                            </div>
                            <asp:Panel ID="pnlFeedback" runat="server" Visible="false" CssClass="border rounded p-3 bg-white">
                                <h6 class="small text-muted text-uppercase mb-2">Was this prediction accurate?</h6>
                                <div class="d-flex gap-2 mb-2">
                                    <asp:LinkButton ID="btnFeedbackYes" runat="server" CssClass="btn btn-success btn-sm" OnClick="btnFeedbackYes_Click">
                                        <i class="bi bi-check-circle me-1"></i>Yes
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="btnFeedbackNo" runat="server" CssClass="btn btn-danger btn-sm" OnClick="btnFeedbackNo_Click">
                                        <i class="bi bi-x-circle me-1"></i>No
                                    </asp:LinkButton>
                                </div>
                                <asp:Panel ID="pnlActualRootCause" runat="server" Visible="false">
                                    <div class="mb-2">
                                        <label class="form-label small fw-semibold">Actual Root Cause</label>
                                        <asp:TextBox ID="txtActualRootCause" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                                    </div>
                                    <asp:LinkButton ID="btnSubmitFeedback" runat="server" CssClass="btn btn-outline-primary btn-sm" OnClick="btnSubmitFeedback_Click">
                                        <i class="bi bi-send me-1"></i>Submit Feedback
                                    </asp:LinkButton>
                                </asp:Panel>
                                <asp:Literal ID="litFeedbackMessage" runat="server" />
                            </asp:Panel>
                        </asp:Panel>
                    </div>
                </div>
            </div>

            <div class="col-lg-6">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                        <h6 class="mb-0"><i class="bi bi-book me-2"></i>Pattern Library</h6>
                        <div class="d-flex align-items-center">
                            <asp:TextBox ID="txtFilterPattern" runat="server" CssClass="form-control form-control-sm me-2" placeholder="Filter..." style="width:150px;" />
                            <asp:LinkButton ID="btnFilterPattern" runat="server" CssClass="btn btn-outline-light btn-sm me-1" OnClick="btnFilterPattern_Click">
                                <i class="bi bi-funnel"></i>
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnClearPatternFilter" runat="server" CssClass="btn btn-outline-light btn-sm" OnClick="btnClearPatternFilter_Click">
                                <i class="bi bi-x-circle"></i>
                            </asp:LinkButton>
                        </div>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litPatternMessage" runat="server" />
                        <div class="table-responsive mb-3" style="max-height: 250px;">
                            <table class="table table-hover table-bordered align-middle mb-0 small">
                                <thead class="table-dark">
                                    <tr>
                                        <th>Keyword</th>
                                        <th>Root Cause</th>
                                        <th class="text-center">Probability</th>
                                        <th class="text-center">Count</th>
                                        <th>Category</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptPatterns" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td class="fw-semibold"><%# Eval("SymptomKeyword") %></td>
                                                <td><%# Eval("RootCause") %></td>
                                                <td class="text-center"><%# Eval("Probability") %></td>
                                                <td class="text-center"><%# Eval("OccurrenceCount") %></td>
                                                <td><%# Eval("Category") %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                        <div class="border-top pt-3">
                            <h6 class="small text-muted text-uppercase mb-2"><i class="bi bi-plus-circle me-1"></i>Add Pattern</h6>
                            <div class="row g-2">
                                <div class="col-md-4">
                                    <asp:TextBox ID="txtPatternKeyword" runat="server" CssClass="form-control form-control-sm" placeholder="Keyword" />
                                </div>
                                <div class="col-md-4">
                                    <asp:TextBox ID="txtPatternRootCause" runat="server" CssClass="form-control form-control-sm" placeholder="Root Cause" />
                                </div>
                                <div class="col-md-4">
                                    <asp:DropDownList ID="ddlPatternCategory" runat="server" CssClass="form-select form-select-sm">
                                        <asp:ListItem Text="-- Category --" Value="" />
                                        <asp:ListItem Text="Equipment" Value="Equipment" />
                                        <asp:ListItem Text="Process" Value="Process" />
                                        <asp:ListItem Text="Material" Value="Material" />
                                        <asp:ListItem Text="Measurement" Value="Measurement" />
                                        <asp:ListItem Text="Environment" Value="Environment" />
                                        <asp:ListItem Text="People" Value="People" />
                                    </asp:DropDownList>
                                </div>
                                <div class="col-12">
                                    <asp:LinkButton ID="btnAddPattern" runat="server" CssClass="btn btn-success btn-sm" OnClick="btnAddPattern_Click">
                                        <i class="bi bi-plus-lg me-1"></i>Add Pattern
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="card shadow-sm mt-4">
            <div class="card-header bg-dark text-white">
                <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>Prediction History</h6>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover align-middle mb-0 small">
                        <thead class="table-dark">
                            <tr>
                                <th>Date</th>
                                <th>Symptoms</th>
                                <th>Predicted Root Cause</th>
                                <th class="text-center">Confidence</th>
                                <th class="text-center">Accurate?</th>
                                <th>Actual Root Cause</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptHistory" runat="server">
                                <ItemTemplate>
                                    <tr>
                                        <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd, yyyy HH:mm") %></td>
                                        <td style="max-width:200px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap;" title='<%# Eval("Symptoms") %>'><%# Eval("Symptoms") %></td>
                                        <td class="fw-semibold"><%# Eval("PredictedRootCause") %></td>
                                        <td class="text-center"><%# GetConfidenceBadge(Eval("Confidence")) %></td>
                                        <td class="text-center"><%# GetAccuracyIcon(Eval("WasAccurate")) %></td>
                                        <td><%# If(Eval("ActualRootCause") IsNot DBNull.Value, Eval("ActualRootCause"), "-") %></td>
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
        <i class="bi bi-info-circle me-2"></i>Please select a project to use the AI Root Cause Predictor.
    </asp:Panel>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>
