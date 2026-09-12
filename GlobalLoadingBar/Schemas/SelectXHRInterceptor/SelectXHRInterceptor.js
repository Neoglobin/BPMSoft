 define("SelectXHRInterceptor", [], function() {
	Ext.define("BPMSoft.configuration.SelectXHRInterceptor", {
		alternateClassName: "BPMSoft.SelectXHRInterceptor",
		singleton: true,

		isLoaded: false,

		init: function() 
		{
			this.isLoaded = true;
		}
	});
	return BPMSoft.SelectXHRInterceptor;
 })