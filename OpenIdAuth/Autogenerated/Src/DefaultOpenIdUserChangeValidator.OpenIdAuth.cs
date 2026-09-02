 namespace BPMSoft.Configuration.OpenIdAuth
{
	using System;
	using BPMSoft.Core;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Factories;

	#region Class: DefaultOpenIdUserChangeValidator

	[DefaultBinding(typeof(IOpenIdUserChangeValidator))]
	public class DefaultOpenIdUserChangeValidator : IOpenIdUserChangeValidator
	{

		#region Methods: Public

		public bool CanChangeUser(UserConnection userConnection, Entity sysAdminUnitModified) {
			Guid userId = sysAdminUnitModified.GetTypedColumnValue<Guid>("Id");
			var schema = userConnection.EntitySchemaManager.GetInstanceByName("SysAdminUnit");
			var sysAdminUnit = schema.CreateEntity(userConnection);
			string[] _columnsToFetch = { "OpenIDSub", "Active" };
			if (!sysAdminUnit.FetchFromDB(sysAdminUnit.Schema.PrimaryColumn.Name, userId, _columnsToFetch)) {
				return true;
			}
			if (string.IsNullOrEmpty(sysAdminUnit.GetTypedColumnValue<string>("OpenIDSub"))) {
				return true;
			}
			if (sysAdminUnitModified.GetTypedColumnValue<bool>("Active") == sysAdminUnit.GetTypedColumnValue<bool>("Active")) {
				return true;
			}
			return false;
		}

		#endregion

	}

	#endregion

} 
