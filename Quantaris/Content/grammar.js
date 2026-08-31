function initAiAssist() {
    const textareas = document.querySelectorAll('textarea');
    textareas.forEach(function (ta) {
        const rows = parseInt(ta.getAttribute('rows'), 10);
        if (isNaN(rows) || rows < 2) return;
        if (ta.dataset.aiAssist === 'true') return;
        ta.dataset.aiAssist = 'true';

        const wrapper = document.createElement('div');
        wrapper.className = 'btn-group btn-group-sm mt-1';

        const mainBtn = document.createElement('button');
        mainBtn.className = 'btn btn-outline-primary dropdown-toggle';
        mainBtn.type = 'button';
        mainBtn.setAttribute('data-bs-toggle', 'dropdown');
        mainBtn.setAttribute('data-bs-auto-close', 'outside');
        mainBtn.innerHTML = '<i class="bi bi-robot me-1"></i>AI Assist <i class="bi bi-caret-down-fill"></i>';

        const menu = document.createElement('ul');
        menu.className = 'dropdown-menu';

        const actions = [
            { label: 'Check Grammar', icon: 'bi-spellcheck', action: 'grammar', style: '' },
            { label: 'Rephrase (Professional)', icon: 'bi-magic', action: 'rephrase', style: 'professional' },
            { label: 'Rephrase (Concise)', icon: 'bi-magic', action: 'rephrase', style: 'concise' },
            { label: 'Rephrase (Formal)', icon: 'bi-magic', action: 'rephrase', style: 'formal' },
            { label: 'Simplify', icon: 'bi-pencil', action: 'simplify', style: 'simple' }
        ];

        actions.forEach(function (item) {
            const li = document.createElement('li');
            const a = document.createElement('a');
            a.className = 'dropdown-item';
            a.href = '#';
            a.innerHTML = '<i class="bi ' + item.icon + ' me-1"></i>' + item.label;
            a.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                const bsDropdown = bootstrap.Dropdown.getInstance(mainBtn);
                if (bsDropdown) bsDropdown.hide();
                processText(ta, mainBtn, item.action, item.style);
            });
            li.appendChild(a);
            menu.appendChild(li);
        });

        wrapper.appendChild(mainBtn);
        wrapper.appendChild(menu);

        ta.parentNode.insertBefore(wrapper, ta.nextSibling);
    });
}

function processText(textarea, btn, action, style) {
    const text = textarea.value.trim();
    if (!text) {
        showToast('No text to process.', 'warning');
        return;
    }

    const origHtml = btn.innerHTML;
    btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status"></span>Working...';
    btn.disabled = true;

    const body = JSON.stringify({ action: action, text: text, style: style });

    fetch('/API/AIHandler.ashx', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: body
    })
    .then(function (res) { return res.json(); })
    .then(function (data) {
        btn.innerHTML = origHtml;
        btn.disabled = false;

        if (data.success && data.result) {
            textarea.value = data.result;
        }

        if (data.issues && data.issues.length > 0) {
            const msgs = data.issues.map(function (i) {
                return '<strong>' + (i.type || 'note') + ':</strong> ' + i.message;
            }).join('<br>');
            showToast(msgs, data.issues[0].type === 'error' ? 'error' : 'info');
        } else if (action === 'grammar') {
            showToast('No issues found!', 'success');
        }
    })
    .catch(function () {
        btn.innerHTML = origHtml;
        btn.disabled = false;
        showToast('Error connecting to server.', 'error');
    });
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initAiAssist);
} else {
    initAiAssist();
}
