<%@ Page Title="Measure - Phase Overview" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Main.aspx.vb" Inherits="DMAIC_Measure_Main" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><span class="badge bg-warning text-dark me-2">2</span>Measure Phase</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Projects/Default.aspx?tenant=" & CurrentTenantCode) %>'>Projects</a></li>
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>'>DMAIC</a></li>
                    <li class="breadcrumb-item active">Measure</li>
                </ol>
            </nav>
        </div>
        <a href='<%= ResolveUrl("~/DMAIC/Dashboard.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="btn btn-outline-secondary btn-sm">
            <i class="bi bi-arrow-left me-1"></i>Back to Dashboard
        </a>
    </div>

    <div class="card shadow-sm">
        <div class="card-body text-center py-5">
            <i class="bi bi-2-circle display-1 text-warning mb-3"></i>
            <h5>Measure Phase</h5>
            <p class="text-muted">Identify what to measure, plan and collect data, and determine baseline performance.</p>
            <div class="list-group list-group-flush text-start mt-4">
                <a href='<%= ResolveUrl("~/DMAIC/Measure/Measure_WhatToMeasure.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-warning text-dark me-3">4</span>Identify What to Measure
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
                <a href='<%= ResolveUrl("~/DMAIC/Measure/Measure_PlanCollectData.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-warning text-dark me-3">5</span>Plan and Collect Data
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
                <a href='<%= ResolveUrl("~/DMAIC/Measure/Measure_Baseline.aspx?projectId=" & Request.QueryString("projectId") & "&tenant=" & CurrentTenantCode) %>' class="list-group-item list-group-item-action d-flex align-items-center">
                    <span class="badge bg-warning text-dark me-3">6</span>Determine Baseline Performance
                    <i class="bi bi-chevron-right ms-auto"></i>
                </a>
            </div>
        </div>
    </div>
</asp:Content>
