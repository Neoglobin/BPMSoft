namespace BPMSoft.Configuration
{
	using System;
	using System.Collections.Generic;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Entities.AsyncOperations;
	using BPMSoft.Sync;

	#region Class: SyncEntityEventAsyncOperationArgs

	public class SyncEntityEventAsyncOperationArgs : EntityEventAsyncOperationArgs
	{

		#region Constructors: Public

		public SyncEntityEventAsyncOperationArgs(Entity entity, EventArgs eventArgs) : base(entity, eventArgs) {
		}

		#endregion

		#region Properties: Public

		/// <summary>
		/// <see cref="ISynchronizationController"/> implementations names.
		/// </summary>
		public List<string> Controllers {
			get; set;
		}

		/// <summary>
		/// System user related <see cref="Contact"/> instance unique identifier.
		/// </summary>
		public Guid UserContactId {
			get; set;
		}

		/// <summary>
		/// <see cref="SyncAction"/>.
		/// </summary>
		public SyncAction Action {
			get; set;
		}

		#endregion

	}

	#endregion

}