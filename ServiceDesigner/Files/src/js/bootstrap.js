(function() {
	require.config({
		paths: {
			"web-service-proxy-component": BPMSoft.getFileContentUrl("ServiceDesigner", "src/js/web-service-proxy-component.js")
		},
		shim: {
			"web-service-proxy-component": {
				deps: ["ng-core"]
			}
		}
	});
}());
