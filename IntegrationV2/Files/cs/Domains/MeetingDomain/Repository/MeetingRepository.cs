namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Client;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Logger;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Synchronization;
	using Newtonsoft.Json.Linq;
	using BPMSoft.Common;
	using BPMSoft.Common.Json;
	using BPMSoft.Configuration;
	using BPMSoft.Core;
	using BPMSoft.Core.DB;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Factories;
	using BPMSoft.EmailDomain;
	using BPMSoft.Sync;

	#region Class: MeetingRepository

	/// <summary>
	/// <see cref="IMeetingRepository"/> implementation.
	/// </summary>
	[DefaultBinding(typeof(IMeetingRepository))]
	public class MeetingRepository: IMeetingRepository
	{

		#region Fields: Private

		/// <summary>
		/// <see cref="UserConnection"/> instance.
		/// </summary>
		private readonly UserConnection _userConnection;

		/// <summary>
		/// <see cref="ICalendarLogger"/> instance.
		/// </summary
		private readonly ICalendarLogger _log;

		/// <summary>
		/// <see cref="ICalendarRepository"/> implementation instance.
		/// </summary>
		private readonly ICalendarRepository _calendarRepository;

		/// <summary>
		/// <see cref="IParticipantRepository"/> implementation instance.
		/// </summary>
		private readonly IParticipantRepository _participantRepository;

		/// <summary>
		/// Activity not started status identifier.
		/// </summary>
		private readonly Guid _notStartedStatusId = new Guid("384D4B84-58E6-DF11-971B-001D60E938C6");

		/// <summary>
		/// Activity completed status identifier.
		/// </summary>
		private readonly Guid _completedStatusId = new Guid("4BDBB88F-58E6-DF11-971B-001D60E938C6");

		/// <summary>
		/// <see cref="IRecordOperationsNotificator"/> implementation instance.
		/// </summary>
		private readonly IRecordOperationsNotificator _recordOperationsNotificator;

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public MeetingRepository(UserConnection uc, string sessionId = null) {
			_userConnection = uc;
			_log = ClassFactory.Get<ICalendarLogger>(new ConstructorArgument("sessionId", sessionId),
				new ConstructorArgument("modelName", GetType().Name));
			_calendarRepository = ClassFactory.Get<ICalendarRepository>(new ConstructorArgument("uc", uc));
			_participantRepository = ClassFactory.Get<IParticipantRepository>(new ConstructorArgument("uc", uc),
				new ConstructorArgument("sessionId", _log?.SessionId));
			_recordOperationsNotificator = ClassFactory.Get<IRecordOperationsNotificator>(
				new ConstructorArgument("userConnection", uc));
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Gets list of <see cref="Meeting"/> models by <see cref="Entity.PrimaryColumnValue"/>.
		/// </summary>
		/// <param name="meetingEntity">Meeting <see cref="Entity"/>.</param>
		/// <returns>List of <see cref="Meeting"/> models by <see cref="Entity.PrimaryColumnValue"/>.</returns>
		private List<Meeting> GetMeetingsInternal(Entity meetingEntity) {
			var participants = _participantRepository.GetMeetingParticipants(meetingEntity.PrimaryColumnValue);
			return GetMeetingsInternal(meetingEntity, participants);
		}

		/// <summary>
		/// Gets list of <see cref="Meeting"/> models by <see cref="Entity.PrimaryColumnValue"/>.
		/// </summary>
		/// <param name="meetingEntity">Meeting <see cref="Entity"/>.</param>
		/// <returns>List of <see cref="Meeting"/> models by <see cref="Entity.PrimaryColumnValue"/>.</returns>
		private List<Meeting> GetMeetingsInternal(Entity meetingEntity, List<Participant> participants) {
			var mettings = new List<Meeting>();
			foreach (var participant in participants) {
				var meeting = new Meeting(meetingEntity, _log?.SessionId);
				AddParticipantsToMeeting(participants, participant, meeting);
				FillCalendarProperties(meeting, participant);
				mettings.Add(meeting);
			}
			UpdateInvitesSent(mettings);
			return mettings;
		}

		private void UpdateInvitesSent(List<Meeting> meetings) {
			if (meetings.Any(m => m.InvitesSent)) {
				foreach (var meeting in meetings) {
					meeting.InvitesSent = true;
				}
			}
		}

		/// </summary>
		/// Add participants to <paramref name="meeting"/>.
		/// </summary>
		/// <param name="participants">List of <see cref="Meeting"/> instances.</param>
		/// <param name="participant"><see cref="Participant"/> instance.</param>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		private void AddParticipantsToMeeting(List<Participant> participants, Participant participant, Meeting meeting) {
			if (meeting.Organizer?.Id == participant.Contact.Id) {
				meeting.AddParticipants(participants);
			} else {
				meeting.AddParticipant(participant);
			}
		}

		/// <summary>
		/// Fill <see cref="Meeting"/> calendar properties.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="participant"><see cref="Participant"/> instance.</param>
		private void FillCalendarProperties(Meeting meeting, Participant participant, Entity metadata = null) {
			FillCalendar(meeting, participant);
			if (metadata != null || TryFetchMetadata(meeting, out metadata)) {
				SetExternalStoreProperties(meeting, metadata);
			}
		}

		/// <summary>
		/// Set integration identifiers to meeting.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="metaData">Meeting metadata <see cref="Entity"/> instance.</param>
		private void SetExternalStoreProperties(Meeting meeting, Entity metaData) {
			var icalUid = metaData.GetTypedColumnValue<string>("RemoteId");
			var extraParameters = metaData.GetTypedColumnValue<string>("ExtraParameters");
			var remoteId = GetExtraparametersPropertyValue<string>(extraParameters, "RemoteId");
			meeting.SetIntegrationsId(new IntegrationId(remoteId, icalUid));
			meeting.SetHash(GetExtraparametersPropertyValue<string>(extraParameters, "ActivityHash"));
			meeting.SetNumberOfParticipants(GetExtraparametersPropertyValue<int>(extraParameters, "NumberOfParticipants"));
			meeting.InvitesSent = GetExtraparametersPropertyValue<bool>(extraParameters, "InvitesSent");
			if(meeting.StartDate == DateTime.MinValue) {
				meeting.StartDate = GetExtraparametersPropertyValue<DateTime>(extraParameters, "StartDate");
			}
			meeting.State = (SyncState)metaData.GetTypedColumnValue<int>("RemoteState");
		}

		/// <summary>
		/// Fill <see cref="Meeting"/> calendar related properties.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="participant"><see cref="Participant"/> instance.</param>
		private void FillCalendar(Meeting meeting, Participant participant) {
			var participantCalendar = _calendarRepository.GetCalendar(participant.Contact.Id, _log?.SessionId);
			meeting.Calendar = participantCalendar;
		}

		/// <summary>
		/// Get extra parameters <paramref name="propertyName"/> value.
		/// </summary>
		/// <param name="rawExtraParameters">Extra parameters string.</param>
		/// <param name="propertyName">Property name.</param>
		/// <returns><typeparamref name="T"/> extra parameter value.</returns>
		private T GetExtraparametersPropertyValue<T>(string rawExtraParameters, string propertyName) {
			if(string.IsNullOrEmpty(rawExtraParameters)) {
				return default;
			}
			JObject extraParams = JObject.Parse(rawExtraParameters);
			if (!extraParams.ContainsKey(propertyName)) {
				return default;
			}
			switch (typeof(T).Name) {
				case "Guid":
					return (T)(object)Guid.Parse(extraParams[propertyName].ToString());
				case "DateTime":
					return (T)(object)DateTime.Parse(extraParams[propertyName].ToString());
				case "Boolean":
					return (T)(object)bool.Parse(extraParams[propertyName].ToString());
				case "Int32":
					return (T)(object)int.Parse(extraParams[propertyName].ToString());
				default:
					return (T)(object)extraParams[propertyName].ToString();
			}
		}

		/// <summary>
		/// Fetch meeting <see cref="Entity"/> instance.
		/// </summary>
		/// <param name="meetingId">Meeting unique identifier.</param>
		/// <param name="meetingEntity">Meeting <see cref="Entity"/> instance.</param>
		/// <returns><c>True</c> if activity fetched, <c>false</c> otherwise.</returns>
		private bool TryFetchMeetingEntity(Guid meetingId, out Entity meetingEntity) {
			var schema = _userConnection.EntitySchemaManager.GetInstanceByName("Activity");
			meetingEntity = schema.CreateEntity(_userConnection);
			if (!meetingEntity.FetchFromDB(meetingId)) {
				meetingEntity = null;
				return false;
			}
			var activityTypeId = meetingEntity.GetTypedColumnValue<Guid>("TypeId");
			var showInCalendar = meetingEntity.GetTypedColumnValue<bool>("ShowInScheduler");
			if (!showInCalendar || activityTypeId == IntegrationConsts.EmailTypeId) {
				meetingEntity = null;
				return false;
			}
			return true;
		}

		/// <summary>
		/// Gets internal meetings for calendar.
		/// </summary>
		/// <param name="contactId">User contact identifier.</param>
		/// <param name="sinceDate">Since date.</param>
		/// <param name="dueDate">Due date.</param>
		/// <returns>Meeting <see cref="Entity"/> instances for <paramref name="calendar"/></returns>
		private EntityCollection GetInternalCalendarMeetings(Guid contactId, DateTime sinceDate, DateTime dueDate = default(DateTime)) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "Activity");
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			esq.AddColumn("Title");
			esq.AddColumn("StartDate").OrderByAsc();
			esq.AddColumn("DueDate");
			esq.AddColumn("Location");
			esq.AddColumn("Notes");
			esq.AddColumn("Priority");
			esq.AddColumn("RemindToOwner");
			esq.AddColumn("RemindToOwnerDate");
			esq.AddColumn("ModifiedOn");
			esq.AddColumn("Status");
			esq.AddColumn("Organizer");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "[ActivityParticipant:Activity:Id].Participant.Id",
				contactId));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "ShowInScheduler", true));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.NotEqual, "Type", IntegrationConsts.EmailTypeId));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.GreaterOrEqual, "StartDate", sinceDate));
			if (dueDate != default(DateTime)) {
				esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.LessOrEqual, "StartDate", dueDate));
			}
			return esq.GetEntityCollection(_userConnection);
		}

		/// <summary>
		/// Fetch meeting <see cref="Entity"/> instance.
		/// </summary>
		/// <param name="iCalUid">External meeting unique identifier.</param>
		/// <returns>Meeting <see cref="Entity"/> instance.</returns>
		private Entity FetchMeetingEntity(string iCalUid) {
			var select = new Select(_userConnection)
						.Top(1)
					.Column("LocalId")
					.From("SysSyncMetaData")
					.Where("RemoteId").IsEqual(Column.Parameter(iCalUid))
					.And("SyncSchemaName").IsEqual(Column.Parameter("Activity")).And().Exists(
						new Select(_userConnection)
							.Column("Id")
						.From("Activity")
						.Where("SysSyncMetaData", "LocalId").IsEqual("Activity", "Id")) as Select;
			var activityId = select.ExecuteScalar<Guid>();
			if (activityId.IsEmpty()) {
				return null;
			}
			TryFetchMeetingEntity(activityId, out Entity activity);
			return activity;
		}

		/// <summary>
		/// Fetch metadata <see cref="Entity"/>.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="metaData">Meeting metadata <see cref="Entity"/> instance.</param>
		/// <returns>Metadata <see cref="Entity"/>.</returns>
		private bool TryFetchMetadata(Meeting meeting, out Entity metadata) {
			var schema = _userConnection.EntitySchemaManager.GetInstanceByName("SysSyncMetaData");
			metadata = schema.CreateEntity(_userConnection);
			return metadata.FetchFromDB(new Dictionary<string, object> {
					{ "CreatedBy", meeting.Calendar?.Owner.Id },
					{ "LocalId", meeting.Id },
				}, false);
		}

		/// <summary>
		/// Get all metadata entities by <paramref name="meetingId"/>.
		/// </summary>
		/// <param name="meetingId">Meeting unique identifier.</param>
		/// <returns>All metadata entities by <paramref name="meetingId"/></returns>
		private List<Entity> GetAllMetaDatas(Guid meetingId) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "SysSyncMetaData");
			esq.UseAdminRights = false;
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			esq.AddColumn("CreatedBy");
			esq.AddColumn("RemoteId");
			esq.AddColumn("ExtraParameters");
			esq.AddColumn("RemoteState");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "LocalId", meetingId));
			return esq.GetEntityCollection(_userConnection).ToList();
		}

		/// <summary>
		/// Fill metadata entity.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="metaData">Meeting metadata <see cref="Entity"/> instance.</param>
		private void FillExtraParameters(Meeting meeting, Entity metadata) {
			var extraParameters = new JObject();
			if (metadata.GetColumnValueNames().Contains("ExtraParameters")) {
				var rawMetadata = metadata.GetTypedColumnValue<string>("ExtraParameters");
				if (rawMetadata.IsNotNullOrEmpty()) {
					extraParameters = JObject.Parse(rawMetadata);
				}
			}
			extraParameters["RemoteId"] = meeting.RemoteId;
			extraParameters["ActivityHash"] = meeting.GetActualHash();
			extraParameters["StatusId"] = meeting.StatusId;
			extraParameters["StartDate"] = meeting.StartDate;
			extraParameters["EndDate"] = meeting.DueDate;
			extraParameters["Title"] = meeting.Title;
			extraParameters["InvitesSent"] = meeting.InvitesSent;
			extraParameters["NumberOfParticipants"] = meeting.Participants.Count;
			metadata.SetColumnValue("ExtraParameters", Json.Serialize(extraParameters));
		}

		/// <summary>
		/// Save activity metadata entity.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		private void SaveMetadata(Meeting meeting) {
			SaveMetadata(meeting, SyncState.Modified);
		}

		/// <summary>
		/// Save activity metadata entity.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		private void SaveMetadata(Meeting meeting, SyncState state) {
			if (TryFetchMetadata(meeting, out Entity metadata)) {
				metadata.SetColumnValue("LocalState", state);
				metadata.SetColumnValue("RemoteState", state);
			} else {
				metadata.SetDefColumnValues();
				metadata.SetColumnValue("CreatedById", meeting.Calendar.Owner.Id);
				metadata.SetColumnValue("LocalId", meeting.Id);
				metadata.SetColumnValue("SyncSchemaName", "Activity");
				metadata.SetColumnValue("RemoteItemName", "ExchangeAppointment");
				metadata.SetColumnValue("SchemaOrder", 0);
				metadata.SetColumnValue("LocalState", SyncState.New);
				metadata.SetColumnValue("RemoteState", SyncState.New);
				metadata.SetColumnValue("CreatedInStoreId", ExchangeConsts.LocalStoreId);
				metadata.SetColumnValue("ModifiedInStoreId", ExchangeConsts.LocalStoreId);
				metadata.SetColumnValue("RemoteStoreId", ExchangeConsts.AppointmentStoreId);
				metadata.SetColumnValue("RemoteId", meeting.ICalUid);
			}
			FillExtraParameters(meeting, metadata);
			metadata.SetColumnValue("Version", meeting.ModifiedOn);
			metadata.Save();
			_log?.LogDebug($"Meeting metadata '{metadata.PrimaryColumnValue}' saved for meeting {meeting}.");
			if (metadata.GetTypedColumnValue<SyncState>("LocalState") == SyncState.New) {
				_participantRepository.Save(meeting);
			}
		}

		/// <summary>
		/// Save activity entity.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		private EntityChangeType SaveActivity(Meeting meeting) {
			if (!meeting.IsChanged(true)) {
				_log?.LogDebug($"Activity is not changed, save skipped {meeting}.");
				return EntityChangeType.None;
			}
			var schema = _userConnection.EntitySchemaManager.GetInstanceByName("Activity");
			var activity = schema.CreateEntity(_userConnection);
			if (!activity.FetchFromDB(meeting.Id)) {
				activity.SetDefColumnValues();
				activity.PrimaryColumnValue = meeting.Id;
				var statusId = meeting.DueDate > _userConnection.CurrentUser.GetCurrentDateTime()
					? _notStartedStatusId
					: _completedStatusId;
				activity.SetColumnValue("StatusId", statusId);
				if (meeting.Organizer != null) {
					activity.SetColumnValue("OrganizerId", meeting.Organizer.Id);
				} else {
					activity.SetColumnValue("OrganizerId", null);
				}
			}
			if (meeting.RemoteCreatedOn != DateTime.MinValue) {
				activity.SetColumnValue("RemoteCreatedOn", meeting.RemoteCreatedOn);
			}
			activity.SetColumnValue("Title", meeting.Title);
			activity.SetColumnValue("Location", meeting.Location);
			activity.SetColumnValue("Notes", meeting.Body);
			activity.SetColumnValue("ShowInScheduler", true);
			activity.SetColumnValue("StartDate", meeting.StartDate);
			activity.SetColumnValue("DueDate", meeting.DueDate);
			activity.SetColumnValue("ModifiedById", _userConnection.CurrentUser.ContactId);
			activity.SetColumnValue("PriorityId", meeting.PriorityId);
			activity.SetColumnValue("RemindToOwner", meeting.RemindToOwner);
			if (meeting.RemindToOwner) {
				activity.SetColumnValue("RemindToOwnerDate", meeting.RemindToOwnerDate);
			} else {
				activity.SetColumnValue("RemindToOwnerDate", null);
			}
			var changeType = activity.ChangeType;
			activity.Save();
			_log?.LogDebug($"Activity saved {meeting}.");
			return changeType;
		}

		private void SendRecordChange(Meeting meeting, EntityChangeType state) {
			if (meeting.Calendar != null) {
				_recordOperationsNotificator.SendRecordChange(meeting.Calendar.Owner.Id, "Activity", meeting.Id, state);
				_log?.LogDebug($"Message to update client UI sent '{EntityChangeType.Updated}' to '{meeting.Calendar.Owner}' for {meeting}.");
			}
		}

		private void DeleteMetadata(Meeting meeting) {
			if (TryFetchMetadata(meeting, out _)) {
				var participantEmail = meeting.Calendar.Settings.SenderEmailAddress;
				_participantRepository.Delete(meeting.Id);
				_log?.LogInfo($"Participant {participantEmail} deleted from meeting {meeting}.");
				SaveMetadata(meeting, SyncState.Deleted);
			}
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="IMeetingRepository.GetMeetings(Guid)"/>
		public List<Meeting> GetMeetings(Guid meetingId) {
			var meetings = new List<Meeting>();
			if (TryFetchMeetingEntity(meetingId, out Entity meetingEntity)) {
				meetings = GetMeetingsInternal(meetingEntity);
			} else { 
				_log?.LogWarn($"Insufficient permissions to read Activity {meetingId}");
			}
			return meetings;
		}

		/// <inheritdoc cref="IMeetingRepository.GetMeetings(Guid, DateTime)"/>
		public List<Meeting> GetMeetings(Guid contactId, DateTime syncSinceDate) {
			var meetings = new List<Meeting>();
			var meetingsEntities = GetInternalCalendarMeetings(contactId, syncSinceDate);
			foreach (var meetingEntity in meetingsEntities) {
				meetings.AddRange(GetMeetingsInternal(meetingEntity));
			}
			return meetings;
		}

		/// <inheritdoc cref="IMeetingRepository.GetMeetings(Guid, DateTime, DateTime)"/>
		public List<Meeting> GetMeetings(Guid contactId, DateTime sinceDate, DateTime dueDate) {
			var meetings = new List<Meeting>();
			var meetingsEntities = GetInternalCalendarMeetings(contactId, sinceDate, dueDate);
			foreach (var meetingEntity in meetingsEntities) {
				meetings.AddRange(GetMeetingsInternal(meetingEntity));
			}
			return meetings;
		}
		

		/// <inheritdoc cref="IMeetingRepository.GetMeetings(string)"/>
		public List<Meeting> GetMeetings(string iCalUid) {
			var meetings = new List<Meeting>();
			var meetingEntity = FetchMeetingEntity(iCalUid);
			if (meetingEntity != null) {
				meetings = GetMeetingsInternal(meetingEntity);
			}
			return meetings.Where(m => m.ICalUid == iCalUid || m.ICalUid.IsNullOrEmpty()).ToList();
		}

		/// <inheritdoc cref="IMeetingRepository.GetDeletedMeetings(SyncSessionArguments)"/>
		public List<Meeting> GetDeletedMeetings(Guid meetingId) {
			var mettings = new List<Meeting>();
			var metadatas = GetAllMetaDatas(meetingId);
			foreach (var metadata in metadatas) {
				var meeting = new Meeting(meetingId, _log?.SessionId);
				var participant = _participantRepository.GetParticipant(meetingId, new Contact(
					metadata.GetTypedColumnValue<Guid>("CreatedById"),
					metadata.GetTypedColumnValue<string>("CreatedByName")
				));
				FillCalendarProperties(meeting, participant, metadata);
				mettings.Add(meeting);
			}
			return mettings;
		}

		/// <inheritdoc cref="IMeetingRepository.Save(Meeting)"/>
		public void Save(Meeting meeting) {
			SaveMetadata(meeting);
			var state = SaveActivity(meeting);
			SendRecordChange(meeting, state);
		}

		/// <inheritdoc cref="IMeetingRepository.Delete(Meeting)"/>
		public void Delete(Meeting meeting) {
			DeleteMetadata(meeting);
			if (TryFetchMeetingEntity(meeting.Id, out Entity activity)) {
				activity.Delete();
			}
		}

		/// <inheritdoc cref="IMeetingRepository.RemoveFromCalendar(Meeting)"/>
		public void RemoveFromCalendar(Meeting meeting) {
			meeting.Calendar?.RemoveMeeting(meeting, _userConnection);
			DeleteMetadata(meeting);
		}

		#endregion

	}

	#endregion

}
