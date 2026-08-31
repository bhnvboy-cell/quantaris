<%@ Page Title="Step 10: Generate, Prioritize and Select Solutions" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Improve_SolutionGeneration.aspx.vb" Inherits="DMAIC_Improve_Improve_SolutionGeneration" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-warning text-dark me-2">10</span>Generate, Prioritize and Select Solutions</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Improve/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Improve</a></li>
                    <li class="breadcrumb-item active">Solution Generation</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />

    <%-- Card 1: Existing Solution Ideas --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-lightbulb me-2"></i>Solution Ideas</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litIdeaCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Title</th>
                            <th>Description</th>
                            <th>Technique</th>
                            <th>Lean Tool</th>
                            <th>Score</th>
                            <th>Selected</th>
                            <th class="text-center" style="width:90px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptSolutions" runat="server" OnItemCommand="rptSolutions_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td class="fw-semibold"><%# Eval("IdeaTitle") %></td>
                                    <td><%# Eval("Description") %></td>
                                    <td><span class="badge bg-info"><%# Eval("Technique") %></span></td>
                                    <td><%# Eval("LeanToolType") %></td>
                                    <td><%# Eval("SelectionScore", "{0:N0}") %></td>
                                    <td>
                                        <asp:CheckBox ID="chkSelected" runat="server" Checked='<%# If(IsDBNull(Eval("IsSelected")), False, CBool(Eval("IsSelected"))) %>' Enabled="false" />
                                    </td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this solution idea?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- Card 2: New / Edit Solution Idea --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-plus-circle me-2"></i>New Solution Idea</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">Idea Title <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtIdeaTitle" runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtIdeaTitle"
                        ErrorMessage="Title is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-3">
                    <label class="form-label">Technique</label>
                    <asp:DropDownList ID="ddlTechnique" runat="server" CssClass="form-select" AutoPostBack="true"
                        OnSelectedIndexChanged="ddlTechnique_SelectedIndexChanged">
                        <asp:ListItem Text="-- Select --" Value="" />
                        <asp:ListItem Text="Brainstorming" Value="Brainstorming" />
                        <asp:ListItem Text="Benchmarking" Value="Benchmarking" />
                        <asp:ListItem Text="Brainwriting" Value="Brainwriting" />
                        <asp:ListItem Text="Assumption Busting" Value="AssumptionBusting" />
                        <asp:ListItem Text="Analogy" Value="Analogy" />
                        <asp:ListItem Text="Anti-Solution" Value="AntiSolution" />
                        <asp:ListItem Text="Lean Tool" Value="LeanTool" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-3" id="divLeanTool" runat="server" visible="false">
                    <label class="form-label">Lean Tool Type</label>
                    <asp:DropDownList ID="ddlLeanToolType" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Select --" Value="" />
                        <asp:ListItem Text="5S" Value="5S" />
                        <asp:ListItem Text="Heijunka" Value="Heijunka" />
                        <asp:ListItem Text="Pull" Value="Pull" />
                        <asp:ListItem Text="JIT" Value="JIT" />
                        <asp:ListItem Text="Other" Value="Other" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-12">
                    <label class="form-label">Description</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" MaxLength="2000" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Category</label>
                    <asp:TextBox ID="txtCategory" runat="server" CssClass="form-control" MaxLength="100" />
                </div>
                <div class="col-md-2">
                    <label class="form-label">Selection Score (1-10)</label>
                    <asp:TextBox ID="txtSelectionScore" runat="server" CssClass="form-control" MaxLength="2" />
                    <asp:RangeValidator ID="rvScore" runat="server" ControlToValidate="txtSelectionScore"
                        MinimumValue="1" MaximumValue="10" Type="Integer"
                        ErrorMessage="Score must be 1-10" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">Selection Criteria</label>
                    <asp:TextBox ID="txtSelectionCriteria" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                </div>
                <div class="col-md-2 d-flex align-items-center">
                    <div class="form-check mt-4">
                        <asp:CheckBox ID="chkIsSelected" runat="server" CssClass="form-check-input" />
                        <label class="form-check-label">Is Selected</label>
                    </div>
                </div>
            </div>
            <div class="mt-3">
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary"
                    OnClick="btnSave_Click"><i class='bi bi-save me-1'></i>Save Idea</asp:LinkButton>
                <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary ms-2"
                    Text="Cancel" OnClick="btnCancel_Click" Visible="false" />
            </div>
        </div>
    </div>

    <%-- Card 3: Solution Selection Matrix --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-grid-3x3-gap me-2"></i>Solution Selection Matrix</h6>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-bordered mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Idea</th>
                            <th>Technique</th>
                            <th>Score</th>
                            <th>Criteria</th>
                            <th>Selected for Pilot</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptSelectionMatrix" runat="server">
                            <ItemTemplate>
                                <tr class='<%# If(CBool(Eval("IsSelected")) AndAlso Not IsDBNull(Eval("SelectionScore")) AndAlso CDec(Eval("SelectionScore")) >= 7, "table-success", "") %>'>
                                    <td class="fw-semibold"><%# Eval("IdeaTitle") %></td>
                                    <td><span class="badge bg-info"><%# Eval("Technique") %></span></td>
                                    <td><%# Eval("SelectionScore", "{0:N0}") %></td>
                                    <td><%# Eval("SelectionCriteria") %></td>
                                    <td class="text-center">
                                        <asp:CheckBox ID="chkSelectedMatrix" runat="server" Checked='<%# If(IsDBNull(Eval("IsSelected")), False, CBool(Eval("IsSelected"))) %>' Enabled="false" />
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
        <a href='<%= ResolveUrl("~/DMAIC/Improve/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Improve
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Improve/Improve_PilotPlan.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: Pilot Solutions <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
