window.app = {
    theme: {
        current() {
            return document.documentElement.classList.contains('dark') ? 'dark' : 'light';
        },
        apply(value) {
            const dark = value === 'dark';
            document.documentElement.classList.toggle('dark', dark);
            try {
                localStorage.setItem('theme', value);
            } catch (e) { }
            return value;
        },
        toggle() {
            return this.apply(this.current() === 'dark' ? 'light' : 'dark');
        },
        init() {
            let value;
            try {
                value = localStorage.getItem('theme');
            } catch (e) { }
            if (!value) {
                value = window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
            }
            return this.apply(value);
        }
    },
    scrollToBottom(elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollTo({ top: element.scrollHeight, behavior: 'smooth' });
        }
    }
};

/*
 * Mouse-aware highlighting. One passive, document-level pointer listener drives
 * every effect so dynamically-rendered Blazor nodes are covered without rebinding:
 *   - global cursor position as --mx / --my (%) on <html> for the ambient glow
 *   - per-element --spot-x / --spot-y (px) for any `.spotlight` / `.spotlight-border`
 */
(function () {
    if (window.__mouseFxInit) {
        return;
    }
    window.__mouseFxInit = true;

    const root = document.documentElement;
    let queued = false;
    let lastEvent = null;

    function apply() {
        queued = false;
        const e = lastEvent;
        if (!e) {
            return;
        }

        root.style.setProperty('--mx', ((e.clientX / window.innerWidth) * 100).toFixed(2) + '%');
        root.style.setProperty('--my', ((e.clientY / window.innerHeight) * 100).toFixed(2) + '%');

        const target = e.target;
        const el = target && target.closest
            ? target.closest('.spotlight, .spotlight-border')
            : null;
        if (el) {
            const r = el.getBoundingClientRect();
            el.style.setProperty('--spot-x', (e.clientX - r.left).toFixed(1) + 'px');
            el.style.setProperty('--spot-y', (e.clientY - r.top).toFixed(1) + 'px');
        }
    }

    window.addEventListener('pointermove', function (e) {
        lastEvent = e;
        if (!queued) {
            queued = true;
            requestAnimationFrame(apply);
        }
    }, { passive: true });
})();
