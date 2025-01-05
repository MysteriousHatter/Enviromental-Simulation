anchors.options = {
    placement: 'right',
    visible: 'hover'
};

anchors.removeAll();
anchors.add('article h2:not(.no-anchor), article h3:not(.no-anchor), article h4:not(.no-anchor)');