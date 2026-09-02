namespace BPMSoft.Configuration
{

	using BPMSoft.Common;
	using BPMSoft.Core;
	using BPMSoft.Core.Configuration;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;

	#region Class: OpenIdSysUserInRoleEventListenerSchema

	/// <exclude/>
	public class OpenIdSysUserInRoleEventListenerSchema : BPMSoft.Core.SourceCodeSchema
	{

		#region Constructors: Public

		public OpenIdSysUserInRoleEventListenerSchema(SourceCodeSchemaManager sourceCodeSchemaManager)
			: base(sourceCodeSchemaManager) {
		}

		public OpenIdSysUserInRoleEventListenerSchema(OpenIdSysUserInRoleEventListenerSchema source)
			: base( source) {
		}

		#endregion

		#region Methods: Protected

		protected override void InitializeProperties() {
			base.InitializeProperties();
			UId = new Guid("1924c96a-4424-4b42-8958-8df3c4ce8fee");
			Name = "OpenIdSysUserInRoleEventListener";
			ParentSchemaUId = new Guid("50e3acc0-26fc-4237-a095-849a1d534bd3");
			CreatedInPackageId = new Guid("5daf09f1-167a-4d95-90ab-547ed370e530");
			ZipBody = new byte[] { 31,139,8,0,0,0,0,0,0,10,229,85,81,111,218,48,16,126,166,82,255,195,45,123,9,18,10,239,45,69,106,105,215,33,149,181,42,180,47,211,30,76,124,16,111,137,29,217,14,29,155,250,223,119,182,147,180,233,64,72,221,227,36,132,196,249,190,187,239,187,251,108,36,43,208,148,44,69,184,184,155,205,213,202,38,19,37,87,98,93,105,102,133,146,201,109,137,114,202,207,43,155,29,31,253,62,62,234,85,70,200,53,204,183,198,98,113,218,254,126,193,22,133,146,187,226,26,147,43,105,133,21,104,14,28,39,87,27,148,118,95,214,39,150,90,165,235,42,244,249,168,113,77,60,97,146,51,99,78,32,208,37,122,15,6,245,84,222,171,28,125,185,27,65,132,37,234,227,35,194,124,245,173,182,157,131,120,158,102,88,176,47,52,14,56,131,168,83,33,234,127,35,84,89,45,115,145,66,234,26,29,236,3,39,112,193,12,134,70,183,79,20,121,67,163,231,134,233,4,180,10,102,104,51,197,73,195,157,86,22,83,139,188,201,40,155,0,108,132,182,21,203,97,163,4,135,73,134,233,143,9,147,51,197,197,106,235,40,204,176,88,146,148,208,21,76,135,157,143,245,193,183,237,109,152,134,202,157,113,18,187,35,47,185,70,187,216,150,200,39,42,175,10,249,200,242,10,71,215,149,224,227,184,29,13,143,250,167,109,49,77,208,119,22,115,137,221,98,142,25,185,80,146,102,55,152,221,69,31,58,73,47,224,13,203,5,103,228,17,194,121,83,4,199,120,22,163,105,216,155,43,51,201,152,92,227,99,147,61,142,107,2,98,5,241,135,182,72,66,243,13,153,47,4,226,46,191,65,61,201,65,61,132,126,51,100,79,7,181,86,122,134,198,176,181,51,150,196,39,184,81,41,149,255,197,150,57,206,173,38,135,191,41,152,220,163,81,149,78,233,84,105,130,13,32,148,235,69,135,92,23,13,32,250,171,186,113,26,108,48,201,121,94,4,159,120,62,137,95,69,212,79,22,170,38,82,207,160,103,51,173,158,60,217,171,159,41,150,142,85,252,90,73,157,247,236,190,159,91,35,163,228,193,203,251,173,237,239,144,191,132,189,225,112,8,35,83,21,5,211,219,241,103,38,121,142,6,48,88,119,42,73,160,117,151,31,157,190,100,52,108,18,91,100,201,52,43,64,210,125,61,139,12,117,38,245,99,63,12,8,191,8,227,83,118,35,48,26,47,50,164,254,136,144,106,92,157,69,139,147,61,175,145,103,116,129,43,138,250,250,231,122,109,34,24,142,65,72,99,153,164,151,51,85,210,50,33,29,93,155,97,211,207,51,7,114,17,235,80,169,159,17,181,161,121,10,142,225,42,223,202,86,113,172,150,223,201,7,181,138,1,236,236,15,216,152,108,73,175,76,242,26,222,224,154,29,237,125,37,234,103,162,31,0,33,251,249,240,106,46,49,199,255,106,51,151,181,224,247,45,166,69,255,235,94,246,220,49,119,226,254,6,95,197,41,4,127,0,183,176,114,248,215,7,0,0 };
		}

		protected override void InitializeLocalizableStrings() {
			base.InitializeLocalizableStrings();
			SetLocalizableStringsDefInheritance();
			LocalizableStrings.Add(CreateCantModifyAlmRoleMessageLocalizableString());
		}

		protected virtual SchemaLocalizableString CreateCantModifyAlmRoleMessageLocalizableString() {
			SchemaLocalizableString localizableString = new SchemaLocalizableString() {
				UId = new Guid("0ce50dac-8e62-7139-ffcb-c63dc6a1c41e"),
				Name = "CantModifyAlmRoleMessage",
				CreatedInPackageId = new Guid("5daf09f1-167a-4d95-90ab-547ed370e530"),
				CreatedInSchemaUId = new Guid("1924c96a-4424-4b42-8958-8df3c4ce8fee"),
				ModifiedInSchemaUId = new Guid("1924c96a-4424-4b42-8958-8df3c4ce8fee")
			};
			return localizableString;
		}

		#endregion

		#region Methods: Public

		public override void GetParentRealUIds(Collection<Guid> realUIds) {
			base.GetParentRealUIds(realUIds);
			realUIds.Add(new Guid("1924c96a-4424-4b42-8958-8df3c4ce8fee"));
		}

		#endregion

	}

	#endregion

}

