namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Client
{
	using System.Diagnostics.CodeAnalysis;

	#region Class: ItegrationId

	[ExcludeFromCodeCoverage]
	public class IntegrationId
	{

		#region Properties: Public

		/// <summary>
		/// Remote integration item identifier.
		/// </summary>
		public string RemoteId { get; set; }

		/// <summary>
		/// ICalUid protocol item identifier.
		/// </summary>
		public string ICalUid { get; set; }

		#endregion

		public IntegrationId(string remoteId, string iCalUid) {
			RemoteId = remoteId;
			ICalUid = iCalUid;
		}
	}

	#endregion

}
