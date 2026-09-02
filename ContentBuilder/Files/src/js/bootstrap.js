(function() {
    require.config({
        paths: {
            "MjmlCore": BPMSoft.getFileContentUrl("ContentBuilder", "src/js/mjml.min.js"),
            "CSSLint": BPMSoft.getFileContentUrl("ContentBuilder", "src/js/csslint.min.js"),
            "HTMLHint": BPMSoft.getFileContentUrl("ContentBuilder", "src/js/htmlhint.min.js")
        },
        shim: {
            "MjmlCore": {
                deps: [""]
            },
            "CSSLint": {
                deps: [""]
            },
            "HTMLHint": {
                deps: ["CSSLint"]
            }
        }
    });
})();