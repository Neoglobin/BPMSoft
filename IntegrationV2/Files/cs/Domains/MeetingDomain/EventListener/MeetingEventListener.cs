namespace IntegrationV2.Files.cs.Domains.MeetingDomain.EventListener
{
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Entities.Events;

	#region Class: MeetingEventListener

	[ExcludeFromCodeCoverage]
	[EntityEventListener(SchemaName = "Activity")]
	class MeetingEventListener: BaseCalendarEventListener
	{
		#region Fields: Private

		/// <summary>
		/// List of listened columns.
		/// </summary>
		private readonly List<string> _listenableColumns = new List<string> {
			"Title", "StartDate", "DueDate", "Location", "PriorityId", "Notes"
		};

		/// <summary>
		/// Listening columns with old values.
		/// </summary>
		private readonly List<string> _listenableOldColumnsValues = new List<string>() {
			"StartDate"
		};

		#endregion

		#region Methods: Private

		/// <summary>
		/// Checking for affected columns.
		/// </summary>
		/// <param name="entity"><see cref="Entity"/> instance of activity entity.</param>
		/// <returns><c>True</c> if calendar columns affected, otherwise returns false.</returns>
		private bool IsAffectedListenableColumns(Entity entity) {
			return entity.ChangeType == EntityChangeType.Deleted ||
				entity.GetChangedColumnValues().Select(c => c.Name).Intersect(_listenableColumns).Any();
		}

		#endregion

		#region Methods: Protected

		/// <inheritdoc cref="BaseCalendarEventListener.IsNeedDoAction(Entity)"/>
		protected override bool IsNeedDoAction(Entity entity) {
			var affectedListenableColumns = IsAffectedListenableColumns(entity);
			return base.IsNeedDoAction(entity) && affectedListenableColumns;
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// <see cref="BaseEntityEventListener.OnUpdated"/>
		/// </summary>
		public override void OnUpdated(object sender, EntityAfterEventArgs e) {
			var entity = (Entity)sender;
			Logger?.LogInfo($"Update existing meeting '{entity.PrimaryColumnValue}' from internal repository.");
			base.OnUpdated(sender, e);
			if (IsNeedDoAction(entity)) {
				var oldColumnsValues = new Dictionary<string, object>();
				foreach (var columnName in _listenableOldColumnsValues) {
					if (entity.GetChangedColumnValues().Any(c => c.Name == columnName)) {
						oldColumnsValues.Add(columnName, entity.GetColumnOldValue(columnName));
					}
				}
				StartMeetingSynchronization(entity.PrimaryColumnValue, oldColumnsValues: oldColumnsValues);
			} else {
				Logger?.LogInfo($"Update existing meeting '{entity.PrimaryColumnValue}' from internal repository no need to action, action skipped.");
			}
		}
		/// <summary>
		/// <see cref="BaseEntityEventListener.OnDeleting"/>
		/// </summary>
		public override void OnDeleted(object sender, EntityAfterEventArgs e) {
			var entity = (Entity)sender;
			Logger?.LogDebug($"Delete meeting '{entity.PrimaryColumnValue}' from internal repository.");
			base.OnDeleted(sender, e);
			if (IsNeedDoAction(entity)) {
				var syncAction = GetIsBackgroundProcess() ? SyncAction.Delete : SyncAction.DeleteWithInvite;
				StartMeetingSynchronization(entity.PrimaryColumnValue, syncAction);
			} else {
				Logger?.LogInfo($"Delete meeting '{entity.PrimaryColumnValue}' from internal repository no need to action, action skipped.");
			}
		}

		#endregion

	}

	#endregion

}
