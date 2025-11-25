$(function () {
    $('#drawer').dxDrawer({
        position: 'before',
        revealMode: 'expand',
        minSize: 0,
        shading: false,
        opened: true,
        closeOnOutsideClick: false,
        openedStateMode: 'shrink'
    });

    $('#menu-item').dxTreeView({
        selectionMode: 'single',
        focusStateEnabled: false,
        expandEvent: 'click',
        width: '100%',
        keyExpr: 'path',
        onItemClick: function (e) { }
    });

    $('#header-toolbox').dxToolbar({
        items: [
            {
                location: 'before',
                widget: 'dxButton',
                stylingMode: 'text',
                options: {
                    icon: 'menu',
                    onClick: function () {
                        var drawer = $('#drawer').dxDrawer('instance');
                        drawer.toggle();
                    }
                }
            },
            {
                location: 'center',
                text: 'WebMvc'
            }
        ]
    });

});