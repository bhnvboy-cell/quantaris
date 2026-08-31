<%@ Page Title="Define - Phase Overview" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Main.aspx.vb" Inherits="DMAIC_Define_Main" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-primary me-2">1</span>Define Phase</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC</a></li>
                    <li class="breadcrumb-item active">Define</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Back to Dashboard
        </a>
    </div>

    <div class="card shadow-sm">
        <div class="card-body text-center py-5">
            <i class="bi bi-1-circle display-1 text-primary mb-3"></i>
            <h5>Define Phase</h5>
            <p class="text-muted">Understand customer and business requirements, complete project charter, and map the as-is process.</p>
            <div class="list-group list-group-flush text-start mt-4">
                <a href='<%= ResolveUrl("~/DMAIC/Define/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-primary me-3">1</span>Understand Customer &amp; Business Requirements
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
                <a href='<%= ResolveUrl("~/DMAIC/Define/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-primary me-3">2</span>Complete Project Charter
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
                <a href='<%= ResolveUrl("~/DMAIC/Define/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-primary me-3">3</span>Complete High-Level As-Is Process Map (SIPOC)
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
            </div>
        </div>
    </div>
</asp:Content>
