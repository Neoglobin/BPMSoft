(function() {
	require.config({
		paths: {
			"ServiceModelNetworkComponent": BPMSoft.getFileContentUrl("ServiceModel", "src/js/service-model-network-component.js"),
		},
		shim: {
			"ServiceModelNetworkComponent": {
				deps: ["ng-core"]
			}
		}
	});
})();