<%@ Page Language="VB" AutoEventWireup="false" CodeFile="Login.aspx.vb" Inherits="Account_Login" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Login - Quantaris</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.min.css" rel="stylesheet" />
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet" />
    <style>
        /* ========== THEMES ========== */
        [data-theme="modern"], :root {
            --ox-bg: #0f0f1a; --ox-bg-card: #1e1e32; --ox-border: #2a2a45;
            --ox-accent: #00d4aa; --ox-accent-hover: #00e8bc; --ox-text: #e8e8f0; --ox-muted: #8b8ba3;
            --ox-input-bg: #1a1a2e; --ox-logo-bg: linear-gradient(135deg, #00d4aa, #0090ff);
        }
        [data-theme="classical"] {
            --ox-bg: #f0f2f5; --ox-bg-card: #ffffff; --ox-border: #dde1e6;
            --ox-accent: #0066cc; --ox-accent-hover: #0052a3; --ox-text: #1a1a2e; --ox-muted: #5a5a6e;
            --ox-input-bg: #f0f2f5; --ox-logo-bg: linear-gradient(135deg, #0066cc, #004499);
        }
        [data-theme="premium"] {
            --ox-bg: #111118; --ox-bg-card: #22222e; --ox-border: #33334a;
            --ox-accent: #d4a843; --ox-accent-hover: #e0b84e; --ox-text: #f0ece4; --ox-muted: #a09888;
            --ox-input-bg: #1e1e2a; --ox-logo-bg: linear-gradient(135deg, #d4a843, #b08830);
        }
        [data-theme="minimal"] {
            --ox-bg: #fafafa; --ox-bg-card: #ffffff; --ox-border: #e5e5e5;
            --ox-accent: #171717; --ox-accent-hover: #404040; --ox-text: #171717; --ox-muted: #525252;
            --ox-input-bg: #f5f5f5; --ox-logo-bg: linear-gradient(135deg, #171717, #404040);
        }

        * { box-sizing: border-box; margin: 0; padding: 0; }
        body {
            background: var(--ox-bg);
            min-height: 100vh;
            display: flex;
            align-items: center;
            justify-content: center;
            font-family: 'Inter', -apple-system, sans-serif;
            color: var(--ox-text);
            transition: background 0.3s, color 0.3s;
        }
        body::before {
            content: '';
            position: fixed;
            top: -50%; left: -50%;
            width: 200%; height: 200%;
            background: radial-gradient(circle at 30% 40%, rgba(var(--ox-accent-rgb, 0,212,170), 0.06) 0%, transparent 50%);
            z-index: 0;
        }
        .login-wrapper {
            position: relative; z-index: 1;
            width: 100%; max-width: 400px; padding: 20px;
        }
        .login-brand {
            text-align: center; margin-bottom: 32px;
        }
        .login-brand .logo {
            width: 48px; height: 48px;
            background: var(--ox-logo-bg);
            border-radius: 12px;
            display: inline-flex; align-items: center; justify-content: center;
            font-size: 22px; color: #fff; font-weight: 700; margin-bottom: 12px;
        }
        .login-brand h1 { font-size: 22px; font-weight: 600; margin: 0; letter-spacing: -0.02em; }
        .login-brand p { font-size: 12px; color: var(--ox-muted); margin-top: 4px; }
        .login-card {
            background: var(--ox-bg-card);
            border: 1px solid var(--ox-border);
            border-radius: 12px; padding: 28px;
            transition: background 0.3s, border-color 0.3s;
        }
        .login-tenant {
            position: absolute; top: 16px; right: 16px;
            background: rgba(var(--ox-accent-rgb, 0,212,170), 0.12);
            color: var(--ox-accent);
            padding: 3px 10px; border-radius: 4px;
            font-size: 10px; font-weight: 600; letter-spacing: 0.03em;
        }
        .login-card h2 { font-size: 16px; font-weight: 600; margin-bottom: 20px; }
        .field-group { margin-bottom: 16px; }
        .field-group label {
            display: block; font-size: 11px; font-weight: 600;
            color: var(--ox-muted); text-transform: uppercase;
            letter-spacing: 0.04em; margin-bottom: 6px;
        }
        .field-input {
            width: 100%;
            background: var(--ox-input-bg);
            border: 1px solid var(--ox-border);
            color: var(--ox-text);
            padding: 10px 12px 10px 36px;
            border-radius: 6px; font-size: 13px; font-family: inherit;
            outline: none; transition: border-color 0.2s, box-shadow 0.2s, background 0.3s;
        }
        .field-input:focus {
            border-color: var(--ox-accent);
            box-shadow: 0 0 0 2px rgba(var(--ox-accent-rgb, 0,212,170), 0.15);
        }
        .field-input::placeholder { color: var(--ox-muted); }
        .field-icon { position: relative; }
        .field-icon i {
            position: absolute; left: 12px; top: 50%; transform: translateY(-50%);
            color: var(--ox-muted); font-size: 14px;
        }
        .field-error { color: #ff4d6a; font-size: 11px; margin-top: 4px; }
        .error-box {
            background: rgba(255, 77, 106, 0.1); border: 1px solid rgba(255, 77, 106, 0.3);
            color: #ff4d6a; padding: 8px 12px; border-radius: 6px;
            font-size: 12px; margin-bottom: 16px;
        }
        .login-check { display: flex; align-items: center; gap: 8px; margin-bottom: 20px; }
        .login-check input[type="checkbox"] { accent-color: var(--ox-accent); width: 14px; height: 14px; }
        .login-check label { font-size: 12px; color: var(--ox-muted); cursor: pointer; }
        .login-btn {
            width: 100%;
            background: var(--ox-accent);
            color: #fff; border: none; padding: 10px;
            border-radius: 6px; font-size: 13px; font-weight: 600;
            font-family: inherit; cursor: pointer; transition: background 0.15s;
        }
        .login-btn:hover { background: var(--ox-accent-hover); }
        .login-footer { text-align: center; margin-top: 16px; font-size: 12px; color: var(--ox-muted); }
        .login-footer a { color: var(--ox-accent); text-decoration: none; }
        .login-version { text-align: center; margin-top: 24px; font-size: 11px; color: var(--ox-muted); }

        /* Theme selector on login */
        .login-themes {
            display: flex; justify-content: center; gap: 8px; margin-bottom: 24px;
        }
        .login-theme-btn {
            width: 36px; height: 36px; border-radius: 50%;
            border: 2px solid var(--ox-border); cursor: pointer;
            transition: all 0.2s; position: relative; overflow: hidden;
        }
        .login-theme-btn:hover { border-color: var(--ox-accent); transform: scale(1.1); }
        .login-theme-btn.active { border-color: var(--ox-accent); box-shadow: 0 0 0 2px rgba(var(--ox-accent-rgb, 0,212,170), 0.3); }
        .login-theme-btn.active::after {
            content: '✓'; position: absolute; top: 50%; left: 50%;
            transform: translate(-50%, -50%); font-size: 12px;
            color: #fff; font-weight: 700; text-shadow: 0 1px 2px rgba(0,0,0,0.5);
        }
        .lt-modern { background: linear-gradient(135deg, #0f0f1a 50%, #00d4aa 50%); }
        .lt-classical { background: linear-gradient(135deg, #f0f2f5 50%, #0066cc 50%); }
        .lt-premium { background: linear-gradient(135deg, #111118 50%, #d4a843 50%); }
        .lt-minimal { background: linear-gradient(135deg, #fafafa 50%, #171717 50%); }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-wrapper">
            <div class="login-brand">
                <div class="logo">Q</div>
                <h1>Quantaris</h1>
                <p>Six Sigma Quality Management</p>
            </div>

            <!-- Theme Selector -->
            <div class="login-themes">
                <button type="button" class="login-theme-btn lt-modern active" data-theme="modern" onclick="setTheme('modern')" title="Modern"></button>
                <button type="button" class="login-theme-btn lt-classical" data-theme="classical" onclick="setTheme('classical')" title="Classical"></button>
                <button type="button" class="login-theme-btn lt-premium" data-theme="premium" onclick="setTheme('premium')" title="Premium"></button>
                <button type="button" class="login-theme-btn lt-minimal" data-theme="minimal" onclick="setTheme('minimal')" title="Minimal"></button>
            </div>

            <div class="login-card" style="position:relative;">
                <span class="login-tenant"><i class="bi bi-building me-1"></i><asp:Literal ID="litTenantCode" runat="server" /></span>
                <h2>Sign In</h2>

                <div id="divError" runat="server" Visible="false" class="error-box"></div>

                <div class="field-group">
                    <label>Username</label>
                    <div class="field-icon">
                        <i class="bi bi-person"></i>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="field-input" placeholder="Enter username" />
                    </div>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtUsername" CssClass="field-error" Display="Dynamic" ErrorMessage="Username is required." />
                </div>

                <div class="field-group">
                    <label>Password</label>
                    <div class="field-icon">
                        <i class="bi bi-lock"></i>
                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="field-input" placeholder="Enter password" />
                    </div>
                    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtPassword" CssClass="field-error" Display="Dynamic" ErrorMessage="Password is required." />
                </div>

                <div class="login-check">
                    <asp:CheckBox ID="chkRemember" runat="server" />
                    <label for="chkRemember">Remember me</label>
                </div>

                <asp:Button ID="btnLogin" runat="server" CssClass="login-btn" Text="Sign In" OnClick="btnLogin_Click" />

                <div class="login-footer">
                    Don't have an account?
                    <a href="Register.aspx?tenant=<%= Request.QueryString("tenant") %>">Register</a>
                </div>
            </div>

            <div class="login-version">&copy; <%= DateTime.Now.Year %> Quantaris v2.0</div>
        </div>
    </form>
    <script>
        function setTheme(name) {
            document.documentElement.setAttribute('data-theme', name);
            localStorage.setItem('ox-theme', name);
            document.querySelectorAll('.login-theme-btn').forEach(function(btn) {
                btn.classList.toggle('active', btn.getAttribute('data-theme') === name);
            });
        }
        (function() {
            var saved = localStorage.getItem('ox-theme') || 'modern';
            setTheme(saved);
        })();
    </script>
</body>
</html>
