window.blazor_ag_grid = {  

    htmlEncode: function (value) {
        //create a in-memory div, set it's inner text(which jQuery automatically encodes)
        //then grab the encoded contents back out. The div never exists on the page.
        return $("<div/>").text(value).html();
    },

    htmlDecode: function (value) {
        return $("<div/>").html(value).text();
    },
    spaceKeyDispatch: function (el, timecnt) {
        if (el && el.length) {
            setTimeout(() => {
                el[0].dispatchEvent(
                    new KeyboardEvent("keydown", {
                        key: " ",
                        code: "Space",
                        keyCode: 32,
                        which: 32,
                    })
                );
            }, timecnt);
        }
    },
    setupAutoComplete: function (selector, urltag, withintag, selecthandler, clearhandler, toupper) {
        var withinHtml = $(document);
        if (withintag) {
            withinHtml = $(withintag);
        }

        $(selector, withinHtml).each(function (i, e) {
            var apiGet = $(e).attr(urltag);
            var $el = $(e);
            var detecthandler = selecthandler;
            var canwrap = !!clearhandler;

            $(e)
                .autoComplete({
                    minChars: 0,
                    cache: false,
                    upper: !!toupper,
                    source: function (searchValue, response) {
                        searchValue = $.trim(searchValue);
                        var params = { search: searchValue };
                        var prop = $el.attr("propname");
                        var exclude = $el.attr("exclude");
                        if (prop) {
                            params["wanted"] = prop;
                            if (exclude) {
                                params["exclude"] = exclude;
                            }
                        }

                        $.ajax({
                            type: "GET",
                            url: apiGet,
                            data: params,
                            success: function (data) {
                                response(data);
                            }
                        });

                    },
                    onSelect: function (el, search, item) {
                        if (item) {
                            $el.val(item.data("lang"));
                            if (detecthandler) {
                                if (item.data("key")) {
                                    detecthandler($el, item.data("lang"), item.data("key"));
                                } else {
                                    detecthandler($el, item.data("lang"));
                                }
                            }
                        }
                    },
                    renderItem: function (item, search) {
                        var rg = /[-/\\^$*+?.()|[\]{}]/g;
                        var fkrg = new RegExp("{", "gi");
                        var bkrg = new RegExp("}", "gi");
                        search = search.replace(rg, "\\$&");

                        if (!item || !apiGet) {
                            return $("<div></div>").html();
                        }
                        if (!item.value) {
                            item.value = '';

                        }
                        if (apiGet) {
                            var re = new RegExp("(" + search.split(" ").join("|") + ")", "gi");
                            var canwrapclass = !!canwrap ? " wrapitem " : "";
                            var encodedvalue = blazor_ag_grid.htmlEncode(item.value.replace(re, "{$1}").replace(/\{\}/g, ""));
                            var $renditem = $(
                                '<div class="autocomplete-suggestion ' +
                                canwrapclass +
                                '" data-lang="' +
                                item.value +
                                '" data-val="' +
                                search +
                                '" data-key="' +
                                item.key +
                                '"> ' +
                                encodedvalue.replace(fkrg, "<b>").replace(bkrg, "</b>") +
                                "</div>"
                            );
                            var fragment = $renditem && $renditem.length && $renditem[0].outerHTML;
                            
                            console.log(fragment)
                            if (!fragment) {
                                fragment = $("<div></div>")[0].outerHTML;
                            }
                            return fragment;
                        }
                    },
                })
                .bind("click", function () {
                    $(this).trigger("focus");
                })
                .bind("focus", function () {
                    if (canwrap) {
                        clearhandler($(this));
                    }

                    if (!$(this).val()) {
                        $(this).keydown();
                    }
                })
                .bind("blur", function () {
                    if (canwrap) {
                        clearhandler($(this), true);
                    }
                });
        });
    },
    callbackMap: {}
    , renderCount: 0
    , createGrid: function (gridDiv, interopOptions, configScript) {
        //console.log("GOT GridOptions: " + blazor_ag_grid.util_stringify(interopOptions));
        var id = interopOptions.CallbackId;
        var op = interopOptions.Options;
        var cb = interopOptions.Callbacks;
        var ev = interopOptions.Events;
        var ds = op.datasource;
        //console.log("JS-creating grid for [" + id + "]...");

        // Remember for subsequent API calls
        blazor_ag_grid.callbackMap[id] = interopOptions;

        if (cb) {
            blazor_ag_grid.createGrid_wrapCallbacks(op, cb);
        }

        if (ev) {
            blazor_ag_grid.createGrid_wrapEvents(op, ev);
        }

        if (ds) {
            console.log("DS Ref: " + blazor_ag_grid.util_stringify(ds));
            blazor_ag_grid.createGrid_wrapDatasource(op, ds);
        }
        console.log("Got columnDefs: " + blazor_ag_grid.util_stringify(op.columnDefs));
        if (op.columnDefs && op.columnDefs.length > 0) {
            op.columnDefs.forEach(function (colDef, i) {
                if (colDef.floatingFilter) {
                    
                    colDef["suppressMenu"] = true;
                    colDef["floatingFilterComponentParams"] = { suppressFilterButton: true };
                    colDef["filterParams"] = {
                        filterOptions: ["contains"].concat(colDef.choices),
                        suppressAndOrCondition: true,

                        textMatcher: function (textparams) {
                            console.log(textparams);

                            if (textparams.filter !== "contains") {
                                return value == textparams.filter;
                            }

                            let rg = new RegExp(textparams.filterText, "gi");
                            if (rg.test(value)) return true;

                            return false;
                        },
                    };
                }

            });

        }

        if (configScript) {
            if (window[configScript]) {
                window[configScript].call(null, op,blazor_ag_grid.setupAutoComplete,blazor_ag_grid.spaceKeyDispatch);
            }
            else {
                console.error("gridOptions local configScript was specified but could not be resolved; ABORTING");
                return;
            }
        }

        // create the grid passing in the div to use together with the columns & data we want to use
        //console.log("have options(BEF): " + blazor_ag_grid.util_stringify(op));
        new agGrid.Grid(gridDiv, op);
        //console.log("have options(AFT): " + blazor_ag_grid.util_stringify(op));
    }
    , destroyGrid: function (gridDiv, id) {
        console.log("JS-destroying grid [" + id + "]...");

        // TODO: What else should we do to properly clean up resources???

        delete blazor_ag_grid.callbackMap[id];
    }
    , createGrid_wrapDatasource: function (op, ds) {
        var nativeDS = blazor_ag_grid.util_wrapDatasource(ds);
        op.datasource = nativeDS;
    }
    , createGrid_wrapCallbacks: function (gridOptions, gridCallbacks) {
        console.log("Got GridCallbacks: " + blazor_ag_grid.util_stringify(gridCallbacks));
        if (gridCallbacks.handlers.GetRowNodeId) {
            //console.log("Wrapping GetRowNodeId handler");
            gridOptions.getRowNodeId = function (data) {
                //console.log("gridOptions.getRowNodeId <<< " + JSON.stringify(data));
                var id = gridCallbacks.handlers.GetRowNodeId.jsRef.invokeMethod("Invoke", data);
                //console.log("gridOptions.getRowNodeId >>> [" + id + "]");
                return id;
            }
        }
        if (gridCallbacks.handlers.GetDataPath) {
            //console.log("Wrapping GetDataPath handler");
            gridOptions.getDataPath = function (data) {
                //console.log("gridOptions.getDataPath <<< " + JSON.stringify(data));
                var path = gridCallbacks.handlers.GetDataPath.jsRef.invokeMethod("Invoke", data);
                //console.log("gridOptions.getDataPath >>> [" + path + "]");
                return path;
            }
        }
    }
    , createGrid_wrapEvents: function (gridOptions, gridEvents) {
        console.log("Got GridEvents: " + blazor_ag_grid.util_stringify(gridEvents));
        if (gridEvents.handlers.SelectionChanged) {
            console.log("Wrapping SelectionChanged handler");
            gridOptions.onSelectionChanged = function () {
                blazor_ag_grid.gridOptions_onSelectionChanged(gridOptions, gridEvents);
            }
        }
        if (gridEvents.handlers.CellValueChanged) {
            console.log("Wrapping CellValueChanged handler");
            gridOptions.onCellValueChanged = function (data) {
                var ev = {
                    rowNodeId: data.node.id,
                    field: data.colDef.field,
                    columnId: data.column.colId,
                    rowIndex: data.rowIndex,
                    oldValue: data.oldValue,
                    newValue: data.value,
                };
                var id = gridEvents.handlers.CellValueChanged.jsRef.invokeMethodAsync("Invoke", ev);
            }
        }
    }
    , gridOptions_callGridApi: function (callbackId, name, args) {
        //console.log("getting gridOptions for [" + callbackId + "]");
        var gridOptions = blazor_ag_grid.callbackMap[callbackId];
        //console.log("got gridOptions: " + gridOptions);
        var op = gridOptions.Options;
        var api = op.api;
        var fn = api[name]
        //console.log("has Grid API [" + name + "]: " + fn);
        fn.apply(api, args || []);
    }
    , gridOptions_callColumnApi: function (callbackId, name, args) {
        //console.log("getting gridOptions for [" + callbackId + "]");
        var gridOptions = blazor_ag_grid.callbackMap[callbackId];
        //console.log("got gridOptions: " + gridOptions);
        var op = gridOptions.Options;
        var api = op.columnApi
        var fn = api[name];
        //console.log("has Column API [" + name + "]: " + fn);
        fn.apply(api, args || []);
    }
    , gridOptions_setDatasource: function (callbackId, ds) {
        //console.log("getting gridOptions for [" + callbackId + "]");
        var gridOptions = blazor_ag_grid.callbackMap[callbackId];
        //console.log("got gridOptions: " + gridOptions);
        var op = gridOptions.Options;
        var api = op.api;

        if (!ds) {
            // Simply reset with existing DS
            console.log("Resetting DS with existing DS");
            api.setDatasource(op.datasource);
        }
        else {
            console.log("Setting DS with NEW DS");
            var nativeDS = blazor_ag_grid.util_wrapDatasource(ds);
            api.setDatasource(nativeDS);
        }
    }
    , gridOptions_onSelectionChanged: function (gridOptions, gridEvents) {
        console.log("js-onSelectionChanged");
        var selectedNodes = gridOptions.api.getSelectedNodes();
        var json = blazor_ag_grid.util_stringify(selectedNodes);
        var mapped = selectedNodes.map(this.util_mapRowNode);
        console.log("js-selectedNodes: " + blazor_ag_grid.util_stringify(mapped));
        gridEvents.handlers.SelectionChanged.jsRef.invokeMethodAsync('Invoke', mapped);
    }
    , datasource_successCallback: function (callbackId, rowsThisBlock, lastRow) {
        var getRowsParams = blazor_ag_grid.callbackMap[callbackId];
        console.log("datasource_successCallback: " + callbackId);
        getRowsParams.successCallback(rowsThisBlock, lastRow);
        console.log("unmapping callback: " + callbackId);
        delete blazor_ag_grid.callbackMap[callbackId];
    }
    , datasource_failCallback: function (callbackId, rowsThisBlock, lastRow) {
        var getRowsParams = blazor_ag_grid.callbackMap[callbackId];
        console.log("datasource_failCallback: " + callbackId);
        getRowsParams.failCallback();
        console.log("unmapping callback: " + callbackId);
        delete blazor_ag_grid.callbackMap[callbackId];
    }
    , util_wrapDatasource: function (ds) {
        // Need to "wrap" the data source
        console.log("Wrapping datasource");
        var nativeDS = {
            getRows: function (getRowsParams) {
                //console.log("getting rows for: " + JSON.stringify(getRowsParams));
                var callbackId = blazor_ag_grid.util_genId();
                blazor_ag_grid.callbackMap[callbackId] = getRowsParams;
                getRowsParams.callbackId = callbackId;
                //console.log("mapped callback ID for ds: " + callbackId + "; " + JSON.stringify(dsRef));
                ds.invokeMethodAsync('GetRows', getRowsParams);
            },
            destroy: function () {
                //console.log("destroying  datasource...");
                ds.invokeMethodAsync('Destroy');
            }
        };
        return nativeDS;
    }
    // Cycle-safe version of JSON.stringify, useful for debugging
    , util_stringify: function (obj) {
        // Note: cache should not be re-used by repeated calls to JSON.stringify.
        var cache = [];
        var json = JSON.stringify(obj, function (key, value) {
            if (typeof value === 'object' && value !== null) {
                if (cache.indexOf(value) !== -1) {
                    // Duplicate reference found, discard key
                    return;
                }
                // Store value in our collection
                cache.push(value);
            }
            return value;
        });
        cache = null; // Enable garbage collection
        return json;
    }
    // Maps raw Row Node objects to something safer for passing back to .NET
    , util_mapRowNode: function (n) {
        let newN = {};

        // Standard properties as defined here:
        //    https://www.ag-grid.com/javascript-grid-row-node/#row-object-aka-row-node
        newN["id"] = n.id;
        newN["data"] = n.data;
        if (n.parent) {
            newN["parent"] = mapNode(n.parent);
        }
        newN["level"] = n.level;
        newN["uiLevel"] = n.uiLevel;
        newN["group"] = n.group;
        newN["rowPinned"] = n.rowPinned;
        newN["canFlower"] = n.canFlower;
        newN["childFlower"] = n.childFlower;
        newN["childIndex"] = n.childIndex;
        newN["firstChild"] = n.firstChild;
        newN["lastChild"] = n.lastChild;
        newN["stub"] = n.stub;
        newN["rowHeight"] = n.rowHeight;
        newN["rowTop"] = n.rowTop;
        newN["quickFilterAggregateText"] = n.quickFilterAggregateText;

        // Additional properties found through observation
        newN["selectable"] = n.selectable;
        newN["alreadyRendered"] = n.alreadyRendered;
        newN["selected"] = n.selected;
        newN["master"] = n.master;
        newN["expanded"] = n.expanded;
        newN["allChildrenCount"] = n.allChildrenCount;
        newN["rowHeightEstimated"] = n.rowHeightEstimated;
        newN["rowIndex"] = n.rowIndex;

        return newN;
    }
    , util_genId: function () {
        return Math.random().toString(36).substr(2);
    }
};
