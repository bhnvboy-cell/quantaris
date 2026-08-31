<%@ Page Title="Design of Experiments" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="DOE.aspx.vb" Inherits="Tools_DOE" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-gear-wide-connected me-2 text-primary"></i>Design of Experiments (DOE)</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">DOE</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-3 mb-3">
        <div class="col-md-5">
            <label class="form-label small fw-semibold">Select Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlProject_SelectedIndexChanged">
                <asp:ListItem Text="-- Select Project --" Value="" />
            </asp:DropDownList>
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100" OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise me-1"></i>Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlDOE" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-5">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-sliders me-2"></i>Experiment Configuration</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="row g-3">
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Experiment Name</label>
                                <asp:TextBox ID="txtExperimentName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. DOE-001" />
                            </div>
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Design Type</label>
                                <asp:DropDownList ID="ddlDesignType" runat="server" CssClass="form-select form-select-sm">
                                    <asp:ListItem Text="Full Factorial" Value="FullFactorial" />
                                    <asp:ListItem Text="Fractional Factorial" Value="Fractional" />
                                    <asp:ListItem Text="Plackett-Burman" Value="PlackettBurman" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-6">
                                <label class="form-label small fw-semibold">Factor Count</label>
                                <asp:DropDownList ID="ddlFactorCount" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlFactorCount_SelectedIndexChanged">
                                    <asp:ListItem Text="2" Value="2" Selected="True" />
                                    <asp:ListItem Text="3" Value="3" />
                                    <asp:ListItem Text="4" Value="4" />
                                    <asp:ListItem Text="5" Value="5" />
                                    <asp:ListItem Text="6" Value="6" />
                                    <asp:ListItem Text="7" Value="7" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-12">
                                <asp:Panel ID="pnlFactors" runat="server" CssClass="p-2 bg-light rounded">
                                    <label class="form-label small fw-semibold mb-2">Factors</label>
                                    <div class="row g-1 mb-2 small fw-semibold text-muted">
                                        <div class="col-1 text-center">#</div>
                                        <div class="col-4">Name</div>
                                        <div class="col-3">Low (-1)</div>
                                        <div class="col-3">High (+1)</div>
                                    </div>
                                    <div class="row g-2 mb-1 align-items-center" id="fr1" runat="server">
                                        <div class="col-1 text-center fw-bold small">1</div>
                                        <div class="col-4"><asp:TextBox ID="txtFacName_1" runat="server" CssClass="form-control form-control-sm" Text="A" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacLow_1" runat="server" CssClass="form-control form-control-sm" Text="-1" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacHigh_1" runat="server" CssClass="form-control form-control-sm" Text="1" /></div>
                                    </div>
                                    <div class="row g-2 mb-1 align-items-center" id="fr2" runat="server">
                                        <div class="col-1 text-center fw-bold small">2</div>
                                        <div class="col-4"><asp:TextBox ID="txtFacName_2" runat="server" CssClass="form-control form-control-sm" Text="B" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacLow_2" runat="server" CssClass="form-control form-control-sm" Text="-1" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacHigh_2" runat="server" CssClass="form-control form-control-sm" Text="1" /></div>
                                    </div>
                                    <div class="row g-2 mb-1 align-items-center" id="fr3" runat="server">
                                        <div class="col-1 text-center fw-bold small">3</div>
                                        <div class="col-4"><asp:TextBox ID="txtFacName_3" runat="server" CssClass="form-control form-control-sm" Text="C" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacLow_3" runat="server" CssClass="form-control form-control-sm" Text="-1" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacHigh_3" runat="server" CssClass="form-control form-control-sm" Text="1" /></div>
                                    </div>
                                    <div class="row g-2 mb-1 align-items-center" id="fr4" runat="server">
                                        <div class="col-1 text-center fw-bold small">4</div>
                                        <div class="col-4"><asp:TextBox ID="txtFacName_4" runat="server" CssClass="form-control form-control-sm" Text="D" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacLow_4" runat="server" CssClass="form-control form-control-sm" Text="-1" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacHigh_4" runat="server" CssClass="form-control form-control-sm" Text="1" /></div>
                                    </div>
                                    <div class="row g-2 mb-1 align-items-center" id="fr5" runat="server">
                                        <div class="col-1 text-center fw-bold small">5</div>
                                        <div class="col-4"><asp:TextBox ID="txtFacName_5" runat="server" CssClass="form-control form-control-sm" Text="E" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacLow_5" runat="server" CssClass="form-control form-control-sm" Text="-1" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacHigh_5" runat="server" CssClass="form-control form-control-sm" Text="1" /></div>
                                    </div>
                                    <div class="row g-2 mb-1 align-items-center" id="fr6" runat="server">
                                        <div class="col-1 text-center fw-bold small">6</div>
                                        <div class="col-4"><asp:TextBox ID="txtFacName_6" runat="server" CssClass="form-control form-control-sm" Text="F" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacLow_6" runat="server" CssClass="form-control form-control-sm" Text="-1" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacHigh_6" runat="server" CssClass="form-control form-control-sm" Text="1" /></div>
                                    </div>
                                    <div class="row g-2 mb-1 align-items-center" id="fr7" runat="server">
                                        <div class="col-1 text-center fw-bold small">7</div>
                                        <div class="col-4"><asp:TextBox ID="txtFacName_7" runat="server" CssClass="form-control form-control-sm" Text="G" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacLow_7" runat="server" CssClass="form-control form-control-sm" Text="-1" /></div>
                                        <div class="col-3"><asp:TextBox ID="txtFacHigh_7" runat="server" CssClass="form-control form-control-sm" Text="1" /></div>
                                    </div>
                                </asp:Panel>
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Response Name</label>
                                <asp:TextBox ID="txtResponseName" runat="server" CssClass="form-control form-control-sm" Text="Response" />
                            </div>
                            <div class="col-12">
                                <asp:LinkButton ID="btnGenerateDesign" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnGenerateDesign_Click">
                                    <i class="bi bi-table me-1"></i>Generate Design
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnCalculateEffects" runat="server" CssClass="btn btn-success btn-sm ms-1" OnClick="btnCalculateEffects_Click">
                                    <i class="bi bi-calculator me-1"></i>Calculate Effects
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-warning btn-sm ms-1" OnClick="btnSave_Click">
                                    <i class="bi bi-floppy me-1"></i>Save
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-lg-7">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-grid-3x3 me-2"></i>Design Matrix</h6>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlDesign" runat="server" Visible="false">
                            <div class="table-responsive">
                                <asp:Literal ID="litDesignMatrix" runat="server" />
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlNoDesign" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-grid-3x3 fs-1 d-block mb-2"></i>
                            <span>Configure factors above, then click <strong>Generate Design</strong>.</span>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <asp:Panel ID="pnlResults" runat="server" Visible="false">
            <div class="card shadow-sm mt-4">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-bar-chart-line me-2"></i>Results</h6>
                </div>
                <div class="card-body">
                    <h6 class="text-muted text-uppercase small fw-semibold mb-2">ANOVA Table</h6>
                    <div class="table-responsive mb-3">
                        <asp:Literal ID="litANOVA" runat="server" />
                    </div>
                    <div class="row g-3">
                        <div class="col-md-6">
                            <h6 class="text-muted text-uppercase small fw-semibold mb-2">Effect Estimates</h6>
                            <div class="table-responsive">
                                <asp:Literal ID="litEffects" runat="server" />
                            </div>
                        </div>
                        <div class="col-md-6">
                            <h6 class="text-muted text-uppercase small fw-semibold mb-2">Pareto of Effects</h6>
                            <div class="table-responsive">
                                <asp:Literal ID="litPareto" runat="server" />
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <div class="card shadow-sm mt-4">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>Experiment History</h6>
            </div>
            <div class="card-body p-0">
                <div class="table-responsive">
                    <table class="table table-hover align-middle mb-0 small">
                        <thead class="table-dark">
                            <tr>
                                <th>Date</th>
                                <th>Experiment Name</th>
                                <th>Design Type</th>
                                <th class="text-center">Factors</th>
                                <th class="text-center">Runs</th>
                                <th>Response</th>
                                <th class="text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd, yyyy") %></td>
                                        <td class="fw-semibold"><%# Eval("ExperimentName") %></td>
                                        <td><%# Eval("DesignType") %></td>
                                        <td class="text-center"><%# Eval("FactorCount") %></td>
                                        <td class="text-center"><%# Eval("Runs") %></td>
                                        <td><%# Eval("ResponseName") %></td>
                                        <td class="text-center">
                                            <asp:LinkButton ID="btnLoad" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                CommandName="LoadRecord" CommandArgument='<%# Eval("Id") %>'>
                                                <i class="bi bi-arrow-counterclockwise"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                CommandName="DeleteRecord" CommandArgument='<%# Eval("Id") %>'
                                                OnClientClick="return confirm('Delete this experiment?');">
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
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project to design experiments.
    </asp:Panel>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="Scripts" runat="server">
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            var respInputs = document.querySelectorAll('[id^="resp_"]');
            respInputs.forEach(function (el) {
                el.addEventListener('input', function () {
                    var val = parseFloat(this.value);
                    if (isNaN(val)) this.style.backgroundColor = '#fff3cd';
                    else this.style.backgroundColor = '';
                });
            });
        });
    </script>
</asp:Content>
