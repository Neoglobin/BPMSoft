namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Model
{
	using System;
	using System.Collections.Generic;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Client;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Logger;
	using Newtonsoft.Json.Linq;
	using BPMSoft.Common;
	using BPMSoft.Common.Json;
	using BPMSoft.Configuration;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Factories;
	using BPMSoft.Core;
	using IntegrationV2.Files.cs.Utils;

	#region Class: Calendar

	/// <summary>
	/// Calendar domain model.
	/// </summary>
	public class Calendar
	{

		#region Fields: Private

		/// <summary>
		/// <see cref="IMeetingRepository"/> instance.
		/// </summary
		private readonly IMeetingRepository _meetingRepository;

		/// <summary>
		/// <see cref="ICalendarLogger"/> instance.
		/// </summary
		private readonly ICalendarLogger _log;

		#endregion

		#region Properties: Public

		public Guid Id { get; }

		public CalendarType Type { get; } = CalendarType.Exchange;

		public CalendarSettings Settings { get; private set; }

		public Contact Owner { get; private set; }

		public DateTime SyncSinceDate { get; private set; }

		public DateTime SyncTillDate { get; private set; }

		public TimeZoneInfo TimeZone { get; }

		public List<string> FolderIds { get; } = new List<string>();

		#endregion

		#region Constructor: Public 

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="entity">Calendar <see cref="Entity"/>.</param>
		/// <param name="type">Calendar type.</param>
		public Calendar(Entity entity, CalendarType type = CalendarType.Exchange) {
			Id = entity.PrimaryColumnValue;
			Type = type;
			SetSyncDates(entity);
			SetSyncFolders(entity);
			TimeZone = entity.UserConnection.CurrentUser.TimeZone;
		}

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="entity">Calendar <see cref="Entity"/>.</param>
		/// <param name="type">Calendar type.</param>
		/// <param name="sessionId">Synchronization session identifier.</param>
		public Calendar(Entity entity, string sessionId, CalendarType type = CalendarType.Exchange) : this(entity, type) {
			_log = ClassFactory.Get<ICalendarLogger>(new ConstructorArgument("sessionId", sessionId),
				new ConstructorArgument("modelName", GetType().Name));
			_meetingRepository = ClassFactory.Get<IMeetingRepository>(
				new ConstructorArgument("uc", entity.UserConnection),
				new ConstructorArgument("sessionId", _log?.SessionId));

		}

		#endregion

		#region Methods: Private

		private void SetSyncFolders(Entity entity) {
			if (entity.GetTypedColumnValue<bool>("ImportAppointmentsAll")) {
				return;
			}
			var rawFolders = entity.GetTypedColumnValue<string>("ImportAppointmentsCalendars");
			var foldersArray = Json.Deserialize(rawFolders) as JArray;
			if (foldersArray == null) {
				return;
			}
			foreach (JToken folder in foldersArray) {
				FolderIds.Add(folder.Value<string>("Path"));
			}
		}

		private void SetSyncDates(Entity entity) {
			var syncPeriodId = entity.GetTypedColumnValue<Guid>("ActivitySyncPeriodId");
			var dateType = LoadFromDateType.GetInstance(entity.UserConnection);
			SyncSinceDate = dateType.GetLoadFromDate(syncPeriodId);
			SyncTillDate = dateType.GetLoadFromDate(syncPeriodId, true);
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Add <see cref="Meeting"/> to calendar.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> model.</param>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public void SaveMeeting(Meeting meeting, UserConnection uc = null) {
			_log?.LogInfo($"Create or update meeting '{meeting}' to '{this}'.");
			if (Settings.SyncEnabled) {
				Settings.RefreshAccessToken(uc);
				var calendarClient = CalendarClientFactory.GetCalendarClient(this, _log?.SessionId);
				var integrationsId = calendarClient.SaveMeeting(meeting, this);
				if (integrationsId != null) {
					if (meeting.RemoteId.IsNullOrEmpty()) {
						meeting.RemoteCreatedOn = DateTime.Now;
					}
					meeting.SetIntegrationsId(integrationsId);
					_meetingRepository.Save(meeting);
					_log?.LogInfo($"Update meeting '{meeting.Id}' RemoteId in the internal repository.");
				}
			}
			_log?.LogInfo($"Meeting '{meeting.Id}' created or updated in '{this}'.");
		}

		/// <summary>
		/// Removes <paramref name="meeting"/> from current calendar.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public void RemoveMeeting(Meeting meeting, UserConnection uc = null) {
			if (Settings.SyncEnabled && !string.IsNullOrEmpty(meeting.RemoteId)) {
				Settings.RefreshAccessToken(uc);
				var calendarClient = CalendarClientFactory.GetCalendarClient(this, _log?.SessionId);
				calendarClient.RemoveMeeting(meeting, this);
			}
		}

		/// <summary>
		/// Sends <see cref="Meeting"/> invites inexternal calendar.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> model.</param>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public void SendInvitations(Meeting meeting, UserConnection uc = null) {
			if (Settings.SyncEnabled) {
				Settings.RefreshAccessToken(uc);
				var calendarClient = CalendarClientFactory.GetCalendarClient(this, _log?.SessionId);
				calendarClient.SendInvitations(meeting, this);
				_meetingRepository.Save(meeting);
			}
		}

		/// <summary>
		/// Set settings property.
		/// </summary>
		/// <param name="calendarSettings"><see cref="CalendarSettings"/> instance.</param>
		public void SetCalendarSettings(CalendarSettings calendarSettings) {
			Settings = calendarSettings;
		}

		/// <summary>
		/// Set ower identifier.
		/// </summary>
		/// <param name="ownerId">Owner identifier.</param>
		/// <param name="name">Owner name.</param>
		/// <param name="email">Owner email.</param>
		public void SetOwner(Guid ownerId, string name, string email) {
			Owner = new Contact(ownerId, name, email);
		}

		public override string ToString() {
			return $"[\"Calendar\" => \"{Id}\" \"{Settings?.SenderEmailAddress}\" \"{Owner?.Name}\"]";
		}

		#endregion

	}

	#endregion

}
