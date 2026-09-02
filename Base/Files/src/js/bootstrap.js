(function() {
	require.config({
		paths: {
			"DriverJs": BPMSoft.getFileContentUrl("Base", "src/js/driver.min.js"),
			"DriverCSS": BPMSoft.getFileContentUrl("Base", "src/css/driver.min.css"),
		}
	});
})();
