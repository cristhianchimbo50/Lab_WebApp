(function () {
    const mqLg = window.matchMedia('(min-width: 992px)');
    const htmlEl = () => document.documentElement;
    const isDesktop = () => mqLg.matches;

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

    document.addEventListener('click', function (ev) {
        const btn = ev.target.closest('.sidebar .btn.btn-toggle');
        if (!btn) return;

        ev.preventDefault();
        ev.stopPropagation();

        const targetSel = btn.getAttribute('data-bs-target');
        const target = targetSel ? document.querySelector(targetSel) : null;
        if (!target) return;

        const collapse = bootstrap.Collapse.getOrCreateInstance(target, { toggle: false });

        if (target.classList.contains('show')) {
            collapse.hide();
            btn.setAttribute('aria-expanded', 'false');

            setTimeout(() => btn.setAttribute('aria-expanded', 'false'), 150);
            return;
        }


        document.querySelectorAll('.sidebar .collapse.show').forEach(el => {
            const c = bootstrap.Collapse.getOrCreateInstance(el, { toggle: false });
            c.hide();
            el.previousElementSibling?.setAttribute('aria-expanded', 'false');
        });

        collapse.show();
        btn.setAttribute('aria-expanded', 'true');
    });

    function enableTooltips() {
        const els = document.querySelectorAll('[data-bs-toggle="tooltip"], [data-rail-tooltip="true"]');
        els.forEach(el => {
            try {
                const existing = bootstrap.Tooltip.getInstance(el);
                if (existing) existing.dispose();
                bootstrap.Tooltip.getOrCreateInstance(el, { placement: 'right', trigger: 'hover' });
            } catch { }
        });
    }

    document.addEventListener('DOMContentLoaded', enableTooltips);

    document.addEventListener('blazor:afterRender', enableTooltips);
    document.addEventListener('click', enableTooltips);
    document.addEventListener('shown.bs.collapse', enableTooltips);

    document.addEventListener('click', function (ev) {
        if (!isDesktop() && htmlEl().classList.contains('sb-open')) {
            const insideSidebar = !!ev.target.closest('.sidebar');
            const onToggle = !!ev.target.closest('.sb-toggle');
            if (!insideSidebar && !onToggle) htmlEl().classList.remove('sb-open');
        }
    }, true);

    document.addEventListener('click', function (ev) {
        if (!isDesktop() && htmlEl().classList.contains('sb-open')) {
            const navLink = ev.target.closest('.sidebar .nav-link');
            if (navLink) htmlEl().classList.remove('sb-open');
        }
    });

    document.addEventListener('keydown', function (ev) {
        if (!isDesktop() && htmlEl().classList.contains('sb-open') && ev.key === 'Escape') {
            htmlEl().classList.remove('sb-open');
        }
    });

    function syncOnResize() {
        if (isDesktop()) htmlEl().classList.remove('sb-open');
    }
    mqLg.addEventListener?.('change', syncOnResize);
    window.addEventListener('resize', syncOnResize);
})();

window.mostrarModalPdfDesdeBytes = function (pdfUrl) {
    const iframe = document.getElementById('iframePdf');
    if (!iframe) return;
    iframe.src = pdfUrl;

    const modalElement = document.getElementById('pdfModal');
    if (!modalElement) return;

    const modal = new bootstrap.Modal(modalElement, { backdrop: false, keyboard: true, focus: true });
    modalElement.style.zIndex = 2000;
    modal.show();
};

console.log("site.js cargado correctamente (Sidebar funcional)");

///
window.marcarModalAbierto = function () {
    document.body.classList.add("modal-activo");
};

window.marcarModalCerrado = function () {
    document.body.classList.remove("modal-activo");
};

