(function() {
	require.config({
		map: {
			"DashboardsModuleMobile": {
				"DashboardsModule": "DashboardsModule"
			},
			"*": {
				"DashboardsModule": "DashboardsModuleMobile"
			}
		}
	});
})();
