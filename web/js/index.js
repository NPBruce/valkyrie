
document.addEventListener('DOMContentLoaded', function () {
    "use strict";

    /* Tabs Logic */
    var tabLinks = document.querySelectorAll('.nav-tabs .nav-link');
    tabLinks.forEach(function (link) {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            // Remove active class from all links and panes
            var container = this.closest('.tabs'); // Assuming tabs are wrapped in .tabs or find closest parent
            if (!container) return;

            container.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
            container.querySelectorAll('.tab-pane').forEach(p => {
                p.classList.remove('show');
                p.classList.remove('active');
            });

            // Add active class to clicked link
            this.classList.add('active');

            // Show target pane
            var targetId = this.getAttribute('href').substring(1);
            var targetPane = document.getElementById(targetId);
            if (targetPane) {
                targetPane.classList.add('show');
                targetPane.classList.add('active');
            }
        });
    });

    /* GitHub Release Fetcher */
    var fallbackDownloadUrl = "https://github.com/NPBruce/valkyrie/releases/latest";
    var valkyrieAndroid = fallbackDownloadUrl;
    var valkyrieLinux = fallbackDownloadUrl;
    var valkyrieMacos = fallbackDownloadUrl;
    var valkyrieWindowsExe = fallbackDownloadUrl;
    var valkyrieWindows7z = fallbackDownloadUrl;
    var valkyrieWindowsZip = fallbackDownloadUrl;

    fetch('https://api.github.com/repos/NPBruce/valkyrie/releases/latest')
        .then(response => response.json())
        .then(data => {
            if (data.assets) {
                data.assets.forEach(function (v) {
                    var name = v.name.toLowerCase();
                    if (name.startsWith("valkyrie-android")) {
                        valkyrieAndroid = v.browser_download_url;
                    } else if (name.startsWith("valkyrie-linux")) {
                        valkyrieLinux = v.browser_download_url;
                    } else if (name.startsWith("valkyrie-macos")) {
                        valkyrieMacos = v.browser_download_url;
                    } else if (name.startsWith("valkyrie-windows") && name.endsWith(".exe")) {
                        valkyrieWindowsExe = v.browser_download_url;
                    } else if (name.startsWith("valkyrie-windows") && name.endsWith(".7z")) {
                        valkyrieWindows7z = v.browser_download_url;
                    } else if (name.startsWith("valkyrie-windows") && name.endsWith(".zip")) {
                        valkyrieWindowsZip = v.browser_download_url;
                    }
                });

                // Update Links
                var setHref = (id, url) => {
                    var el = document.getElementById(id);
                    if (el) el.setAttribute('href', url);
                };

                setHref('valkyrieWindowsZip', valkyrieWindowsZip);
                setHref('valkyrieAndroid2', valkyrieAndroid);
                setHref('valkyrieAndroid', valkyrieAndroid);
                setHref('valkyrieLinux', valkyrieLinux);
                setHref('valkyrieMacos', valkyrieMacos);
                setHref('valkyrieWindows7z', valkyrieWindows7z);
                setHref('valkyrieWindowsExe', valkyrieWindowsExe);
            }
        })
        .catch(err => {
            console.log("Could not load latest release data: " + err);
        });

});
