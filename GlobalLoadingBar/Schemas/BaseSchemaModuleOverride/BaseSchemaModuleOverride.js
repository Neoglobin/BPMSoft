define("BaseSchemaModuleOverride", ["LoadingBarModule"], function() {
	Ext.define("BPMSoft.configuration.BaseSchemaModuleOverride", {
		override: "BPMSoft.configuration.BaseSchemaModule",
		alternateClassName: "BPMSoft.BaseSchemaModuleOverride",

		isLoadingBarActive: false,

		init: function() 
		{
			BPMSoft.SysSettings.querySysSettingsItem("GlobalLoadingBarActive", (isActive) => {
				if (isActive) 
				{
					this.isLoadingBarActive = isActive;
					BPMSoft.LoadingBarModule.init();
					BPMSoft.LoadingBarModule.show();
				}
			}, this);
			this.callParent(arguments);
		},

		render: function() 
		{
			this.callParent(arguments);
			if (this.isLoadingBarActive) 
			{
				BPMSoft.LoadingBarModule.hide();
			}
		}
	});
	return BPMSoft.BaseSchemaModuleOverride;
});
