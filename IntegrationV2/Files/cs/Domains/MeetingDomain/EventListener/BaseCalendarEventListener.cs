namespace IntegrationV2.Files.cs.Domains.MeetingDomain.EventListener
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Logger;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Synchronization;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Utils;
	using IntegrationV2.Files.cs.Utils;
	using BPMSoft.Common;
	using BPMSoft.Configuration;
	using BPMSoft.Core;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Entities.Events;
	using BPMSoft.Core.Factories;
	using BPMSoft.Core.Tasks;
	using BPMSoft.Web.Http.Abstractions;

	#region Class: BaseCalendarEventListener

	/// <summary>
	/// Contains basic logic of calendar event listeners.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class BaseCalendarEventListener: BaseEntityEventListener
	{

		#region Properties: Protected

		private ICalendarLogger _logger;

		protected ICalendarLogger Logger { 
			get {
				if (_logger == null && ClassFactory.HasBinding<ICalendarLogger>()) {
					_logger = ClassFactory.Get<ICalendarLogger>();
				}
				return _logger;
			}
		}

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		public BaseCalendarEventListener() {
			Logger?.SetModelName(GetType().Name);
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Returns is feature enabled for <paramref name="uc"/>.
		/// </summary>
		/// <param name="userConnection"><see cref="UserConnection"/> instance.</param>
		/// <param name="code">Feature code.</param>
		/// <returns><c>True</c> if feature enabled, otherwise returns false.</returns>
		private bool GetIsFeatureEnabled(UserConnection userConnection, string code) {
			var featureUtil = ClassFactory.Get<IFeatureUtilities>();
			return featureUtil.GetIsFeatureEnabled(userConnection, code);
		}

		#endregion

		#region Methods: Protected

		/// <summary>
		/// Checks whether an event listener action has been started from background process.
		/// </summary>
		/// <returns>
		/// <c>True</c>, if event listener action has been started from background process, 
		/// otherwise <c>false</c>.
		/// </returns>
		protected bool GetIsBackgroundProcess() {
			var accessor = ClassFactory.Get<IHttpContextAccessor>();
			var httpContext = accessor.GetInstance();
			return httpContext == null;
		}

		/// <summary>
		/// Checks conditions for doing domain action.
		/// </summary>
		/// <param name="entity"><see cref="Entity"/> instance of activity entity.</param>
		/// <param name="sender">Meeting object.</param>
		protected virtual bool IsNeedDoAction(Entity entity) {
			return GetIsFeatureEnabled(entity.UserConnection, "NewMeetingIntegration");
		}

		/// <summary>
		/// Starts meeting synchroniztion session for <paramref name="meetingId"/>.
		/// </summary>
		/// <param name="meetingId">Meeting identifier.</param>
		/// <param name="syncAction">Sync action.</param>
		/// <param name="oldColumnsValues">Columns with old values.</param>
		protected void StartMeetingSynchronization(Guid meetingId, SyncAction syncAction = default,
			Dictionary<string, object> oldColumnsValues = null) {
			var arguments = new SyncSessionArguments {
				MeetingId = meetingId,
				SyncAction = syncAction,
				SessionId = Logger?.SessionId,
				IsBackgroundProcess = GetIsBackgroundProcess()
			};
			if (oldColumnsValues != null) {
				arguments.OldColumnsValues = oldColumnsValues;
			}
			StartMeetingSynchronization(arguments);
		}

		/// <summary>
		/// Starts meeting synchroniztion session for <paramref name="meetingId"/>.
		/// <paramref name="action"/> will be aplied for <paramref name="contacts"/> calendars only.
		/// </summary>
		/// <param name="meetingId">Meeting identifier.</param>
		/// <param name="contacts">Changed participants identifiers.</param>
		/// <param name="action">Sync action.</param>
		protected void StartMeetingSynchronization(Guid meetingId, List<Contact> contacts,
				SyncAction syncAction = SyncAction.CreateOrUpdate) {
			contacts = contacts.Where(c => c != null && c.Id.IsNotEmpty())
				.Distinct(ComparerUtils.GetComparer<Contact>())
				.ToList();
			if (contacts.IsEmpty()) {
				return;
			}
			StartMeetingSynchronization(new SyncSessionArguments {
				MeetingId = meetingId,
				Contacts = contacts,
				SyncAction = syncAction,
				SessionId = Logger?.SessionId,
				IsBackgroundProcess = GetIsBackgroundProcess()
			});
		}

		/// <summary>
		/// Starts meeting synchroniztion session for <paramref name="meetingId"/>.
		/// </summary>
		/// <param name="syncArgs"><see cref="SyncSessionArguments"/> instance.</param>
		protected void StartMeetingSynchronization(SyncSessionArguments syncArgs) {
			try {
				Task.StartNewWithUserConnection<CalendarSyncSession, SyncSessionArguments>(syncArgs);
				var contactsLog = syncArgs.Contacts.Any() && !syncArgs.Contacts.Any(x => x == null)
					? $"for contacts {syncArgs.Contacts.GetString()}"
					: string.Empty;
				Logger?.LogInfo($"Meeting '{syncArgs.MeetingId}' synchronization scheduled {contactsLog}.");
			} catch (Exception e) {
				Logger?.LogError("Calendar synchronization session start failed.", e);
			}
		}

		#endregion

	}

	#endregion

}
