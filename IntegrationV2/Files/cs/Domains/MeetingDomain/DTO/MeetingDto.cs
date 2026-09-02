namespace IntegrationV2.Files.cs.Domains.MeetingDomain.DTO {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;

	#region Class: MeetingDto

	[ExcludeFromCodeCoverage]
	public class MeetingDto {

		#region Properties: Public

		public Guid Id { get; } = Guid.NewGuid();

		public string Title { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime DueDate { get; set; }

		public string Location { get; set; }

		public string Body { get; set; }

		public Guid PriorityId { get; set; }

		public bool InvitesSent { get; set; }

		public Dictionary<string, InvitationState> Participants { get; set; } = 
			new Dictionary<string, InvitationState>(StringComparer.InvariantCultureIgnoreCase);

		public bool RemindToOwner { get; set; }

		public DateTime RemindToOwnerDate { get; set; }

		public string RemoteId { get; set; }

		public string ICalUid { get; set; }

		public string OrganizerEmail { get; set; }

		public bool IsPrivate { get; set; }

		public DateTime RemoteCreatedOn { get; set; }

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="object.ToString()"/>
		public override string ToString() {
			return $"[\"{Id}\" \"{Title}\" \"{StartDate}\" \"{DueDate}\" \"{ICalUid}\" \"Participants\": \"{string.Join(", ", Participants.Keys)}\"]";
		}

		#endregion

	}

	#endregion

}
