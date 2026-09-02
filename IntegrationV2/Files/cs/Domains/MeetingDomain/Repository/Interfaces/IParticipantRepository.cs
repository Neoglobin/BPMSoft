namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces
{
	using System;
	using System.Collections.Generic;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;

	#region Interface: IParticipantRepository

	/// <summary>
	/// Participant repository interface.
	/// </summary>
	public interface IParticipantRepository
	{

		#region Methods: public

		/// <summary>
		/// Gets <see cref="Participant"/> collection by <paramref name="meetingId"/>.
		/// </summary>
		/// <param name="meetingId"><see cref="Meeting"/> instance identifier.</param>
		/// <returns><see cref="Participant"/> collection.</returns>
		List<Participant> GetMeetingParticipants(Guid meetingId);

		/// <summary>
		/// Gets <see cref="Participant"/> by identifier.
		/// </summary>
		/// <param name="participantId"><see cref="Participant"/> identifier.</param>
		/// <returns><see cref="Participant"/> instance.</returns>
		Participant GetParticipant(Guid participantId);

		/// <summary>
		/// Creates <see cref="Participant"/> instance for participant record.
		/// </summary>
		/// <param name="meetingId">Meeting identifier.</param>
		/// <param name="contact">Contact identifier.</param>
		/// <returns><see cref="Participant"/> instance.</returns>
		Participant GetParticipant(Guid meetingId, Contact contact);

		/// <summary>
		/// Returns list of <see cref="Participant"/> by email.
		/// </summary>
		/// <param name="meetingId">Meeting identifier.</param>
		/// <param name="emails">Participant emails.</param>
		/// <returns>List of <see cref="Participant"/>.</returns>
		List<Participant> GetParticipants(Guid meetingId, List<string> emails);

		/// <summary>
		/// Returns list of <see cref="Contact"/> that are related to <paramref name="emails"/>.
		/// </summary>
		/// <param name="emails">Emails list.</param>
		/// <returns>List of <see cref="Contact"/>.</returns>
		List<Contact> GetParticipantContacts(List<string> emails);

		/// <summary>
		/// Updates meeting participants list in DB.
		/// Deletes existing participants that are not in <paramref name="actualParticipants"/>,
		/// craetes not existing participants from <paramref name="actualParticipants"/>.
		/// </summary>
		/// <param name="actualParticipants">Actual participants list.</param>
		void UpdateMeetingParticipants(List<Participant> actualParticipants);

		/// <summary>
		/// Update invitation status of participant in the meeting.
		/// </summary>
		/// <param name="participant"><see cref="Participant"/> instance.</param>
		void UpdateParticipantInvitation(Participant participant);

		/// <summary>
		/// Delete participants.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance identifier.</param>
		void Delete(Guid meetingId);

		/// <summary>
		/// Save participant
		/// </summary>
		/// <param name="participantId">Participant unique identifier.</param>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		void Save(Meeting meeting);

		#endregion

	}

	#endregion

}
