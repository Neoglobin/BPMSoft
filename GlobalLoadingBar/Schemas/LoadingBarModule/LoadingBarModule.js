  define("LoadingBarModule", ["css!LoadingBarModule"], function() {
	Ext.define("BPMSoft.configuration.LoadingBarModule", {
		alternateClassName: "BPMSoft.LoadingBarModule",
		singleton: true,

		bar: null,
		isRendered: false,
		isLoaded: false,

		init: function () {
			if (this.bar) {
				return;
			}

			this.bar = Ext.getBody().createChild({
				tag: "div",
				cls: "global-loading-bar"
			});
			this.isLoaded = true;
		},

		show: function () {
			this.init();

			this.bar.show();
			this.bar.addCls("global-loading-bar--active");
			this.isRendered = true;
		},

		hide: function () {
			if (!this.bar) {
				return;
			}

			this.bar.removeCls("global-loading-bar--active");

			// Небольшая задержка нужна, чтобы закончилась CSS-анимация.
			Ext.defer(function () {
				if (this.bar && !this.bar.hasCls("global-loading-bar--active")) {
					this.bar.hide();
					this.isRendered = false;
				}
			}, 200, this);
		}
	});

	return BPMSoft.LoadingBarModule;
});