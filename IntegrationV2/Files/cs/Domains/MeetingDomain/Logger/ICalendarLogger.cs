namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Logger
{
	using System;
	using System.Runtime.Serialization;

	#region Interface: ICalendarLogger

	/// <summary>
	/// Calendar logger.
	/// </summary>
	public interface ICalendarLogger
	{

		#region Properties: Public

		/// <summary>
		/// Logger session unique identifier.
		/// </summary>
		[DataMember]
		string SessionId { get; }

		#endregion

		#region Methods: Public

		/// <summary>
		/// Log <paramref name="message"/> with debug level.
		/// </summary>
		/// <param name="message">Log message.</param>
		void LogDebug(string message);

		/// <summary>
		/// Log <paramref name="message"/> with information level.
		/// </summary>
		/// <param name="message">Log message.</param>
		void LogInfo(string message);

		/// <summary>
		/// Log <paramref name="message"/> with warning level.
		/// </summary>
		/// <param name="message">Log message.</param>
		void LogWarn(string message, Exception e = null);

		/// <summary>
		/// Log <paramref name="message"/> with error level.
		/// </summary>
		/// <param name="message">Log message.</param>
		void LogError(string message, Exception e);

		/// <summary>
		/// Set model name to logger.
		/// </summary>
		/// <param name="mailboxName">Model name.</param>
		void SetModelName(string modelName);

		/// <summary>
		/// Set new action name to logger while <paramref name="action"/> is executing.
		/// </summary>
		/// <param name="syncAction">Action name.</param>
		/// <param name="action"><see cref="Action"/></param>
		void SetAction(SyncAction syncAction);

		#endregion

	}

	#endregion

}
