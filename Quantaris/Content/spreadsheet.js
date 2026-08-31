var Spreadsheet = (function () {
    var instances = {};

    function init(containerId, options) {
        options = options || {};
        var defaults = {
            columns: [{ name: 'C1', label: 'Measurement' }],
            rows: 50,
            onCellChange: null
        };
        var cfg = Object.assign({}, defaults, options);

        var state = {
            columns: cfg.columns.map(function (c) { return { name: c.name, label: c.label || c.name }; }),
            data: [],
            activeCell: null,
            cfg: cfg
        };

        for (var r = 0; r < cfg.rows; r++) {
            var row = [];
            for (var c = 0; c < state.columns.length; c++) {
                row.push('');
            }
            state.data.push(row);
        }

        var container = document.getElementById(containerId);
        if (!container) return null;
        container.innerHTML = '';
        container.classList.add('spreadsheet-container');

        var wrapper = document.createElement('div');
        wrapper.className = 'spreadsheet-wrapper';

        var table = document.createElement('table');
        table.className = 'spreadsheet-table';
        table.setAttribute('tabindex', '0');

        var thead = document.createElement('thead');
        var headerRow = document.createElement('tr');

        var thRowNum = document.createElement('th');
        thRowNum.className = 'spreadsheet-header rowNum';
        thRowNum.textContent = '';
        headerRow.appendChild(thRowNum);

        for (var c = 0; c < state.columns.length; c++) {
            var th = document.createElement('th');
            th.className = 'spreadsheet-header';
            th.dataset.col = c;
            var labelSpan = document.createElement('span');
            labelSpan.className = 'spreadsheet-col-label';
            labelSpan.textContent = state.columns[c].label;
            labelSpan.title = 'Double-click to rename';
            th.appendChild(labelSpan);

            var delBtn = document.createElement('span');
            delBtn.className = 'spreadsheet-col-del';
            delBtn.title = 'Delete column';
            delBtn.textContent = '\u00d7';
            delBtn.dataset.col = c;
            delBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                var ci = parseInt(this.dataset.col);
                if (state.columns.length <= 1) return;
                state.columns.splice(ci, 1);
                for (var r = 0; r < state.data.length; r++) {
                    state.data[r].splice(ci, 1);
                }
                rebuildTable(containerId);
            });
            th.appendChild(delBtn);

            th.addEventListener('dblclick', function () {
                var ci = parseInt(this.dataset.col);
                var span = this.querySelector('.spreadsheet-col-label');
                var current = state.columns[ci].label;
                var input = document.createElement('input');
                input.type = 'text';
                input.value = current;
                input.className = 'spreadsheet-col-input';
                span.replaceWith(input);
                input.focus();
                input.select();
                function finish() {
                    var val = input.value.trim() || current;
                    state.columns[ci].label = val;
                    var newSpan = document.createElement('span');
                    newSpan.className = 'spreadsheet-col-label';
                    newSpan.textContent = val;
                    input.replaceWith(newSpan);
                }
                input.addEventListener('blur', finish);
                input.addEventListener('keydown', function (ev) {
                    if (ev.key === 'Enter') finish();
                    if (ev.key === 'Escape') {
                        input.value = current;
                        finish();
                    }
                });
            });

            headerRow.appendChild(th);
        }

        var thAdd = document.createElement('th');
        thAdd.className = 'spreadsheet-header spreadsheet-add-col';
        thAdd.textContent = '+';
        thAdd.title = 'Add column';
        thAdd.addEventListener('click', function () {
            var newIdx = state.columns.length;
            var newName = 'C' + (newIdx + 1);
            state.columns.push({ name: newName, label: newName });
            for (var r = 0; r < state.data.length; r++) {
                state.data[r].push('');
            }
            rebuildTable(containerId);
        });
        headerRow.appendChild(thAdd);

        thead.appendChild(headerRow);
        table.appendChild(thead);

        var tbody = document.createElement('tbody');
        for (var r = 0; r < state.data.length; r++) {
            var tr = document.createElement('tr');
            var tdRowNum = document.createElement('td');
            tdRowNum.className = 'spreadsheet-rowNum';
            tdRowNum.textContent = r + 1;
            tr.appendChild(tdRowNum);

            for (var c = 0; c < state.columns.length; c++) {
                var td = document.createElement('td');
                td.className = 'spreadsheet-cell';
                td.contentEditable = 'true';
                td.dataset.row = r;
                td.dataset.col = c;
                td.textContent = state.data[r][c] || '';
                td.spellcheck = false;

                td.addEventListener('focus', function () {
                    if (state.activeCell) state.activeCell.classList.remove('active');
                    this.classList.add('active');
                    state.activeCell = this;
                });

                td.addEventListener('blur', function () {
                    var row = parseInt(this.dataset.row);
                    var col = parseInt(this.dataset.col);
                    var val = this.textContent.trim();
                    state.data[row][col] = val;
                    if (typeof state.cfg.onCellChange === 'function') {
                        state.cfg.onCellChange(row, col, val);
                    }
                });

                td.addEventListener('keydown', function (ev) {
                    var row = parseInt(this.dataset.row);
                    var col = parseInt(this.dataset.col);
                    if (ev.key === 'Tab') {
                        ev.preventDefault();
                        var nextCol = ev.shiftKey ? col - 1 : col + 1;
                        var nextRow = row;
                        if (nextCol >= state.columns.length) {
                            nextCol = 0;
                            nextRow = row + 1;
                        } else if (nextCol < 0) {
                            nextCol = state.columns.length - 1;
                            nextRow = row - 1;
                        }
                        if (nextRow >= 0 && nextRow < state.data.length) {
                            var nextCell = table.querySelector('td[data-row="' + nextRow + '"][data-col="' + nextCol + '"]');
                            if (nextCell) { nextCell.focus(); }
                        }
                    } else if (ev.key === 'Enter') {
                        ev.preventDefault();
                        var nextRow2 = row + 1;
                        if (nextRow2 < state.data.length) {
                            var nextCell2 = table.querySelector('td[data-row="' + nextRow2 + '"][data-col="' + col + '"]');
                            if (nextCell2) { nextCell2.focus(); }
                        }
                    }
                });

                tr.appendChild(td);
            }
            tbody.appendChild(tr);
        }

        table.appendChild(tbody);
        wrapper.appendChild(table);
        container.appendChild(wrapper);

        // Clipboard paste support
        container.addEventListener('paste', function (ev) {
            ev.preventDefault();
            var activeCell = container.querySelector('.spreadsheet-cell.active');
            if (!activeCell) return;

            var startRow = parseInt(activeCell.dataset.row);
            var startCol = parseInt(activeCell.dataset.col);
            var text = ev.clipboardData.getData('text');
            var lines = text.split(/\r?\n/);

            for (var r = 0; r < lines.length; r++) {
                if (lines[r].trim() === '' && r === lines.length - 1) continue;
                var cells = lines[r].split(/\t/);
                for (var c = 0; c < cells.length; c++) {
                    var tgtRow = startRow + r;
                    var tgtCol = startCol + c;
                    if (tgtRow < state.data.length && tgtCol < state.columns.length) {
                        state.data[tgtRow][tgtCol] = cells[c].replace(/"/g, '').trim();
                        var td = table.querySelector('td[data-row="' + tgtRow + '"][data-col="' + tgtCol + '"]');
                        if (td) td.textContent = state.data[tgtRow][tgtCol];
                    }
                }
            }
        });

        instances[containerId] = { state: state, table: table, container: container };
        return instances[containerId];
    }

    function rebuildTable(containerId) {
        var inst = instances[containerId];
        if (!inst) return;
        init(containerId, inst.state.cfg);
        var newInst = instances[containerId];
        newInst.state.columns = inst.state.columns;
        newInst.state.data = inst.state.data;
        var table = newInst.table;
        for (var r = 0; r < inst.state.data.length && r < newInst.state.data.length; r++) {
            for (var c = 0; c < inst.state.columns.length && c < newInst.state.data[r].length; c++) {
                var td = table.querySelector('td[data-row="' + r + '"][data-col="' + c + '"]');
                if (td) td.textContent = inst.state.data[r][c] || '';
            }
        }
    }

    function getData(containerId) {
        var inst = instances[containerId];
        if (!inst) return { columns: [], rows: [] };
        var columns = inst.state.columns.map(function (c) {
            return { name: c.name, label: c.label };
        });
        var rows = [];
        for (var r = 0; r < inst.state.data.length; r++) {
            rows.push(inst.state.data[r].slice());
        }
        return { columns: columns, rows: rows };
    }

    function setData(containerId, data) {
        var inst = instances[containerId];
        if (!inst || !data) return;
        if (data.columns && data.columns.length > 0) {
            inst.state.columns = data.columns.map(function (c) {
                return { name: c.name || 'C1', label: c.label || c.name || 'C1' };
            });
        }
        inst.state.data = [];
        var rowCount = (data.rows && data.rows.length > 0) ? data.rows.length : 50;
        for (var r = 0; r < rowCount; r++) {
            var row = [];
            for (var c = 0; c < inst.state.columns.length; c++) {
                row.push((data.rows && data.rows[r] && data.rows[r][c]) ? data.rows[r][c] : '');
            }
            inst.state.data.push(row);
        }
        rebuildTable(containerId);
        instances[containerId].state.cfg = inst.state.cfg;
    }

    function getColumn(containerId, colIndex) {
        var inst = instances[containerId];
        if (!inst) return [];
        var result = [];
        for (var r = 0; r < inst.state.data.length; r++) {
            var val = parseFloat(inst.state.data[r][colIndex]);
            result.push(isNaN(val) ? null : val);
        }
        return result;
    }

    return {
        render: init,
        getData: getData,
        setData: setData,
        getColumn: getColumn
    };
})();
