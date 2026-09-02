namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Model
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using IntegrationApi.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Client;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.DTO;
	using BPMSoft.Common;
	using BPMSoft.Core;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Factories;
	using BPMSoft.Sync;
	using SyncAction = IntegrationV2.Files.cs.Domains.MeetingDomain.SyncAction;

	#region Class: Meeting

	/// <summary>
	/// Meeting domain model.
	/// </summary>
	public class Meeting
	{

		#region Constants: Private

		/// <summary>
		/// "Russian Standard Time" time zone identifier. 
		/// </summary>
		private const string RussianStandardTimeTimeZoneId = "Russian Standard Time";

		/// <summary>
		/// Custom "Russian Standard Time" time zone identifier. 
		/// </summary>
		private const string CustomRussianStandardTimeTimeZoneId = "Custom Russian Standard Time";

		/// <summary>
		/// Custom "Russian Standard Time" time zone offset. 
		/// </summary>
		private const int CustomRussianStandardTimeTotalHoursOffset = 3;

		#endregion

		#region Fields: Private

		/// <summary>
		/// <see cref="IActivityUtils"/> instance.
		/// </summary>
		private readonly IActivityUtils _utils = ClassFactory.Get<IActivityUtils>();

		/// <summary>
		/// Hash value.
		/// </summary>
		private string _hash = string.Empty;

		/// <summary>
		/// Number of participants.
		/// </summary>
		private int _numberOfParticipants;

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="meeetingId">Meeting identifier.</param>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public Meeting(Guid meeetingId, string sessionId) {
			Id = meeetingId;
		}

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="entity">Meeting <see cref="Entity"/>.</param>
		public Meeting(Entity entity, string sessionId) : this(entity.PrimaryColumnValue, sessionId) {
			var userConnection = entity.UserConnection;
			Title = entity.GetTypedColumnValue<string>("Title");
			StartDate = entity.GetTypedColumnValue<DateTime>("StartDate");
			DueDate = entity.GetTypedColumnValue<DateTime>("DueDate");
			Location = entity.GetTypedColumnValue<string>("Location");
			Body = entity.GetTypedColumnValue<string>("Notes");
			PriorityId = entity.GetTypedColumnValue<Guid>("PriorityId");
			StartTimeZone = GetExchangeAppointmentTimeZoneInfo(userConnection, StartDate);
			EndTimeZone = GetExchangeAppointmentTimeZoneInfo(userConnection, StartDate);
			RemindToOwner = entity.GetTypedColumnValue<bool>("RemindToOwner");
			RemindToOwnerDate = entity.GetTypedColumnValue<DateTime>("RemindToOwnerDate");
			ModifiedOn = entity.GetTypedColumnValue<DateTime>("ModifiedOn");
			StatusId = entity.GetTypedColumnValue<Guid>("StatusId");
			var organizerId = entity.GetTypedColumnValue<Guid>("OrganizerId");
			if (organizerId.IsNotEmpty()) {
				Organizer = new Contact(organizerId, entity.GetTypedColumnValue<string>("OrganizerName"));
			}
		}

		#endregion

		#region Properties: Public

		public Guid Id { get; }

		public string Title { get; private set; }

		public DateTime StartDate { get; internal set; }

		public TimeZoneInfo StartTimeZone { get; private set; }

		public DateTime DueDate { get; private set; }

		public TimeZoneInfo EndTimeZone { get; private set; }

		public string Location { get; private set; }

		public string Body { get; private set; }

		public Guid PriorityId { get; private set; }

		private bool _invitesSent = false;
		public bool InvitesSent {
			get {
				return _invitesSent;
			}
			set {
				if (_invitesSent) {
					return;
				}
				_invitesSent = value;
			} 
		}

		public List<Participant> Participants { get; } = new List<Participant>();

		public bool RemindToOwner { get; private set; }

		public DateTime RemindToOwnerDate { get; private set; }

		public string RemoteId { get; private set; }

		public string ICalUid { get; private set; }

		public Contact Organizer { get; set; }

		public Guid StatusId { get; }

		public DateTime ModifiedOn { get; }

		public DateTime RemoteCreatedOn { get; set; }

		public Calendar Calendar { get; internal set; }

		public bool IsNeedSendInvitations { get; internal set; }

		public SyncState State { get; set; } = SyncState.New;

		public Dictionary<string, object> OldColumnsValues { get; set; } = new Dictionary<string, object>();

		#endregion

		#region Methods: Private

		/// <summary>
		/// Create a custom time zone.
		/// </summary>
		/// <param name="offset">TimeZone offset.</param>
		/// <param name="timeZoneCode">Time zone code.</param>
		/// <returns><see cref="TimeZoneInfo"/> instance.</returns>
		private TimeZoneInfo GetCustomTimeZone(TimeSpan offset, string timeZoneCode) {
			return TimeZoneInfo.CreateCustomTimeZone(timeZoneCode, offset,
				timeZoneCode, timeZoneCode);
		}

		/// <summary>
		/// Check the platform of the current system is Unix.
		/// </summary>
		/// <returns>True if current system is Unix, otherwise - false.</returns>
		private bool IsUnixOS() {
			OperatingSystem os = Environment.OSVersion;
			PlatformID platform = os.Platform;
			return platform == PlatformID.Unix || platform == PlatformID.MacOSX;
		}

		/// <summary>
		/// Gets exchange appointment <see cref="TimeZoneInfo"/>.
		/// </summary>
		/// <param name="userConnection"><see cref="UserConnection"/> instance.</param>
		/// <param name="time"><see cref="DateTime"/> timezone search time.</param>
		/// <returns>Exchange appointment <see cref="TimeZoneInfo"/>.</returns>
		private TimeZoneInfo GetExchangeAppointmentTimeZoneInfo(UserConnection userConnection, DateTime time) {
			var userTimeZoneId = userConnection.CurrentUser.TimeZoneId;
			var timeZoneInfo = userTimeZoneId == RussianStandardTimeTimeZoneId
				? GetCustomTimeZone(new TimeSpan(CustomRussianStandardTimeTotalHoursOffset, 0, 0), CustomRussianStandardTimeTimeZoneId)
				: TimeZoneUtilities.GetTimeZoneInfo(userTimeZoneId);
			if (IsUnixOS()) {
				timeZoneInfo = GetCustomTimeZone(userConnection.CurrentUser.TimeZone.GetUtcOffset(time), userTimeZoneId);
			}
			var adjustmentRules = timeZoneInfo.GetAdjustmentRules();
			if (adjustmentRules.Any() && !adjustmentRules.Any(ar => ar.DateEnd == DateTime.MaxValue.Date)) {
				var tz = timeZoneInfo;
				return TimeZoneInfo.CreateCustomTimeZone(tz.Id, tz.BaseUtcOffset, tz.DisplayName, tz.StandardName);
			}
			return timeZoneInfo;
		}

		/// <summary>
		/// Gets is meeting date in sync period.
		/// </summary>
		/// <param name="date">Date time of meeting.</param>
		/// <returns><c>True</c>, meeting in sync period, otherwise false.</returns>
		private bool IsDateInSyncPeriod(DateTime date) {
			return date >= Calendar?.SyncSinceDate;
		}

		/// <summary>
		/// Validating old start date.
		/// </summary>
		/// <returns><c>True</c> if in the period, otherwise - <c>False</c>.</returns>
		private bool IsOldStartDateInSyncPerion() {
			return OldColumnsValues.ContainsKey("StartDate")
				&& IsDateInSyncPeriod((DateTime)OldColumnsValues["StartDate"]);
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Adds <see cref="Participant"/>`s to <see cref="Meeting"/>.
		/// </summary>
		/// <param name="participants">Participants list.</param>
		public void AddParticipants(List<Participant> participants) {
			foreach (var participant in participants) {
				AddParticipant(participant);
			}
		}

		/// <summary>
		/// Adds <see cref="Participant"/>`s to <see cref="Meeting"/>.
		/// </summary>
		/// <param name="participant">Participant instance.</param>
		public void AddParticipant(Participant participant) {
			if (!Participants.Any(p => p.Contact.Id == participant.Contact.Id)) {
				Participants.Add(participant);
			}
		}

		/// <summary>
		/// Delete <paramref name="participants"/> from meeting.
		/// </summary>
		/// <param name="participants">List of removal <see cref="Participant"/>.</param>
		public void DeleteParticipants(List<Participant> participants) {
			foreach (var participant in participants) {
				Participants.RemoveAll(p => p.Contact.Id == participant.Contact.Id);
			}
		}

		/// <summary>
		/// Clears participant list.
		/// </summary>
		public void ClearParticipants() {
			Participants.Clear();
		}

		/// <summary>
		/// Saves synchronization metadata.
		/// </summary>
		/// <param name="integrationId"><see cref="IntegrationId"/> instance.</param>
		public void SetIntegrationsId(IntegrationId integrationId) {
			RemoteId = integrationId.RemoteId;
			ICalUid = integrationId.ICalUid;
		}

		/// <summary>
		/// Setting the hash value for current meeting.
		/// </summary>
		/// <param name="hash">Hash value.</param>
		public void SetHash(string hash) {
			_hash = hash;
		}

		/// <summary>
		/// Set number of participants.
		/// </summary>
		/// <param name="numberOfParticipants">Participants count.</param>
		public void SetNumberOfParticipants(int numberOfParticipants) {
			_numberOfParticipants = numberOfParticipants;
		}

		/// <summary>
		/// Calculate and get the current value of hash.
		/// </summary>
		public string GetActualHash() {
			return _utils.GetActivityHash(
				Title,
				Location,
				StartDate,
				DueDate,
				PriorityId,
				Body,
				Calendar?.TimeZone
			);
		}

		/// <summary>
		/// #hecks if the actual meeting matches the last one.
		/// </summary>
		/// <returns><c>True</c> if current activity not matches with latest, otherwise - <c>False</c>.</returns>
		public bool IsChanged(bool ignoreParticipants = false) {
			var actualHash = GetActualHash();
			var actualParticipantCount = Participants.Count;
			return ignoreParticipants 
				? (actualHash != _hash)
				: (actualHash != _hash || actualParticipantCount != _numberOfParticipants);
		}

		/// <summary>
		/// Checks is current meeting can be exported to external calendar.
		/// </summary>
		/// <returns><c>True</c> if meeting can be synced, <c>false</c> otherwise.</returns>
		public bool CanBeExported() {
			return !InvitesSent || (Organizer != null && Organizer.Id.Equals(Calendar?.Owner.Id));
		}

		/// <summary>
		/// Checks is current meeting can be imported from external calendar.
		/// </summary>
		/// <returns><c>True</c> if meeting can be synced, <c>false</c> otherwise.</returns>
		public bool CanBeImported() {
			return !InvitesSent
				|| Organizer == null
				|| Organizer.Id.Equals(Calendar?.Owner.Id);
		}

		/// <summary>
		/// Adds this meeting to calendar.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public void AddToCalendar(UserConnection uc = null) {
			if (CanBeExported()) {
				Calendar?.SaveMeeting(this, uc);
			}
		}

		/// <summary>
		/// Loads data from <paramref name="sourceMeeting"/> to current instance properties.
		/// </summary>
		/// <param name="sourceMeeting"><see cref="MeetingDto"/> instance.</param>
		public void LoadData(MeetingDto sourceMeeting, UserConnection uc) {
			Title = sourceMeeting.IsPrivate ? _utils.GetLczPrivateMeeting(uc) : sourceMeeting.Title;
			Location = sourceMeeting.IsPrivate ? string.Empty : sourceMeeting.Location;
			Body = sourceMeeting.IsPrivate ? string.Empty : sourceMeeting.Body;
			StartDate = sourceMeeting.StartDate;
			DueDate = sourceMeeting.DueDate;
			PriorityId = sourceMeeting.PriorityId;
			_invitesSent = sourceMeeting.InvitesSent;
			RemindToOwner = sourceMeeting.RemindToOwner;
			RemindToOwnerDate = sourceMeeting.RemindToOwnerDate;
			RemoteCreatedOn = sourceMeeting.RemoteCreatedOn;
		}

		/// <summary>
		/// Gets is meeting in sync period.
		/// </summary>
		/// <returns><c>True</c>, meeting in sync period, otherwise false.</returns>
		public bool InSyncPeriod() {
			return IsDateInSyncPeriod(StartDate) || IsOldStartDateInSyncPerion();
		}

		/// <inheritdoc cref="object.ToString"/>
		public override string ToString() {
			return $"[\"{Title}\" \"{Calendar?.Settings.SenderEmailAddress}\" \"{Id}\" \"{ICalUid}\"]";
		}

		#endregion

	}

	#endregion

}
