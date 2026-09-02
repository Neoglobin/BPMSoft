namespace BPMSoft.Configuration
{

	using BPMSoft.Common;
	using BPMSoft.Core;
	using BPMSoft.Core.Configuration;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Globalization;

	#region Class: DefaultOpenIdUserChangeValidatorSchema

	/// <exclude/>
	public class DefaultOpenIdUserChangeValidatorSchema : BPMSoft.Core.SourceCodeSchema
	{

		#region Constructors: Public

		public DefaultOpenIdUserChangeValidatorSchema(SourceCodeSchemaManager sourceCodeSchemaManager)
			: base(sourceCodeSchemaManager) {
		}

		public DefaultOpenIdUserChangeValidatorSchema(DefaultOpenIdUserChangeValidatorSchema source)
			: base( source) {
		}

		#endregion

		#region Methods: Protected

		protected override void InitializeProperties() {
			base.InitializeProperties();
			UId = new Guid("8ec53e46-ef7a-45a0-abd4-217bf5a8fdf0");
			Name = "DefaultOpenIdUserChangeValidator";
			ParentSchemaUId = new Guid("50e3acc0-26fc-4237-a095-849a1d534bd3");
			CreatedInPackageId = new Guid("cafc62fc-f7d7-4a5d-acf5-62f836ef940a");
			ZipBody = new byte[] { 31,139,8,0,0,0,0,0,0,10,141,83,219,106,227,48,16,125,118,161,255,48,235,125,177,33,248,3,218,237,66,226,52,197,15,105,3,105,247,165,148,69,177,198,182,192,150,140,46,5,83,242,239,213,197,155,198,75,66,243,98,208,140,206,101,206,200,192,73,135,170,39,37,194,98,179,222,138,74,103,185,224,21,171,141,36,154,9,158,61,245,200,11,58,55,186,185,190,250,184,190,138,140,98,188,134,237,160,52,118,183,135,243,23,86,226,233,106,118,207,53,211,12,213,153,246,138,148,90,200,208,183,55,126,74,172,173,60,228,45,81,234,6,150,88,17,211,234,96,230,69,161,204,27,194,107,252,67,90,70,137,197,121,204,235,120,107,193,56,181,252,137,30,122,20,85,82,156,69,165,233,155,133,245,102,215,178,18,74,167,244,173,16,220,192,121,62,75,102,35,178,223,131,253,53,234,70,80,59,192,198,139,132,230,40,184,19,162,133,156,240,64,226,232,18,207,41,56,199,210,101,15,102,114,156,129,143,112,0,53,168,57,237,24,127,225,76,175,5,101,21,67,154,130,91,78,20,61,24,70,61,174,160,112,119,242,102,246,128,250,217,38,67,115,209,154,142,91,243,6,127,57,216,239,36,46,104,156,186,253,68,209,59,145,160,202,6,59,98,105,166,62,194,38,135,173,239,174,9,39,53,74,71,90,112,165,9,47,113,49,60,218,55,149,196,219,35,241,9,237,81,221,121,244,60,89,46,145,104,12,212,201,84,112,196,42,45,237,86,95,223,224,111,233,157,171,103,177,66,93,54,150,227,3,98,191,148,229,214,236,226,25,196,115,139,123,199,24,246,1,201,42,72,126,28,203,102,30,184,146,162,91,46,146,73,35,76,149,109,36,235,136,28,66,68,153,155,103,54,166,58,251,95,62,253,23,125,36,81,27,201,65,75,227,255,129,40,218,31,212,131,247,172,80,143,166,109,159,228,125,215,219,41,39,194,167,182,162,60,202,238,229,107,184,244,50,185,75,247,238,222,160,229,31,243,74,225,110,250,102,46,193,124,231,103,44,87,164,85,161,110,203,254,23,65,78,195,95,226,207,161,58,45,238,225,19,107,23,78,24,159,4,0,0 };
		}

		#endregion

		#region Methods: Public

		public override void GetParentRealUIds(Collection<Guid> realUIds) {
			base.GetParentRealUIds(realUIds);
			realUIds.Add(new Guid("8ec53e46-ef7a-45a0-abd4-217bf5a8fdf0"));
		}

		#endregion

	}

	#endregion

}

