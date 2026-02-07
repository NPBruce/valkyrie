
document.addEventListener('DOMContentLoaded', function () {
    "use strict";

    /* Preloader */
    window.addEventListener('load', function () {
        var preloader = document.querySelector('.spinner-wrapper');
        if (preloader) {
            setTimeout(function () {
                preloader.style.opacity = '0';
                setTimeout(function () {
                    preloader.style.display = 'none';
                }, 500);
            }, 500);
        }
    });

    /* Navbar Scripts */
    // Collapse navbar on scroll
    var navbar = document.querySelector(".navbar");
    window.addEventListener('scroll', function () {
        if (window.scrollY > 20) {
            navbar.classList.add("top-nav-collapse");
        } else {
            navbar.classList.remove("top-nav-collapse");
        }
    });

    // Mobile Menu Toggle
    var navbarToggler = document.querySelector('.navbar-toggler');
    var navbarCollapse = document.querySelector('.navbar-collapse');

    if (navbarToggler && navbarCollapse) {
        navbarToggler.addEventListener('click', function () {
            navbarCollapse.classList.toggle('show');
        });
    }

    // Close mobile menu on item click
    var navbarLinks = document.querySelectorAll(".navbar-nav li a");
    navbarLinks.forEach(function (link) {
        link.addEventListener('click', function () {
            if (!this.parentElement.classList.contains('dropdown')) {
                navbarCollapse.classList.remove('show');
            }
        });
    });

    // Dropdown Toggle (for mobile/touch)
    var dropdownToggles = document.querySelectorAll('.dropdown-toggle');
    dropdownToggles.forEach(function (toggle) {
        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            var dropdownMenu = this.nextElementSibling;
            if (dropdownMenu) {
                dropdownMenu.classList.toggle('show');
            }
        });
    });

    // Close dropdowns when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.dropdown')) {
            document.querySelectorAll('.dropdown-menu.show').forEach(function (menu) {
                menu.classList.remove('show');
            });
        }
    });
});
