(function() {
    require.config({
        paths: {
            "AnalyticsDashboard": BPMSoft.getFileContentUrl("AnalyticsDashboard", "src/js/analytics-dashboard.js"),
           
        },
        shim: {
            "AnalyticsDashboard": {
                deps: ["ng-core"]
            }
        }
    });
})();
