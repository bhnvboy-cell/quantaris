<%@ Page Language="VB" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Server Error - Quantaris</title>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@300;400;500;600;700&display=swap" rel="stylesheet" />
    <style>
        [data-theme="modern"], :root { --ox-bg: #0f0f1a; --ox-card: #1e1e32; --ox-border: #2a2a45; --ox-accent: #ff4d6a; --ox-text: #e8e8f0; --ox-muted: #8b8ba3; --ox-logo-bg: linear-gradient(135deg, #00d4aa, #0090ff); }
        [data-theme="classical"] { --ox-bg: #f0f2f5; --ox-card: #ffffff; --ox-border: #dde1e6; --ox-accent: #dc2626; --ox-text: #1a1a2e; --ox-muted: #5a5a6e; --ox-logo-bg: linear-gradient(135deg, #0066cc, #004499); }
        [data-theme="premium"] { --ox-bg: #111118; --ox-card: #22222e; --ox-border: #33334a; --ox-accent: #d45454; --ox-text: #f0ece4; --ox-muted: #a09888; --ox-logo-bg: linear-gradient(135deg, #d4a843, #b08830); }
        [data-theme="minimal"] { --ox-bg: #fafafa; --ox-card: #ffffff; --ox-border: #e5e5e5; --ox-accent: #dc2626; --ox-text: #171717; --ox-muted: #525252; --ox-logo-bg: linear-gradient(135deg, #171717, #404040); }
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body { background: var(--ox-bg); min-height: 100vh; display: flex; align-items: center; justify-content: center; font-family: 'Inter', sans-serif; color: var(--ox-text); transition: background 0.3s, color 0.3s; }
        .error-container { text-align: center; max-width: 400px; padding: 20px; }
        .error-code { font-size: 80px; font-weight: 700; color: var(--ox-accent); line-height: 1; margin-bottom: 16px; }
        .error-title { font-size: 20px; font-weight: 600; margin-bottom: 8px; }
        .error-desc { font-size: 13px; color: var(--ox-muted); margin-bottom: 24px; line-height: 1.6; }
        .error-btn { display: inline-flex; align-items: center; gap: 8px; background: var(--ox-accent); color: #fff; border: none; padding: 10px 20px; border-radius: 6px; font-size: 13px; font-weight: 600; text-decoration: none; font-family: inherit; cursor: pointer; transition: opacity 0.15s; }
        .error-btn:hover { opacity: 0.9; color: #fff; }
        .error-themes { display: flex; justify-content: center; gap: 6px; margin-bottom: 24px; }
        .error-theme-btn { width: 28px; height: 28px; border-radius: 50%; border: 2px solid var(--ox-border); cursor: pointer; transition: all 0.2s; }
        .error-theme-btn:hover { transform: scale(1.1); border-color: var(--ox-accent); }
        .et-modern { background: linear-gradient(135deg, #0f0f1a 50%, #00d4aa 50%); }
        .et-classical { background: linear-gradient(135deg, #f0f2f5 50%, #0066cc 50%); }
        .et-premium { background: linear-gradient(135deg, #111118 50%, #d4a843 50%); }
        .et-minimal { background: linear-gradient(135deg, #fafafa 50%, #171717 50%); }
    </style>
</head>
<body>
    <div class="error-container">
        <div class="error-themes">
            <button class="error-theme-btn et-modern" data-theme="modern" onclick="setTheme('modern')"></button>
            <button class="error-theme-btn et-classical" data-theme="classical" onclick="setTheme('classical')"></button>
            <button class="error-theme-btn et-premium" data-theme="premium" onclick="setTheme('premium')"></button>
            <button class="error-theme-btn et-minimal" data-theme="minimal" onclick="setTheme('minimal')"></button>
        </div>
        <div class="error-code">500</div>
        <div class="error-title">Server Error</div>
        <div class="error-desc">Something went wrong. Please try again or contact your administrator.</div>
        <a href="/Default.aspx" class="error-btn">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 9l9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
            Back to Dashboard
        </a>
    </div>
    <script>
        function setTheme(n){document.documentElement.setAttribute('data-theme',n);localStorage.setItem('ox-theme',n);document.querySelectorAll('.error-theme-btn').forEach(function(b){b.style.borderColor=b.getAttribute('data-theme')===n?'var(--ox-accent)':''})}
        (function(){var s=localStorage.getItem('ox-theme')||'modern';setTheme(s)})();
    </script>
</body>
</html>
