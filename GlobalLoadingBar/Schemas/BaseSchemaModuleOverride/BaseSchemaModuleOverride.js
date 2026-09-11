define("BaseSchemaModuleOverride", ["LoadingBarModule"], function() {
	Ext.define("BPMSoft.configuration.BaseSchemaModuleOverride", {
		override: "BPMSoft.configuration.BaseSchemaModule",
		alternateClassName: "BPMSoft.BaseSchemaModuleOverride",

		isLoadingBarActive: false,
		isLoadingBarRendered: false,
		isModuleInitialized: false,
		intervalId: null,
		delay: 0,
		loadingBarRenderDelayEdge: 0,
		loadingBarDestroyDelayEdge: 0,
		

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

			if (this.isLoadingBarActive) 
			{
				if (!this.intervalId && !this.isModuleInitialized) 
				{
					BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarRenderDelayEdge", (edgeValue) => {
						if (edgeValue) 
						{
							this.loadingBarRenderDelayEdge = edgeValue;
						}
					}, this);
					BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarDestroyDelayEdge", (edgeValue) => {
						if (edgeValue) 
						{
							this.loadingBarDestroyDelayEdge = edgeValue;
						}
					}, this);
	
					this.intervalId = setInterval(() => {
	
						if (this.isModuleInitialized || this.delay >= this.loadingBarDestroyDelayEdge) 
						{
							clearInterval(this.intervalId);

							if (this.delay >= this.loadingBarDestroyDelayEdge) 
							{
								BPMSoft.LoadingBarModule.hide();
								this.isLoadingBarRendered = false;
							}

							return;
						}
		
						if (!this.isLoadingBarRendered && this.delay >= this.loadingBarRenderDelayEdge) 
						{
							BPMSoft.LoadingBarModule.show();
							this.isLoadingBarRendered = true;
						}
						
						this.delay += 100; 
					}, 100);
				}
			}
		},

		render: function() 
		{			
			this.callParent(arguments);

			if (this.isLoadingBarActive) 
			{
				if (this.isLoadingBarRendered) 
				{
					BPMSoft.LoadingBarModule.hide();
					this.isLoadingBarRendered = false;
				}
			}
		},

		destroy: function() 
		{
			this.isLoadingBarActive = false;
			this.isLoadingBarRendered = false;
			this.isModuleInitialized = false;
			this.intervalId = null;
			this.delay = 0;
			this.loadingBarRenderDelayEdge = 0;
			this.loadingBarDestroyDelayEdge = 0;

			this.callParent(arguments);
		}
	});
	return BPMSoft.BaseSchemaModuleOverride;
});
