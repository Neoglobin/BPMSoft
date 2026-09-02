(function() {
    require.config({
        paths: {
            "DuplicatesWidgetComponent": BPMSoft.getFileContentUrl("Deduplication", "src/js/duplicates-widget.js"),
           
        },
        shim: {
            "DuplicatesWidgetComponent": {
                deps: ["ng-core", "chartjs"]
            }
        }
    });
})();
