namespace BPMSoft.IntegrationV2.Logging
{
	using System;
	using global::Common.Logging;
	using Newtonsoft.Json;
	using BPMSoft.Core.Factories;
	using BPMSoft.IntegrationV2.Logging.Interfaces;
	using BPMSoft.IntegrationV2.Utils;
	using BPMSoft.Messaging.Common;

	#region Class: SynchronizationLogger

	[DefaultBinding(typeof(ISynchronizationLogger))]
	internal class SynchronizationLogger : ISynchronizationLogger
	{

		#region Fields: Private

		private readonly ILog _log = LogManager.GetLogger("ExchangeListener");

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="ISynchronizationLogger.ErrorFormat(string, Exception, object[])"/>
		public void ErrorFormat(string format, Exception exception = null, params object[] args) {
			_log.ErrorFormat(format, exception, args);
		}

		/// <inheritdoc cref="ISynchronizationLogger.InfoFormat(string, object[])"/>
		public void InfoFormat(string format, params object[] args) {
			_log.InfoFormat(format, args);
		}

		/// <inheritdoc cref="ISynchronizationLogger.DebugFormat(string, object[])"/>
		public void DebugFormat(string format, params object[] args) {
			_log.DebugFormat(format, args);
		}

		/// <inheritdoc cref="ISynchronizationLogger.Error(string, Exception)"/>
		public void Error(string message, Exception exception = null) {
			_log.Error(message, exception);
		}

		/// <inheritdoc cref="ISynchronizationLogger.Warn(string)"/>
		public void Warn(string message) {
			_log.Warn(message);
		}

		/// <inheritdoc cref="ISynchronizationLogger.Info(string)"/>
		public void Info(string message) {
			_log.Info(message);
		}

		#endregion

	}

	#endregion

}
