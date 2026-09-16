define("BaseSchemaModuleOverride", ["LoadingBarModule", "sandbox"], function() {
	Ext.define("BPMSoft.configuration.BaseSchemaModuleOverride", {
		override: "BPMSoft.configuration.BaseSchemaModule",
		alternateClassName: "BPMSoft.BaseSchemaModuleOverride",
		sandbox: null,
		singleton: true,

		isLoadingBarActive: false,
		isModuleInitialized: false,
		intervalId: null,
		startTime: null,

		/**
		 * @override
		 * @inheritdoc BaseSchemaModule#init 
		 */
		init: function(callback, scope) 
		{
			this.callParent([function() {
				this.isModuleInitialized = true;
				this.registerPendingRequestsMessage();
				this.sandbox.subscribe("OnPendingRequestsMessage", this.setLoadingInterval, this);

				callback.call(scope || this);
			}, this]);

			this.processGlobalLoadingBar();
		},

		processGlobalLoadingBar: function() 
		{
			if (!this.isLoadingBarActive) 
			{
				if (BPMSoft.Features.getIsEnabled("GlobalLoadingBar")) 
				{
					this.isLoadingBarActive = true;
					if (!Ext.ClassManager.get("BPMSoft.SelectXHRInterceptor")) 
					{
						BPMSoft.require(["SelectXHRInterceptor"], function(interceptor) {
							if (interceptor && !interceptor?.isLoaded) 
							{
								BPMSoft.SelectXHRInterceptor.init();
								BPMSoft.SelectXHRInterceptor.start();
							}
						}, this);
					}

					BPMSoft.LoadingBarModule.load(function(response) {
						if (!this.isModuleInitialized) 
						{
							this.setLoadingInterval();
						}
					}, this);
				}
			}
		},
		
		setLoadingInterval: function()  
		{
			if (this.intervalId) return;

			this.startTime = Date.now();
			this.intervalId = setInterval(() => {

				const currentTime = Date.now();
				const delay = currentTime - this.startTime;

				if ((this.isModuleInitialized && BPMSoft.SelectXHRInterceptor.pendingRequests == 0) 
					|| delay >= BPMSoft.LoadingBarModule.loadingBarDestroyDelayEdge) 
				{
					clearInterval(this.intervalId);
					this.intervalId = null;
					this.startTime = null;		

					BPMSoft.LoadingBarModule.hide();
					return;
				}	
	
				if (!BPMSoft.LoadingBarModule.isRendered 
					&& delay >= BPMSoft.LoadingBarModule.loadingBarRenderDelayEdge) 
				{
					BPMSoft.LoadingBarModule.show();
				}
					
			}, 200);
		},

		registerPendingRequestsMessage: function() 
		{
			const messageConfig = {};

			messageConfig["OnPendingRequestsMessage"] = {
				mode: BPMSoft.MessageMode.PTP,
				direction: BPMSoft.MessageDirectionType.SUBSCRIBE
			};

			this.sandbox.registerMessages(messageConfig);
		}
	});
	return BPMSoft.BaseSchemaModuleOverride;
});
