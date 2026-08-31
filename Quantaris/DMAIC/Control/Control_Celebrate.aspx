<%@ Page Title="Step 15: Celebrate and Recognize Success" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Control_Celebrate.aspx.vb" Inherits="DMAIC_Control_Control_Celebrate" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-success me-2">15</span>Celebrate and Recognize Success</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Control/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Control</a></li>
                    <li class="breadcrumb-item active">Celebrate</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfEditId" runat="server" Value="0" />

    <div class="row g-4">
        <div class="col-lg-6">
            <%-- Card 1: New Entry --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-plus-circle me-2"></i>New Entry</h6>
                </div>
                <div class="card-body">
                    <div class="row g-3">
                        <div class="col-md-6">
                            <label class="form-label">Type <span class="text-danger">*</span></label>
                            <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select">
                                <asp:ListItem Text="-- Select --" Value="" />
                                <asp:ListItem Text="Lesson Learned" Value="LessonLearned" />
                                <asp:ListItem Text="Recognition" Value="Recognition" />
                                <asp:ListItem Text="Celebration" Value="Celebration" />
                            </asp:DropDownList>
                            <asp:RequiredFieldValidator ID="rfvType" runat="server" ControlToValidate="ddlType"
                                ErrorMessage="Type is required" CssClass="text-danger small" Display="Dynamic" />
                        </div>
                        <div class="col-md-6">
                            <label class="form-label">Recognition Date</label>
                            <asp:TextBox ID="txtRecognitionDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>
                        <div class="col-12">
                            <label class="form-label">Title <span class="text-danger">*</span></label>
                            <asp:TextBox ID="txtTitle" runat="server" CssClass="form-control" MaxLength="200" />
                            <asp:RequiredFieldValidator ID="rfvTitle" runat="server" ControlToValidate="txtTitle"
                                ErrorMessage="Title is required" CssClass="text-danger small" Display="Dynamic" />
                        </div>
                        <div class="col-12">
                            <label class="form-label">Description</label>
                            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                        </div>
                        <div class="col-12">
                            <label class="form-label">Participants</label>
                            <asp:TextBox ID="txtParticipants" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" placeholder="List participants or teams involved" />
                        </div>
                    </div>
                    <div class="mt-3">
                        <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary"
                            OnClick="btnSave_Click"><i class='bi bi-save me-1'></i>Save Entry</asp:LinkButton>
                        <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary ms-2"
                            Text="Cancel" OnClick="btnCancel_Click" Visible="false" />
                    </div>
                </div>
            </div>

            <%-- Card 3: Post-Implementation Review Summary --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-journal-text me-2"></i>Post-Implementation Review Summary</h6>
                </div>
                <div class="card-body">
                    <div class="mb-3">
                        <label class="form-label">Overall Lessons Learned Summary</label>
                        <asp:TextBox ID="txtPIRSummary" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="6"
                            placeholder="Summarize key lessons learned throughout the entire project&#10;What went well? What could be improved?&#10;What are the key takeaways for future projects?" />
                    </div>
                    <div>
                        <asp:LinkButton ID="btnSavePIR" runat="server" CssClass="btn btn-primary btn-sm"
                            OnClick="btnSavePIR_Click"><i class='bi bi-save me-1'></i>Save PIR Summary</asp:LinkButton>
                        <asp:Label ID="lblPIRSaved" runat="server" CssClass="text-success small ms-2" Visible="false"><i class="bi bi-check-circle"></i> Saved</asp:Label>
                    </div>
                </div>
            </div>
        </div>

        <div class="col-lg-6">
            <%-- Card 2: Entries List --%>
            <div class="card shadow-sm mb-4">
                <div class="card-header bg-white d-flex justify-content-between align-items-center">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-list-check me-2"></i>Entries</h6>
                    <span class="badge bg-secondary"><asp:Literal ID="litCount" runat="server" /></span>
                </div>
                <div class="card-body p-0">
                    <div class="table-responsive">
                        <table class="table table-hover table-striped mb-0 small">
                            <thead class="table-light">
                                <tr>
                                    <th>Type</th>
                                    <th>Title</th>
                                    <th>Date</th>
                                    <th class="text-center" style="width:90px;">Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                <asp:Repeater ID="rptEntries" runat="server" OnItemCommand="rptEntries_ItemCommand" OnItemDataBound="rptEntries_ItemDataBound">
                                    <ItemTemplate>
                                        <tr>
                                            <td><asp:Literal ID="litTypeBadge" runat="server" /></td>
                                            <td class="fw-semibold"><%# Eval("Title") %></td>
                                            <td><%# If(Eval("RecognitionDate") IsNot DBNull.Value, CDate(Eval("RecognitionDate")).ToString("yyyy-MM-dd"), If(Eval("CreatedOn") IsNot DBNull.Value, CDate(Eval("CreatedOn")).ToString("yyyy-MM-dd"), "")) %></td>
                                            <td class="text-center">
                                                <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                                    CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                                <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                    CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                                    OnClientClick="return confirm('Delete this entry?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                            </td>
                                        </tr>
                                    </ItemTemplate>
                                    
                                </asp:Repeater>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <%-- Card 4: Project Completion --%>
            <div class="card shadow-sm mb-4 border-success">
                <div class="card-header bg-success text-white">
                    <h6 class="fw-semibold mb-0"><i class="bi bi-check2-circle me-2"></i>Project Completion</h6>
                </div>
                <div class="card-body text-center">
                    <asp:Panel ID="pnlAllStepsComplete" runat="server" Visible="false">
                        <i class="bi bi-trophy display-4 text-success mb-3"></i>
                        <h5 class="text-success">All 15 DMAIC Steps Complete!</h5>
                        <p class="text-muted">Your project is ready to be marked as complete. This will set the project status to "Complete" and record the actual end date.</p>
                        <asp:LinkButton ID="btnCompleteProject" runat="server" CssClass="btn btn-success btn-lg"
                            OnClick="btnCompleteProject_Click"
                            OnClientClick="return confirm('Are you sure you want to mark this project as complete? This action cannot be undone.');"><i class='bi bi-check2-circle me-2'></i>Mark Project Complete</asp:LinkButton>
                    </asp:Panel>
                    <asp:Panel ID="pnlStepsRemaining" runat="server" Visible="false">
                        <i class="bi bi-hourglass-split display-4 text-warning mb-3"></i>
                        <h5 class="text-warning">Steps Remaining</h5>
                        <p class="text-muted">Complete all 15 DMAIC steps before marking the project as complete.</p>
                        <asp:Literal ID="litStepsRemaining" runat="server" />
                    </asp:Panel>
                </div>
            </div>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between mt-2 mb-4">
        <a href='<%= ResolveUrl("~/DMAIC/Control/Control_BestPractices.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back: Best Practices
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-primary">
            <i class="bi bi-speedometer2 me-1"></i>Return to Dashboard
        </a>
    </div>
</asp:Content>
