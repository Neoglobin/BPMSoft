namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Logger
{
	using Common.Logging;
	using System;
	using System.Runtime.Serialization;
	using BPMSoft.Core.Factories;

	#region Class: CalendarLogger

	[DefaultBinding(typeof(ICalendarLogger))] 
	public class CalendarLogger: ICalendarLogger
	{

		#region Fields: Private

		private readonly ILog _log = LogManager.GetLogger("Calendar");

		private string _sessionId;

		#endregion

		#region Properties: Private

		private string Action { get; set; }

		private string ModelName { get; set; }

		#endregion

		#region Properties: Public

		private string _fullSessionId;

		/// <summary>
		/// Logger session unique identifier.
		/// </summary>
		public string SessionId {
			get {
				if (string.IsNullOrEmpty(_sessionId)) {
					_fullSessionId = GenerateSessionId();
				}
				return _fullSessionId;
			}
			private set {
				_fullSessionId = GenerateSessionId(value);
			}
		}

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="modelName">Model name.</param>
		public CalendarLogger(string modelName) {
			ModelName = modelName;
		}

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="sessionId">Syncronization session identifier.</param>
		/// <param name="modelName">Model name.</param>
		public CalendarLogger(string sessionId, string modelName, SyncAction syncAction = SyncAction.CreateOrUpdate) : this(modelName) {
			SetActionInternal(syncAction);
			ParseSessionId(sessionId);
		}

		/// <summary>
		/// .ctor.
		/// </summary>
		public CalendarLogger() { }

		#endregion

		#region Methods: Private

		/// <summary>
		/// Generate new synchronization session unique identifier.
		/// </summary>
		/// <param name="newSessionId">New session logger identifier.</param>
		/// <returns>New synchronization session unique identifier</returns>
		private string GenerateSessionId(string newSessionId = null) {
			if (string.IsNullOrEmpty(newSessionId)) {
				var base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
				base64Guid = base64Guid.Replace("+", string.Empty).Replace("/", string.Empty);
				_sessionId = base64Guid.Substring(0, base64Guid.Length - 2);
			} else {
				_sessionId = newSessionId;
			}
			return string.IsNullOrEmpty(Action) ? _sessionId : _sessionId + Action;
		}

		/// <summary>
		/// Get result log message.
		/// </summary>
		/// <param name="message">Log message.</param>
		/// <returns>Result log message.</returns>
		private string GetLogMeesage(string message) {
			var modelPrefix = string.IsNullOrEmpty(ModelName) ? string.Empty : $" [{ModelName}]";
			return $"[{SessionId}]{modelPrefix} {message}";
		}

		/// <summary>
		/// Sets action suffix to log message
		/// </summary>
		/// <param name="action"></param>
		private void SetActionInternal(SyncAction action) {
			switch (action) {
				case SyncAction.Delete:
					Action = ".delete";
					break;
				case SyncAction.DeleteWithInvite:
					Action = ".del_inv";
					break;
				case SyncAction.SendInvite:
					Action = ".invite";
					break;
				case SyncAction.ImportPeriod:
					Action = ".import";
					break;
				case SyncAction.ExportPeriod:
					Action = ".export";
					break;
				case SyncAction.UpdateWithInvite:
					Action = ".up_inv";
					break;
				default:
					Action = ".cr_up";
					break;
			}
		}

		/// <summary>
		/// Parse session identifier.
		/// </summary>
		/// <param name="sessionId">Session identifier.</param>
		private void ParseSessionId(string sessionId) {
			if (string.IsNullOrEmpty(sessionId)) {
				SessionId = GenerateSessionId();
			} else {
				var sptilts = sessionId.Split('.');
				if (sptilts.Length > 1) {
					Action = "." + sptilts[1];
				}
				SessionId = sptilts[0];
			}
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="ICalendarLogger.LogDebug(string)"/>
		public void LogDebug(string message) {
			_log.Debug(GetLogMeesage(message));
		}

		/// <inheritdoc cref="ICalendarLogger.LogInfo(string)"/>
		public void LogInfo(string message) {
			_log.Info(GetLogMeesage(message));
		}

		/// <inheritdoc cref="ICalendarLogger.LogWarn(string, Exception)"/>
		public void LogWarn(string message, Exception e = null) {
			if(e == null) {
				_log.Warn(GetLogMeesage(message));
			} else {
				_log.Warn(GetLogMeesage(message), e);
			}
		}

		/// <inheritdoc cref="ICalendarLogger.LogError(string, Exception)"/>
		public void LogError(string message, Exception e) {
			_log.Error(GetLogMeesage(message), e);
		}

		/// <inheritdoc cref="ICalendarLogger.SetModelName(string)"/>
		public void SetModelName(string modelName) {
			ModelName = string.IsNullOrEmpty(ModelName) ? modelName : $"{ModelName}.{modelName}";
		}

		/// <inheritdoc cref="ICalendarLogger.SetAction(SyncAction)"/>
		public void SetAction(SyncAction syncAction) {
			SetActionInternal(syncAction);
			_fullSessionId = _sessionId + Action;
		}

		#endregion

	}

	#endregion

}
