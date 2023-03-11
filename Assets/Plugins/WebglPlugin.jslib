mergeInto(LibraryManager.library, {

    IsMobileDevice: function () {
        return isMobile();
    },
    OpenUrl: function (url) {

        if (window.navigator) {
            if (document.fullscreenElement !== null) {
                window.document.exitFullscreen();
            }
        }

        url = UTF8ToString(url);
        window.open(url, '_blank');
    },
    OpenEmail: function () {

        if (window.navigator) {
            if (document.fullscreenElement !== null) {
                window.document.exitFullscreen();
            }
        }

        window.location.href = "mailto:ruanlucaspgbr@outlook.com"
    }
});
