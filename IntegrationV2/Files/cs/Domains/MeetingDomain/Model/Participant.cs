namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Model
{
	using System;
	using BPMSoft.EmailDomain;
	using BPMSoft.Core.Entities;

	#region Class: Participant

	/// <summary>
	/// Participant domain model.
	/// </summary>
	public class Participant
	{

		#region Constants: Private

		private readonly Guid _responsibleRoleId = IntegrationConsts.Calendar.ParticipantRole.Responsible;

		private readonly Guid _participantRoleId = IntegrationConsts.Calendar.ParticipantRole.Participant;

		#endregion

		#region Properties: Public

		public Guid Id { get; }

		public Guid RoleId { get; }

		public bool IsInvited { get; private set; }

		public Guid InviteResponseId { get; private set;  }

		public Contact Contact { get; }

		public Guid MeetingId { get; }

		public string EmailAddress {
			get {
				return Contact?.EmailAddress;
			} set {
				Contact?.SetEmail(value);
			}
		}

		public bool IsRequired => (RoleId == _responsibleRoleId || RoleId == _participantRoleId);

		#endregion

		#region Consctructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		public Participant(Contact contact, Guid meetingId) {
			Contact = contact;
			MeetingId = meetingId;
		}

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="entity">Participant <see cref="Entity"/>.</param>
		public Participant(Entity entity) {
			Id = entity.PrimaryColumnValue;
			RoleId = entity.GetTypedColumnValue<Guid>("RoleId");
			Contact = new Contact(
				entity.GetTypedColumnValue<Guid>("ParticipantId"),
				entity.GetTypedColumnValue<string>("ParticipantName")
			);
			MeetingId = entity.GetTypedColumnValue<Guid>("ActivityId");
			InviteResponseId = entity.GetTypedColumnValue<Guid>("InviteResponseId");
			IsInvited = entity.GetTypedColumnValue<bool>("InviteParticipant");
		}

		#endregion

		#region Methods: Private

		/// <summary>
		/// Returns <see cref="ParticipantResponse"/> id for <paramref name="invitationState"/> invite response.
		/// </summary>
		/// <param name="invitationState"><see cref="InvitationState"/> invitation state.</param>
		/// <returns>Participant response id.</returns>
		private Guid ConvertToInternalParticipantResponse(InvitationState invitationState) {
			switch (invitationState) {
				case InvitationState.Confirmed:
					return IntegrationConsts.Calendar.ParticipantResponse.Confirmed;
				case InvitationState.Declined:
					return IntegrationConsts.Calendar.ParticipantResponse.Declined;
				case InvitationState.InDoubt:
					return IntegrationConsts.Calendar.ParticipantResponse.InDoubt;
				default:
					return Guid.Empty;
			}
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Set participant invitation response.
		/// </summary>
		/// <param name="invitationState"><see cref="InvitationState"/> participant invitation state.</param>
		public void SetInvitationState(InvitationState invitationState) {
			InviteResponseId = ConvertToInternalParticipantResponse(invitationState);
			IsInvited = InviteResponseId != Guid.Empty;
		}

		/// <summary>
		/// Set that the participant has been invited.
		/// </summary>
		public void SetInvited() {
			if (InviteResponseId != Guid.Empty && !IsInvited) {
				IsInvited = true;
			} else {
				SetInvitationState(InvitationState.InDoubt);
			}
		}

		#endregion

	}

	#endregion

}
