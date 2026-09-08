define("BaseSchemaModuleOverride", ["LoadingBarModule"], function() {
	Ext.define("BPMSoft.configuration.BaseSchemaModuleOverride", {
		override: "BPMSoft.configuration.BaseSchemaModule",
		alternateClassName: "BPMSoft.BaseSchemaModuleOverride",

		isLoadingBarActive: false,
		isLoadingBarRendered: false,
		isModuleInitialized: false,
		intervalId: null,
		delay: 0,
		delayEdge: 0,
		

		init: function(callback, scope) 
		{
			BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarActive", (isActive) => {
				if (isActive) 
				{
					this.isLoadingBarActive = true;
					BPMSoft.LoadingBarModule.init();
				}
			}, this);

			this.callParent([function() {
				this.isModuleInitialized = true;
				callback.call(scope || this);
			}, this]);

			if (!this.intervalId && !this.isModuleInitialized) 
			{
				BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarRenderDelayEdge", (edgeValue) => {
					if (edgeValue) 
					{
						this.delayEdge = edgeValue;
					}
				}, this);

				this.intervalId = setInterval(() => {

					if (this.isModuleInitialized) 
					{
						clearInterval(this.intervalId);
						return;
					}
	
					if (!this.isLoadingBarRendered && this.delay >= this.delayEdge) 
					{
						BPMSoft.LoadingBarModule.show();
						this.isLoadingBarRendered = true;
					}
					
					this.delay += 100; 
				}, 100);
			}
		},

		render: function() 
		{			
			this.callParent(arguments);

			if (this.isLoadingBarRendered) 
			{
				BPMSoft.LoadingBarModule.hide();
				this.isLoadingBarRendered = false;
			}
		}
	});
	return BPMSoft.BaseSchemaModuleOverride;
});
