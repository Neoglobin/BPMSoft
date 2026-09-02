namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Client.Interfaces
	{
	using System.Collections.Generic;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Client;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.DTO;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;

	#region Interface: ICalendarClient

	/// <summary>
	/// Calendar client interface.
	/// </summary>
	public interface ICalendarClient
	{

		#region Methods: Public

		/// <summary>
		/// Save <see cref="Meeting"/> in <see cref="Calendar"/>.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> model instance.</param>
		/// <param name="calendar"><see cref="Calendar"/> model instance.</param>
		/// <returns><see cref="Meeting"/> unique identifier in the external store.</returns>
		IntegrationId SaveMeeting(Meeting meeting, Calendar calendar);

		/// <summary>
		/// Removes <paramref name="meeting"/> from <paramref name="calendar"/>.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="calendar"><see cref="Calendar"/> instance.</param>
		void RemoveMeeting(Meeting meeting, Calendar calendar);

		/// <summary>
		/// Sends <paramref name="meeting"/> invites in <paramref name="calendar"/>.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		/// <param name="calendar"><see cref="Calendar"/> instance.</param>
		void SendInvitations(Meeting meeting, Calendar calendar);

		/// <summary>
		/// Requests appointments from external calendar and converts them to meetings.
		/// </summary>
		/// <param name="calendar"><see cref="Calendar"/> instance.</param>
		/// <param name="hasErrors">Errors </param>
		/// <returns>List of <see cref="MeetingDto"/> instances.</returns>
		List<MeetingDto> GetSyncPeriodMeetings(Calendar calendar, out bool hasErrors);

		#endregion

	}

	#endregion

}
