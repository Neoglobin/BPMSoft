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
			this.callParent([function() {
				this.isModuleInitialized = true;
				callback.call(scope || this);
			}, this]);

			this.processGlobalLoadingBar();
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

		processGlobalLoadingBar: function() 
		{
			BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarActive", (isActive) => 
			{
				if (!isActive) return; 
	
				this.isLoadingBarActive = true;
				BPMSoft.LoadingBarModule.init();
	
				if (!this.intervalId && !this.isModuleInitialized) 
				{
					BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarRenderDelayEdge", (edgeValue) => 
					{
	
						this.loadingBarRenderDelayEdge = edgeValue ?? 0;
						
						BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarDestroyDelayEdge", (edgeValue) => 
						{
	
							this.loadingBarDestroyDelayEdge = edgeValue ?? 15000;
						
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
	
						}, this);
					}, this);
				}
			}, this);
		},

	});
	return BPMSoft.BaseSchemaModuleOverride;
});
