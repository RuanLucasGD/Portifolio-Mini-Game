mergeInto(LibraryManager.library, {
    IsMobileDevice: function () {
        return isMobile();
    },
    OpenUrl: function (url) {
        window.document.exitFullscreen();
        url = UTF8ToString(url);
        window.open(url, '_blank');
    }
});