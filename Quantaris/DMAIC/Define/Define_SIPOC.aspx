<%@ Page Title="SIPOC Diagram" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Define_SIPOC.aspx.vb" Inherits="DMAIC_Define_Define_SIPOC" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-diagram-2 me-2"></i>SIPOC Diagram</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC Dashboard</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Define/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>Define</a></li>
                    <li class="breadcrumb-item active">SIPOC</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Dashboard
        </a>
    </div>

    <asp:HiddenField ID="hfProjectId" runat="server" Value='<%# Request.QueryString("projectId") %>' />
    <asp:HiddenField ID="hfSipocId" runat="server" Value="0" />

    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-table me-2"></i>SIPOC Map</h6>
        </div>
        <div class="card-body">
            <div class="row g-3">
                <div class="col">
                    <label class="form-label fw-semibold text-primary">Suppliers</label>
                    <asp:TextBox ID="txtSuppliers" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"
                        placeholder="Enter suppliers (one per line)" />
                </div>
                <div class="col">
                    <label class="form-label fw-semibold text-success">Inputs</label>
                    <asp:TextBox ID="txtInputs" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"
                        placeholder="Enter inputs (one per line)" />
                </div>
                <div class="col">
                    <label class="form-label fw-semibold text-warning">Process Steps</label>
                    <asp:TextBox ID="txtProcessSteps" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"
                        placeholder="Enter process steps (one per line)" />
                </div>
                <div class="col">
                    <label class="form-label fw-semibold text-info">Outputs</label>
                    <asp:TextBox ID="txtOutputs" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"
                        placeholder="Enter outputs (one per line)" />
                </div>
                <div class="col">
                    <label class="form-label fw-semibold text-danger">Customers</label>
                    <asp:TextBox ID="txtCustomers" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="8"
                        placeholder="Enter customers (one per line)" />
                </div>
            </div>

            <div class="mt-4 text-center">
                <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-primary btn-lg px-5"
                    OnClick="btnSave_Click"><i class='bi bi-save me-2'></i>Save SIPOC</asp:LinkButton>
            </div>
        </div>
    </div>

    <%-- Preview / Display Section --%>
    <div class="card shadow-sm mb-4">
        <div class="card-header bg-white">
            <h6 class="fw-semibold mb-0"><i class="bi bi-eye me-2"></i>SIPOC Preview</h6>
        </div>
        <div class="card-body p-0">
            <div class="table-responsive">
                <table class="table table-bordered mb-0 text-center align-middle">
                    <thead class="table-dark">
                        <tr>
                            <th class="bg-primary text-white">S<br/>U<br/>P<br/>P<br/>L<br/>I<br/>E<br/>R<br/>S</th>
                            <th class="bg-success text-white">I<br/>N<br/>P<br/>U<br/>T<br/>S</th>
                            <th class="bg-warning text-dark" style="width:30%;">P<br/>R<br/>O<br/>C<br/>E<br/>S<br/>S</th>
                            <th class="bg-info text-white">O<br/>U<br/>T<br/>P<br/>U<br/>T<br/>S</th>
                            <th class="bg-danger text-white">C<br/>U<br/>S<br/>T<br/>O<br/>M<br/>E<br/>R<br/>S</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td><asp:Literal ID="litSuppliers" runat="server" /></td>
                            <td><asp:Literal ID="litInputs" runat="server" /></td>
                            <td><asp:Literal ID="litProcessSteps" runat="server" /></td>
                            <td><asp:Literal ID="litOutputs" runat="server" /></td>
                            <td><asp:Literal ID="litCustomers" runat="server" /></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <%-- Navigation --%>
    <div class="d-flex justify-content-between">
        <a href='<%= ResolveUrl("~/DMAIC/Define/Define_Charter.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left me-1"></i>Back to Charter
        </a>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-success">
            <i class="bi bi-check-circle me-1"></i>Complete Define Phase
        </a>
    </div>
</asp:Content>
