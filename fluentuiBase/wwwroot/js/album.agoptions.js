// This file is to show how a library package may provide JavaScript interop features
// wrapped in a .NET API

window.fl_album_config = function (gridOptions) {
    gridOptions.getRowNodeId = function (node) {
        var id = node.id;

        return "ID#" + id;
    };
    gridOptions.getRowStyle = function (params) {
        //params.data
        //data specific stuff
        return {};
    };
};
