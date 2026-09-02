namespace BPMSoft.Configuration
{
	using BPMSoft.Core;
	using BPMSoft.Core.Factories;
	using BPMSoft.Social.OAuth;

	/// <summary>
	/// Represents an Office365 OAuth client.
	/// </summary>
	[DefaultBinding(typeof(OAuthClient), Name = "Office365OAuthClient")]
	public class Office365OAuthClient : OAuthClient
	{
		#region Constructors: Public

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="userLogin"></param>
		public Office365OAuthClient(string userLogin, UserConnection userConnection)
			: base(userLogin, userConnection) {
		}

		#endregion

		#region Methods: Protected

		/// <summary>
		/// Returns authenticator for the Office365 OAuth client.
		/// </summary>
		/// <returns><see cref="BPMSoft.Social.OAuth.IOAuthAuthenticator"/> instance.</returns>
		protected override IOAuthAuthenticator GetAuthenticator(UserConnection userConnection) {
			return new Office365OAuthAuthenticator(userConnection);
		}

		#endregion
	}
}