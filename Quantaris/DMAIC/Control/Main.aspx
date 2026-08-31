<%@ Page Title="Control - Phase Overview" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Main.aspx.vb" Inherits="DMAIC_Control_Main" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-info me-2">5</span>Control Phase</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC</a></li>
                    <li class="breadcrumb-item active">Control</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Back to Dashboard
        </a>
    </div>

    <div class="card shadow-sm">
        <div class="card-body text-center py-5">
            <i class="bi bi-5-circle display-1 text-info mb-3"></i>
            <h5>Control Phase</h5>
            <p class="text-muted">Institutionalize solutions, replicate best practices, and celebrate success.</p>
            <div class="list-group list-group-flush text-start mt-4">
                <a href='<%= ResolveUrl("~/DMAIC/Control/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-info me-3">13</span>Institutionalize the Solutions
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
                <a href='<%= ResolveUrl("~/DMAIC/Control/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-info me-3">14</span>Replicate and Share Best Practices
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
                <a href='<%= ResolveUrl("~/DMAIC/Control/Main.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-info me-3">15</span>Celebrate and Recognize Success
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
            </div>
        </div>
    </div>
</asp:Content>
