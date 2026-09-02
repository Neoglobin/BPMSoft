namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Repository
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Logger;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using BPMSoft.Configuration;
	using BPMSoft.Core;
	using BPMSoft.Core.Factories;
	using BPMSoft.Core.Entities;
	using BPMSoft.Common;
	using BPMSoft.Core.DB;
	using BPMSoft.Sync;

	#region Class: ParticipantRepository

	/// <summary>
	/// <see cref="IParticipantRepository"/> implementation.
	/// </summary>
	[DefaultBinding(typeof(IParticipantRepository))]
	public class ParticipantRepository : IParticipantRepository
	{

		#region Fields: Private

		/// <summary>
		/// <see cref="UserConnection"/> instance.
		/// </summary>
		private readonly UserConnection _userConnection;

		/// <summary>
		/// <see cref="ICalendarRepository"/> implementation instance.
		/// </summary>
		private readonly ICalendarRepository _calendarRepository;

		/// <summary>
		/// <see cref="IRecordOperationsNotificator"/> implementation instance.
		/// </summary>
		private readonly IRecordOperationsNotificator _recordOperationsNotificator;

		/// <summary>
		/// <see cref="ICalendarLogger"/> instance.
		/// </summary
		private readonly ICalendarLogger _log;

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public ParticipantRepository(UserConnection uc) {
			_userConnection = uc;
			_calendarRepository = ClassFactory.Get<ICalendarRepository>(new ConstructorArgument("uc", uc));
			_recordOperationsNotificator = ClassFactory.Get<IRecordOperationsNotificator>(
				new ConstructorArgument("userConnection", uc));
		}

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public ParticipantRepository(UserConnection uc, string sessionId): this(uc) {
			_log = ClassFactory.Get<ICalendarLogger>(new ConstructorArgument("sessionId", sessionId),
				new ConstructorArgument("modelName", GetType().Name));
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Get activity participants <see cref="EntityCollection"/> by <paramref name="meetingId"/>.
		/// </summary>
		/// <param name="meetingId"><see cref="Meeting"/> instance identifier.</param>
		/// <returns>Activity participants <see cref="EntityCollection"/>.</returns>
		private EntityCollection GetActivityParticipantEntities(Guid meetingId) {
			var esq = GetAllParticipantEsq();
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Activity", meetingId));
			return esq.GetEntityCollection(_userConnection);
		}

		/// <summary>
		/// Get <see cref="EntitySchemaQuery"/> of all participant.
		/// </summary>
		/// <param name="allColumns">All columns need to be added.</param>
		/// <returns><see cref="EntitySchemaQuery"/> of all participant.</returns>
		private EntitySchemaQuery GetAllParticipantEsq(bool allColumns = false) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "ActivityParticipant");
			if (allColumns) {
				esq.AddAllSchemaColumns();
			} else {
				esq.PrimaryQueryColumn.IsAlwaysSelect = true;
				esq.AddColumn("Participant");
				esq.AddColumn("InviteResponse");
				esq.AddColumn("InviteParticipant");
				esq.AddColumn("Role");
				esq.AddColumn("Activity");
				esq.AddColumn("Participant.Email").Name = "Email";
			}
			return esq;
		}

		/// <summary>
		/// Create <see cref="Participant"/> model.
		/// </summary>
		/// <param name="activityParticipantEntity">Activity participant <see cref="Entity"/> instance.</param>
		/// <returns><see cref="Participant"/> model.</returns>
		private Participant CreateParticipantModel(Entity activityParticipantEntity) {
			if (activityParticipantEntity == null) {
				return default;
			}
			var participant = new Participant(activityParticipantEntity);
			var calendar = _calendarRepository.GetCalendar(participant.Contact.Id);
			var email = calendar == null
				? activityParticipantEntity.GetTypedColumnValue<string>("Email")
				: calendar.Settings.SenderEmailAddress;
			participant.EmailAddress = email;
			return participant;
		}

		/// <summary>
		/// Returns contact identifiers list containing list of <paramref name="emails"/>.
		/// </summary>
		/// <param name="emails"> A list of email addresses.</param>
		/// <returns>List of contacts.</returns>
		private List<Contact> GetContactsByEmails(List<string> emails) {
			var contacts = new List<Contact>();
			var searchEmails = emails.Select(e => e.Trim().ToLower()).Where(e => e.IsNotNullOrEmpty());
			if (!searchEmails.Any()) {
				return contacts;
			}
			var select = new Select(_userConnection)
					.Column("ContactCommunication", "ContactId")
					.Column("ContactCommunication", "SearchNumber").As("EmailAddress")
					.Column("Contact", "Name").As("ContactName")
				.From("ContactCommunication")
					.InnerJoin("Contact").On("Contact", "Id").IsEqual("ContactCommunication", "ContactId")
				.Where("SearchNumber").In(Column.Parameters(searchEmails))
				.And("CommunicationTypeId").IsEqual(Column.Parameter(Guid.Parse("ee1c85c3-cfcb-df11-9b2a-001d60e938c6")))
				.And().OpenBlock("SearchNumber").IsNotEqual(Column.Parameter(string.Empty))
					.Or("SearchNumber").Not().IsNull()
					.CloseBlock() as Select;
			using (DBExecutor dbExecutor = _userConnection.EnsureDBConnection()) {
				using (var reader = select.ExecuteReader(dbExecutor)) {
					while (reader.Read()) {
						contacts.Add(new Contact(
							reader.GetColumnValue<Guid>("ContactId"),
							reader.GetColumnValue<string>("ContactName"),
							reader.GetColumnValue<string>("EmailAddress")
						));
					}
				}
			}
			return contacts;
		}

		/// <summary>
		/// Add new participant metadata.
		/// </summary>
		/// <param name="participantId"><see cref="Participant"/> unique identifier.</param>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="parentMetadata">Meeting metadta <see cref="Entity"/>.</param>
		private void AddParticipantMetadata(Guid participantId, Meeting meeting, Entity parentMetadata = null) {
			var schema = _userConnection.EntitySchemaManager.GetInstanceByName("SysSyncMetaData");
			var metadata = schema.CreateEntity(_userConnection);
			var entityCreatedBy = parentMetadata?.GetTypedColumnValue<Guid>("CreatedById") ?? Guid.Empty;
			var entityRemoteId = parentMetadata?.GetTypedColumnValue<string>("RemoteId");
			var createdBy = entityCreatedBy == Guid.Empty ? meeting.Calendar?.Owner.Id : entityCreatedBy;
			var remoteId = entityRemoteId.IsNullOrEmpty() ? meeting.ICalUid : entityRemoteId;
			metadata.SetDefColumnValues();
			metadata.SetColumnValue("CreatedById", createdBy);
			metadata.SetColumnValue("ModifiedById", createdBy);
			metadata.SetColumnValue("LocalId", participantId);
			metadata.SetColumnValue("SyncSchemaName", "ActivityParticipant");
			metadata.SetColumnValue("RemoteItemName", "ExchangeAppointment");
			metadata.SetColumnValue("Version", meeting.ModifiedOn);
			metadata.SetColumnValue("SchemaOrder", 1);
			metadata.SetColumnValue("LocalState", SyncState.New);
			metadata.SetColumnValue("CreatedInStoreId", ExchangeConsts.LocalStoreId);
			metadata.SetColumnValue("ModifiedInStoreId", ExchangeConsts.LocalStoreId);
			metadata.SetColumnValue("RemoteStoreId", ExchangeConsts.AppointmentStoreId);
			metadata.SetColumnValue("RemoteId", remoteId);
			metadata.Save();
			_log?.LogDebug($"Participant '{participantId}' meatadata '{metadata.PrimaryColumnValue}' added {meeting}.");
		}

		/// <summary>
		/// Fetch meeting metadata entities.
		/// </summary>
		/// <param name="meetingId"></param>
		/// <returns>Entity collection of <see cref="Meeting"/> metadata <see cref="Entity"/>.</returns>
		private EntityCollection FetchParentMetadatas(Guid meetingId) {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "SysSyncMetaData");
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			esq.AddColumn("RemoteId");
			esq.AddColumn("CreatedBy");
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "LocalId", meetingId));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "SyncSchemaName", "Activity"));
			return esq.GetEntityCollection(_userConnection);
		}

		/// <summary>
		/// Add participants metadatas to parent <paramref name="meeting"/> metadata.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="participantId"><see cref="Participant"/> unique identifier.</param>
		private void ActualizeParticipantsForParentMetedata(Meeting meeting, Guid participantId) {
			var missingParticipants = GetActivityParticipantEntities(meeting.Id).Where(p => p.PrimaryColumnValue != participantId);
			foreach (var missingParticipant in missingParticipants) {
				AddParticipantMetadata(missingParticipant.PrimaryColumnValue, meeting);
			}
		}

		/// <summary>
		/// Add participant metadata to each parents <paramref name="meeting"/> metadatas.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="participantId"><see cref="Participant"/> unique identifier.</param>
		private void AddParticipantToParentsMetadatas(Meeting meeting, Guid participantId) {
			var adllParentsMetadatas = FetchParentMetadatas(meeting.Id);
			foreach (var metadata in adllParentsMetadatas) {
				AddParticipantMetadata(participantId, meeting, metadata);
			}
		}

		private void UpdateParticipants(List<Participant> actualParticipants, EntityCollection existingParticipants) {
			var toUpdate = existingParticipants
				.Where(ep => actualParticipants.Any(ap =>
					ep.GetTypedColumnValue<Guid>("ParticipantId").Equals(ap.Contact.Id) &&
					!ep.GetTypedColumnValue<Guid>("InviteResponseId").Equals(ap.InviteResponseId)))
				.ToList();
			foreach (var entity in toUpdate) {
				var participantId = entity.GetTypedColumnValue<Guid>("ParticipantId");
				var participant = actualParticipants.Find(act => act.Contact.Id == participantId);
				if (participant.InviteResponseId != Guid.Empty) {
					entity.SetColumnValue("InviteResponseId", participant.InviteResponseId);
				} else {
					entity.SetColumnValue("InviteResponseId", null);
				}
				entity.SetColumnValue("InviteParticipant", participant.IsInvited);
				entity.Save();
				_log?.LogDebug($"Update invite for participant '{entity.PrimaryColumnValue}' {participant.Contact} to meeting {participant.MeetingId}.");
			}
		}

		private void CreateParticipants(List<Participant> actualParticipants, Guid meetingId, EntityCollection existingParticipants) {
			var toCreate = actualParticipants
				.Where(ap => !existingParticipants.Any(ep => ep.GetTypedColumnValue<Guid>("ParticipantId").Equals(ap.Contact.Id)))
				.ToList();
			var schema = _userConnection.EntitySchemaManager.GetInstanceByName("ActivityParticipant");
			foreach (var newParticipant in toCreate) {
				var entity = schema.CreateEntity(_userConnection);
				entity.SetDefColumnValues();
				entity.SetColumnValue("ParticipantId", newParticipant.Contact.Id);
				entity.SetColumnValue("ActivityId", newParticipant.MeetingId);
				if (newParticipant.InviteResponseId != Guid.Empty) {
					entity.SetColumnValue("InviteResponseId", newParticipant.InviteResponseId);
					entity.SetColumnValue("InviteParticipant", newParticipant.IsInvited);
				}
				entity.Save();
				_log?.LogDebug($"Add participant '{entity.PrimaryColumnValue}' {newParticipant.Contact} to meeting {newParticipant.MeetingId}.");
			}
			var contactIds = toCreate.Select(p => p.Contact.Id).ToList();
			_recordOperationsNotificator.SendRecordChange(contactIds, "Activity", meetingId, EntityChangeType.Updated);
			_log?.LogDebug($"Message to update client UI sent '{EntityChangeType.Updated}' " +
				$"to contacts '{string.Join(", ", contactIds)}' for meeting {meetingId}.");
		}

		private void DeleteParticipants(List<Participant> actualParticipants, Guid meetingId, EntityCollection existingParticipants) {
			var toDelete = existingParticipants
				.Where(ep => !actualParticipants.Any(ap => ep.GetTypedColumnValue<Guid>("ParticipantId").Equals(ap.Contact.Id)))
				.ToList();
			foreach (var participant in toDelete) {
				participant.Delete();
				_log?.LogDebug($"Delete participant {participant.GetTypedColumnValue<string>("Email")} from meeting {meetingId}.");
			}
			var contactIds = toDelete.Select(p => p.GetTypedColumnValue<Guid>("ParticipantId")).ToList();
			_recordOperationsNotificator.SendRecordChange(contactIds, "Activity", meetingId, EntityChangeType.Deleted);
			_log?.LogDebug($"Message to update client UI sent '{EntityChangeType.Deleted}' " +
				$"to contacts '{string.Join(", ", contactIds)}' for meeting {meetingId}.");
		}
		#endregion

		#region Methods: Public

		/// <inheritdoc cref="IParticipantRepository.GetMeetingParticipants(Guid)"/>
		public List<Participant> GetMeetingParticipants(Guid meetingId) {
			var activityParticipantsEntities = GetActivityParticipantEntities(meetingId);
			var result = new List<Participant>();
			foreach (var activityParticipantEntity in activityParticipantsEntities) {
				result.Add(CreateParticipantModel(activityParticipantEntity));
			}
			return result;
		}

		/// <inheritdoc cref="IParticipantRepository.GetParticipant(Guid)"/>
		public Participant GetParticipant(Guid participantId) {
			var esq = GetAllParticipantEsq();
			var activityParticipantEntity = esq.GetEntity(_userConnection, participantId);
			return CreateParticipantModel(activityParticipantEntity);
		}

		/// <inheritdoc cref="IParticipantRepository.GetParticipant(Guid, Contact)"/>
		public Participant GetParticipant(Guid meetingId, Contact contact) {
			return new Participant(contact, meetingId);
		}

		/// <inheritdoc cref="IParticipantRepository.GetParticipantContacts(List{string})"/>
		public List<Contact> GetParticipantContacts(List<string> emails) {
			var result = new List<Contact>();
			var contactEmails = new List<string>();
			foreach (var email in emails) {
				var calendar = _calendarRepository.GetCalendar(email);
				if (calendar != null) {
					result.AddIfNotExists(calendar.Owner);
				} else {
					contactEmails.AddIfNotExists(email);
				}
			}
			result.AddRangeIfNotExists(GetContactsByEmails(contactEmails));
			return result;
		}

		/// <inheritdoc cref="IParticipantRepository.GetParticipants(Guid, List{string}"/>
		public List<Participant> GetParticipants(Guid meetingId, List<string> emails) {
			var participants = new List<Participant>();
			var participantContacts = GetParticipantContacts(emails);
			foreach (var participantContact in participantContacts) {
				participants.Add(GetParticipant(meetingId, participantContact));
			}
			return participants;
		}

		/// <inheritdoc cref="IParticipantRepository.UpdateMeetingParticipants(List{Participant})"/>
		public void UpdateMeetingParticipants(List<Participant> actualParticipants) {
			var esq = GetAllParticipantEsq();
			var meetingId = actualParticipants.First().MeetingId;
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.Equal, "Activity", meetingId));
			var existingParticipants = esq.GetEntityCollection(_userConnection);
			DeleteParticipants(actualParticipants, meetingId, existingParticipants);
			UpdateParticipants(actualParticipants, existingParticipants);
			CreateParticipants(actualParticipants, meetingId, existingParticipants);
		}

		/// <inheritdoc cref="IParticipantRepository.UpdateParticipantInvitation(Participant)"/>
		public void UpdateParticipantInvitation(Participant participant) {
			var esq = GetAllParticipantEsq(true);
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.NotEqual,
				"InviteResponse", participant.InviteResponseId));
			esq.Filters.Add(esq.CreateFilterWithParameters(FilterComparisonType.NotEqual,
				"InviteParticipant", participant.IsInvited));
			var participantEntity = esq.GetEntity(_userConnection, participant.Id);
			if (participantEntity != null) {
				participantEntity.SetColumnValue("InviteResponseId", participant.InviteResponseId);
				participantEntity.SetColumnValue("InviteParticipant", participant.IsInvited);
				participantEntity.Save(false);
			}
		}

		/// <inheritdoc cref="IParticipantRepository.Delete(Guid)"/>
		public void Delete(Guid meetingId) {
			var parentRemote = FetchParentMetadatas(meetingId).Select(m => m.GetTypedColumnValue<string>("RemoteId"));
			var delete = new Delete(_userConnection)
					.From("SysSyncMetaData")
					.Where("SyncSchemaName").IsEqual(Column.Parameter("ActivityParticipant"))
					.And("RemoteId").In(Column.Parameters(parentRemote)) as Delete;
			delete.Execute();
		}

		/// <inheritdoc cref="IParticipantRepository.Save(Guid, Meeting)"/>
		public void Save(Meeting meeting) {
			var participant = meeting.Participants.Where(p => p.Contact.Id == meeting.Calendar?.Owner.Id).FirstOrDefault();
			if (participant != null) {
				AddParticipantToParentsMetadatas(meeting, participant.Id);
				ActualizeParticipantsForParentMetedata(meeting, participant.Id);
			}
		}

		#endregion

	}

	#endregion

}
