(function() {
	require.config({
		paths: {
			"OmnichannelMessagingComponent": BPMSoft.getFileContentUrl("OmnichannelMessaging", "src/js/omnichannel-messaging-component.js"),
		},
		shim: {
			"OmnichannelMessagingComponent": {
				deps: ["ng-core"]
			}
		}
	});
})();