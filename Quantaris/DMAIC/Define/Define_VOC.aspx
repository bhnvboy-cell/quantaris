<%@ Page Title="Step 1: Voice of Customer (VOC)" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Define_VOC.aspx.vb" Inherits="DMAIC_Define_Define_VOC" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-megaphone me-2"></i>Step 1: Voice of Customer (VOC)</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Define/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Define</a></li>
                    <li class="breadcrumb-item active">VOC</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />

    <%-- SECTION 1: VOC Entry Form --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-plus-circle me-2"></i>VOC Entry</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col-md-6">
                    <label class="form-label">Customer Name <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtCustomerName" runat="server" CssClass="form-control" MaxLength="200" />
                    <asp:RequiredFieldValidator ID="rfvCustomerName" runat="server" ControlToValidate="txtCustomerName"
                        ErrorMessage="Customer name is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Kano Classification</label>
                    <asp:DropDownList ID="ddlKano" runat="server" CssClass="form-select">
                        <asp:ListItem Text="-- Select --" Value="" />
                        <asp:ListItem Text="Basic (Must-Be)" Value="Basic" />
                        <asp:ListItem Text="Performance" Value="Performance" />
                        <asp:ListItem Text="Excitement (Delighter)" Value="Excitement" />
                        <asp:ListItem Text="Indifferent" Value="Indifferent" />
                        <asp:ListItem Text="Reverse" Value="Reverse" />
                    </asp:DropDownList>
                </div>
                <div class="col-12">
                    <label class="form-label">Voice Statement <span class="text-danger">*</span></label>
                    <asp:TextBox ID="txtVoice" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                    <asp:RequiredFieldValidator ID="rfvVoice" runat="server" ControlToValidate="txtVoice"
                        ErrorMessage="Voice statement is required" CssClass="text-danger small" Display="Dynamic" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Affinity Grouping</label>
                    <asp:TextBox ID="txtAffinity" runat="server" CssClass="form-control" MaxLength="200" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">Translated Need</label>
                    <asp:TextBox ID="txtNeed" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                </div>
                <div class="col-12">
                    <label class="form-label">Requirement</label>
                    <asp:TextBox ID="txtRequirement" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                </div>
            </div>
            <div class="mt-3">
                <asp:LinkButton ID="btnSaveNew" runat="server" CssClass="btn btn-outline-primary me-2"
                    OnClick="btnSaveNew_Click"><i class='bi bi-plus-lg'></i> Save &amp; New</asp:LinkButton>
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary"
                    OnClick="btnSave_Click"><i class='bi bi-check-lg'></i> Save</asp:LinkButton>
            </div>
        </div>
    </div>

    <%-- SECTION 2: VOC Records Table --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white d-flex justify-content-between align-items-center">
            <h6 class="fw-semibold mb-0"><i class="bi bi-table me-2"></i>VOC Records</h6>
            <span class="badge bg-secondary"><asp:Literal ID="litRecordCount" runat="server" /></span>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-hover table-striped mb-0 small">
                    <thead class="table-light">
                        <tr>
                            <th>Customer</th>
                            <th>Voice Statement</th>
                            <th>Affinity</th>
                            <th>Translated Need</th>
                            <th>Requirement</th>
                            <th>Kano</th>
                            <th>Created By</th>
                            <th>Date</th>
                            <th class="text-center" style="width:90px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptVOC" runat="server" OnItemCommand="rptVOC_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("CustomerName") %></td>
                                    <td><%# Eval("VoiceStatement") %></td>
                                    <td><%# Eval("AffinityGrouping") %></td>
                                    <td><%# Eval("TranslatedNeed") %></td>
                                    <td><%# Eval("Requirement") %></td>
                                    <td><span class="badge bg-info"><%# Eval("KanoClassification") %></span></td>
                                    <td><%# Eval("CreatedByName") %></td>
                                    <td><%# CDate(Eval("CreatedOn")).ToString("yyyy-MM-dd") %></td>
                                    <td class="text-center">
                                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                                            CommandName="Edit" CommandArgument='<%# Eval("Id") %>'><i class="bi bi-pencil"></i></asp:LinkButton>
                                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                            CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                            OnClientClick="return confirm('Delete this VOC record?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- SECTION 3: CTQ Link --%>
    <div class="d-flex justify-content-between">
        <a href='<%= ResolveUrl("~/DMAIC/Define/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Define
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Define/Define_CTQ.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-primary">
            Next: CTQ Requirements <i class="bi bi-arrow-right ms-1"></i>
        </a>
    </div>
</asp:Content>
