 define("SelectXHRInterceptor", [], function() {
	Ext.define("BPMSoft.configuration.SelectXHRInterceptor", {
		alternateClassName: "BPMSoft.SelectXHRInterceptor",
		singleton: true,
		sandbox: null,

		isLoaded: false,
		isStarted: false,
		prototypeOpen: null,
		prototypeSend: null,
		pendingRequests: 0,

		init: function() 
		{
			this.isLoaded = true;
			BPMSoft.require(["sandbox"], function(sandbox) {
				this.sandbox = sandbox;
				this.registerMessages();
			}, this);
		},

		start: function() 
		{
			if (this.isStarted) return;
			try 
			{
				this.isStarted = true;
				this.prototypeOpen = XMLHttpRequest.prototype.open;
				this.prototypeSend = XMLHttpRequest.prototype.send;
	
				const interceptor = this;
	
				XMLHttpRequest.prototype.open = function(method, url) 
				{
					this._trackedUrl = url;
					return interceptor.prototypeOpen.apply(this, arguments);
				}
				
				XMLHttpRequest.prototype.send = function() 
				{
					if (this._trackedUrl?.includes("SelectQuery")) 
					{
						if (interceptor.pendingRequests == 0) 
						{
							interceptor.sandbox.prototype.publish("OnPendingRequestsMessage");
						}
						interceptor.pendingRequests++;

						this.addEventListener("loadend", () => interceptor.pendingRequests--);
					}
	
					return interceptor.prototypeSend.apply(this, arguments);
				}
			}
			catch (ex) {}
		},

		stop: function() 
		{
			XMLHttpRequest.prototype.open = this.prototypeOpen;
			XMLHttpRequest.prototype.send = this.prototypeSend;
			this.pendingRequests = 0;
			this.isStarted = false;
		},

		registerMessages: function() 
		{
			const messages = {};

			messages["OnPendingRequestsMessage"] = {
				mode: BPMSoft.MessageMode.PTP,
				direction: BPMSoft.MessageDirectionType.PUBLISH
			};

			this.sandbox.prototype.registerMessages(messages);
		},

		destroy: function() 
		{
			this.stop();
			this.callParent(arguments);
		}
	});
	return BPMSoft.SelectXHRInterceptor;
 })