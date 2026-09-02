namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces
{
	using System;
	using System.Collections.Generic;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Logger;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;

	#region Interface: IMeetingRepository

	/// <summary>
	/// Meeting repository interface.
	/// </summary>
	public interface IMeetingRepository
	{

		#region Methods: Public

		/// <summary>
		/// Gets list of <see cref="Meeting"/> models by <paramref name="meetingId"/>.
		/// </summary>
		/// <param name="activityId">Activity record identifier.</param>
		/// <returns><see cref="Meeting"/> instance collection.</returns>
		List<Meeting> GetMeetings(Guid meetingId);

		/// <summary>
		/// Gets internal calendar meetings for export.
		/// </summary>
		/// <param name="contactId">User contact identifier.</param>
		/// <param name="syncSinceDate">Since date.</param>
		/// <returns><see cref="Meeting"/> instance collection.</returns>
		List<Meeting> GetMeetings(Guid contactId, DateTime syncSinceDate);

		/// <summary>
		/// Gets internal calendar meetings for export.
		/// </summary>
		/// <param name="contactId">User contact identifier.</param>
		/// <param name="sinceDate">Since date.</param>
		/// <param name="dueDate">Due date.</param>
		/// <returns><see cref="Meeting"/> instance collection.</returns>
		List<Meeting> GetMeetings(Guid contactId, DateTime sinceDate, DateTime dueDate);

		/// <summary>
		/// Gets list of <see cref="Meeting"/> models by <paramref name="iCalUid"/>.
		/// </summary>
		/// <param name="iCalUid">External calendar record identifier.</param>
		/// <returns><see cref="Meeting"/> instance collection.</returns>
		List<Meeting> GetMeetings(string iCalUid);

		/// <summary>
		/// Gets list of <see cref="Meeting"/> models by <paramref name="activityId"/>.
		/// </summary>
		/// <param name="args">Meeting unique identifier.</param>
		/// <returns>List of <see cref="Meeting"/> instances.</returns>
		List<Meeting> GetDeletedMeetings(Guid meetingId);

		/// <summary>
		/// Saves synchronization metadata.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> model instance.</param>
		void Save(Meeting meeting);

		/// <summary>
		/// Removes synchronization metadata and entity.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		void Delete(Meeting meeting);

		/// <summary>
		/// Removes synchronization metadata.
		/// </summary>
		/// <param name="meeting"><see cref="Meeting"/> instance.</param>
		void RemoveFromCalendar(Meeting meeting);

		#endregion

	}

	#endregion

}
