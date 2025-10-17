(function () {
    const mqLg = window.matchMedia('(min-width: 992px)');
    const htmlEl = () => document.documentElement;

    function isDesktop() { return mqLg.matches; }

    function closeAllGroups() {
        document.querySelectorAll('.sidebar .collapse.show').forEach(el => {
            const c = bootstrap.Collapse.getOrCreateInstance(el, { toggle: false });
            c.hide();
        });
        document.querySelectorAll('.sidebar .btn.btn-toggle[aria-expanded="true"]').forEach(btn => {
            btn.setAttribute('aria-expanded', 'false');
        });
    }

    window.toggleSidebar = function () {
        if (isDesktop()) {
            htmlEl().classList.toggle('sb-collapsed');
        } else {
            const willOpen = !htmlEl().classList.contains('sb-open');
            htmlEl().classList.toggle('sb-open');
            if (willOpen) closeAllGroups();
        }
    };

    function enableTooltips() {
        document.querySelectorAll('.sidebar [data-bs-toggle="tooltip"]').forEach(el => {
            bootstrap.Tooltip.getOrCreateInstance(el);
        });
    }
    document.addEventListener('DOMContentLoaded', enableTooltips);

    function syncOnResize() {
        if (isDesktop()) {
            htmlEl().classList.remove('sb-open');
        }
    }
    mqLg.addEventListener?.('change', syncOnResize);
    window.addEventListener('resize', syncOnResize);

    document.addEventListener('click', function (ev) {
        if (!isDesktop() && htmlEl().classList.contains('sb-open')) {
            const insideSidebar = !!ev.target.closest('.sidebar');
            const onToggle = !!ev.target.closest('.sb-toggle');
            if (!insideSidebar && !onToggle) {
                htmlEl().classList.remove('sb-open');
            }
        }
    }, true);

    document.addEventListener('click', function (ev) {
        if (!isDesktop() && htmlEl().classList.contains('sb-open')) {
            const navLink = ev.target.closest('.sidebar .nav-link');
            if (navLink) {
                htmlEl().classList.remove('sb-open');
            }
        }
    });

    document.addEventListener('keydown', function (ev) {
        if (!isDesktop() && htmlEl().classList.contains('sb-open') && ev.key === 'Escape') {
            htmlEl().classList.remove('sb-open');
        }
    });

    document.addEventListener('click', function (ev) {
        const btnToggleGroup = ev.target.closest('.sidebar .btn.btn-toggle');
        if (!btnToggleGroup) return;

        if (isDesktop() && htmlEl().classList.contains('sb-collapsed')) {
            ev.preventDefault();
            ev.stopPropagation();
            htmlEl().classList.remove('sb-collapsed');

            const targetSel = btnToggleGroup.getAttribute('data-bs-target');
            if (targetSel) {
                const target = document.querySelector(targetSel);
                if (target) {
                    const c = bootstrap.Collapse.getOrCreateInstance(target, { toggle: false });
                    c.show();
                    btnToggleGroup.setAttribute('aria-expanded', 'true');
                }
            }
            return;
        }

        if (!isDesktop() && !htmlEl().classList.contains('sb-open')) {
            ev.preventDefault();
            ev.stopPropagation();
            htmlEl().classList.add('sb-open');
            closeAllGroups();
            return;
        }

    });
})();

(function () {
    const mqLg = window.matchMedia('(min-width: 992px)');
    function isDesktop() { return mqLg.matches; }
    function html() { return document.documentElement; }

    function openSidebarIfClosed(e, forceTarget) {
        const el = html();
        if (isDesktop()) {
            if (el.classList.contains('sb-collapsed')) {
                e && e.preventDefault();
                el.classList.remove('sb-collapsed');
                if (forceTarget) {
                    const c = bootstrap.Collapse.getOrCreateInstance(forceTarget, { toggle: false });
                    c.show();
                }
                return true;
            }
        } else {
            if (!el.classList.contains('sb-open')) {
                e && e.preventDefault();
                el.classList.add('sb-open');
                if (forceTarget) {
                    const c = bootstrap.Collapse.getOrCreateInstance(forceTarget, { toggle: false });
                    c.show();
                }
                return true;
            }
        }
        return false;
    }

    document.addEventListener('click', function (ev) {
        const btnToggle = ev.target.closest('.sidebar .btn.btn-toggle');
        if (btnToggle) {
            const targetSel = btnToggle.getAttribute('data-bs-target');
            const target = targetSel ? document.querySelector(targetSel) : null;
            const openedByExpand = openSidebarIfClosed(ev, target);
            if (!openedByExpand && target) {
                const c = bootstrap.Collapse.getOrCreateInstance(target);
                c.toggle();
            }
            return;
        }
        const navLink = ev.target.closest('.sidebar .nav-link');
        if (navLink) {
            const wasClosed = openSidebarIfClosed(ev, null);
            if (wasClosed) return;
        }
    });

    function initTooltips() {
        document.querySelectorAll('.sidebar [data-bs-toggle="tooltip"], .sidebar [data-rail-tooltip="true"]').forEach(el => {
            bootstrap.Tooltip.getOrCreateInstance(el, { placement: 'right', trigger: 'hover' });
        });
    }
    initTooltips();

    mqLg.addEventListener?.('change', () => {
        if (isDesktop()) html().classList.remove('sb-open');
    });

    window.toggleSidebar = window.toggleSidebar || function () {
        const el = document.documentElement;
        if (mqLg.matches) {
            el.classList.toggle('sb-collapsed');
        } else {
            el.classList.toggle('sb-open');
        }
    };
})();

window.mostrarModalPdfDesdeBytes = function (pdfUrl) {
    console.log("mostrarModalPdfDesdeBytes ejecutado con:", pdfUrl);

    const iframe = document.getElementById('iframePdf');
    if (!iframe) {
        console.error("No se encontró el iframe con id 'iframePdf'.");
        return;
    }

    iframe.src = pdfUrl;

    const modalElement = document.getElementById('pdfModal');
    if (!modalElement) {
        console.error("No se encontró el modal con id 'pdfModal'.");
        return;
    }

    const modal = new bootstrap.Modal(modalElement, {
        backdrop: false,
        keyboard: true,
        focus: true
    });
    modalElement.style.zIndex = 2000;
    modal.show();
};

console.log("site.js cargado correctamente (Blazor Server)");