document.addEventListener('DOMContentLoaded', function () {
    const navbarCollapse = document.getElementById('navbarContent');

    if (navbarCollapse && window.bootstrap && window.innerWidth < 992) {
        const collapse = bootstrap.Collapse.getOrCreateInstance(navbarCollapse, { toggle: false });
        navbarCollapse.querySelectorAll('.nav-link, .dropdown-item').forEach(function (link) {
            link.addEventListener('click', function () {
                if (navbarCollapse.classList.contains('show')) {
                    collapse.hide();
                }
            });
        });
    }

    document.querySelectorAll('.member-option').forEach(function (option) {
        const checkbox = option.querySelector('input[type="checkbox"]');
        if (!checkbox) {
            return;
        }

        const syncState = function () {
            option.classList.toggle('is-selected', checkbox.checked);
        };

        checkbox.addEventListener('change', syncState);
        syncState();
    });
});
