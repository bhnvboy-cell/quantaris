<%@ Page Title="Step 4: Identify What to Measure" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Measure_WhatToMeasure.aspx.vb" Inherits="DMAIC_Measure_WhatToMeasure" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-warning text-dark me-2">4</span>Identify What to Measure</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Measure/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Measure</a></li>
                    <li class="breadcrumb-item active">What to Measure</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />

    <%-- Card 1: Process Map Reference --%>
    <div class="card shadow-sm mb-4 border-start border-primary border-4">
        <div class="card-body">
            <div class="d-flex align-items-center">
                <i class="bi bi-diagram-2 fs-1 text-primary me-3"></i>
                <div>
                    <h6 class="fw-semibold mb-1">Process Map Reference</h6>
                    <p class="text-muted mb-1">Use your SIPOC diagram from the Define phase as the starting point for identifying what to measure. Each process step, input, and output should be considered as a potential measurement point.</p>
                    <a href='<%= ResolveUrl("~/DMAIC/Define/Define_SIPOC.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-sm btn-outline-primary">
                        <i class="bi bi-box-arrow-up-right me-1"></i>View SIPOC
                    </a>
                </div>
            </div>
        </div>
    </div>

    <%-- Card 2: COPQ --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex align-items-center">
            <i class="bi bi-currency-dollar text-danger me-2 fs-5"></i>
            <h6 class="fw-semibold mb-0">COPQ — Cost of Poor Quality</h6>
        </div>
        <div class="card-body">
            <p class="text-muted small mb-3">Identify and estimate costs associated with poor quality across four categories.</p>
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">Appraisal Costs</label>
                    <asp:TextBox ID="txtAppraisalCosts" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"
                        placeholder="Inspection, testing, auditing, measurement equipment calibration, etc." />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Prevention Costs</label>
                    <asp:TextBox ID="txtPreventionCosts" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"
                        placeholder="Training, process planning, preventive maintenance, quality improvement projects, etc." />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Internal Failure Costs</label>
                    <asp:TextBox ID="txtInternalFailure" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"
                        placeholder="Rework, scrap, downtime, yield losses, excess inventory, etc." />
                </div>
                <div class="col-md-6">
                    <label class="form-label">External Failure Costs</label>
                    <asp:TextBox ID="txtExternalFailure" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3"
                        placeholder="Warranty claims, returns, lost customers, penalties, brand damage, etc." />
                </div>
            </div>
        </div>
    </div>

    <%-- Card 3: 8 Wastes --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex align-items-center">
            <i class="bi bi-trash text-warning me-2 fs-5"></i>
            <h6 class="fw-semibold mb-0">8 Wastes Checklist <span class="badge bg-secondary ms-2">DOWNTIME</span></h6>
        </div>
        <div class="card-body">
            <p class="text-muted small mb-3">Check each waste type that is present in your current process.</p>
            <div class="row g-3">
                <div class="col-md-6">
                    <div class="form-check mb-3">
                        <asp:CheckBox ID="chkDefects" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label fw-semibold" for="chkDefects">D — Defects</label>
                        <div class="text-muted small ms-4">Errors, rework, scrap, incorrect information, non-conformance</div>
                    </div>
                    <div class="form-check mb-3">
                        <asp:CheckBox ID="chkOverproduction" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label fw-semibold" for="chkOverproduction">O — Overproduction</label>
                        <div class="text-muted small ms-4">Producing more than needed, producing too early, batch sizes too large</div>
                    </div>
                    <div class="form-check mb-3">
                        <asp:CheckBox ID="chkWaiting" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label fw-semibold" for="chkWaiting">W — Waiting</label>
                        <div class="text-muted small ms-4">Downtime, delays, idle time, waiting for materials/approvals/information</div>
                    </div>
                    <div class="form-check mb-3">
                        <asp:CheckBox ID="chkTalent" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label fw-semibold" for="chkTalent">N — Non-utilized Talent</label>
                        <div class="text-muted small ms-4">Underused skills, lack of training, not empowering employees, unused creativity</div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="form-check mb-3">
                        <asp:CheckBox ID="chkTransportation" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label fw-semibold" for="chkTransportation">T — Transportation</label>
                        <div class="text-muted small ms-4">Excessive movement of materials, multiple handling, unnecessary transfers</div>
                    </div>
                    <div class="form-check mb-3">
                        <asp:CheckBox ID="chkInventory" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label fw-semibold" for="chkInventory">I — Inventory</label>
                        <div class="text-muted small ms-4">Excess WIP, finished goods, raw materials, obsolete stock, overstocking</div>
                    </div>
                    <div class="form-check mb-3">
                        <asp:CheckBox ID="chkMotion" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label fw-semibold" for="chkMotion">M — Motion</label>
                        <div class="text-muted small ms-4">Excessive walking, reaching, bending, searching for tools or documents</div>
                    </div>
                    <div class="form-check mb-3">
                        <asp:CheckBox ID="chkExtraProcessing" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label fw-semibold" for="chkExtraProcessing">E — Extra-processing</label>
                        <div class="text-muted small ms-4">Redundant approvals, unnecessary steps, over-engineering, double entry</div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Card 4: VSM --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex align-items-center">
            <i class="bi bi-signpost-2 text-info me-2 fs-5"></i>
            <h6 class="fw-semibold mb-0">Value Stream Map (Current State)</h6>
        </div>
        <div class="card-body">
            <p class="text-muted small mb-3">Describe the current state value stream. Include process steps, cycle times, wait times, inventory levels, and information flows.</p>
            <asp:TextBox ID="txtVSM" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="6"
                placeholder="Describe the current state value stream:&#10;- Process steps and sequence&#10;- Cycle times and changeover times&#10;- Wait times between steps&#10;- Inventory at each stage&#10;- Information flow&#10;- Number of operators" />
        </div>
    </div>

    <%-- Card 5: Takt Time --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex align-items-center">
            <i class="bi bi-clock-history text-success me-2 fs-5"></i>
            <h6 class="fw-semibold mb-0">Takt Time</h6>
        </div>
        <div class="card-body">
            <p class="text-muted small mb-3">Takt time is the rate at which you need to produce to meet customer demand. <code>Takt Time = Available Work Time / Customer Demand</code></p>
            <div class="row g-3 align-items-end">
                <div class="col-md-4">
                    <label class="form-label">Available Work Time <span class="text-muted">(minutes per shift/day)</span></label>
                    <asp:TextBox ID="txtWorkTime" runat="server" CssClass="form-control" MaxLength="10"
                        oninput="calcTaktTime()" placeholder="e.g. 480" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Customer Demand <span class="text-muted">(units per shift/day)</span></label>
                    <asp:TextBox ID="txtDemand" runat="server" CssClass="form-control" MaxLength="10"
                        oninput="calcTaktTime()" placeholder="e.g. 200" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Takt Time <span class="text-muted">(minutes per unit)</span></label>
                    <div class="input-group">
                        <asp:TextBox ID="txtTaktTime" runat="server" CssClass="form-control bg-light" ReadOnly="true"
                            placeholder="Auto-calculated" />
                        <span class="input-group-text">min/unit</span>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <%-- Save & Navigation --%>
    <div class="d-flex justify-content-between">
        <a href='<%= ResolveUrl("~/DMAIC/Measure/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Measure
        </a>
        <div>
            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary me-2"
                OnClick="btnSave_Click"><i class='bi bi-save me-1'></i>Save</asp:LinkButton>
            <a href='<%= ResolveUrl("~/DMAIC/Measure/Measure_PlanCollectData.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
                Next: Plan &amp; Collect Data <i class="bi bi-arrow-right ms-1"></i>
            </a>
        </div>
    </div>

    <script>
        function calcTaktTime() {
            var workTime = parseFloat(document.getElementById('<%= txtWorkTime.ClientID %>').value) || 0;
            var demand = parseFloat(document.getElementById('<%= txtDemand.ClientID %>').value) || 0;
            var takt = document.getElementById('<%= txtTaktTime.ClientID %>');
            if (workTime > 0 && demand > 0) {
                takt.value = (workTime / demand).toFixed(2);
            } else {
                takt.value = '';
            }
        }
    </script>
</asp:Content>
