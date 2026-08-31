<%@ Page Title="Monte Carlo Simulation" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="MonteCarlo.aspx.vb" Inherits="Tools_MonteCarlo" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .mc-histogram { display: flex; align-items: flex-end; gap: 2px; height: 200px; }
        .mc-bar { flex: 1; background: #0d6efd; min-width: 4px; border-radius: 2px 2px 0 0; transition: height 0.3s; position: relative; }
        .mc-bar:hover { background: #0b5ed7; }
        .mc-stat-card { border-left: 3px solid #0d6efd; }
        .mc-stat-card.mean { border-left-color: #198754; }
        .mc-stat-card.std { border-left-color: #ffc107; }
        .mc-stat-card.pct { border-left-color: #6f42c1; }
        .input-row { background: #f8f9fa; padding: 8px; border-radius: 4px; margin-bottom: 6px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-dice-6 me-2 text-primary"></i>Monte Carlo Simulation</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Monte Carlo</li>
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
        <div class="col-md-3">
            <label class="form-label small fw-semibold">Model Name</label>
            <asp:TextBox ID="txtModelName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Tolerance Stack-up" />
        </div>
        <div class="col-md-2">
            <label class="form-label small fw-semibold">Iterations</label>
            <asp:TextBox ID="txtIterations" runat="server" CssClass="form-control form-control-sm" TextMode="Number" Text="10000" />
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100" OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise"></i> Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlMC" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-5">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-sliders me-2"></i>Input Variables</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div id="inputList">
                            <asp:Repeater ID="rptInputs" runat="server">
                                <ItemTemplate>
                                    <div class="input-row">
                                        <div class="row g-2 align-items-center">
                                            <div class="col-3">
                                                <input type="text" name="inp_name_<%# Container.ItemIndex %>"
                                                    value="<%# Eval("InputName") %>" class="form-control form-control-sm" placeholder="Name" />
                                            </div>
                                            <div class="col-3">
                                                <select name="inp_dist_<%# Container.ItemIndex %>" class="form-select form-select-sm">
                                                    <option value="Normal" <%# IIf(Eval("DistributionType").ToString()="Normal","selected","") %>>Normal</option>
                                                    <option value="Uniform" <%# IIf(Eval("DistributionType").ToString()="Uniform","selected","") %>>Uniform</option>
                                                    <option value="Triangular" <%# IIf(Eval("DistributionType").ToString()="Triangular","selected","") %>>Triangular</option>
                                                </select>
                                            </div>
                                            <div class="col-2">
                                                <input type="text" name="inp_p1_<%# Container.ItemIndex %>"
                                                    value="<%# Eval("Param1") %>" class="form-control form-control-sm param1" placeholder="P1" />
                                            </div>
                                            <div class="col-2">
                                                <input type="text" name="inp_p2_<%# Container.ItemIndex %>"
                                                    value="<%# Eval("Param2") %>" class="form-control form-control-sm param2" placeholder="P2" />
                                            </div>
                                            <div class="col-2">
                                                <button type="button" class="btn btn-outline-danger btn-sm remove-input"><i class="bi bi-x"></i></button>
                                            </div>
                                        </div>
                                        <div class="small text-muted mt-1 param-hint">
                                            <%# GetParamHint(Eval("DistributionType").ToString()) %>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>

                        <div class="mt-2 d-flex gap-2">
                            <button type="button" class="btn btn-outline-primary btn-sm" onclick="addInput()">
                                <i class="bi bi-plus-circle"></i> Add Variable
                            </button>
                        </div>

                        <div class="mt-3">
                            <label class="form-label small fw-semibold">Output Formula</label>
                            <asp:TextBox ID="txtFormula" runat="server" CssClass="form-control form-control-sm font-monospace"
                                placeholder="e.g. X1 + X2 - X3" Rows="2" TextMode="MultiLine" />
                            <div class="small text-muted mt-1">Use variable names (Case-sensitive). Supports +, -, *, /, (, ), Math.Sqrt, Math.Pow</div>
                        </div>

                        <div class="mt-3">
                            <label class="form-label small fw-semibold">Spec Limits (optional)</label>
                            <div class="row g-2">
                                <div class="col-6">
                                    <asp:TextBox ID="txtUSL" runat="server" CssClass="form-control form-control-sm" placeholder="USL" />
                                </div>
                                <div class="col-6">
                                    <asp:TextBox ID="txtLSL" runat="server" CssClass="form-control form-control-sm" placeholder="LSL" />
                                </div>
                            </div>
                        </div>

                        <div class="mt-3 d-flex gap-2">
                            <asp:LinkButton ID="btnRun" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnRun_Click">
                                <i class="bi bi-play-fill me-1"></i>Run Simulation
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-outline-success btn-sm" OnClick="btnSave_Click" Visible="false">
                                <i class="bi bi-floppy me-1"></i>Save Model
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnClear" runat="server" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClear_Click">
                                <i class="bi bi-x-circle me-1"></i>Clear
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-lg-7">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-graph-up me-2"></i>Simulation Results</h6>
                    </div>
                    <div class="card-body">
                        <asp:Panel ID="pnlResults" runat="server" Visible="false">
                            <div class="row g-2 mb-3">
                                <div class="col-3">
                                    <div class="card mc-stat-card mean py-2 px-3">
                                        <div class="small text-muted">Mean</div>
                                        <div class="fw-bold"><asp:Literal ID="litMean" runat="server" /></div>
                                    </div>
                                </div>
                                <div class="col-3">
                                    <div class="card mc-stat-card std py-2 px-3">
                                        <div class="small text-muted">Std Dev</div>
                                        <div class="fw-bold"><asp:Literal ID="litStdDev" runat="server" /></div>
                                    </div>
                                </div>
                                <div class="col-3">
                                    <div class="card mc-stat-card pct py-2 px-3">
                                        <div class="small text-muted">P5 / P95</div>
                                        <div class="fw-bold"><asp:Literal ID="litP5" runat="server" /> / <asp:Literal ID="litP95" runat="server" /></div>
                                    </div>
                                </div>
                                <div class="col-3">
                                    <div class="card mc-stat-card pct py-2 px-3">
                                        <div class="small text-muted">Min / Max</div>
                                        <div class="fw-bold"><asp:Literal ID="litMin" runat="server" /> / <asp:Literal ID="litMax" runat="server" /></div>
                                    </div>
                                </div>
                            </div>

                            <div class="mb-3">
                                <label class="form-label small fw-semibold">Distribution Histogram</label>
                                <div class="mc-histogram" id="histogram">
                                    <asp:Literal ID="litHistogram" runat="server" />
                                </div>
                            </div>

                            <asp:Panel ID="pnlCapability" runat="server" Visible="false">
                                <div class="alert alert-info py-2 small">
                                    <strong>Process Capability:</strong>
                                    Cp: <asp:Literal ID="litCp" runat="server" /> |
                                    Cpk: <asp:Literal ID="litCpk" runat="server" /> |
                                    Defect Rate: <asp:Literal ID="litDefectRate" runat="server" />
                                </div>
                            </asp:Panel>
                        </asp:Panel>
                        <asp:Panel ID="pnlNoResults" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-dice-6 fs-1 d-block mb-2"></i>
                            <span>Configure variables and run simulation.</span>
                        </asp:Panel>
                    </div>
                </div>

                <div class="card shadow-sm mt-4">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>Simulation History</h6>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <table class="table table-hover align-middle mb-0 small">
                                <thead class="table-dark">
                                    <tr>
                                        <th>Date</th>
                                        <th>Model</th>
                                        <th class="text-center">Iterations</th>
                                        <th class="text-center">Variables</th>
                                        <th class="text-end">Mean</th>
                                        <th class="text-end">Std Dev</th>
                                        <th class="text-center">Actions</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                        <ItemTemplate>
                                            <tr>
                                                <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd") %></td>
                                                <td class="fw-semibold"><%# Eval("ModelName") %></td>
                                                <td class="text-center"><%# Eval("Iterations") %></td>
                                                <td class="text-center"></td>
                                                <td class="text-end"><%# Eval("MeanResult") %></td>
                                                <td class="text-end"><%# Eval("StdDevResult") %></td>
                                                <td class="text-center">
                                                    <asp:LinkButton ID="btnLoad" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                        CommandName="LoadModel" CommandArgument='<%# Eval("Id") %>'>
                                                        <i class="bi bi-arrow-counterclockwise"></i>
                                                    </asp:LinkButton>
                                                    <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                        CommandName="DeleteModel" CommandArgument='<%# Eval("Id") %>'
                                                        OnClientClick="return confirm('Delete this model?');">
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
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project.
    </asp:Panel>

    <script>
        function addInput() {
            var list = document.getElementById('inputList');
            var idx = list.querySelectorAll('.input-row').length;
            var div = document.createElement('div');
            div.className = 'input-row';
            div.innerHTML =
                '<div class="row g-2 align-items-center">' +
                '<div class="col-3"><input type="text" name="inp_name_' + idx + '" class="form-control form-control-sm" placeholder="Name" /></div>' +
                '<div class="col-3"><select name="inp_dist_' + idx + '" class="form-select form-select-sm" onchange="updateHint(this)">' +
                '<asp:ListItem Value="Normal">Normal</asp:ListItem><asp:ListItem Value="Uniform">Uniform</asp:ListItem><asp:ListItem Value="Triangular">Triangular</asp:ListItem></select></div>' +
                '<div class="col-2"><input type="text" name="inp_p1_' + idx + '" class="form-control form-control-sm param1" placeholder="Mean" /></div>' +
                '<div class="col-2"><input type="text" name="inp_p2_' + idx + '" class="form-control form-control-sm param2" placeholder="StdDev" /></div>' +
                '<div class="col-2"><button type="button" class="btn btn-outline-danger btn-sm" onclick="this.closest(\'.input-row\').remove()"><i class="bi bi-x"></i></button></div>' +
                '</div>' +
                '<div class="small text-muted mt-1 param-hint">Normal(Mean, StdDev)</div>';
            list.appendChild(div);
        }

        function updateHint(sel) {
            var row = sel.closest('.input-row');
            var hint = row.querySelector('.param-hint');
            var p1 = row.querySelector('.param1');
            var p2 = row.querySelector('.param2');
            switch (sel.value) {
                case 'Normal':
                    hint.textContent = 'Normal(Mean, StdDev)';
                    p1.placeholder = 'Mean';
                    p2.placeholder = 'StdDev';
                    break;
                case 'Uniform':
                    hint.textContent = 'Uniform(Min, Max)';
                    p1.placeholder = 'Min';
                    p2.placeholder = 'Max';
                    break;
                case 'Triangular':
                    hint.textContent = 'Triangular(Min, Mode, Max)';
                    p1.placeholder = 'Min';
                    p2.placeholder = 'Mode';
                    break;
            }
        }
    </script>
</asp:Content>
