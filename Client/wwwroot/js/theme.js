// Light/dark theme. Runs synchronously in <head> so the stored theme is applied before first paint.
// Bootstrap 5.3 reads data-bs-theme, Syncfusion bootstrap5.3 theme reads the e-dark-mode class.
// Without a stored choice the theme follows the OS setting (prefers-color-scheme).
(function () {
    var storageKey = 'theme';
    var media = window.matchMedia('(prefers-color-scheme: dark)');

    function stored() {
        try {
            var t = localStorage.getItem(storageKey);
            return t === 'light' || t === 'dark' ? t : null;
        } catch (e) {
            return null;
        }
    }

    function current() {
        return stored() || (media.matches ? 'dark' : 'light');
    }

    function apply(theme) {
        var root = document.documentElement;
        root.setAttribute('data-bs-theme', theme);
        root.classList.toggle('e-dark-mode', theme === 'dark');
    }

    apply(current());
    media.addEventListener('change', function () {
        if (!stored()) apply(current());
    });

    window.appTheme = {
        get: current,
        set: function (theme) {
            try { localStorage.setItem(storageKey, theme); } catch (e) { }
            apply(theme);
        }
    };
})();
