namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Client
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Security;
	using System.Security.Cryptography.X509Certificates;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.DTO;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Client.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Logger;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using Microsoft.Exchange.WebServices.Data;
	using BPMSoft.Common;
	using BPMSoft.Configuration;
	using BPMSoft.Core.Factories;
	using BPMSoft.EmailDomain;

	#region Class: ExchangeCalendarClient

	/// <summary>
	/// <see cref="ICalendarClient"/> implementation.
	/// </summary>
	[DefaultBinding(typeof(ICalendarClient), Name = "Exchange")]
	public class ExchangeCalendarClient: ICalendarClient
	{

		#region Constants: Private

		/// <summary>
		/// Definition of property which contains record identifier in local storage.
		/// </summary>
		private readonly ExtendedPropertyDefinition _localIdProperty = new ExtendedPropertyDefinition(
			DefaultExtendedPropertySet.PublicStrings, "LocalId", MapiPropertyType.String);

		/// <summary>
		/// First class property set.
		/// </summary>
		private readonly PropertySet _propertySet = new PropertySet(BasePropertySet.FirstClassProperties);

		/// <summary>
		/// Priority activity - Low.
		/// </summary>
		private readonly Guid _activityLowPriorityId = new Guid("AC96FA02-7FE6-DF11-971B-001D60E938C6");

		/// <summary>
		/// Priority activity - Medium.
		/// </summary>
		private readonly Guid _activityNormalPriorityId = new Guid("AB96FA02-7FE6-DF11-971B-001D60E938C6");

		/// <summary>
		/// Priority activity - High.
		/// </summary>
		private readonly Guid _activityHighPriorityId = new Guid("D625A9FC-7EE6-DF11-971B-001D60E938C6");

		/// <summary>
		/// Requested items limit.
		/// </summary>
		private readonly int _maxItemsPerQuery = 150;

		#endregion

		#region Fields: Private

		private ExchangeService _service;

		/// <summary>
		/// <see cref="ICalendarLogger"/> instance.
		/// </summary
		private readonly ICalendarLogger _log;

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		public ExchangeCalendarClient() {
			_propertySet.RequestedBodyType = BodyType.HTML;
			_propertySet.Add(_localIdProperty);
		}

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="sessionId">Synchronization session identifier.</param>
		public ExchangeCalendarClient(string sessionId) : this() {
			_log = ClassFactory.Get<ICalendarLogger>(new ConstructorArgument("sessionId", sessionId),
				new ConstructorArgument("modelName", GetType().Name));
		}

		#endregion

		#region Methods: Private 

		/// <summary>
		/// Creates appointment in <see cref="Calendar"/>.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> model instance.</param>
		/// <returns><see cref="Meeting"/> unique identifier in the external store.</returns>
		private IntegrationId CreateAppointment(Meeting meeting) {
			Appointment appointment = new Appointment(_service);
			appointment.SetExtendedProperty(_localIdProperty, meeting.Id.ToString());
			appointment.Sensitivity = Sensitivity.Normal;
			appointment.LegacyFreeBusyStatus = LegacyFreeBusyStatus.Busy;
			FillAppointment(appointment, meeting);
			var sendInvitationsMode = GetSendInvitationsMode(meeting);
			SaveExchangeAppointment(appointment, sendInvitationsMode);
			_log?.LogInfo($"Meeting added to external repository with {sendInvitationsMode} mode. {meeting}");
			appointment.Load(new PropertySet(AppointmentSchema.ICalUid));
			return new IntegrationId(appointment.Id.UniqueId, appointment.ICalUid);
		}

		/// <summary>
		/// Creates appointment in <see cref="Calendar"/>.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> model instance.</param>
		/// <param name="integrationId"><see cref="Meeting"/> unique identifier in the external store.</param>
		private void UpdateAppointment(Meeting meeting, IntegrationId integrationId) {
			if (!TryBindAppointment(integrationId.RemoteId, out Appointment appointment)) {
				return;
			}
			appointment.Load(_propertySet);
			FillAppointment(appointment, meeting);
			var sendInvitationsMode = GetSendInvitationsMode(meeting);
			SaveExchangeAppointment(appointment, sendInvitationsMode);
			_log?.LogInfo($"Meeting updated in external repository with {sendInvitationsMode} mode. {meeting}");
		}

		/// <summary>
		/// Binds and removes appointment from calendar.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		private void RemoveExchangeAppointment(Meeting meeting) {
			if (!TryBindAppointment(meeting.RemoteId, out Appointment appointment)) {
				return;
			}
			meeting.InvitesSent = GetInvitesSent(appointment, meeting.Calendar);
			var sendInvitationsMode = (SendCancellationsMode)GetSendInvitationsMode(meeting);
			appointment?.Delete(DeleteMode.MoveToDeletedItems, sendInvitationsMode);
			_log?.LogInfo($"Meeting deleted from external repository with {sendInvitationsMode} mode. {meeting}");
		}

		private void FillAppointment(Appointment appointment, Meeting meeting) {
			if (appointment.Sensitivity != Sensitivity.Private) {
				appointment.Subject = meeting.Title;
				appointment.Location = meeting.Location;
				appointment.Body = meeting.Body;
			}
			appointment.Start = meeting.StartDate;
			appointment.StartTimeZone = meeting.StartTimeZone;
			appointment.End = meeting.DueDate;
			appointment.EndTimeZone = meeting.EndTimeZone;
			appointment.Importance = GetExchangeImportance(meeting.PriorityId);
			appointment.IsReminderSet = meeting.RemindToOwner;
			var remindToOwnerDate = meeting.RemindToOwnerDate;
			if (remindToOwnerDate != DateTime.MinValue && remindToOwnerDate <= meeting.StartDate) {
				TimeSpan duration = meeting.StartDate - remindToOwnerDate;
				appointment.ReminderMinutesBeforeStart = Convert.ToInt32(duration.TotalMinutes);
			} else {
				appointment.ReminderMinutesBeforeStart = 0;
			}
			appointment.RequiredAttendees.Clear();
			if (meeting.Organizer?.Id == meeting.Calendar.Owner.Id) {
				foreach (var participant in meeting.Participants) {
					var emailAddress = participant.EmailAddress;
					if (emailAddress.IsNullOrEmpty()) {
						continue;
					}
					if (participant.IsRequired) {
						appointment.RequiredAttendees.Add(emailAddress);
					} else {
						appointment.OptionalAttendees.Add(emailAddress);
					}
				}
			}
			meeting.InvitesSent = GetInvitesSent(appointment, meeting.Calendar);
		}

		/// <summary>
		/// Saves an appointment on an external calendar.
		/// </summary>
		/// <param name="appointment"><see cref="Appointment"/> instance.</param>
		/// <param name="sendInvitationsMode">Send invitaions mode.</param>
		/// <returns><see cref="Appointment"/> instance.</returns>
		private void SaveExchangeAppointment(Appointment appointment, SendInvitationsMode sendInvitationsMode) {
			if (appointment.Id == null) {
				appointment.Save(sendInvitationsMode);
			} else {
				appointment.Update(ConflictResolutionMode.AlwaysOverwrite, (SendInvitationsOrCancellationsMode)sendInvitationsMode);
			}
		}

		/// <summary>
		/// Get <see cref="SendInvitationsMode"/> for saving appontment.
		/// </summary>
		/// <param name="meeting"></param>
		/// <returns><see cref="SendInvitationsMode"/> for saving appontment.</returns>
		private SendInvitationsMode GetSendInvitationsMode(Meeting meeting) {
			if (meeting.InvitesSent && meeting.IsNeedSendInvitations) {
				return SendInvitationsMode.SendOnlyToAll;
			} else {
				return SendInvitationsMode.SendToNone;
			}
		}

		private bool GetInvitesSent(Appointment appointment, Calendar calendar) {
			if (appointment.Id == null) {
				return false;
			}
			var organizerEmail = appointment.Organizer.Address.ExtractEmailAddress();
			return appointment.MeetingRequestWasSent || !string.Equals(organizerEmail, calendar.Settings.SenderEmailAddress, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Returns <see cref="Exchange.Item"/> importance.
		/// </summary>
		/// <param name="activityPriorityId"><see cref="ActivityPriority"/> instance id.</param>
		/// <returns><see cref="Exchange.Importance"/> instance.</returns>
		private Importance GetExchangeImportance(Guid activityPriorityId) {
			if (activityPriorityId == _activityHighPriorityId) {
				return Importance.High;
			} else if (activityPriorityId == _activityNormalPriorityId) {
				return Importance.Normal;
			}
			return Importance.Low;
		}

		/// <summary>
		/// Returns <paramref name="importance"/> identifier.
		/// </summary>
		/// <param name="importance">Appointment <see cref="Importance"/>.</param>
		/// <returns>Activity importance identifier.</returns>
		private Guid GetActivityPriority(Importance importance) {
			switch (importance) {
				case Importance.High:
					return _activityHighPriorityId;
				case Importance.Low:
					return _activityLowPriorityId;
				default:
					return _activityNormalPriorityId;
			}
		}

		/// <summary>
		/// Get an instance of Exchange service.
		/// </summary>
		/// <param name="settings"><see cref="CalendarSettings"/> instance.</param>
		/// <returns><see cref="ExchangeService"/> instance.</returns>
		private ExchangeService GetExchangeService(CalendarSettings settings) {
			var exchangeService = new ExchangeService(ExchangeVersion.Exchange2010_SP1, TimeZoneInfo.Utc);
			exchangeService.Url = new Uri(string.Format("https://{0}/EWS/Exchange.asmx", settings.ServiceUrl));
			if (settings.UseOAuth) {
				string token = settings.AccessToken;
				exchangeService.Credentials = new OAuthCredentials(token);
			} else {
				exchangeService.Credentials = new WebCredentials(settings.Login, settings.Password);
			}
			TestConnection(exchangeService, settings.SenderEmailAddress);
			return exchangeService;
		}

		/// <summary>
		/// Callback function to verify the server certificate.
		/// </summary>
		/// <param name="sender">An object that contains state information for this verification.</param>
		/// <param name="certificate">Certificate, used to verify the authenticity of the remote side.</param>
		/// <param name="chain">CA chain associated with the remote certificate.</param>
		/// <param name="policyErrors">One or more errors associated with the remote certificate.</param>
		/// <returns>Result command execution.</returns>
		private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain,
				SslPolicyErrors policyErrors) {
			return true;
		}

		/// <summary>
		/// Set security protocol options.
		/// </summary>
		private void SetSecurityProtocolOptions() {
			ServicePointManager.ServerCertificateValidationCallback += ValidateRemoteCertificate;
		}

		/// <summary>
		/// Reset security protocol options
		/// </summary>
		private void ResetSecurityProtocolOptions() {
			ServicePointManager.ServerCertificateValidationCallback -= ValidateRemoteCertificate;
		}

		/// <summary>
		/// Check connection.
		/// </summary>
		/// <param name="service"><see cref="ExchangeService"/> instance.</param>
		/// <param name="emailAddress">Email Address.</param>
		private void TestConnection(ExchangeService service, string emailAddress = "") {
			var id = new FolderId(WellKnownFolderName.MsgFolderRoot, emailAddress);
			service.FindFolders(id, new FolderView(1));
		}

		/// <summary>
		/// Calls <paramref name="action"/>. SSL errors will be skipped.
		/// </summary>
		/// <param name="action">Sync action.</param>
		/// <returns><c>True</c> if <paramref name="action"/> invoked without errors. Otherwise returns <c>false</c>.</returns>
		private bool InvokeInSafeContext(CalendarSettings settings, Action action) {
			SetSecurityProtocolOptions();
			try {
				_service = GetExchangeService(settings);
				action();
				return true;
			} catch(Exception e) {
				_log?.LogError($"Invoke service action is failed for {settings}.", e);
				return false;
			} finally {
				ResetSecurityProtocolOptions();
			}
		}

		/// <summary>
		/// Returns exchange sync folders.
		/// </summary>
		/// <param name="calendar"><see cref="Calendar"/> instance.</param>
		/// <returns>Exchange folders list.</returns>
		private List<Folder> GetSyncFolders(Calendar calendar) {
			if (calendar.FolderIds.Any()) {
				return SafeBindFolders(calendar.FolderIds);
			} else {
				var result = new List<Folder>();
				var rootFolder = Folder.Bind(_service, WellKnownFolderName.MsgFolderRoot);
				GetAllFolders(result, rootFolder);
				return result;
			}
		}

		/// <summary>
		/// Binds exchange folders. Bind errors will be skipped.
		/// </summary>
		/// <param name="folderRemoteIds">Exchange folders identifiers.</param>
		/// <returns><see cref="Folder"/> list.</returns>
		private List<Folder> SafeBindFolders(IEnumerable<string> folderRemoteIds) {
			var result = new List<Folder>();
			foreach (string uniqueId in folderRemoteIds) {
				if (string.IsNullOrEmpty(uniqueId)) {
					continue;
				}
				try {
					var folder = Folder.Bind(_service, new FolderId(uniqueId));
					result.Add(folder);
				} catch (Exception exception) when (exception.GetBaseException() is ServiceResponseException) {
				}
			}
			return result;
		}

		private bool TryBindAppointment(string remoteId, out Appointment appointment) {
			if (remoteId.IsNotNullOrEmpty()) {
				try {
					var item = Item.Bind(_service, new ItemId(remoteId));
					appointment = item as Appointment;
					return true;
				} catch (Exception exception) when (exception.GetBaseException() is ServiceResponseException) {
					_log?.LogError($"Failed binding remote item {remoteId}.", exception);
				}
			}
			appointment = null;
			return false;
		}

		/// <summary>
		/// Loads all folders for calendar.
		/// </summary>
		/// <param name="list">Folders buffer.</param>
		/// <param name="folder">Current root folder instance.</param>
		private void GetAllFolders(List<Folder> list, Folder folder) {
			if (folder.ChildFolderCount > 0 && list != null) {
				var folderView = new FolderView(folder.ChildFolderCount);
				foreach (Folder childFolder in folder.FindFolders(folderView)) {
					if (childFolder.FolderClass == ExchangeConsts.AppointmentFolderClassName) {
						list.Add(childFolder);
					}
					GetAllFolders(list, childFolder);
				}
			}
		}

		/// <summary>
		/// Returns <see cref="InvitationState"/> for <paramref name="responseType"/> invitation state.
		/// </summary>
		/// <param name="responseType"><see cref="MeetingResponseType?"/> instance.</param>
		/// <returns><see cref="InvitationState"/> participant invitation state.</returns>
		private InvitationState ConvertToInternalInvitationState(MeetingResponseType? responseType) {
			switch (responseType) {
				case MeetingResponseType.Accept:
				case MeetingResponseType.Organizer:
					return InvitationState.Confirmed;
				case MeetingResponseType.Decline:
					return InvitationState.Declined;
				case MeetingResponseType.Unknown:
				case MeetingResponseType.Tentative:
					return InvitationState.InDoubt;
				default:
					return InvitationState.Empty;
			}
		}

		/// <summary>
		/// Add a participant to the meeting DTO.
		/// </summary>
		/// <param name="meeting"><see cref="MeetingDto"/> instance.</param>
		/// <param name="email">Participant email.</param>
		/// <param name="response"><see cref="MeetingResponseType"/> instance.</param>
		private void AddParticipantToMeetingDto(MeetingDto meetingDto, string email, MeetingResponseType? response = null) {
			email = email.ExtractEmailAddress();
			if (!meetingDto.Participants.ContainsKey(email)) {
				meetingDto.Participants.Add(email, ConvertToInternalInvitationState(response));
			}
		}

		/// <summary>
		/// Creates <see cref="MeetingDto"/> instance and fills it with data from <paramref name="appointment"/>.
		/// </summary>
		/// <param name="appointment"><see cref="Appointment"/> instance.</param>
		/// <param name="calendar"><see cref="Calendar"/> instance.</param>
		/// <returns><see cref="MeetingDto"/> instance.</returns>
		private MeetingDto ConvertToMeetingDto(Appointment appointment, Calendar calendar) {
			appointment.Load(_propertySet);
			var startDate = GetUserDateTime(SafeGetValue<DateTime>(appointment, AppointmentSchema.Start), calendar.TimeZone);
			var dueDate = GetUserDateTime(SafeGetValue<DateTime>(appointment, AppointmentSchema.End), calendar.TimeZone);
			var remindToOwner = SafeGetValue<bool>(appointment, ItemSchema.IsReminderSet);
			var organizerEmail = appointment.Organizer.Address.ExtractEmailAddress();
			var meeting = new MeetingDto() {
				Title = appointment.Subject,
				StartDate = startDate,
				DueDate = dueDate,
				Location = appointment.Location,
				Body = appointment.Body.Text,
				PriorityId = GetActivityPriority(appointment.Importance),
				InvitesSent = GetInvitesSent(appointment, calendar),
				RemindToOwner = remindToOwner,
				RemindToOwnerDate = remindToOwner ? startDate.AddMinutes(appointment.ReminderMinutesBeforeStart) : DateTime.MinValue,
				OrganizerEmail = organizerEmail,
				RemoteId = appointment.Id.UniqueId,
				ICalUid = GetICalUid(appointment),
				IsPrivate = appointment.Sensitivity == Sensitivity.Private,
				RemoteCreatedOn = appointment.DateTimeCreated
			};
			appointment.RequiredAttendees.ForEach(attendee => 
				AddParticipantToMeetingDto(meeting, attendee.Address, attendee.ResponseType)
			);
			appointment.OptionalAttendees.ForEach(attendee =>
				AddParticipantToMeetingDto(meeting, attendee.Address, attendee.ResponseType)
			);
			AddParticipantToMeetingDto(meeting, organizerEmail);
			return meeting;
		}

		/// <summary>
		/// Loads property value from <paramref name="item"/>. Value not loaded errors will be skipped.
		/// </summary>
		/// <typeparam name="T">Return value type.</typeparam>
		/// <param name="item">Exchange item instance.</param>
		/// <param name="propertyDefinition">Requested property definition.</param>
		/// <returns>Requested property value.</returns>
		private T SafeGetValue<T>(Item item, PropertyDefinition propertyDefinition) {
			item.TryGetProperty(propertyDefinition, out T value);
			return value;
		}

		/// <summary>
		/// Returns <see cref="DateTime"/> instance converted to user timezone.
		/// </summary>
		/// <param name="utcDateTime"><see cref="DateTime"/> instance.</param>
		/// <param name="timeZone"><see cref="TimeZoneInfo"/> instance.</param>
		/// <returns><see cref="DateTime"/> instance converted to user timezone.</returns>
		private DateTime GetUserDateTime(DateTime utcDateTime, TimeZoneInfo timeZone) {
			return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime.Kind == DateTimeKind.Utc ?
				utcDateTime : utcDateTime.ToUniversalTime(), timeZone);
		}

		private string GetICalUid(Appointment appointment) {
			return appointment.IsRecurring && appointment.ICalRecurrenceId != null
				? GetICalUid(appointment.ICalUid, appointment.ICalRecurrenceId.Value)
				: appointment.ICalUid;
		}

		private string GetICalUid(string masterICalUid, DateTime occurrenceStart) {
			return masterICalUid + occurrenceStart.ToString("_yyyy_MM_dd");
		}

		private List<MeetingDto> GetChangedAppointments(Calendar calendar) {
			var result = new List<MeetingDto>();
			var startDate = calendar.SyncSinceDate;
			var endDate = calendar.SyncTillDate;
			foreach (var calendarFolder in GetSyncFolders(calendar)) {
				var currentStartDate = startDate;
				_log?.LogDebug($"Load meetings from folder '{calendarFolder.DisplayName} {calendarFolder.Id}'.");
				while (currentStartDate < endDate) {
					var calendarItemView = new CalendarView(currentStartDate, currentStartDate.AddDays(1), _maxItemsPerQuery);
					_log?.LogDebug($"Request all meetings from period '{calendarItemView.StartDate}' - '{calendarItemView.EndDate}'.");
					foreach (var item in _service.FindAppointments(calendarFolder.Id, calendarItemView)) {
						var meeting = ConvertToMeetingDto(item, calendar);
						_log?.LogDebug($"Meeting loaded by import. {meeting} {calendar}.");
						result.Add(meeting);
					}
					_log?.LogDebug($"Meetings from period '{calendarItemView.StartDate}' - '{calendarItemView.EndDate}' loaded.");
					currentStartDate = currentStartDate.AddDays(1);
				}
				_log?.LogDebug($"Load meetings from folder '{calendarFolder.DisplayName}' ended '{calendarFolder.Id}'.");
			}
			return result;
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="ICalendarClient.SaveMeeting(Meeting, Calendar)"/>
		public IntegrationId SaveMeeting(Meeting meeting, Calendar calendar) {
			if (!calendar.Settings.SyncEnabled) {
				_log?.LogInfo($"Export disabled when save meeting. {meeting} {calendar}.");
				return null;
			}
			var integrationId = new IntegrationId(meeting.RemoteId, meeting.ICalUid);
			InvokeInSafeContext(calendar.Settings, () => {
				if (string.IsNullOrEmpty(integrationId.RemoteId)) {
					_log?.LogInfo($"Export meeting to external repository. {meeting} {calendar}");
					integrationId = CreateAppointment(meeting);
					_log?.LogInfo($"Meeting successfully exported to external repository. {meeting} {calendar}");
				} else {
					_log?.LogInfo($"Updating meeting in external repository. {meeting} {calendar}.");
					UpdateAppointment(meeting, integrationId);
					_log?.LogInfo($"Meeting updated in external repository. {meeting} {calendar}.");
				}
			});
			return integrationId;
		}

		/// <inheritdoc cref="ICalendarClient.RemoveMeeting(Meeting, Calendar)"/>
		public void RemoveMeeting(Meeting meeting, Calendar calendar) {
			if (!calendar.Settings.SyncEnabled) {
				_log?.LogWarn($"Export disabled when deleting meeting {meeting} {calendar}.");
				return;
			}
			InvokeInSafeContext(calendar.Settings, () => {
				_log?.LogInfo($"Delete meeting {meeting} {calendar}.");
				RemoveExchangeAppointment(meeting);
				_log?.LogInfo($"Meeting deleted from external repository. {meeting} {calendar}.");
			});
		}

		/// <inheritdoc cref="ICalendarClient.SendInvitations(Meeting, Calendar)"/>
		public void SendInvitations(Meeting meeting, Calendar calendar) {
			if (string.IsNullOrEmpty(meeting.RemoteId) || !calendar.Settings.SyncEnabled) {
				return;
			}
			meeting.InvitesSent = true;
			meeting.IsNeedSendInvitations = true;
			SaveMeeting(meeting, calendar);
		}

		/// <inheritdoc cref="ICalendarClient.GetSyncPeriodMeetings(Calendar, bool)"/>
		public List<MeetingDto> GetSyncPeriodMeetings(Calendar calendar, out bool hasErrors) {
			var result = new List<MeetingDto>();
			if (!calendar.Settings.SyncEnabled) {
				_log?.LogWarn($"Import disabled when sync meetings. {calendar}.");
				hasErrors = false;
				return result;
			}
			hasErrors = !InvokeInSafeContext(calendar.Settings, () => {
				result.AddRange(GetChangedAppointments(calendar));
			});
			_log?.LogDebug($"External repository loaded {result.Count} meetings for import.");
			return result;
		}

		#endregion

	}

	#endregion

}
