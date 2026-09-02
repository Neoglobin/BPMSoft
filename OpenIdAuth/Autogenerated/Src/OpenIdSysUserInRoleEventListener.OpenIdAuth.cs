namespace BPMSoft.Configuration.OpenIdAuth
{
	using System;
	using BPMSoft.Common;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Entities.Events;
	using BPMSoft.Core.Factories;
	
	#region Class: OpenIdSysUserInRoleEventListener

	[EntityEventListener(SchemaName = "SysUserInRole")]
	public class OpenIdSysUserInRoleEventListener : BaseEntityOwnerEventListener
	{
		
		#region Methods: Protected
		
		protected virtual void CheckCanModifyRoleMember(Entity sysUserInRoleEntity) {
			var userId = sysUserInRoleEntity.GetTypedColumnValue<Guid>("SysUserId");
			var roleId = sysUserInRoleEntity.GetTypedColumnValue<Guid>("SysRoleId");
			var userConnection = sysUserInRoleEntity.UserConnection;
			var validator = ClassFactory.Get<IOpenIdRoleChangeValidator>();
			if (!validator.CanChangeUserInRole(userConnection, userId, roleId)) {
				var errorMessage = new LocalizableString(userConnection.ResourceStorage, 
					"OpenIdSysUserInRoleEventListener", "LocalizableStrings.CantModifyAlmRoleMessage.Value").ToString();
				throw new Exception(errorMessage);
			}
		}
		
		#endregion
		
		#region Methods: Public

		/// <summary>Handles entity Inserting event.</summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">The <see cref="T:BPMSoft.Core.Entities.EntityBeforeEventArgs" /> instance containing the
		/// event data.</param>
		public override void OnInserting(object sender, EntityBeforeEventArgs e) {
			base.OnInserting(sender, e);
			CheckCanModifyRoleMember((Entity)sender);
		}

		/// <summary>Handles entity Deleting event.</summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">The <see cref="T:BPMSoft.Core.Entities.EntityBeforeEventArgs" /> instance containing the
		/// event data.</param>
		public override void OnDeleting(object sender, EntityBeforeEventArgs e) {
			base.OnDeleting(sender, e);
			CheckCanModifyRoleMember((Entity)sender);
		}
		
		#endregion
		
	}
	
	#endregion
}
 
