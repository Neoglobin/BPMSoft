namespace BPMSoft.EmailDomain.EventProcessing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using EmailContract.DTO;
	using IntegrationApi.Email;
	using IntegrationApi.Interfaces;
	using Newtonsoft.Json;
	using BPMSoft.Configuration;
	using BPMSoft.Core;
	using BPMSoft.Core.Configuration;
	using BPMSoft.Core.Entities;
	using BPMSoft.Core.Factories;
	using BPMSoft.EmailDomain.Interfaces;
	using BPMSoft.IntegrationV2.Logging.Interfaces;
	using BPMSoft.IntegrationV2.Utils;

	#region Class: LoadEmailEventExecutor

	/// <summary>
	/// Synchronization action for full emails events.
	/// </summary>
	[DefaultBinding(typeof(LoadEmailEventExecutor))]
	public class LoadEmailEventExecutor : BaseLoadEmailEventExecutor
	{

		#region Constants: Private

		private const string _integrationSystemName = "EmailSynchronization";

		#endregion

		#region Fields: Private

		private ISynchronizationLogger _log;

		#endregion

		#region Methods: Private

		/// <summary>
		/// Locks email for synchronization in current task.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		/// <param name="emailDTO"><see cref="Email"/> instance.</param>
		/// <param name="helper"><see cref="IEntitySynchronizerHelper"/> implementation instance.</param>
		/// <returns><c>True</c> if email locked for current task. Otherwise returns <c>false</c>.</returns>
		private bool LockItemForSync(UserConnection uc, Email emailDTO, IEntitySynchronizerHelper helper) {
			return helper.CanCreateEntityInLocalStore(emailDTO.MessageId, uc, _integrationSystemName);
		}

		/// <summary>
		/// Unlocks locked in this session emails.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		/// <param name="ids">Locked record identifiers.</param>
		/// <param name="helper"><see cref="IEntitySynchronizerHelper"/> implementation instance.</param>
		private void UnlockSyncedEntities(UserConnection uc, List<string> ids, IEntitySynchronizerHelper helper) {
			if (!ids.Any()) {
				return;
			}
			helper.UnlockEntities(ids, _integrationSystemName, uc);
		}

		/// <summary>
		/// Unlocks locked in this session emails.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		/// <param name="emailDTO"><see cref="Email"/> instance.</param>
		/// <param name="helper"><see cref="IEntitySynchronizerHelper"/> implementation instance.</param>
		private void UnlockEmail(UserConnection uc, Email emailDTO, IEntitySynchronizerHelper helper) {
			UnlockSyncedEntities(uc, new List<string> { emailDTO.MessageId }, helper);
		}

		/// <summary>
		/// Action on EmailMessageData.
		/// </summary>
		/// <param name="userConnection"><see cref="UserConnection"/> instance.</param>
		/// <param name="synsSessionId">Session identifier.</param>
		/// <param name="emailMessageData"><see cref="Entity"/> instance of EmailMessageData.</param>
		private void PerformMessageData(UserConnection userConnection, string synsSessionId, Entity emailMessageData) {
			if (emailMessageData != null && emailMessageData.StoringState == StoringObjectState.New) {
				emailMessageData.Save();
				SaveSyncSession(userConnection, synsSessionId);
			}
		}

		/// <inheritdoc cref="BaseLoadEmailEventExecutor.Synchronize(UserConnection, IDictionary{string, object})"/>
		protected override void Synchronize(UserConnection uc, IDictionary<string, object> parameters) {
			var mailboxId = (Guid)parameters["MailboxId"];
			var synsSessionId = string.Format("LoadEmailEventSyncSession_{0}", Guid.NewGuid());
			_log.Info($"[mailbox {mailboxId} session {synsSessionId}] LoadEmailEventExecutor.Synchronize started");
			var emails = parameters["Items"] as IEnumerable<Email>;
			if (!emails.Any()) {
				_log.Info($"[mailbox {mailboxId} session {synsSessionId}] LoadEmailEventExecutor.Synchronize - no emails passed");
				return;
			}
			var subject = emails.First().Subject;
			_log.Info($"SessionId: {synsSessionId}. LoadEmailEventExecutor.Synchronize started for mailbox: {mailboxId}. Email subject: {subject}");
			if (ListenerUtils.GetIsFeatureDisabled(uc, "SkipLicenseCheckForEmailSync") && !uc.LicHelper.GetHasOperationLicense("Login")) {
				_log.Info($"[mailbox {mailboxId} session {synsSessionId}] LoadEmailEventExecutor.Synchronize - user has no license!");
				return;
			}
			var emailService = ClassFactory.Get<IEmailService>(new ConstructorArgument("uc", uc));
			var lockedEmailIds = new List<string>();
			var helper = ClassFactory.Get<IEntitySynchronizerHelper>();
			try {
				foreach (var emailDto in emails) {
					if (emailDto == null) {
						continue;
					}
					if (string.IsNullOrEmpty(emailDto.MessageId)) {
						_log.Warn($"[mailbox {mailboxId} session {synsSessionId}] - item with empty MessageId is skipped, \r\n" +
							$"{JsonConvert.SerializeObject(emailDto, Formatting.Indented)}.");
						continue;
					}
					if (LockItemForSync(uc, emailDto, helper)) {
						lockedEmailIds.Add(emailDto.MessageId);
						_log.Info($"SessionId: {synsSessionId}. Item {emailDto.MessageId} locked for sync for mailbox: {mailboxId}. Email subject: {subject}");
						var emailMessageData = emailService.GetEmailMessageData(emailDto, mailboxId, synsSessionId);
						UnlockEmail(uc, emailDto, helper);
						PerformMessageData(uc, synsSessionId, emailMessageData);
					} else {
						_log.Info($"[mailbox {mailboxId} session {synsSessionId}] - item {emailDto.MessageId} already locked");
						NeedReRun = true;
					}
				}
			} finally {
				UnlockSyncedEntities(uc, lockedEmailIds, helper);
			}
			_log.Info($"SessionId: {synsSessionId}. LoadEmailEventExecutor.Synchronize ended for mailbox: {mailboxId}. Email subject: {subject}");
		}

		/// <summary>
		/// Sends synchronization session finish message.
		/// </summary>
		/// <param name="userConnection"><see cref="UserConnection"/> instance.</param>
		/// <param name="synsSessionId">Session identifier.</param>
		protected void SaveSyncSession(UserConnection userConnection, string synsSessionId) {
			_log.Info($"SessionId: {synsSessionId}. Start FSS");
			var userConnectionParam = new ConstructorArgument("userConnection", userConnection);
			var helper = ClassFactory.Get<IEmailMessageHelper>(userConnectionParam);
			helper.SendSyncSessionFinished(synsSessionId);
			_log.Info($"SessionId: {synsSessionId}. Finished FSS");
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="BaseLoadEmailEventExecutor.Run(IDictionary{string, object})"/>
		public override void Run(IDictionary<string, object> parameters) {
			var mailboxId = (Guid)parameters["MailboxId"];
			_log = ClassFactory.Get<ISynchronizationLogger>();
			_log.Info($"LoadEmailEventExecutor.Run started for mailbox: {mailboxId}.");
			try {
				base.Run(parameters);
			} catch (RetryExchangeEventExeption) {
				var emails = parameters["Items"] as IEnumerable<Email>;
				var emailsStr = emails != null && emails.Any()
					? string.Join("; ", emails.Select(e => $"{e.Subject}, SendDate timestamp {e.SendDateTimeStamp}, From {e.Sender}, id {e.MessageId}"))
					: string.Empty;
				_log.Warn($"LoadEmailEventExecutor.Run limit reached for mailbox: {mailboxId}, emails: [{emailsStr}]");
			}
		}

		#endregion

	}

	#endregion

}
