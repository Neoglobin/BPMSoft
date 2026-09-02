(function() {
    require.config({
        paths: {
            "VanillaboxMin" :BPMSoft.getFileContentUrl("UIv2", "src/js/jquery.vanillabox-0.1.7.min.js"),
            "VanillaboxCSS" :BPMSoft.getFileContentUrl("UIv2", "src/css/vanillabox.css"),
            "OrgChart" :BPMSoft.getFileContentUrl("UIv2", "src/js/jquery.orgchart.min.js"),
            "OrgChartCSS" :BPMSoft.getFileContentUrl("UIv2", "src/css/jquery.orgchart.min.css"),
            "FileViewerComponent": BPMSoft.getFileContentUrl("UIv2", "src/js/file-viewer/script.js"),
        },
        shim: {
            "VanillaboxMin": {
                deps: ["jQuery"]
            },
            "OrgChart": {
                deps: ["jQuery"]
            }
        }
    });
})();