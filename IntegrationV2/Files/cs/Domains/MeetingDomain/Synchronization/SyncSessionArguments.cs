namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Synchronization
{
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	#region Class: SyncSessionArguments

	/// <summary>
	/// Synchronization session arguments.
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class SyncSessionArguments
	{

		#region Properties: Public

		public Guid MeetingId { get; set; }

		public List<Contact> Contacts { get; set; } = new List<Contact>();

		public SyncAction SyncAction { get; set; } = SyncAction.CreateOrUpdate;

		public string SessionId { get; set; }

		public bool IsBackgroundProcess { get; set; }

		public Dictionary<string, object> OldColumnsValues { get; set; } = new Dictionary<string, object>();

		#endregion

	}

	#endregion

}
