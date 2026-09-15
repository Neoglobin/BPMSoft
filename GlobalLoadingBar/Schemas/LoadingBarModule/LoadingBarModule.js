  define("LoadingBarModule", ["css!LoadingBarModule"], function() {
	Ext.define("BPMSoft.configuration.LoadingBarModule", {
		alternateClassName: "BPMSoft.LoadingBarModule",
		singleton: true,

		bar: null,
		isRendered: false,
		isLoaded: false,
		loadingBarRenderDelayEdge: 0,
		loadingBarDestroyDelayEdge: 0,

		load: function (callback, scope) 
		{
			if (this.bar || this.isLoaded) {
				return callback.call(scope || this);
			}

			BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarRenderDelayEdge", (edgeValue) => 
			{
				this.loadingBarRenderDelayEdge = edgeValue ?? 0;

				BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarDestroyDelayEdge", (edgeValue) => 
				{
					this.loadingBarDestroyDelayEdge = edgeValue ?? 15000;

					this.bar = Ext.getBody().createChild({
						tag: "div",
						cls: "global-loading-bar"
					});
					
					this.isLoaded = true;
					callback.call(scope || this);
					
				}, this);
			}, this);

		},

		show: function () 
		{
			this.bar.show();
			this.bar.addCls("global-loading-bar--active");
			this.isRendered = true;
		},

		hide: function () 
		{
			if (!this.bar) {
				return;
			}

			this.bar.removeCls("global-loading-bar--active");

			setTimeout(() => {
				if (this.bar && !this.bar.hasCls("global-loading-bar--active")) {
					this.bar.hide();
					this.isRendered = false;
				}
			}, 200);
		}
	});

	return BPMSoft.LoadingBarModule;
});