import { state, applyFilters, applySort } from '../state.js';
import { escapeHtml } from '../utils.js';
import { renderEmptyState } from './components.js';
import { renderVersionCell, renderStatusPill } from './renderers.js';
import { renderBulkBar, showOverview } from './dashboard.js';

export function renderHealthBanner(mods) {
    const healthBanner = document.getElementById('health-banner');
    if (!healthBanner) return;
    
    if (mods.length === 0) {
        healthBanner.hidden = true;
        return;
    }

    const activeMods = mods.filter(m => !m.isIgnored);
    const actionableMods = activeMods.filter(m => ['UpdateAvailable', 'UpdateBlocked', 'Incompatible'].includes(m.status)).length;

    healthBanner.hidden = false;
    healthBanner.className = 'health-banner'; 

    if (actionableMods === 0) {
        healthBanner.classList.add('health-banner-ok');
        healthBanner.textContent = `All ${activeMods.length} active mods up to date ✓`;
    } else {
        healthBanner.classList.add('health-banner-warn');
        healthBanner.textContent = `${actionableMods} of ${activeMods.length} active mods need attention ⚠`;
    }
}

export function renderStats(mods, filters) {
    // Top health board removed. Stats are now handled in Workspace Overview.
}

export function renderTable(filteredMods, sort, ui) {
    const modsList = document.getElementById('mods-list');
    if (!modsList) return;

    if (!filteredMods || filteredMods.length === 0) {
        renderEmptyState(state.mods.length > 0 ? "No mods match your current filters." : "No mods detected in target directory.", 'info', state.mods.length > 0);
        return;
    }

    modsList.innerHTML = '';
    
    filteredMods.forEach(mod => {
        const tr = document.createElement('tr');
        if (ui.selectedIds.has(String(mod.id))) tr.classList.add('selected');
        tr.dataset.id = mod.id;
        
        let statusClass = 'status-unknown';
        if (mod.status === 'UpToDate') statusClass = 'status-ok';
        else if (mod.status === 'NewerInstalled') statusClass = 'status-newer';
        else if (mod.status === 'UpdateAvailable') statusClass = 'status-warn';
        else if (['UpdateBlocked', 'Incompatible', 'Error', 'NoVersionsFound'].includes(mod.status)) statusClass = 'status-error';

        const escapedName = escapeHtml(mod.name || 'Unknown');
        const escapedAuthor = escapeHtml(mod.author || 'Unknown');
        const warningHtml = mod.hasWarnings ? '<span title="Mod has warnings. Check details pane." style="color: var(--status-warning); margin-left: 5px; font-size: 0.9rem;">⚠️</span>' : '';
        const typeLabel = mod.isServerMod 
            ? '<span style="color: var(--status-success); font-weight: 600;" title="Server Mod">Server</span>' 
            : '<span style="color: var(--status-info); font-weight: 600;" title="Client Mod">Client</span>';
        
        const actionHtml = renderVersionCell(mod);
        const statusPill = renderStatusPill(mod.status);

        tr.innerHTML = `
            <td>
                <input type="checkbox" class="row-checkbox action-select" value="${escapeHtml(mod.id)}" aria-label="Select mod" ${ui.selectedIds.has(String(mod.id)) ? 'checked' : ''}>
            </td>
            <td data-label="Status">
                <div style="display:flex; align-items:center; gap:var(--space-md);">
                    <div class="status-block ${statusClass}" title="${mod.status}" style="border-radius: 50%; box-shadow: 0 0 5px var(--status-${statusClass.split('-')[1]}); width: 12px; height: 12px; min-width: 12px;"></div>
                    ${statusPill}
                </div>
            </td>
            <td data-label="Mod Name">
                <div class="mod-card-primary">
                    <div class="mod-card-title" style="font-weight: 600;">${escapedName}${warningHtml}</div>
                    <div class="mod-card-meta">by ${escapedAuthor} • ${typeLabel}</div>
                </div>
            </td>
            <td data-label="Version" class="col-version" style="text-align: right;">
                ${actionHtml}
            </td>
        `;
        
        modsList.appendChild(tr);
    });
}

export function updateTitle(mods) {
    if (!mods || mods.length === 0) {
        document.title = "CheckModsExtended // MANAGER";
        return;
    }
    const outdatedCount = mods.filter(m => m.status === 'UpdateAvailable' || m.status === 'UpdateBlocked').length;
    if (outdatedCount > 0) {
        document.title = `(${outdatedCount} outdated) Check Mods Extended`;
    } else {
        document.title = "CheckModsExtended // MANAGER";
    }
}

export function renderChipCounts(mods, filteredMods, filters) {
    const countAll = document.getElementById('chip-count-all');
    const countOk = document.getElementById('chip-count-ok');
    const countAttention = document.getElementById('chip-count-attention');
    const countIgnored = document.getElementById('chip-count-ignored');
    const elSearchCount = document.getElementById('search-count');

    const ignoredMods = mods.filter(m => m.isIgnored === true);
    const upToDateMods = mods.filter(m => !m.isIgnored && (m.status === 'UpToDate' || m.status === 'NewerInstalled'));
    const attentionMods = mods.filter(m => !m.isIgnored && ['UpdateAvailable', 'UpdateBlocked', 'Incompatible'].includes(m.status));

    if (countAll) countAll.textContent = mods.length;
    if (countOk) countOk.textContent = upToDateMods.length;
    if (countAttention) countAttention.textContent = attentionMods.length;
    if (countIgnored) countIgnored.textContent = ignoredMods.length;

    if (elSearchCount) {
        if (filters.search || filters.status !== 'all') {
            elSearchCount.textContent = `Showing ${filteredMods.length} of ${mods.length}`;
        } else {
            elSearchCount.textContent = '';
        }
    }
    
    document.querySelectorAll('th[data-sortable]').forEach(th => {
        if (th.dataset.sortable === state.sort.column) {
            th.setAttribute('aria-sort', state.sort.direction === 'asc' ? 'ascending' : 'descending');
        } else {
            th.setAttribute('aria-sort', 'none');
        }
    });
}

export function render() {
    state.filteredMods = applyFilters(state.mods, state.filters);
    state.filteredMods = applySort(state.filteredMods, state.sort);
    
    renderHealthBanner(state.mods);
    renderStats(state.mods, state.filters);
    renderChipCounts(state.mods, state.filteredMods, state.filters);
    renderTable(state.filteredMods, state.sort, state.ui);
    renderBulkBar(state.ui.selectedIds);
    updateTitle(state.mods);

    // Sync select-all checkbox
    const selectAll = document.getElementById('select-all');
    if (selectAll) {
        selectAll.checked = state.filteredMods.length > 0 && state.filteredMods.every(m => state.ui.selectedIds.has(String(m.id)));
        selectAll.indeterminate = state.filteredMods.some(m => state.ui.selectedIds.has(String(m.id))) && !selectAll.checked;
    }

    // Show overview if no mod is selected
    if (!document.querySelector('#mods-list tr.selected')) {
        showOverview();
    }
}

export function setFilter(filter) {
    state.filters.status = filter;
    localStorage.setItem('cme-filter-status', filter);
    document.querySelectorAll('.chip').forEach(c => {
        if (c.dataset.filter === filter) c.classList.add('active');
        else c.classList.remove('active');
    });
    render();
}

export function setTheme(theme) {
    state.meta.theme = theme;
    document.documentElement.dataset.theme = theme;
    localStorage.setItem('cme-theme', theme);
    const btnTheme = document.getElementById('btn-theme');
    if (btnTheme) btnTheme.textContent = theme === 'dark' ? '🌙' : '☀️';
}
