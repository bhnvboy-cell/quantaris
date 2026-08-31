// Quantaris - Custom JavaScript

// Auto-calculate RPN for FMEA
function calculateRPN(severityId, occurrenceId, detectionId, resultId) {
    var s = parseInt(document.getElementById(severityId).value) || 0;
    var o = parseInt(document.getElementById(occurrenceId).value) || 0;
    var d = parseInt(document.getElementById(detectionId).value) || 0;
    document.getElementById(resultId).value = s * o * d;
}

// Auto-calculate cumulative percentage for Pareto
function calculatePareto() {
    var total = 0;
    var frequencies = document.querySelectorAll('.pareto-frequency');
    frequencies.forEach(function(el) {
        total += parseFloat(el.value) || 0;
    });
    var cumulative = 0;
    var percentages = document.querySelectorAll('.pareto-percentage');
    var cumulatives = document.querySelectorAll('.pareto-cumulative');
    frequencies.forEach(function(el, idx) {
        var freq = parseFloat(el.value) || 0;
        var pct = total > 0 ? (freq / total * 100) : 0;
        cumulative += pct;
        if (percentages[idx]) percentages[idx].value = pct.toFixed(1);
        if (cumulatives[idx]) cumulatives[idx].value = cumulative.toFixed(1);
    });
}

// Auto-calculate Takt Time
function calculateTakt() {
    var workTime = parseFloat(document.getElementById('txtWorkTime')?.value) || 0;
    var demand = parseFloat(document.getElementById('txtDemand')?.value) || 0;
    var takt = document.getElementById('txtTaktTime');
    if (takt && demand > 0) {
        takt.value = (workTime / demand).toFixed(2);
    }
}

// Confirm delete
function confirmDelete(message) {
    return confirm(message || 'Are you sure you want to delete this record?');
}

// Update step status via AJAX (simple approach)
function markStepComplete(projectId, stepNumber, tenantCode) {
    // This would typically be an AJAX call to a handler
    // For simplicity, uses form submission pattern
    return true;
}

// Control chart calculations
function calculateControlChart() {
    var dataText = document.getElementById('txtControlChartData')?.value;
    if (!dataText) return;

    var values = dataText.split('\n')
        .map(function(v) { return parseFloat(v.trim()); })
        .filter(function(v) { return !isNaN(v); });

    if (values.length < 2) return;

    var sum = values.reduce(function(a, b) { return a + b; }, 0);
    var mean = sum / values.length;
    var sumSqDiff = values.reduce(function(a, b) { return a + Math.pow(b - mean, 2); }, 0);
    var variance = sumSqDiff / (values.length - 1);
    var stdDev = Math.sqrt(variance);

    var cl = document.getElementById('txtCL');
    var ucl = document.getElementById('txtUCL');
    var lcl = document.getElementById('txtLCL');
    var meanDisplay = document.getElementById('litMean');
    var stdDisplay = document.getElementById('litStdDev');

    if (cl) cl.value = mean.toFixed(4);
    if (ucl) ucl.value = (mean + 3 * stdDev).toFixed(4);
    if (lcl) lcl.value = Math.max(0, mean - 3 * stdDev).toFixed(4);
    if (meanDisplay) meanDisplay.innerText = mean.toFixed(4);
    if (stdDisplay) stdDisplay.innerText = stdDev.toFixed(4);
}

// Bootstrap 5 sidebar toggle
document.addEventListener('DOMContentLoaded', function() {
    var sidebarToggle = document.getElementById('sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function() {
            document.getElementById('sidebar').classList.toggle('active');
        });
    }
});
