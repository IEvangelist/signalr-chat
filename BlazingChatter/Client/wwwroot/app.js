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
