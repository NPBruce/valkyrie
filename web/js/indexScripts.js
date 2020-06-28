//scripts for index.html
$(document).ready(function () {
    fallbackDownloadUrl = "https://github.com/NPBruce/valkyrie/releases/latest";
    valkyrieAndroid = fallbackDownloadUrl;
    valkyrieLinux = fallbackDownloadUrl;
    valkyrieMacos = fallbackDownloadUrl;
    valkyrieWindowsExe = fallbackDownloadUrl;
    valkyrieWindows7z = fallbackDownloadUrl;
    valkyrieWindowsZip = fallbackDownloadUrl;

    try
    {
        $.get('https://api.github.com/repos/NPBruce/valkyrie/releases/latest', function (data) {
            $.each(data.assets, function(i, v) {
                if (v.name.toLowerCase().startsWith("valkyrie-android") == true){
                    valkyrieAndroid = v.browser_download_url;
                }
                else if (v.name.toLowerCase().startsWith("valkyrie-linux") == true){
                    valkyrieLinux = v.browser_download_url;
                }
                else if (v.name.toLowerCase().startsWith("valkyrie-macos") == true){
                    valkyrieMacos = v.browser_download_url;
                }
                else if (v.name.toLowerCase().startsWith("valkyrie-windows") == true && v.name.toLowerCase().endsWith(".exe") == true){
                    valkyrieWindowsExe = v.browser_download_url;
                }
                else if (v.name.toLowerCase().startsWith("valkyrie-windows") == true && v.name.toLowerCase().endsWith(".7z") == true){
                    valkyrieWindows7z = v.browser_download_url;
                }
                else if (v.name.toLowerCase().startsWith("valkyrie-windows") == true && v.name.toLowerCase().endsWith(".zip") == true){
                    valkyrieWindowsZip = v.browser_download_url;
                }
            });

            $('#valkyrieWindowsZip').prop('href', valkyrieWindowsZip);
            $('#valkyrieAndroid2').prop('href', valkyrieAndroid);
            $('#valkyrieAndroid').prop('href', valkyrieAndroid);
            $('#valkyrieLinux').prop('href', valkyrieLinux);
            $('#valkyrieMacos').prop('href', valkyrieMacos);
            $('#valkyrieWindows7z').prop('href', valkyrieWindows7z);
            $('#valkyrieWindowsExe').prop('href', valkyrieWindowsExe);
        });
    }
    catch(err)
    {
        if (window.console)
        {
            console.log("Could not load latest release data for download links" + err);
        }
    }
});