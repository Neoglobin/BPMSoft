namespace IntegrationV2.Files.cs.Domains.MeetingDomain
{

	/// <summary>
	/// Invitation statuses.
	/// </summary>
	public enum InvitationState {
		
		/// <summary>
		/// No invitation status.
		/// </summary>
		Empty,

		/// <summary>
		/// Invitation confirmed.
		/// </summary>
		Confirmed,

		/// <summary>
		/// Invitation declined.
		/// </summary>
		Declined,

		/// <summary>
		/// Doubt invitation.
		/// </summary>
		InDoubt

	}

}
