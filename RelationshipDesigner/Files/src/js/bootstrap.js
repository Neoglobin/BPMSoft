(function () {
	require.config({
		map: {
			"*": {
			"RelationshipDiagramComponent": BPMSoft.getFileContentUrl("RelationshipDesigner", "src/js/relationship-diagram-component/relationship-diagram-component.js"),
			"GenericRelationshipDiagramComponent": BPMSoft.getFileContentUrl("RelationshipDesigner", "src/js/relationship-diagram-component/relationship-diagram-component.js")
			},
		},
		shim: {
			"RelationshipDiagramComponent": {
				deps: ["ng-core"]
			},
			"GenericRelationshipDiagramComponent": {
				deps: ["ng-core"]
			},
		}
	});
}());