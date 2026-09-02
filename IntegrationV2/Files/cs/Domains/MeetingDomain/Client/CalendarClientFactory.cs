namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Client
{
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Client.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using BPMSoft.Core.Factories;

	#region Class: CalendarClientFactory

	/// <summary>
	/// Calendar client factory.
	/// </summary>
	public class CalendarClientFactory
	{

		#region Consts: Private

		private const string _exchangeClientBindName = "Exchange";

		private const string _googleClientBindName = "Google";

		#endregion

		#region Methods: Public

		/// <summary>
		/// Get <see cref="ICalendarClient"/> instance.
		/// </summary>
		/// <param name="calendar"><see cref="Calendar"/> model.</param>
		/// <returns><see cref="ICalendarClient"/> instance.</returns>
		public static ICalendarClient GetCalendarClient(Calendar calendar) {
			var calendarClientName = calendar.Type == CalendarType.Exchange ? _exchangeClientBindName : _googleClientBindName;
			return ClassFactory.Get<ICalendarClient>(calendarClientName);
		}


		/// <summary>
		/// Get <see cref="ICalendarClient"/> instance.
		/// </summary>
		/// <param name="calendar"><see cref="Calendar"/> model.</param>
		/// <param name="sessionId">Synchronization session identifier.</param>
		/// <returns><see cref="ICalendarClient"/> instance.</returns>
		public static ICalendarClient GetCalendarClient(Calendar calendar, string sessionId) {
			var calendarClientName = calendar.Type == CalendarType.Exchange ? _exchangeClientBindName : _googleClientBindName;
			return ClassFactory.Get<ICalendarClient>(calendarClientName, new ConstructorArgument("sessionId", sessionId));
		}

		#endregion

	}

	#endregion

}
