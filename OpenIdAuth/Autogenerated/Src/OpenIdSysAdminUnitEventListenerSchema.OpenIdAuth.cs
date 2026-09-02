namespace BPMSoft.Configuration
{

	using BPMSoft.Common;
	using BPMSoft.Core;
	using BPMSoft.Core.Configuration;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;

	#region Class: OpenIdSysAdminUnitEventListenerSchema

	/// <exclude/>
	public class OpenIdSysAdminUnitEventListenerSchema : BPMSoft.Core.SourceCodeSchema
	{

		#region Constructors: Public

		public OpenIdSysAdminUnitEventListenerSchema(SourceCodeSchemaManager sourceCodeSchemaManager)
			: base(sourceCodeSchemaManager) {
		}

		public OpenIdSysAdminUnitEventListenerSchema(OpenIdSysAdminUnitEventListenerSchema source)
			: base( source) {
		}

		#endregion

		#region Methods: Protected

		protected override void InitializeProperties() {
			base.InitializeProperties();
			UId = new Guid("6031c2b2-7861-40a6-9700-b2042b435ba9");
			Name = "OpenIdSysAdminUnitEventListener";
			ParentSchemaUId = new Guid("50e3acc0-26fc-4237-a095-849a1d534bd3");
			CreatedInPackageId = new Guid("cafc62fc-f7d7-4a5d-acf5-62f836ef940a");
			ZipBody = new byte[] { 31,139,8,0,0,0,0,0,0,3,141,148,81,143,218,48,12,199,159,65,226,59,120,221,11,72,168,188,115,128,4,136,109,72,199,56,13,184,151,105,15,161,53,52,27,77,170,36,45,99,19,223,253,156,164,101,192,129,152,132,144,226,248,111,255,28,219,5,193,82,212,25,139,16,70,47,179,133,220,152,112,44,197,134,111,115,197,12,151,34,156,103,40,166,241,48,55,73,163,254,183,81,175,229,154,139,45,44,14,218,96,250,116,58,255,211,166,169,20,183,236,10,195,137,48,220,112,212,15,174,195,73,129,194,220,243,250,196,34,35,85,25,133,126,31,21,110,137,19,198,59,166,117,23,60,46,225,13,227,148,139,149,224,198,69,123,230,196,43,80,53,234,36,249,238,50,29,46,46,154,139,40,193,148,125,165,215,128,62,4,231,1,130,214,143,255,19,189,238,223,203,178,124,189,227,17,68,150,238,17,28,116,97,196,52,250,68,243,61,89,174,216,107,182,1,182,232,83,213,51,52,137,140,169,238,23,37,13,70,6,227,202,35,171,12,80,112,101,114,182,131,66,242,24,198,9,70,191,198,76,172,178,152,25,92,105,42,194,231,3,125,208,246,232,79,45,112,169,106,5,83,144,147,149,102,66,80,52,155,178,127,233,25,174,46,174,159,78,178,130,237,56,229,144,138,20,174,57,190,115,135,240,51,154,222,212,63,133,211,38,76,108,241,181,242,30,52,91,62,6,223,64,243,195,41,72,72,204,222,211,49,95,50,181,175,224,43,122,199,129,74,73,53,67,173,217,214,118,73,224,30,158,101,68,113,255,176,245,14,23,70,209,136,93,197,11,191,161,150,185,138,232,86,42,146,181,125,180,90,240,160,129,65,27,130,119,177,181,69,55,51,25,243,205,193,50,14,41,69,129,37,80,72,117,231,24,180,194,165,44,73,202,234,107,38,81,114,239,104,39,191,35,204,44,86,243,188,148,210,239,104,255,143,167,177,64,17,251,201,184,63,40,110,32,221,30,212,58,157,14,244,116,158,166,76,29,6,95,152,136,119,168,1,253,56,76,5,177,26,187,126,104,75,12,123,157,202,241,164,204,152,98,41,216,15,72,63,208,148,153,30,96,224,222,3,252,137,52,206,229,182,2,131,193,50,65,202,143,8,145,194,77,63,88,118,239,124,15,28,209,8,55,100,117,241,135,106,171,3,232,12,128,11,109,152,160,111,87,36,133,97,92,88,92,147,96,149,207,145,3,205,15,187,64,41,119,82,22,244,158,60,70,191,24,115,191,18,182,5,114,253,147,230,160,44,162,13,55,211,3,86,67,182,166,149,13,207,212,149,172,234,208,141,141,43,87,174,229,93,189,223,209,183,228,188,131,116,246,214,75,35,217,224,13,202,139,56,230,183,5,0,0 };
		}

		protected override void InitializeLocalizableStrings() {
			base.InitializeLocalizableStrings();
			SetLocalizableStringsDefInheritance();
			LocalizableStrings.Add(CreateCantModifyUserActiveMessageLocalizableString());
		}

		protected virtual SchemaLocalizableString CreateCantModifyUserActiveMessageLocalizableString() {
			SchemaLocalizableString localizableString = new SchemaLocalizableString() {
				UId = new Guid("9bf23670-1e6a-e323-1820-4acf68e5b432"),
				Name = "CantModifyUserActiveMessage",
				CreatedInPackageId = new Guid("cafc62fc-f7d7-4a5d-acf5-62f836ef940a"),
				CreatedInSchemaUId = new Guid("6031c2b2-7861-40a6-9700-b2042b435ba9"),
				ModifiedInSchemaUId = new Guid("6031c2b2-7861-40a6-9700-b2042b435ba9")
			};
			return localizableString;
		}

		#endregion

		#region Methods: Public

		public override void GetParentRealUIds(Collection<Guid> realUIds) {
			base.GetParentRealUIds(realUIds);
			realUIds.Add(new Guid("6031c2b2-7861-40a6-9700-b2042b435ba9"));
		}

		#endregion

	}

	#endregion

}

