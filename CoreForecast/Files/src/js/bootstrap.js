(function() {
    require.config({
        paths: {
            "ForecastComponent": BPMSoft.getFileContentUrl("CoreForecast", "src/js/forecast-component.js"),
           
        },
        shim: {
            "ForecastComponent": {
                deps: ["ng-core"]
            }
        }
    });
})();
