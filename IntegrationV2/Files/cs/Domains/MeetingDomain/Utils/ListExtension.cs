namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Utils
{
	using System.Collections.Generic;
	using System.Linq;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.DTO;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;

	#region Class: ListExtension

	/// <summary>
	/// List<T> extensions.
	/// </summary>
	static class ListExtension
	{

		#region Methods: Public

		/// <summary>
		/// Converting <paramref name="meetings"/> to readable string.
		/// </summary>
		/// <param name="meetings"><see cref="Meeting"/> list.</param>
		/// <returns>Readable string of <paramref name="meetings"/>.</returns>
		public static string GetString(this List<Meeting> meetings) {
			return string.Join(",\r\n", meetings.Select(m => m.ToString()));
		}

		/// <summary>
		/// Converting <paramref name="meetings"/> to readable string.
		/// </summary>
		/// <param name="meetings"><see cref="MeetingDto"/> list.</param>
		/// <returns>Readable string of <paramref name="meetings"/>.</returns>
		public static string GetString(this List<MeetingDto> meetings) {
			return string.Join(",\r\n", meetings.Select(m => m.ToString()));
		}

		/// <summary>
		/// Converting <paramref name="contacts"/> to readable string.
		/// </summary>
		/// <param name="meetings"><see cref="Contact"/> list.</param>
		/// <returns>Readable string of <paramref name="contacts"/>.</returns>
		public static string GetString(this List<Contact> contacts) {
			return string.Join(", ", contacts.Select(m => m.ToString()));
		}

		#endregion

	}

	#endregion

}
