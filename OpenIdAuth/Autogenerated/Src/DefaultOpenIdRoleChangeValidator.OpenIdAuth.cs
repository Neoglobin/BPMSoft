namespace BPMSoft.Configuration.OpenIdAuth
{
	using System;
	using BPMSoft.Core;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Factories;

	#region Class: DefaultOpenIdRoleChangeValidator

	[DefaultBinding(typeof(IOpenIdRoleChangeValidator))]
	public class DefaultOpenIdRoleChangeValidator : IOpenIdRoleChangeValidator
	{
		#region Methods: Private

		private Entity GetEntity(UserConnection userConnection, string entityName) {
			var schema = userConnection.EntitySchemaManager.GetInstanceByName(entityName);
			return schema.CreateEntity(userConnection);
		}

		private bool IsUserFromOpenId(UserConnection userConnection, Guid userId) {
			const string openIdSubColumnName = "OpenIDSub";
			var sysAdminUnit = GetEntity(userConnection, "SysAdminUnit");
			string[] _columnsToFetch = { openIdSubColumnName };
			if (sysAdminUnit.FetchFromDB(sysAdminUnit.Schema.PrimaryColumn.Name, userId, _columnsToFetch)) {
				string openIdSub = sysAdminUnit.GetTypedColumnValue<string>(openIdSubColumnName);
				return !string.IsNullOrEmpty(openIdSub);
			}
			return false;
		}

		private bool IsRoleFromOpenId(UserConnection userConnection, Guid roleId) {
			var sysAlmRole = GetEntity(userConnection, "SysAlmRole");
			if (sysAlmRole.FetchFromDB("Role", roleId, useDisplayValues: false)) {
				return true;
			}
			return false;
		}

		#endregion

		#region Methods: Public

		public bool CanChangeUserInRole(UserConnection userConnection, Guid userId, Guid roleId) {
			bool isUserFromOpenId = IsUserFromOpenId(userConnection, userId);
			if (!isUserFromOpenId) {
				return true;
			}
			bool isRoleFromOpenId = IsRoleFromOpenId(userConnection, roleId);
			return !isRoleFromOpenId;
		}

		#endregion

	}

	#endregion

} 
