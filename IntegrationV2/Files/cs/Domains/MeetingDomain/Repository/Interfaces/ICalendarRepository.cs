namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces
{
	using System;
	using System.Collections.Generic;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;

	#region Interface: ICalendarRepository

	/// <summary>
	/// Calendar repository interface.
	/// </summary>
	public interface ICalendarRepository
	{

		#region Methods: Public

		/// <summary>
		/// Get participant <see cref="Calendar"/>.
		/// </summary>
		/// <param name="ownerId">Calendar owner identifier.</param>
		/// <param name="importRequired">Import required.</param>
		/// <param name="exportRequired">Export required.</param>
		/// <returns>Participant <see cref="Calendar"/> instance.</returns>
		Calendar GetCalendar(Guid ownerId, bool importRequired = true, bool exportRequired = true);

		/// <summary>
		/// Get participant <see cref="Calendar"/>.
		/// </summary>
		/// <param name="senderEmailAddress">Calendar email.</param>
		/// <param name="importRequired">Import required.</param>
		/// <param name="exportRequired">Export required.</param>
		/// <returns>Participant <see cref="Calendar"/> instance.</returns>
		Calendar GetCalendar(string senderEmailAddress, bool importRequired = true, bool exportRequired = true);

		/// <summary>
		/// Get participant <see cref="Calendar"/>.
		/// </summary>
		/// <param name="ownerId">Calendar owner identifier.</param>
		/// <param name="sessionId">Synchronization session identifier.</param>
		/// <param name="importRequired">Import required.</param>
		/// <param name="exportRequired">Export required.</param>
		/// <returns>Participant <see cref="Calendar"/> instance.</returns>
		Calendar GetCalendar(Guid ownerId, string sessionId, bool importRequired = true, bool exportRequired = true);

		/// <summary>
		/// Get all participant <see cref="Calendar"/>.
		/// </summary>
		/// <param name="ownerId">Calendar owner identifier.</param>
		/// <param name="sessionId">Synchronization session identifier.</param>
		/// <returns>List of all owner <see cref="Calendar"/>`s.</returns>
		List<Calendar> GetAllCalendars(Guid ownerId, string sessionId);

		#endregion

	}

	#endregion

}
