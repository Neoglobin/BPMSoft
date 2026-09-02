(function() {
    require.config({
        paths: {
            "PivotTableComponent": BPMSoft.getFileContentUrl("PivotTable", "src/js/pivot-table-component.js"),
           
        },
        shim: {
            "PivotTableComponent": {
                deps: ["ng-core"]
            }
        }
    });
})();
