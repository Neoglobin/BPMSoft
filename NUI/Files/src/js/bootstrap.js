(function() {
	require.config({
		paths: {
			"StructureExplorerComponent": BPMSoft.getFileContentUrl("NUI", "src/js/structure-explorer-component.js"),
			"ErrorListDialogComponent": BPMSoft.getFileContentUrl("NUI", "src/js/error-list-dialog-component.js"),
			"SchemaViewComponent": BPMSoft.getFileContentUrl("NUI", "src/js/schema-view-component/main.js"),
			"SchemaViewComponentStyles": BPMSoft.getFileContentUrl("NUI", "src/js/schema-view-component/styles.js"),
			"NavMenuComponent": BPMSoft.getFileContentUrl("NUI", "src/js/nav-menu/script.js"),
		},
		shim: {
			"StructureExplorerComponent": {
				deps: ["ng-core"]
			},
			"ErrorListDialogComponent": {
				deps: ["ng-core"]
			},
			"SchemaViewComponent": {
				deps: ["ng-core", "chartjs", "SchemaViewComponentStyles"]
			}
		}
	});
})();
