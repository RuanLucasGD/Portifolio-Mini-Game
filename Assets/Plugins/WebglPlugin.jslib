mergeInto(LibraryManager.library, {
    IsMobileDevice: function () {
        return isMobile();
    }
});