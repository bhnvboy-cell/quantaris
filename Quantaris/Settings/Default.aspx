<%@ Page Title="Settings" Language="VB" MasterPageFile="~/SiteMaster/Site.Master" AutoEventWireup="false" CodeFile="Default.aspx.vb" Inherits="Settings_Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="max-width:600px; margin:0 auto;">
        <h2 style="color:var(--ox-text-primary); margin-bottom:20px;"><i class="bi bi-gear"></i> Settings</h2>

        <div class="ox-card" style="padding:24px; margin-bottom:20px;">
            <h4 style="color:var(--ox-text-primary); margin-bottom:12px;"><i class="bi bi-robot"></i> OpenAI API Configuration</h4>
            <p style="color:var(--ox-text-secondary); font-size:12px; margin-bottom:16px;">
                Optional. Enable AI-powered rephrasing and grammar correction. Get your key at 
                <a href="https://platform.openai.com/api-keys" target="_blank" style="color:var(--ox-accent);">platform.openai.com</a>
            </p>

            <div style="margin-bottom:12px;">
                <label style="color:var(--ox-text-secondary); font-size:11px; text-transform:uppercase; letter-spacing:0.04em; display:block; margin-bottom:4px;">API Key</label>
                <asp:TextBox ID="txtApiKey" runat="server" TextMode="Password" CssClass="ox-input" 
                    placeholder="sk-..." style="width:100%; padding:8px 12px; background:var(--ox-bg-input); border:1px solid var(--ox-border); color:var(--ox-text-primary); border-radius:6px; font-size:13px;" />
            </div>

            <div style="display:flex; align-items:center; gap:12px; margin-bottom:12px;">
                <asp:Button ID="btnSave" runat="server" Text="Save Key" CssClass="ox-btn" OnClick="btnSave_Click"
                    style="padding:8px 20px; background:var(--ox-accent); color:#fff; border:none; border-radius:6px; cursor:pointer; font-size:13px; font-weight:500;" />
                <asp:Button ID="btnClear" runat="server" Text="Remove Key" CssClass="ox-btn" OnClick="btnClear_Click"
                    style="padding:8px 20px; background:var(--ox-bg-input); color:var(--ox-text-secondary); border:1px solid var(--ox-border); border-radius:6px; cursor:pointer; font-size:13px;" />
            </div>

            <asp:Label ID="lblStatus" runat="server" Style="font-size:12px;" />
        </div>

        <div class="ox-card" style="padding:24px;">
            <h4 style="color:var(--ox-text-primary); margin-bottom:12px;"><i class="bi bi-info-circle"></i> About Grammar Check</h4>
            <p style="color:var(--ox-text-secondary); font-size:12px; line-height:1.7;">
                Grammar checking uses <strong>LanguageTool</strong> (free, no key needed) for spelling, grammar, and style detection.<br/>
                Adding an OpenAI API key enables <strong>AI rephrasing</strong> and <strong>sentence simplification</strong> features on DMAIC text fields.
            </p>
        </div>
    </div>
</asp:Content>
