 namespace BPMSoft.Configuration.OpenIdAuth
{
	using System;
	using BPMSoft.Common;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Entities.Events;
	using BPMSoft.Core.Factories;
	
	#region Class: OpenIdSysAdminUnitEventListener

	[EntityEventListener(SchemaName = "SysAdminUnit")]
	[EntityEventListener(SchemaName = "VwSysAdminUnit")]
	public class OpenIdSysAdminUnitEventListener : BaseEntityOwnerEventListener
	{
		
		#region Methods: Protected
		
		protected virtual void CheckCanUpdateUser(Entity sysUserEntity) {
			var userConnection = sysUserEntity.UserConnection;
			var validator = ClassFactory.Get<IOpenIdUserChangeValidator>();
			if (!validator.CanChangeUser(userConnection, sysUserEntity)) {
				var errorMessage = new LocalizableString(userConnection.ResourceStorage,
					"OpenIdSysAdminUnitEventListener", "LocalizableStrings.CantModifyUserActiveMessage.Value").ToString();
				throw new Exception(errorMessage);
			}
		}
		
		#endregion
		
		#region Methods: Public

		/// <summary>Handles entity Inserting event.</summary>
		/// <param name="sender">Event sender.</param>
		/// <param name="e">The <see cref="T:BPMSoft.Core.Entities.EntityBeforeEventArgs" /> instance containing the
		/// event data.</param>
		public override void OnUpdating(object sender, EntityBeforeEventArgs e) {
			base.OnUpdating(sender, e);
			CheckCanUpdateUser((Entity)sender);
		}

		#endregion

	}

	#endregion

}
 
