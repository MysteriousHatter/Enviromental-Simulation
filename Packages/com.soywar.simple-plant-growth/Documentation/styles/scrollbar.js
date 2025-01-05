var sidebar = document.querySelector('nav#sidebar');
var mainPanel = document.querySelector('main.main-panel');

const ClickScrollPlugin = {
    ['__osClickScrollPlugin']: {
        O: (
            moveHandleRelative,
            getHandleOffset,
            startOffset,
            handleLength,
            relativeTrackPointerOffset
        ) => {
            moveHandleRelative(startOffset);
        },
    },
};

OverlayScrollbarsGlobal.OverlayScrollbars.plugin([OverlayScrollbarsGlobal.SizeObserverPlugin, ClickScrollPlugin]);

OverlayScrollbarsGlobal.OverlayScrollbars(sidebar, {
    overflow: {
        x: 'none',
        y: 'auto'
    },
    scrollbars: {
        theme: 'os-scrollbar-theme',
        autoHide: 'never',
        visibility: 'visible',
        dragScroll: true,
        clickScroll: true
    }
});

OverlayScrollbarsGlobal.OverlayScrollbars(mainPanel, {
    overflow: {
        x: 'none',
        y: 'auto'
    },
    scrollbars: {
        theme: 'os-scrollbar-theme',
        autoHide: 'never',
        visibility: 'visible',
        dragScroll: true,
        clickScroll: true
    }
});