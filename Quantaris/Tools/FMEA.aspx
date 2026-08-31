<%@ Page Title="FMEA Tool" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="FMEA.aspx.vb" Inherits="Tools_FMEA" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-exclamation-triangle me-2"></i>FMEA - Failure Mode &amp; Effects Analysis</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">FMEA</li>
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
        <div class="col-md-3">
            <label class="form-label small fw-semibold">&nbsp;</label>
            <div>
                <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnRefresh_Click">
                    <i class="bi bi-arrow-clockwise me-1"></i>Refresh
                </asp:LinkButton>
            </div>
        </div>
    </div>

    <asp:Panel ID="pnlFMEA" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-6">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-pencil-square me-2"></i>FMEA Entry Form</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="row g-3">
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Process Step</label>
                                <asp:TextBox ID="txtProcessStep" runat="server" CssClass="form-control form-control-sm" />
                                <asp:RequiredFieldValidator ID="rfvStep" runat="server" ControlToValidate="txtProcessStep" Display="Dynamic" CssClass="text-danger small" ErrorMessage="Required" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Failure Mode</label>
                                <asp:TextBox ID="txtFailureMode" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Effect of Failure</label>
                                <asp:TextBox ID="txtEffect" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Cause of Failure</label>
                                <asp:TextBox ID="txtCause" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Current Control</label>
                                <asp:TextBox ID="txtCurrentControl" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                            </div>
                            <div class="col-md-4">
                                <label class="form-label small fw-semibold">Severity (S)</label>
                                <asp:DropDownList ID="ddlSeverity" runat="server" CssClass="form-select form-select-sm" AutoPostBack="false">
                                    <asp:ListItem Text="1 - None" Value="1" />
                                    <asp:ListItem Text="2 - Very Minor" Value="2" />
                                    <asp:ListItem Text="3 - Minor" Value="3" />
                                    <asp:ListItem Text="4 - Low" Value="4" />
                                    <asp:ListItem Text="5 - Moderate" Value="5" />
                                    <asp:ListItem Text="6 - Significant" Value="6" />
                                    <asp:ListItem Text="7 - Major" Value="7" />
                                    <asp:ListItem Text="8 - Extreme" Value="8" />
                                    <asp:ListItem Text="9 - Serious" Value="9" />
                                    <asp:ListItem Text="10 - Hazardous" Value="10" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label small fw-semibold">Occurrence (O)</label>
                                <asp:DropDownList ID="ddlOccurrence" runat="server" CssClass="form-select form-select-sm" AutoPostBack="false">
                                    <asp:ListItem Text="1 - Remote" Value="1" />
                                    <asp:ListItem Text="2 - Very Low" Value="2" />
                                    <asp:ListItem Text="3 - Low" Value="3" />
                                    <asp:ListItem Text="4 - Moderately Low" Value="4" />
                                    <asp:ListItem Text="5 - Moderate" Value="5" />
                                    <asp:ListItem Text="6 - Moderately High" Value="6" />
                                    <asp:ListItem Text="7 - High" Value="7" />
                                    <asp:ListItem Text="8 - Very High" Value="8" />
                                    <asp:ListItem Text="9 - Extremely High" Value="9" />
                                    <asp:ListItem Text="10 - Almost Certain" Value="10" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-4">
                                <label class="form-label small fw-semibold">Detection (D)</label>
                                <asp:DropDownList ID="ddlDetection" runat="server" CssClass="form-select form-select-sm" AutoPostBack="false">
                                    <asp:ListItem Text="1 - Almost Certain" Value="1" />
                                    <asp:ListItem Text="2 - Very High" Value="2" />
                                    <asp:ListItem Text="3 - High" Value="3" />
                                    <asp:ListItem Text="4 - Moderately High" Value="4" />
                                    <asp:ListItem Text="5 - Moderate" Value="5" />
                                    <asp:ListItem Text="6 - Low" Value="6" />
                                    <asp:ListItem Text="7 - Very Low" Value="7" />
                                    <asp:ListItem Text="8 - Remote" Value="8" />
                                    <asp:ListItem Text="9 - Very Remote" Value="9" />
                                    <asp:ListItem Text="10 - Absolute Uncertainty" Value="10" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">RPN (S &times; O &times; D)</label>
                                <asp:TextBox ID="txtRPN" runat="server" CssClass="form-control form-control-sm fw-bold bg-light" ReadOnly="true" Text="1" />
                            </div>
                            <div class="col-12">
                                <label class="form-label small fw-semibold">Recommended Action</label>
                                <asp:TextBox ID="txtRecommendedAction" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                            </div>
                            <div class="col-12">
                                <asp:LinkButton ID="btnSaveFMEA" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnSaveFMEA_Click">
                                    <i class="bi bi-floppy me-1"></i>Save FMEA Record
                                </asp:LinkButton>
                                <asp:LinkButton ID="btnClearForm" runat="server" CssClass="btn btn-outline-secondary btn-sm ms-2" OnClick="btnClearForm_Click">
                                    <i class="bi bi-x-circle me-1"></i>Clear
                                </asp:LinkButton>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-lg-6">
                <div class="card shadow-sm h-100">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-arrow-up-circle me-2"></i>Action Tracking</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litActionMessage" runat="server" />
                        <div class="mb-3">
                            <label class="form-label small fw-semibold">Select FMEA Record</label>
                            <asp:DropDownList ID="ddlFMEARecord" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlFMEARecord_SelectedIndexChanged">
                                <asp:ListItem Text="-- Select Record --" Value="" />
                            </asp:DropDownList>
                        </div>
                        <asp:Panel ID="pnlAction" runat="server" Visible="false">
                            <div class="row g-3">
                                <div class="col-12">
                                    <label class="form-label small fw-semibold">Previous RPN</label>
                                    <asp:TextBox ID="txtPrevRPN" runat="server" CssClass="form-control form-control-sm bg-light" ReadOnly="true" />
                                </div>
                                <div class="col-12">
                                    <label class="form-label small fw-semibold">Action Taken</label>
                                    <asp:TextBox ID="txtActionTaken" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="2" />
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label small fw-semibold">New Severity</label>
                                    <asp:DropDownList ID="ddlNewSeverity" runat="server" CssClass="form-select form-select-sm" AutoPostBack="false">
                                        <asp:ListItem Text="1 - None" Value="1" />
                                        <asp:ListItem Text="2 - Very Minor" Value="2" />
                                        <asp:ListItem Text="3 - Minor" Value="3" />
                                        <asp:ListItem Text="4 - Low" Value="4" />
                                        <asp:ListItem Text="5 - Moderate" Value="5" />
                                        <asp:ListItem Text="6 - Significant" Value="6" />
                                        <asp:ListItem Text="7 - Major" Value="7" />
                                        <asp:ListItem Text="8 - Extreme" Value="8" />
                                        <asp:ListItem Text="9 - Serious" Value="9" />
                                        <asp:ListItem Text="10 - Hazardous" Value="10" />
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label small fw-semibold">New Occurrence</label>
                                    <asp:DropDownList ID="ddlNewOccurrence" runat="server" CssClass="form-select form-select-sm" AutoPostBack="false">
                                        <asp:ListItem Text="1 - Remote" Value="1" />
                                        <asp:ListItem Text="2 - Very Low" Value="2" />
                                        <asp:ListItem Text="3 - Low" Value="3" />
                                        <asp:ListItem Text="4 - Moderately Low" Value="4" />
                                        <asp:ListItem Text="5 - Moderate" Value="5" />
                                        <asp:ListItem Text="6 - Moderately High" Value="6" />
                                        <asp:ListItem Text="7 - High" Value="7" />
                                        <asp:ListItem Text="8 - Very High" Value="8" />
                                        <asp:ListItem Text="9 - Extremely High" Value="9" />
                                        <asp:ListItem Text="10 - Almost Certain" Value="10" />
                                    </asp:DropDownList>
                                </div>
                                <div class="col-md-4">
                                    <label class="form-label small fw-semibold">New Detection</label>
                                    <asp:DropDownList ID="ddlNewDetection" runat="server" CssClass="form-select form-select-sm" AutoPostBack="false">
                                        <asp:ListItem Text="1 - Almost Certain" Value="1" />
                                        <asp:ListItem Text="2 - Very High" Value="2" />
                                        <asp:ListItem Text="3 - High" Value="3" />
                                        <asp:ListItem Text="4 - Moderately High" Value="4" />
                                        <asp:ListItem Text="5 - Moderate" Value="5" />
                                        <asp:ListItem Text="6 - Low" Value="6" />
                                        <asp:ListItem Text="7 - Very Low" Value="7" />
                                        <asp:ListItem Text="8 - Remote" Value="8" />
                                        <asp:ListItem Text="9 - Very Remote" Value="9" />
                                        <asp:ListItem Text="10 - Absolute Uncertainty" Value="10" />
                                    </asp:DropDownList>
                                </div>
                                <div class="col-12">
                                    <label class="form-label small fw-semibold">New RPN (S &times; O &times; D)</label>
                                    <asp:TextBox ID="txtNewRPN" runat="server" CssClass="form-control form-control-sm fw-bold bg-light" ReadOnly="true" />
                                </div>
                                <div class="col-12">
                                    <asp:LinkButton ID="btnUpdateAction" runat="server" CssClass="btn btn-warning btn-sm" OnClick="btnUpdateAction_Click">
                                        <i class="bi bi-arrow-up-circle me-1"></i>Update Action
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>

        <div class="card shadow-sm mt-4">
            <div class="card-header bg-dark text-white d-flex justify-content-between align-items-center">
                <h6 class="mb-0"><i class="bi bi-table me-2"></i>FMEA Records</h6>
                <div class="d-flex align-items-center">
                    <label class="text-light me-2 small">Filter:</label>
                    <asp:TextBox ID="txtFilterStep" runat="server" CssClass="form-control form-control-sm me-2" placeholder="Process step..." style="width:180px;" />
                    <asp:LinkButton ID="btnFilter" runat="server" CssClass="btn btn-outline-light btn-sm me-2" OnClick="btnFilter_Click">
                        <i class="bi bi-funnel"></i>
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnClearFilter" runat="server" CssClass="btn btn-outline-light btn-sm" OnClick="btnClearFilter_Click">
                        <i class="bi bi-x-circle"></i>
                    </asp:LinkButton>
                    <asp:LinkButton ID="btnExportCsv" runat="server" CssClass="btn btn-outline-success btn-sm rounded-pill px-3" OnClick="btnExportCsv_Click">
                        <i class="bi bi-download me-1"></i>Export CSV
                    </asp:LinkButton>
                </div>
            </div>
            <div class="card-body">
                <div class="table-responsive">
                    <table class="table table-hover table-bordered align-middle mb-0 small">
                        <thead class="table-dark">
                            <tr>
                                <th>Process Step</th>
                                <th>Failure Mode</th>
                                <th class="text-center">S</th>
                                <th class="text-center">O</th>
                                <th class="text-center">D</th>
                                <th class="text-center">RPN</th>
                                <th>Recommended Action</th>
                                <th>Action Taken</th>
                                <th class="text-center">New RPN</th>
                                <th class="text-center">Actions</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptFMEARecords" runat="server" OnItemCommand="rptFMEARecords_ItemCommand">
                                <ItemTemplate>
                                    <tr>
                                        <td class="fw-semibold"><%# Eval("ProcessStep") %></td>
                                        <td><%# Eval("FailureMode") %></td>
                                        <td class="text-center"><%# Eval("Severity") %></td>
                                        <td class="text-center"><%# Eval("Occurrence") %></td>
                                        <td class="text-center"><%# Eval("Detection") %></td>
                                        <td class="text-center">
                                            <span class="badge <%# GetRPNBadge(If(IsDBNull(Eval("RPN")), 0, CInt(Eval("RPN")))) %>"><%# Eval("RPN") %></span>
                                        </td>
                                        <td><%# Eval("RecommendedAction") %></td>
                                        <td><%# If(String.IsNullOrEmpty(Eval("ActionTaken").ToString()), "-", Eval("ActionTaken")) %></td>
                                        <td class="text-center">
                                            <%# If(Eval("NewRPN") IsNot DBNull.Value, CInt(Eval("NewRPN")).ToString(), "-") %>
                                        </td>
                                        <td class="text-center" style="min-width: 80px;">
                                            <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-info me-1"
                                                CommandName="EditRecord" CommandArgument='<%# Eval("Id") %>'>
                                                <i class="bi bi-pencil"></i>
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                CommandName="DeleteRecord" CommandArgument='<%# Eval("Id") %>'
                                                OnClientClick="return confirm('Delete this FMEA record?');">
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
        <i class="bi bi-info-circle me-2"></i>Please select a project to manage FMEA records.
    </asp:Panel>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Scripts" runat="server">
    <script>
        function calculateRPN(severityId, occurrenceId, detectionId, targetId) {
            var s = parseInt(document.getElementById(severityId).value) || 1;
            var o = parseInt(document.getElementById(occurrenceId).value) || 1;
            var d = parseInt(document.getElementById(detectionId).value) || 1;
            document.getElementById(targetId).value = s * o * d;
        }

        function setupRPNElements() {
            var s = document.getElementById('<%= ddlSeverity.ClientID %>');
            var o = document.getElementById('<%= ddlOccurrence.ClientID %>');
            var d = document.getElementById('<%= ddlDetection.ClientID %>');
            var t = document.getElementById('<%= txtRPN.ClientID %>');
            if (s && o && d && t) {
                s.onchange = function () { calculateRPN(s.id, o.id, d.id, t.id); };
                o.onchange = function () { calculateRPN(s.id, o.id, d.id, t.id); };
                d.onchange = function () { calculateRPN(s.id, o.id, d.id, t.id); };
            }

            var s2 = document.getElementById('<%= ddlNewSeverity.ClientID %>');
            var o2 = document.getElementById('<%= ddlNewOccurrence.ClientID %>');
            var d2 = document.getElementById('<%= ddlNewDetection.ClientID %>');
            var t2 = document.getElementById('<%= txtNewRPN.ClientID %>');
            if (s2 && o2 && d2 && t2) {
                s2.onchange = function () { calculateRPN(s2.id, o2.id, d2.id, t2.id); };
                o2.onchange = function () { calculateRPN(s2.id, o2.id, d2.id, t2.id); };
                d2.onchange = function () { calculateRPN(s2.id, o2.id, d2.id, t2.id); };
            }
        }

        if (document.readyState === 'complete') {
            setupRPNElements();
        } else {
            document.addEventListener('DOMContentLoaded', setupRPNElements);
        }
    </script>
</asp:Content>
