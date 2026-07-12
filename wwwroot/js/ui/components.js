import { state } from '../state.js';
import { escapeHtml, logToConsole } from '../utils.js';
import { setFilter } from './table.js';

export function renderEmptyState(message, type = 'info', isFilterEmpty = false) {
    const modsList = document.getElementById('mods-list');
    if (!modsList) return;
    modsList.innerHTML = `
        <tr>
            <td colspan="4">
                <div class="empty-state" style="color: ${type === 'error' ? 'var(--status-error)' : 'inherit'};">
                    ${escapeHtml(message)}
                    ${isFilterEmpty ? '<br><br><button class="btn-secondary" id="btn-clear-filters">Clear Filters</button>' : ''}
                </div>
            </td>
        </tr>
    `;
    if (isFilterEmpty) {
        const btnClear = document.getElementById('btn-clear-filters');
        if (btnClear) {
            btnClear.addEventListener('click', () => {
                const searchInput = document.getElementById('search-input');
                if (searchInput) searchInput.value = '';
                state.filters.search = '';
                setFilter('all');
            });
        }
    }
}

export function toggleConsole(collapsed) {
    state.ui.consoleCollapsed = collapsed;
    localStorage.setItem('cme-console-collapsed', collapsed);
    const consoleDrawer = document.getElementById('console-drawer');
    const btnConsoleToggle = document.getElementById('btn-console-toggle');
    if (collapsed) {
        consoleDrawer.classList.add('collapsed');
        btnConsoleToggle.textContent = '▲';
        btnConsoleToggle.setAttribute('aria-expanded', 'false');
    } else {
        consoleDrawer.classList.remove('collapsed');
        btnConsoleToggle.textContent = '▼';
        btnConsoleToggle.setAttribute('aria-expanded', 'true');
    }
}

export function handleCopyLog() {
    const consoleLogs = document.getElementById('console-logs');
    const btnCopyLog = document.getElementById('btn-copy-log');
    const text = Array.from(consoleLogs.querySelectorAll('.log-line'))
        .map(el => el.textContent)
        .join('\n');
    navigator.clipboard.writeText(text)
        .then(() => {
            const originalText = btnCopyLog.textContent;
            btnCopyLog.textContent = 'COPIED!';
            setTimeout(() => btnCopyLog.textContent = originalText, 2000);
        })
        .catch(err => logToConsole(`Failed to copy logs: ${err}`, 'error'));
}

export function updateLastScanTime() {
    if (!state.meta.lastScan) return;
    const lastScanEl = document.getElementById('last-scan-time');
    if (!lastScanEl) return;
    
    const seconds = Math.floor((Date.now() - state.meta.lastScan) / 1000);
    let text = 'Just now';
    if (seconds > 59) {
        const minutes = Math.floor(seconds / 60);
        text = `${minutes}m ago`;
    } else if (seconds > 10) {
        text = `${seconds}s ago`;
    }
    lastScanEl.textContent = `Last scanned: ${text}`;
}

let loaderState = { active: false, interval: null };
export function startLoaderAnimation() {
    loaderState.active = true;
    const loaderText = document.getElementById('loader-text');
    const fill = document.querySelector('.progress-bar-fill');
    if (!loaderText || !fill) return;

    fill.classList.remove('done');
    fill.style.width = '0%';
    
    const steps = [
        { text: '> Initiating workspace scan...', wait: 600, p: 10 },
        { text: '> Indexing local mod directories...', wait: 800, p: 35 },
        { text: '> Connecting to Forge API...', wait: 900, p: 60 },
        { text: '> Reconciling version hashes...', wait: 1200, p: 75 },
        { text: '> Analyzing dependencies...', wait: 2000, p: 90 }
    ];

    let i = 0;
    
    async function runSequence() {
        while(loaderState.active && i < steps.length) {
            loaderText.textContent = steps[i].text;
            fill.style.width = steps[i].p + '%';
            await new Promise(r => setTimeout(r, steps[i].wait));
            i++;
        }
    }
    runSequence();
}

export function stopLoaderAnimation() {
    loaderState.active = false;
    const loaderText = document.getElementById('loader-text');
    const fill = document.querySelector('.progress-bar-fill');
    if (loaderText) loaderText.textContent = '> SCAN COMPLETE';
    if (fill) {
        fill.classList.add('done');
        fill.style.width = '100%';
    }
}
