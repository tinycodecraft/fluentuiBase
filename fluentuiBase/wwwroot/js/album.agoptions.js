// This file is to show how a library package may provide JavaScript interop features
// wrapped in a .NET API

window.fl_album_config = function (gridOptions, autoCompletefn,keyDispatch) {

    var coldefs = gridOptions.columnDefs;
    gridOptions.getRowNodeId = function (node) {
        var id = node.id;

        return "ID#" + id;
    };
    gridOptions.getRowStyle = function (params) {
        //params.data
        //data specific stuff
        return {};
    };
    gridOptions.onGridReady = function (params) {
        var autolist = [];
        var targetselector = 'autocp-';
        var sourceattr = 'urlis';
        var propattr = 'propname';
        var autoselector = '.ag-header-row div[class*="' + targetselector + '"]';
        var listofautourl = '.ag-header-row div[ref="eFloatingFilterInput"]';
        var filterbodyselector = 'div[ref="eFloatingFilterBody"]';
        var clearorginselector = ".clearcontainer.origin";
        var autocpSelector = "." + targetselector.slice(0, -1);
        $(autoselector).each(function (i, el) {
            autolist.push(el);
            var $nextrow = $(el).parent().next();
            var $filterbody = $nextrow.find(filterbodyselector);
            var $input = $($filterbody[i]).find("input");
            //this is used to capture id prefix inside autoclass
            var autoclass = "";
            if ($input && $input.length) {
                el.classList.forEach(function (e, i) {
                    if (e.startsWith(targetselector)) {
                        autoclass = e.replace(targetselector, "");
                        return;
                    }
                });

                
                const colDef = coldefs.find(function (c) { return c.field == autoclass });
                if (colDef && colDef.wanted && colDef.urlis) {
                    $input.attr(sourceattr, colDef.urlis);
                    $input.attr(propattr, colDef.wanted);
                    $input.addClass(targetselector.slice(0, -1));

                }

            }

        });

        autoCompletefn(
            autocpSelector,
            sourceattr,
            null,
            function (el, val) {
                //select handle
                //the below is only helping to understand the filter inside the api
                //var thisapi = params.api;
                //var thisfilter = thisapi.getFilterInstance(el.attr('propname'));
                //thisfilter.applyModel();
                //thisfilter.eventService.dispatchEvent(new Event('change',{bubbles: true}));
                keyDispatch(el.parent().parent(), 100);
            },
            function (vl, isblur) {
                //blur handle
                //try to clone clear button piece from layout
                //class origin just to avoid misleading for clone version
                //because the clear button piece will attach to the floating container by grid
                var clearClselector = clearorginselector.slice(0, clearorginselector.lastIndexOf("."));
                var originClass = clearorginselector.slice(clearClselector.length + 1);
                if (!isblur) {
                    var $clearUI = $(clearClselector, vl.parent());
                    if (!$clearUI || $clearUI.length == 0) {
                        vl.parent().append($("body " + clearorginselector).clone());
                        $clearUI = $(clearClselector, vl.parent());
                        $clearUI.removeClass(originClass);
                        $clearUI.attr("tabindex", -1);
                    }
                    $clearUI.removeClass("hidden");
                    $clearUI.off("click");
                    $clearUI.on("click", function () {
                        vl.val("");
                        $clearUI.addClass("hidden");
                        keyDispatch(vl.parent().parent(), 100);
                    });
                } else {
                    var $clearUI = $(clearClselector, vl.parent());
                    if (!vl.val()) {
                        $clearUI.addClass("hidden");
                    }
                }
            }
        );

    };
};
