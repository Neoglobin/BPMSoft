namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Synchronization {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using IntegrationApi.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Client;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.DTO;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Logger;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Utils;
	using IntegrationV2.Files.cs.Utils;
	using BPMSoft.Common;
	using BPMSoft.Configuration;
	using BPMSoft.Core;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Factories;
	using BPMSoft.Core.Tasks;
	using SyncState = BPMSoft.Sync.SyncState;

	#region Class: CalendarSyncSession

	/// <summary>
	/// Calendar <see cref="ISyncSession"/> implementation.
	/// </summary>
	[DefaultBinding(typeof(ISyncSession), Name = "Calendar")]
	public class CalendarSyncSession : IBackgroundTask<SyncSessionArguments>, IUserConnectionRequired, ISyncSession {

		#region Fields: Private

		/// <summary>
		/// <see cref="ICalendarLogger"/> instance.
		/// </summary
		private ICalendarLogger _log;

		/// <summary>
		/// <see cref="UserConnection"/> instance.
		/// </summary
		private UserConnection _uc;

		/// <summary>
		/// <see cref="ICalendarRepository"/> implementation instance.
		/// </summary>
		private ICalendarRepository _calendarRepository;

		/// <summary>
		/// <see cref="IMeetingRepository"/> implementation instance.
		/// </summary>
		private IMeetingRepository _meetingRepository;

		/// <summary>
		/// <see cref="IParticipantRepository"/> implementation instance.
		/// </summary>
		private IParticipantRepository _participantRepository;

		/// <summary>
		/// <see cref="IEntitySynchronizerHelper"/> implementation instance.
		/// </summary>
		private IEntitySynchronizerHelper _syncHelper;

		/// <summary>
		/// <see cref="IActivityUtils"/> implementation instance.
		/// </summary>
		private IActivityUtils _utils;

		/// <summary>
		/// <see cref="IRecordOperationsNotificator"/> implementation instance.
		/// </summary>
		private IRecordOperationsNotificator _recordOperationsNotificator;

		private const string _integrationSystemName = "NewCalendarSync";

		#endregion

		#region Constructors: Public

		public CalendarSyncSession() {
		}

		public CalendarSyncSession(UserConnection uc) {
			SetDependencies(uc);
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Set dependendcies.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		private void SetDependencies(UserConnection uc) {
			_uc = uc;
			_recordOperationsNotificator = ClassFactory.Get<IRecordOperationsNotificator>(
				new ConstructorArgument("userConnection", uc));
			_calendarRepository = ClassFactory.Get<ICalendarRepository>(new ConstructorArgument("uc", uc));
			_participantRepository = ClassFactory.Get<IParticipantRepository>(new ConstructorArgument("uc", uc));
			_syncHelper = ClassFactory.Get<IEntitySynchronizerHelper>();
			_utils = ClassFactory.Get<IActivityUtils>();
		}

		private void ExportMeeting(SyncSessionArguments options) {
			var meetings = GetMeetingsForExport(options).Where(m => m.State != SyncState.Deleted);
			foreach (var meeting in meetings) {
				if (GetIsItemLockedForSync(meeting)) {
					_log?.LogInfo($"Meeting item is locked, item skipped {meeting} in calendar '{meeting.Calendar}'.");
					continue;
				}
				if (options.Contacts.Count < 2 && options.SyncAction != SyncAction.Delete && !CanCreateEntityInRemoteStore(meeting)) {
					_log?.LogInfo($"Meeting item is locked for export, item skipped {meeting} in calendar '{meeting.Calendar}'.");
					continue;
				}
				switch (options.SyncAction) {
					case SyncAction.UpdateWithInvite:
					case SyncAction.ExportPeriod:
					case SyncAction.CreateOrUpdate:
						if (meeting.IsChanged()) {
							if (!meeting.InSyncPeriod()) {
								_log?.LogInfo($"Meeting being processed outside the sync period, item skipped {meeting} in calendar '{meeting.Calendar}'.");
								break; 
							}
							meeting.IsNeedSendInvitations = options.SyncAction == SyncAction.UpdateWithInvite;
							meeting.AddToCalendar(_uc);
							SendRecordChange(meeting, EntityChangeType.Updated);
						} else {
							_log?.LogInfo($"Meeting is not changed, item skipped {meeting} in calendar '{meeting.Calendar}'.");
						}
						break;
					case SyncAction.Delete:
					case SyncAction.DeleteWithInvite:
						meeting.IsNeedSendInvitations = options.SyncAction == SyncAction.DeleteWithInvite;
						if (meeting.InSyncPeriod()) {
							DeleteMeeting(meeting);
						} else {
							if (options.IsBackgroundProcess) {
								_log?.LogInfo($"Delete meeting is out of sync period from background process, item skipped {meeting} in calendar '{meeting.Calendar}'.");
								break;
							}
							DeleteMeeting(meeting);
						}
						break;
				}
				UnlockItem(meeting);
			}
		}

		/// <summary>
		/// Delete <paramref name="meeting"/>.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		private void DeleteMeeting(Meeting meeting) {
			_meetingRepository.RemoveFromCalendar(meeting);
			SendRecordChange(meeting, EntityChangeType.Deleted);
		}

		private void SendRecordChange(Meeting meeting, EntityChangeType state) {
			if (meeting.Calendar != null) {
				_recordOperationsNotificator.SendRecordChange(meeting.Calendar.Owner.Id,
					"Activity",
					meeting.Id,
					state
				);
				_log?.LogDebug($"Message to update client UI sent '{state}' to '{meeting.Calendar.Owner}' for {meeting}.");
			}
		}

		private void ImportPeriod(SyncSessionArguments options) {
			foreach (var calendarOwner in options.Contacts) {
				var calendar = _calendarRepository.GetCalendar(calendarOwner.Id);
				if (calendar == null || !calendar.Settings.SyncEnabled) {
					continue;
				}
				_log?.LogInfo($"Start import meetings from {calendar}.");
				calendar.Settings.RefreshAccessToken(_uc);
				var calendarClient = CalendarClientFactory.GetCalendarClient(calendar, _log.SessionId);
				var rawData = calendarClient.GetSyncPeriodMeetings(calendar, out bool hasError)
					.Distinct(ComparerUtils.GetComparer<MeetingDto>())
					.ToList();
				var meetings = GetMeetingsForImport(rawData, calendar);
				var localCopies = FilterLocalCopies(meetings);
				var resultMeetings = meetings.Where(m => m.CanBeImported() && !localCopies.Contains(m)).ToList();
				_log?.LogInfo($"Result raw meetings collection for import ({resultMeetings.Count}) => {resultMeetings.GetString()}");
				foreach (var meeting in resultMeetings) {
					try {
						if (LockItemForSync(meeting)) {
							_log.LogInfo($"Start import meeting {meeting}.");
							SaveMeetingToInternalRepository(meeting);
							_log.LogInfo($"Meeting imported successfully {meeting}.");
						} else {
							_log.LogInfo($"Meeting already synced, metting import skipped {meeting}.");
						}
					} catch(Exception e) {
						_log.LogError($"Failed import for meeting {meeting}.", e);
					} finally {
						UnlockItem(meeting);
					}
				}
				foreach(var localCopy in localCopies) {
					_log?.LogDebug($"Delete import meeting {localCopy} started.");
					localCopy.InvitesSent = true;
					_meetingRepository.RemoveFromCalendar(localCopy);
					_log?.LogDebug($"Delete import meeting {localCopy} ended.");
				}
				if (hasError) {
					continue;
				}
				foreach (var deletedMeeting in GetImportedMeetingsForDelete(rawData, calendar)) {
					_meetingRepository.Delete(deletedMeeting);
					SendRecordChange(deletedMeeting, EntityChangeType.Deleted);
				}
			}
		}

		/// <summary>
		/// Get meetings for export for the period for contact.
		/// </summary>
		/// <param name="contact"><see cref="Contact"/> instance.</param>
		/// <returns><see cref="Meeting"/> instance collection.</returns>
		private List<Meeting> GetMeetingsForPeriodExport(Contact contact) {
			var meetings = new List<Meeting>();
			if (contact != null) {
				var calendar = _calendarRepository.GetCalendar(contact.Id);
				if (calendar != null) {
					meetings = _meetingRepository.GetMeetings(contact.Id, calendar.SyncSinceDate);
				}
			}
			return meetings;
		}

		/// <summary>
		/// Get meetings for export.
		/// </summary>
		/// <param name="options"><see cref="SyncSessionArguments"/> instance.</param>
		/// <returns><see cref="Meeting"/> instance collection.</returns>
		private List<Meeting> GetMeetingsForExport(SyncSessionArguments options) {
			List<Meeting> meetings;
			switch (options.SyncAction) {
				case SyncAction.Delete:
				case SyncAction.DeleteWithInvite:
					meetings = _meetingRepository.GetDeletedMeetings(options.MeetingId);
					break;
				case SyncAction.ExportPeriod:
					meetings = GetMeetingsForPeriodExport(options.Contacts.FirstOrDefault());
					break;
				default:
					meetings = _meetingRepository.GetMeetings(options.MeetingId);
					meetings.ForEach(item => item.OldColumnsValues = options.OldColumnsValues);
					break;
			}
			_log?.LogDebug($"Loaded '{meetings.Count}' meetings for export => \r\n{meetings.GetString()}");
			if (options.Contacts.Any()) {
				meetings = meetings.Where(m => m.Calendar != null && options.Contacts.Any(c => c.Id == m.Calendar.Owner.Id)).ToList();
			} else {
				meetings = meetings.Where(m => m.Calendar != null).ToList();
			}
			_log?.LogDebug($"Result meetings count '{meetings.Count}' for export processing => \r\n{meetings.GetString()}");
			return meetings;
		}

		private List<Meeting> GetMeetingsForImport(List<MeetingDto> rawData, Calendar calendar) {
			var result = new List<Meeting>();
			foreach (var rawMeeting in rawData) {
				var allMeetings = _meetingRepository.GetMeetings(rawMeeting.ICalUid);
				var currentCalendarMeetings = allMeetings.Where(m => m.Calendar?.Owner.Id == calendar.Owner.Id).ToList();
				var meetingId = allMeetings.Any()
					? allMeetings.First().Id
					: rawMeeting.Id;
				if (currentCalendarMeetings.Any()) {
					foreach (var existingMeeting in currentCalendarMeetings) {
						FillMeeting(existingMeeting, rawMeeting);
						if (existingMeeting.ICalUid.IsNullOrEmpty()) {
							existingMeeting.SetIntegrationsId(new IntegrationId(rawMeeting.RemoteId, rawMeeting.ICalUid));
						}
						_log?.LogDebug($"Add exist meeting to raw import collection {existingMeeting}.");
						result.Add(existingMeeting);
					}
				} else {
					var newMeeting = new Meeting(meetingId, _log?.SessionId);
					newMeeting.Calendar = calendar;
					newMeeting.SetIntegrationsId(new IntegrationId(rawMeeting.RemoteId, rawMeeting.ICalUid));
					FillMeeting(newMeeting, rawMeeting);
					_log?.LogDebug($"Add new meeting to raw import collection {newMeeting}.");
					result.Add(newMeeting);
				}
			}
			return result;
		}

		private List<Meeting> GetImportedMeetingsForDelete(List<MeetingDto> rawData, Calendar calendar) {
			return _meetingRepository.GetMeetings(calendar.Owner.Id, calendar.SyncSinceDate, calendar.SyncTillDate)
				.Where(m => 
					m.Calendar?.Id == calendar.Id && 
					!rawData.Any(rd => rd.ICalUid == m.ICalUid)
					&& m.State != SyncState.Deleted
					&& m.CanBeExported()
				)
				.ToList();
		}

		private List<Meeting> FilterLocalCopies(List<Meeting> meetings) {
			var invitesSentMeetings = meetings.Where(m => !m.CanBeImported());
			var copiesToDelete = new List<Meeting>();
			foreach(var invitesSentMeeting in invitesSentMeetings) {
				copiesToDelete.AddRangeIfNotExists(
					meetings.Where(m =>
						m.Id == invitesSentMeeting.Id &&
						m.ICalUid != invitesSentMeeting.ICalUid &&
						!m.InvitesSent &&
						m.Organizer != null &&
						m.Organizer.Id != invitesSentMeeting.Organizer.Id
				));
			}
			return copiesToDelete.ToList();
		}

		private void FillMeeting(Meeting meeting, MeetingDto meetingDto) {
			meetingDto.Title = _utils.FixActivityTitle(meetingDto.Title, _uc);
			meeting.LoadData(meetingDto, _uc);
			meeting.ClearParticipants();
			meeting.Organizer = SetMeetingOrganizer(meetingDto);
			if (meetingDto.Participants.IsEmpty()) {
				return;
			}
			var participants = _participantRepository.GetParticipants(meeting.Id, meetingDto.Participants.Keys.ToList());
			foreach (var participant in participants) {
				if (meetingDto.Participants.ContainsKey(participant.EmailAddress)) {
					var invitationState = meetingDto.Participants[participant.EmailAddress];
					if (participant.Contact.Id == meeting.Calendar.Owner.Id) {
						participant.SetInvitationState(InvitationState.Confirmed);
					} else {
						participant.SetInvitationState(invitationState);
					}
				}
				meeting.AddParticipant(participant);
			}
		}

		private Contact SetMeetingOrganizer(MeetingDto meetingDto) {
			var organizer = _participantRepository.GetParticipantContacts(new List<string> { meetingDto.OrganizerEmail }).FirstOrDefault();
			if (organizer == null) {
				return null;
			}
			var organizerCalendar = _calendarRepository.GetCalendar(organizer.Id);
			if (organizerCalendar == null) {
				return null;
			}
			return organizer;
		}

		private void SaveMeetingToInternalRepository(Meeting meeting) {
			_log?.LogDebug($"Save meeting to internal repository {meeting} in calendar '{meeting.Calendar}'.");
			_meetingRepository.Save(meeting);
			_log?.LogDebug($"Meeting saved to internal repository {meeting} in calendar '{meeting.Calendar}'.");
			if (meeting.InvitesSent 
				&& (meeting.Organizer == null || meeting.Organizer.Id == meeting.Calendar?.Owner.Id)
				&& meeting.Participants.Any()) {
				_participantRepository.UpdateMeetingParticipants(meeting.Participants);
			}
		}

		private bool LockItemForSync(Meeting meeting) {
			var lockResult = _syncHelper.LockItemForSync(meeting.Id.ToString(), meeting.Calendar.Owner.Id, _integrationSystemName, _uc);
			if (lockResult) {
				_log?.LogDebug($"Lock meeting for sync {meeting}.");
			} else {
				_log?.LogInfo($"Meeting already synced, lock skipped {meeting}.");
			}
			return lockResult;
		}

		private void UnlockItem(Meeting meeting) {
			_log?.LogDebug($"Unlock meeting for sync {meeting}.");
			_syncHelper.UnlockEntity(meeting.Id, meeting.Calendar.Owner.Id, _integrationSystemName, _uc);
			_syncHelper.UnlockEntity(meeting.Id, _uc.CurrentUser.ContactId, _integrationSystemName, _uc);
		}

		private bool GetIsItemLockedForSync(Meeting meeting) {
			return _syncHelper.GetEntityLockedForSync(meeting.Id.ToString(), _integrationSystemName, _uc);
		}

		private bool CanCreateEntityInRemoteStore(Meeting meeting) {
			return _syncHelper.CanCreateEntityInRemoteStore(meeting.Id, _uc, _integrationSystemName);
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="IBackgroundTask.Run(TParameters)"/>.
		public void Run(SyncSessionArguments runOptions) {
			_log = ClassFactory.Get<ICalendarLogger>(new ConstructorArgument("sessionId", runOptions.SessionId),
				new ConstructorArgument("modelName", GetType().Name),
				new ConstructorArgument("syncAction", runOptions.SyncAction));
			_meetingRepository = ClassFactory.Get<IMeetingRepository>(new ConstructorArgument("uc", _uc),
				new ConstructorArgument("sessionId", _log.SessionId));
			try {
				switch (runOptions.SyncAction) {
					case SyncAction.UpdateWithInvite:
					case SyncAction.DeleteWithInvite:
					case SyncAction.Delete:
					case SyncAction.ExportPeriod:
					case SyncAction.CreateOrUpdate:
						_log?.LogInfo($"Calendar meetings export started.");
						ExportMeeting(runOptions);
						_log?.LogInfo($"Calendar meetings export ended.");
						break;
					case SyncAction.ImportPeriod:
						_log?.LogInfo($"Calendar meetings import started.");
						ImportPeriod(runOptions);
						_log?.LogInfo($"Calendar meetings import ended.");
						break;
				}
			} catch (Exception e) {
				_log.LogError($"Calendar synchronization session executing failed.", e);
			}
		}

		/// <inheritdoc cref="IUserConnectionRequired.SetUserConnection(UserConnection)"/>.
		public void SetUserConnection(UserConnection userConnection) {
			SetDependencies(userConnection);
		}

		public void Start() {
			Run(new SyncSessionArguments {
				Contacts = new List<Contact> {
					new Contact(_uc.CurrentUser.ContactId, _uc.CurrentUser.Name)
				},
				SyncAction = SyncAction.ImportPeriod
			});
		}

		public void StartFailover() {
			throw new NotImplementedException();
		}

		#endregion

	}

	#endregion

}

