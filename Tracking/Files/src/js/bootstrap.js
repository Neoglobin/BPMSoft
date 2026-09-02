(function() {
    require.config({
        paths: {
            "TrackingEventsFeedElement": BPMSoft.getFileContentUrl("Tracking", "src/js/tracking-ng-elements.js")
		},
        shim: {
            "TrackingEventsFeedElement": {
                deps: ["ng-core"]
            }
        }
    });
})();