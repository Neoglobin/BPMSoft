define("BaseSchemaModuleOverride", [], function() {
	Ext.define("BPMSoft.configuration.BaseSchemaModuleOverride", {
		override: "BPMSoft.configuration.BaseSchemaModule",
		alternateClassName: "BPMSoft.BaseSchemaModuleOverride",

		isLoadingBarActive: false,
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
				if (BPMSoft.LoadingBarModule.isRendered) 
				{
					BPMSoft.LoadingBarModule.hide();
				}
			}
		},

		processGlobalLoadingBar: function() 
		{
			BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarActive", (isActive) => 
			{
				if (!isActive) return; 
				this.isLoadingBarActive = true;

				if (!Ext.ClassManager.get("BPMSoft.LoadingBarModule")) 
				{
					BPMSoft.require(["LoadingBarModule"], function(loadingBar) {
						if (loadingBar && !loadingBar.isLoaded) 
						{
							BPMSoft.LoadingBarModule.init();
						}
					}, this); 
				} 

				if (!Ext.ClassManager.get("BPMSoft.SelectXHRInterceptor")) 
				{
					BPMSoft.require(["SelectXHRInterceptor"], function(interceptor) {
						if (interceptor && !interceptor.isLoaded) 
						{
							BPMSoft.SelectXHRInterceptor.init();
						}
					}, this);
				}
	
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
									}
		
									return;
								}
				
								if (!BPMSoft.LoadingBarModule.isRendered && this.delay >= this.loadingBarRenderDelayEdge) 
								{
									BPMSoft.LoadingBarModule.show();
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
