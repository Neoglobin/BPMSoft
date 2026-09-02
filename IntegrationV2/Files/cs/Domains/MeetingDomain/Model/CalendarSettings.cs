namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Model
{
	using System;
	using IntegrationV2.Files.cs.Utils;
	using BPMSoft.Core;

	#region Class: CalendarSettings

	/// <summary>
	/// Calendar settings domain model.
	/// </summary>
	public class CalendarSettings
	{

		#region Fields: Private

		private readonly string _oauthClassName;

		#endregion

		#region Properties: Public

		public Guid Id { get; set; }

		public string SenderEmailAddress { get; set; }

		public string Login { get; set; }

		public string Password { get; set; }

		public string ServiceUrl { get; set; }

		public Guid OAuthApplicationId { get; set; }

		public string AccessToken { get; private set; }

		public bool SyncEnabled { get; set; }

		public bool UseOAuth {
			get {
				return OAuthApplicationId != Guid.Empty;
			}
		}

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="oauthClassName">Oauth class name.</param>
		public CalendarSettings(string oauthClassName = null) {
			_oauthClassName = oauthClassName;
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Refresh calendar access token.
		/// </summary>
		/// <param name="oauthClassName"><see cref="UserConnection"/> instane.</param>
		/// <exception cref="ArgumentException">When <paramref name="uc"/>, <see cref="UseOAuth"/> 
		/// or <see cref="SenderEmailAddress"/> has invalid states.</exception>
		public void RefreshAccessToken(UserConnection uc) {
			if (!UseOAuth) {
				return;
			}
			if (string.IsNullOrEmpty(_oauthClassName) || uc == null || string.IsNullOrEmpty(SenderEmailAddress)) {
				throw new ArgumentException("Failed refresh calendar token, invalid arguments states.");
			}
			AccessToken = OauthUtils.RefreshAccessToken(_oauthClassName, SenderEmailAddress, uc);
		}

		public override string ToString() {
			return $"[Calendar settings => \"{Id}\" \"{SenderEmailAddress}\" \"{ServiceUrl}\" \"{(UseOAuth ? "Oauth" : $"Login:{Login}")}\"]";
		}

		#endregion
	}

	#endregion

}