(function() {
	require.config({
		paths: {
			"TermCalculationComponent": BPMSoft.getFileContentUrl("CaseService", "src/js/term-calculation-component.js"),
		},
		shim: {
			"TermCalculationComponent": {
				deps: ["ng-core"]
			}
		}
	});
})();