<%@ Page Title="Workflow Designer" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="WorkflowDesigner.aspx.vb" Inherits="Tools_WorkflowDesigner" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-diagram-3 me-2 text-primary"></i>Workflow Designer &amp; Approval Engine</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">Workflow Designer</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-4">
        <div class="col-lg-7">
            <div class="card shadow-sm">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-pencil-square me-2"></i>Workflow Designer</h6>
                </div>
                <div class="card-body">
                    <asp:Literal ID="litMessage" runat="server" />
                    <div class="row g-3 mb-3">
                        <div class="col-md-6">
                            <label class="form-label small fw-semibold">Workflow Name</label>
                            <asp:TextBox ID="txtWorkflowName" runat="server" CssClass="form-control form-control-sm" />
                            <asp:RequiredFieldValidator ID="rfvWorkflowName" runat="server" ControlToValidate="txtWorkflowName" Display="Dynamic" CssClass="text-danger small" ErrorMessage="Required" />
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small fw-semibold">Entity Type</label>
                            <asp:DropDownList ID="ddlEntityType" runat="server" CssClass="form-select form-select-sm">
                                <asp:ListItem Text="Project" Value="Project" />
                                <asp:ListItem Text="FMEA" Value="FMEA" />
                                <asp:ListItem Text="CAPA" Value="CAPA" />
                                <asp:ListItem Text="Audit" Value="Audit" />
                            </asp:DropDownList>
                        </div>
                        <div class="col-md-3">
                            <label class="form-label small fw-semibold">Description</label>
                            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control form-control-sm" />
                        </div>
                    </div>

                    <div class="card bg-light border-0 mb-3">
                        <div class="card-body py-3">
                            <h6 class="small text-muted text-uppercase mb-2"><i class="bi bi-diagram-2 me-1"></i>State Management</h6>
                            <div class="table-responsive mb-2">
                                <table class="table table-bordered table-sm mb-0 small">
                                    <thead class="table-secondary">
                                        <tr>
                                            <th>State Name</th>
                                            <th style="width:70px;">Order</th>
                                            <th style="width:80px;">Initial</th>
                                            <th style="width:80px;">Final</th>
                                            <th>Assigned Role</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rptStates" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%# Eval("StateName") %></td>
                                                    <td class="text-center"><%# Eval("StateOrder") %></td>
                                                    <td class="text-center"><%# If(If(IsDBNull(Eval("IsInitial")), False, CBool(Eval("IsInitial"))), "<i class='bi bi-check-lg text-success'></i>", "") %></td>
                                                    <td class="text-center"><%# If(If(IsDBNull(Eval("IsFinal")), False, CBool(Eval("IsFinal"))), "<i class='bi bi-check-lg text-success'></i>", "") %></td>
                                                    <td><%# Eval("AssignedRole") %></td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                            </div>
                            <div class="row g-2 align-items-end">
                                <div class="col-md-3">
                                    <label class="form-label small">Name</label>
                                    <asp:TextBox ID="txtStateName" runat="server" CssClass="form-control form-control-sm" />
                                </div>
                                <div class="col-md-1">
                                    <label class="form-label small">Order</label>
                                    <asp:TextBox ID="txtStateOrder" runat="server" CssClass="form-control form-control-sm" TextMode="Number" />
                                </div>
                                <div class="col-md-1">
                                    <div class="form-check">
                                        <asp:CheckBox ID="chkIsInitial" runat="server" CssClass="form-check-input" />
                                        <label class="form-check-label small">Init</label>
                                    </div>
                                </div>
                                <div class="col-md-1">
                                    <div class="form-check">
                                        <asp:CheckBox ID="chkIsFinal" runat="server" CssClass="form-check-input" />
                                        <label class="form-check-label small">Final</label>
                                    </div>
                                </div>
                                <div class="col-md-3">
                                    <label class="form-label small">Role</label>
                                    <asp:DropDownList ID="ddlAssignedRole" runat="server" CssClass="form-select form-select-sm" />
                                </div>
                                <div class="col-md-2">
                                    <asp:LinkButton ID="btnAddState" runat="server" CssClass="btn btn-outline-primary btn-sm w-100" OnClick="btnAddState_Click">
                                        <i class="bi bi-plus-lg me-1"></i>Add
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="card bg-light border-0 mb-3">
                        <div class="card-body py-3">
                            <h6 class="small text-muted text-uppercase mb-2"><i class="bi bi-arrow-left-right me-1"></i>Transition Management</h6>
                            <div class="table-responsive mb-2">
                                <table class="table table-bordered table-sm mb-0 small">
                                    <thead class="table-secondary">
                                        <tr>
                                            <th>From State</th>
                                            <th>To State</th>
                                            <th>Transition Name</th>
                                            <th>Required Role</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <asp:Repeater ID="rptTransitions" runat="server">
                                            <ItemTemplate>
                                                <tr>
                                                    <td><%# Eval("FromStateName") %></td>
                                                    <td><%# Eval("ToStateName") %></td>
                                                    <td><%# Eval("TransitionName") %></td>
                                                    <td><%# Eval("RequiredRole") %></td>
                                                </tr>
                                            </ItemTemplate>
                                        </asp:Repeater>
                                    </tbody>
                                </table>
                            </div>
                            <div class="row g-2 align-items-end">
                                <div class="col-md-3">
                                    <label class="form-label small">From State</label>
                                    <asp:DropDownList ID="ddlFromState" runat="server" CssClass="form-select form-select-sm" />
                                </div>
                                <div class="col-md-3">
                                    <label class="form-label small">To State</label>
                                    <asp:DropDownList ID="ddlToState" runat="server" CssClass="form-select form-select-sm" />
                                </div>
                                <div class="col-md-3">
                                    <label class="form-label small">Transition Name</label>
                                    <asp:TextBox ID="txtTransitionName" runat="server" CssClass="form-control form-control-sm" />
                                </div>
                                <div class="col-md-2">
                                    <label class="form-label small">Required Role</label>
                                    <asp:DropDownList ID="ddlRequiredRole" runat="server" CssClass="form-select form-select-sm" />
                                </div>
                                <div class="col-md-1">
                                    <asp:LinkButton ID="btnAddTransition" runat="server" CssClass="btn btn-outline-primary btn-sm w-100" OnClick="btnAddTransition_Click">
                                        <i class="bi bi-plus-lg"></i>
                                    </asp:LinkButton>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div class="d-flex gap-2">
                        <asp:LinkButton ID="btnSaveWorkflow" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnSaveWorkflow_Click">
                            <i class="bi bi-floppy me-1"></i>Save Workflow
                        </asp:LinkButton>
                        <asp:LinkButton ID="btnClearDesigner" runat="server" CssClass="btn btn-outline-secondary btn-sm" OnClick="btnClearDesigner_Click">
                            <i class="bi bi-x-circle me-1"></i>Clear
                        </asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-5">
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-dark text-white">
                    <h6 class="mb-0"><i class="bi bi-play-circle me-2"></i>Active Instances</h6>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label small fw-semibold">Select Workflow</label>
                        <asp:DropDownList ID="ddlWorkflowDef" runat="server" CssClass="form-select form-select-sm" AutoPostBack="true" OnSelectedIndexChanged="ddlWorkflowDef_SelectedIndexChanged">
                            <asp:ListItem Text="-- Select Workflow --" Value="" />
                        </asp:DropDownList>
                    </div>
                    <div class="table-responsive">
                        <table class="table table-hover table-bordered align-middle mb-0 small">
                            <thead class="table-dark">
                                <tr>
                                    <th>Entity</th>
                                    <th>Entity ID</th>
                                    <th>Current State</th>
                                    <th>Assigned User</th>
                                    <th>Created</th>
                                    <th></th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptActiveInstances" runat="server" OnItemCommand="rptActiveInstances_ItemCommand">
                                    <ItemTemplate>
                                        <tr>
                                            <td><%# Eval("EntityType") %></td>
                                            <td><%# Eval("EntityId") %></td>
                                            <td><%# Eval("CurrentState") %></td>
                                            <td><%# Eval("AssignedUser") %></td>
                                            <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd") %></td>
                                            <td class="text-center">
                                                <asp:LinkButton ID="btnHistory" runat="server" CssClass="btn btn-sm btn-outline-info"
                                                    CommandName="ViewHistory" CommandArgument='<%# Eval("Id") %>'>
                                                    <i class="bi bi-clock-history"></i>
                                                </asp:LinkButton>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                    <asp:Panel ID="pnlApprovalHistory" runat="server" Visible="false" CssClass="mt-3">
                        <hr />
                        <h6 class="small text-muted text-uppercase"><i class="bi bi-clock-history me-1"></i>Approval History</h6>
                        <div class="table-responsive">
                            <table class="table table-sm table-bordered mb-0 small">
                                <thead class="table-secondary">
                                    <tr>
                                        <th>From</th>
                                        <th>To</th>
                                        <th>Transition</th>
                                        <th>Decision</th>
                                        <th>By</th>
                                        <th>Date</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptApprovalHistory" runat="server">
                                        <ItemTemplate>
                                            <tr>
                                                <td><%# Eval("FromState") %></td>
                                                <td><%# Eval("ToState") %></td>
                                                <td><%# Eval("TransitionName") %></td>
                                                <td>
                                                    <span class="badge <%# If(Eval("Decision").ToString() = "Approved", "bg-success", If(Eval("Decision").ToString() = "Rejected", "bg-danger", "bg-warning text-dark")) %>">
                                                        <%# Eval("Decision") %>
                                                    </span>
                                                </td>
                                                <td><%# Eval("ApprovedByName") %></td>
                                                <td class="text-nowrap"><%# If(Eval("DecidedOn") IsNot DBNull.Value, CDate(Eval("DecidedOn")).ToString("MMM dd, HH:mm"), "-") %></td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <div class="card shadow-sm mt-2">
        <div class="card-header bg-dark text-white">
            <h6 class="mb-0"><i class="bi bi-inbox me-2"></i>My Approvals</h6>
        </div>
        <div class="card-body">
            <asp:Literal ID="litApprovalMessage" runat="server" />
            <div class="mb-2">
                <label class="form-label small fw-semibold">Comments</label>
                <asp:TextBox ID="txtApprovalComments" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="1" />
            </div>
            <div class="table-responsive">
                <table class="table table-hover table-bordered align-middle mb-0 small">
                    <thead class="table-dark">
                        <tr>
                            <th>Entity Type</th>
                            <th>Entity ID</th>
                            <th>Current State</th>
                            <th>Transition</th>
                            <th>Requested On</th>
                            <th class="text-center">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptPendingApprovals" runat="server" OnItemCommand="rptPendingApprovals_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("EntityType") %></td>
                                    <td><%# Eval("EntityId") %></td>
                                    <td><%# Eval("CurrentState") %></td>
                                    <td><%# Eval("TransitionName") %></td>
                                    <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd, HH:mm") %></td>
                                    <td class="text-center" style="min-width:150px;">
                                        <asp:LinkButton ID="btnApprove" runat="server" CssClass="btn btn-success btn-sm me-1"
                                            CommandName="Approve" CommandArgument='<%# Eval("Id") %>'>
                                            <i class="bi bi-check-circle me-1"></i>Approve
                                        </asp:LinkButton>
                                        <asp:LinkButton ID="btnReject" runat="server" CssClass="btn btn-danger btn-sm"
                                            CommandName="Reject" CommandArgument='<%# Eval("Id") %>'>
                                            <i class="bi bi-x-circle me-1"></i>Reject
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
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="Scripts" runat="server">
</asp:Content>
