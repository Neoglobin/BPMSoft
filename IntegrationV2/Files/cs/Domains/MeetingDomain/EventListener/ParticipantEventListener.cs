namespace IntegrationV2.Files.cs.Domains.MeetingDomain.EventListener
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Linq;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Entities.Events;
	using BPMSoft.Core.Factories;

	#region Class: ParticipantEventListener

	[ExcludeFromCodeCoverage]
	[EntityEventListener(SchemaName = "ActivityParticipant")]
	class ParticipantEventListener: BaseCalendarEventListener
	{

		#region Methods: Private

		/// <summary>
		/// Gets meeting organizer identifier.
		/// </summary>
		/// <param name="entity"><see cref="Entity"/> instance.</param>
		/// <returns>Meeting organizer identifier.</returns>
		private Contact GetMeetingOrganizer(Entity entity) {
			var meetingId = entity.GetTypedColumnValue<Guid>("ActivityId");
			var meetingRepository = ClassFactory.Get<IMeetingRepository>(
				new ConstructorArgument("uc", entity.UserConnection),
				new ConstructorArgument("sessionId", Logger.SessionId));
			var meetings = meetingRepository.GetMeetings(meetingId);
			var organizer = meetings.FirstOrDefault()?.Organizer;
			if (organizer != null) {
				Logger?.LogDebug($"Meeting with participant '{entity.PrimaryColumnValue}' has organizer {organizer}.");
			}
			return organizer;
		}

		/// <summary>
		/// Delete <see cref="Participant"/> from meeting.
		/// </summary>
		/// <param name="entity"><see cref="Entity"/> instance.</param>
		private void DeleteParticipantFromMeeting(Entity entity) {
			var meetingId = entity.GetTypedColumnValue<Guid>("ActivityId");
			var participantId = entity.GetTypedColumnValue<Guid>("ParticipantId");
			var participantName = entity.GetTypedColumnValue<string>("ParticipantName");
			var organizer = GetMeetingOrganizer(entity);
			var organizerSyncAction = GetIsBackgroundProcess() ? SyncAction.CreateOrUpdate : SyncAction.UpdateWithInvite;
			StartMeetingSynchronization(meetingId, new List<Contact> { organizer }, organizerSyncAction);
			StartMeetingSynchronization(meetingId, new List<Contact> { new Contact(participantId, participantName) }, SyncAction.Delete);
		}

		/// <summary>
		/// Add <see cref="Participant"/> to meeting.
		/// </summary>
		/// <param name="entity"><see cref="Entity"/> instance.</param>
		private void AddParticipantToMeeting(Entity entity) {
			var meetingId = entity.GetTypedColumnValue<Guid>("ActivityId");
			var participantContact = new Contact(
				entity.GetTypedColumnValue<Guid>("ParticipantId"),
				entity.GetTypedColumnValue<string>("ParticipantName")
			);
			var organizer = GetMeetingOrganizer(entity);
			StartMeetingSynchronization(meetingId, new List<Contact> { participantContact, organizer });
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// <see cref="BaseEntityEventListener.OnInserted(object, EntityAfterEventArgs)"/>
		/// </summary>
		public override void OnInserted(object sender, EntityAfterEventArgs e) {
			var entity = (Entity)sender;
			Logger?.LogInfo($"Add new participant '{entity.PrimaryColumnValue}' to meeting.");
			base.OnInserted(sender, e);
			if (IsNeedDoAction(entity)) {
				AddParticipantToMeeting(entity);
			} else {
				Logger?.LogInfo($"Add new participant '{entity.PrimaryColumnValue}' to meeting no need to action, action skipped.");
			}
		}

		/// <summary>
		/// <see cref="BaseEntityEventListener.OnDeleted(object, EntityAfterEventArgs)"/>
		/// </summary>
		public override void OnDeleted(object sender, EntityAfterEventArgs e) {
			var entity = (Entity)sender;
			Logger?.LogDebug($"Delete participant '{entity.PrimaryColumnValue}' from meeting.");
			base.OnDeleted(sender, e);
			if (IsNeedDoAction(entity)) {
				DeleteParticipantFromMeeting(entity);
			} else {
				Logger?.LogInfo($"Delete participant '{entity.PrimaryColumnValue}' from meeting no need to action, action skipped.");
			}
		}
		
		#endregion

	}

	#endregion

}
