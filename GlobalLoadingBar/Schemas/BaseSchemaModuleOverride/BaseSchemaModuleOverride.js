define("BaseSchemaModuleOverride", ["LoadingBarModule"], function() {
	Ext.define("BPMSoft.configuration.BaseSchemaModuleOverride", {
		override: "BPMSoft.configuration.BaseSchemaModule",
		alternateClassName: "BPMSoft.BaseSchemaModuleOverride",

		init: function() 
		{
			BPMSoft.LoadingBarModule.init();
			BPMSoft.LoadingBarModule.show();
			this.callParent(arguments);
		},

		render: function() 
		{
			this.callParent(arguments);
			BPMSoft.LoadingBarModule.hide();
		}
	});
	return BPMSoft.BaseSchemaModuleOverride;
});
