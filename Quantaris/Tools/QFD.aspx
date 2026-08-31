<%@ Page Title="QFD - House of Quality" Language="VB" MasterPageFile="~/SiteMaster/Site.Master"
    AutoEventWireup="false" CodeFile="QFD.aspx.vb" Inherits="Tools_QFD" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .qfd-matrix { font-size: 11px; }
        .qfd-matrix th, .qfd-matrix td { text-align: center; vertical-align: middle; padding: 4px 6px; }
        .qfd-matrix input[type=number] { width: 40px; text-align: center; font-size: 11px; }
        .qfd-corner { background: #212529; color: #fff; font-size: 9px; writing-mode: vertical-lr; text-orientation: mixed; }
        .qfd-row-label { text-align: left !important; font-weight: 500; min-width: 120px; }
        .qfd-col-label { writing-mode: vertical-lr; text-orientation: mixed; height: 100px; white-space: nowrap; font-weight: 500; font-size: 10px; }
        .qfd-roof { background: #f8f9fa; }
        .qfd-roof td { border: 1px solid #dee2e6; }
        .qfd-strong { background: #198754; color: #fff; }
        .qfd-moderate { background: #ffc107; color: #000; }
        .qfd-weak { background: #fd7e14; color: #fff; }
        .qfd-none { background: #f8f9fa; }
        .corr-strong-pos { background: #198754; }
        .corr-pos { background: #a3d9a5; }
        .corr-neg { background: #f5a3a3; }
        .corr-strong-neg { background: #dc3545; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex justify-content-between align-items-center mb-4">
        <div>
            <h4 class="mb-1"><i class="bi bi-house-heart me-2 text-primary"></i>QFD - House of Quality</h4>
            <nav aria-label="breadcrumb">
                <ol class="breadcrumb mb-0 small">
                    <li class="breadcrumb-item"><a href='<%= ResolveUrl("~/Default.aspx?tenant=" & CurrentTenantCode) %>'>Home</a></li>
                    <li class="breadcrumb-item active">QFD</li>
                </ol>
            </nav>
        </div>
    </div>

    <div class="row g-3 mb-4">
        <div class="col-md-5">
            <label class="form-label small fw-semibold">Project</label>
            <asp:DropDownList ID="ddlProject" runat="server" CssClass="form-select" AutoPostBack="true"
                OnSelectedIndexChanged="ddlProject_SelectedIndexChanged" />
        </div>
        <div class="col-md-3">
            <label class="form-label small fw-semibold">Model Name</label>
            <asp:TextBox ID="txtModelName" runat="server" CssClass="form-control form-control-sm" placeholder="e.g. Product Design QFD" />
        </div>
        <div class="col-md-2 d-flex align-items-end">
            <asp:LinkButton ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary btn-sm w-100" OnClick="btnRefresh_Click">
                <i class="bi bi-arrow-clockwise"></i> Refresh
            </asp:LinkButton>
        </div>
    </div>

    <asp:Panel ID="pnlQFD" runat="server" Visible="false">
        <div class="row g-4">
            <div class="col-lg-3">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-list me-2"></i>Requirements</h6>
                    </div>
                    <div class="card-body">
                        <asp:Literal ID="litMessage" runat="server" />
                        <div class="mb-3">
                            <label class="form-label small fw-semibold">Customer Requirements</label>
                            <asp:TextBox ID="txtCustomerReqs" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="4"
                                placeholder="One per line, e.g.&#10;Easy to use (5)&#10;Reliable (4)" />
                            <div class="small text-muted mt-1">Add weight in parentheses (1-5).</div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label small fw-semibold">Technical Requirements</label>
                            <asp:TextBox ID="txtTechReqs" runat="server" CssClass="form-control form-control-sm" TextMode="MultiLine" Rows="4"
                                placeholder="One per line, e.g.&#10;Response time&#10;Durability" />
                        </div>
                        <div class="d-flex gap-2">
                            <asp:LinkButton ID="btnBuild" runat="server" CssClass="btn btn-primary btn-sm" OnClick="btnBuild_Click">
                                <i class="bi bi-buildings me-1"></i>Build Matrix
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnSave" runat="server" CssClass="btn btn-outline-success btn-sm" OnClick="btnSave_Click" Visible="false">
                                <i class="bi bi-floppy me-1"></i>Save
                            </asp:LinkButton>
                        </div>
                    </div>
                </div>

                <div class="card shadow-sm mt-4">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-clock-history me-2"></i>History</h6>
                    </div>
                    <div class="card-body p-0">
                        <div class="table-responsive">
                            <table class="table table-hover align-middle mb-0 small">
                                <thead class="table-dark">
                                    <tr><th>Date</th><th>Model</th><th class="text-center">Actions</th></tr>
                                </thead>
                                <tbody>
                                    <asp:Repeater ID="rptHistory" runat="server" OnItemCommand="rptHistory_ItemCommand">
                                        <ItemTemplate>
                                            <tr>
                                                <td class="text-nowrap"><%# CDate(Eval("CreatedOn")).ToString("MMM dd") %></td>
                                                <td><%# Eval("ModelName") %></td>
                                                <td class="text-center">
                                                    <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                                                        CommandName="DeleteModel" CommandArgument='<%# Eval("Id") %>'
                                                        OnClientClick="return confirm('Delete this QFD model?');"><i class="bi bi-trash"></i></asp:LinkButton>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </div>

            <div class="col-lg-9">
                <div class="card shadow-sm">
                    <div class="card-header bg-dark text-white">
                        <h6 class="mb-0"><i class="bi bi-grid-3x3 me-2"></i>House of Quality Matrix</h6>
                    </div>
                    <div class="card-body p-2 overflow-auto" style="max-height:600px;">
                        <asp:Panel ID="pnlMatrix" runat="server" Visible="false">
                            <asp:Literal ID="litMatrix" runat="server" />
                        </asp:Panel>
                        <asp:Panel ID="pnlNoMatrix" runat="server" CssClass="text-center text-muted py-5">
                            <i class="bi bi-house-heart fs-1 d-block mb-2"></i>
                            <span>Add requirements and build the matrix.</span>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlNoProject" runat="server" CssClass="alert alert-info" Visible="false">
        <i class="bi bi-info-circle me-2"></i>Please select a project.
    </asp:Panel>
</asp:Content>
