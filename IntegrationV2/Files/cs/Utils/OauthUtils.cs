namespace IntegrationV2.Files.cs.Utils
{
	using BPMSoft.Core;
	using BPMSoft.Core.Factories;
	using BPMSoft.Social.OAuth;


	public static class OauthUtils
	{

		#region Methods: Public

		/// <summary>
		/// Refresh access token for <paramref name="senderEmailAddress"/> account.
		/// </summary>
		/// <param name="oauthClassName">Oauth calss name.</param>
		/// <param name="senderEmailAddress"> Sender email address.</param>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		/// <returns>New access token.</returns>
		public static string RefreshAccessToken(string oauthClassName, string senderEmailAddress, UserConnection uc) {
			OAuthClient oauthClient = ClassFactory.Get<OAuthClient>(oauthClassName,
							new ConstructorArgument("userLogin", senderEmailAddress),
							new ConstructorArgument("userConnection", uc));
			return oauthClient.GetAccessToken();
		}

		#endregion

	}
}
