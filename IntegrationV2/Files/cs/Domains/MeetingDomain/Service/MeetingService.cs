namespace IntegrationV2.Files.cs.Domains.MeetingDomain.MeetingService 
{
	using System;
	using BPMSoft.Configuration;
	using BPMSoft.Core.Factories;
	using BPMSoft.Core;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Logger;
	using BPMSoft.Common;
	using System.Linq;
	using static BPMSoft.EmailDomain.IntegrationConsts.Calendar;
	using IntegrationApi.Calendar.DTO;
	using System.Globalization;

	/// <summary>
	/// Meeting service implementation.
	/// </summary>
	[DefaultBinding(typeof(IMeetingService))]
	public class MeetingService : IMeetingService
	{

		#region Fields: Private

		/// <summary>
		/// <see cref="IMeetingRepository"/> instance.
		/// </summary>
		private readonly IMeetingRepository _meetingRepository;

		/// <summary>
		/// <see cref="ICalendarLogger"/> instance.
		/// </summary
		private ICalendarLogger Log { get; } = ClassFactory.Get<ICalendarLogger>();

		/// <summary>
		/// <see cref="IParticipantRepository"/> instance.
		/// </summary
		private readonly IParticipantRepository _participantRepository;

		/// <summary>
		/// <see cref="ICalendarRepository"/> instance.
		/// </summary
		private readonly ICalendarRepository _calendarRepository;

		/// <summary>
		/// <see cref="UserConnection"/> instance.
		/// </summary
		private readonly UserConnection _uc;

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public MeetingService(UserConnection uc) {
			_uc = uc;
			Log.SetModelName(GetType().Name);
			_meetingRepository = ClassFactory.Get<IMeetingRepository>(new ConstructorArgument("uc", uc),
				new ConstructorArgument("sessionId", Log.SessionId));
			_participantRepository = ClassFactory.Get<IParticipantRepository>(new ConstructorArgument("uc", uc));
			_calendarRepository = ClassFactory.Get<ICalendarRepository>(new ConstructorArgument("uc", uc));
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Checking if the meeting start date is outdated.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <returns><c>True</c> if meeting is outdated, <c>false</c> otherwise.</returns>
		private bool IsOutdatedMeeting(Meeting meeting) {
			var utcStartDate = meeting.StartDate.ToUniversalTime();
			return utcStartDate < DateTime.UtcNow;
		}

		/// <summary>
		/// Checks if invitations can be sent to the current meeting.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="userContactId">User contact identifier.</param>
		/// <returns><c>True</c> if can be send invitations, <c>false</c> otherwise.</returns>
		private bool CanSendInvitations(Meeting meeting, Guid userContactId) {
			var isExportedMeeting = meeting.RemoteId.IsNotNullOrEmpty();
			if (!isExportedMeeting) {
				Log?.LogInfo($"Failed to send invitations to contact {meeting.Calendar?.Owner} for a non-existent meeting {meeting}.");
			}
			var isOrganizerSent = meeting.Organizer != null && meeting.Organizer.Id.Equals(meeting.Calendar?.Owner.Id) &&
				userContactId.Equals(meeting.Calendar?.Owner.Id);
			if (!isOrganizerSent) {
				Log?.LogInfo($"Invitations can be sent only meeting organizer {meeting.Organizer}," +
					$" but tried to send {meeting.Calendar?.Owner}. {meeting}");
			}
			return isExportedMeeting && isOrganizerSent;
		}

		/// <summary>
		/// Send invitations to participants.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="userContactId">User contact identifier.</param>
		private void SendMeetingInvitation(Meeting meeting, Guid userContactId) {
			if (CanSendInvitations(meeting, userContactId)) {
				Log?.LogInfo($"Send invitations for meeting {meeting}.");
				meeting.Calendar.SendInvitations(meeting, _uc);
				foreach (var participant in meeting.Participants) {
					participant.SetInvited();
					if (participant.Contact.Id == meeting.Calendar.Owner.Id) {
						participant.SetInvitationState(InvitationState.Confirmed);
					}
					_participantRepository.UpdateParticipantInvitation(participant);
					Log?.LogInfo($"Invitation was sent {meeting.Organizer} for participant '{participant.EmailAddress}' {participant.Contact}.");
				}
				Log?.LogInfo($"Invitations was sent for meeting {meeting}.");
			}
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="IMeetingService.SendInvitations(Guid, Guid)"/>
		public void SendInvitations(Guid meetingId, Guid userContactId) {
			Log.SetAction(SyncAction.SendInvite);
			var meetings = _meetingRepository.GetMeetings(meetingId);
			meetings.ForEach(meeting => SendMeetingInvitation(meeting, userContactId));
		}

		/// <inheritdoc cref="IMeetingService.GetMeetingInvitationInfo(Guid, Guid)"/>
		public MeetingInvitationInfo GetMeetingInvitationInfo(Guid meetingId, Guid userContactId) {
			var meetingInvitationInfo = new MeetingInvitationInfo();
			var meetings = _meetingRepository.GetMeetings(meetingId);
			meetingInvitationInfo.IsSynchronized = meetings.Any(m => m.RemoteId.IsNotNullOrEmpty());
			meetingInvitationInfo.IsParticipantsInvited = meetings.Any(m => m.InvitesSent);
			meetingInvitationInfo.IsParticipantsExist = meetings.Any(m =>
				m.Participants.Count > 1 &&
				m.Participants.Any(p => p.EmailAddress.IsNotNullOrEmpty() &&
					p.Contact.Id != m.Organizer.Id
				)
			);
			meetingInvitationInfo.HasCalendarIntegration = meetings.Any(m => m.Calendar?.Owner.Id == userContactId);
			meetingInvitationInfo.IsOutdatedMeeting = IsOutdatedMeeting(meetings.First());
			var meeting = meetings.FirstOrDefault(m => m.Calendar?.Owner.Id == userContactId);
			if (meeting != null) {
				meetingInvitationInfo.CalendarSyncSinceDate = meeting.Calendar.SyncSinceDate.ToString(CultureInfo.InvariantCulture);
			}
			return meetingInvitationInfo;
		}

		/// <inheritdoc cref="IMeetingService.CanUserSendInvite(Guid, Guid)"/>
		public ChangeMeetingResponse CanUserChangeMeeting(Guid meetingId, Guid userContactId) {
			var organizerMeeting = _meetingRepository.GetMeetings(meetingId)
				.FirstOrDefault(m => m.Participants.Any(p => p.Contact.Id == m.Organizer.Id));
			if (organizerMeeting == null || !organizerMeeting.InvitesSent || organizerMeeting.Participants.Count == 1) {
				return ChangeMeetingResponse.Yes;
			}
			if (organizerMeeting.Organizer.Id == userContactId) {
				return IsOutdatedMeeting(organizerMeeting)
					? ChangeMeetingResponse.YesWithObsoleteNotification
					: ChangeMeetingResponse.YesWithNotification;
			}
			return ChangeMeetingResponse.No;
		}

		/// <inheritdoc cref="IMeetingService.CanUserChangeCalendar(string, Guid)"/>
		public bool CanUserChangeCalendar(string senderEmailAddress, Guid userContactId) {
			var userCalendars = _calendarRepository.GetAllCalendars(userContactId, Log.SessionId);
			var hasActiveCalendar = userCalendars.Count(c => c.Settings.SyncEnabled) != 0;
			var isCurrentActiveCalendar = userCalendars.Any(c => c.Settings.SyncEnabled && c.Settings.SenderEmailAddress == senderEmailAddress);
			return !hasActiveCalendar || isCurrentActiveCalendar;
		}

		#endregion

	}
}
