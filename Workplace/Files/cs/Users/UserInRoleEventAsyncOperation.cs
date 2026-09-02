namespace BPMSoft.Configuration.Users 
{
	using System;
	using BPMSoft.Configuration.Workplace;
	using BPMSoft.Core;
	using BPMSoft.Core.Entities.AsyncOperations;
	using BPMSoft.Core.Entities.AsyncOperations.Interfaces;
	using BPMSoft.Core.Factories;

	#region Class: UserInRoleEventAsyncOperation

	/// <summary>
	/// Class implementats <see cref="IEntityEventAsyncOperation"/> interface for SysUserInRole and
	/// SysAdminUnitInWorkplace entities.
	/// </summary>
	internal class UserInRoleEventAsyncOperation : IEntityEventAsyncOperation
	{

		#region Methods: Private

		/// <summary>
		/// Creates <see cref="IWorkplaceManager"/> implementation instance.
		/// </summary>
		/// <param name="userConnection"><see cref="UserConnection"/> instance.</param>
		/// <returns><see cref="IWorkplaceManager"/> implementation instance.</returns>
		private IWorkplaceManager GetWorkplaceManager(UserConnection userConnection) {
			return ClassFactory.Get<IWorkplaceManager>(new ConstructorArgument("uc", userConnection));
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="IEntityEventAsyncOperation.Execute"/>
		public void Execute(UserConnection userConnection, EntityEventAsyncOperationArgs arguments) {
			var manager = GetWorkplaceManager(userConnection);
			manager.ReloadWorkplaces();
		}

		#endregion

	}

	#endregion

}
