namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Repository
{
	using System;
	using System.Linq;
	using System.Collections.Generic;
	using BPMSoft.Core;
	using BPMSoft.Core.Factories;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Repository.Interfaces;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using BPMSoft.Core.Entities;
	using BPMSoft.Common;
	using BPMSoft.Social.OAuth;

	#region Class: CalendarRepository

	/// <summary>
	/// <see cref="ICalendarRepository"/> implementation.
	/// </summary>
	[DefaultBinding(typeof(ICalendarRepository))]
	public class CalendarRepository : ICalendarRepository
	{

		#region Fields: Private

		/// <summary>
		/// <see cref="UserConnection"/> instance.
		/// </summary>
		private readonly UserConnection _userConnection;

		/// <summary>
		/// <param name="sessionId">Synchronization session identifier.</param>
		/// </summary
		string _sessionId;

		#endregion

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		/// <param name="uc"><see cref="UserConnection"/> instance.</param>
		public CalendarRepository(UserConnection uc) {
			_userConnection = uc;
		}

		#endregion

		#region Methods: Private

		private void AddEnabledCalendarFilters(EntitySchemaQuery esq, bool importRequired, bool exportRequired) {
			var filterGroup = new EntitySchemaQueryFilterCollection(esq, LogicalOperationStrict.Or);
			if (importRequired) {
				filterGroup.Add(esq.CreateFilterWithParameters(
					FilterComparisonType.Equal,
					"ImportAppointments",
					importRequired
				));
			}
			if (exportRequired) { 
				filterGroup.Add(esq.CreateFilterWithParameters(
					FilterComparisonType.Equal,
					"ExportActivities",
					exportRequired
				));
			}
			esq.Filters.Add(filterGroup);
		}

		private List<Calendar> ConvertEntitiesToModels(EntitySchemaQuery esq) {
			var calendarsEntities = esq.GetEntityCollection(_userConnection);
			var result = new List<Calendar>();
			foreach (var calendarEntity in calendarsEntities) {
				var calendar = new Calendar(calendarEntity, _sessionId);
				var senderEmailAddress = calendarEntity.GetTypedColumnValue<string>("SenderEmailAddress");
				var calendarSettings = new CalendarSettings(calendarEntity.GetTypedColumnValue<string>("OAuthClassName")) {
					Id = calendarEntity.GetTypedColumnValue<Guid>("SettingsId"),
					OAuthApplicationId = calendarEntity.GetTypedColumnValue<Guid>("OAuthApplicationId"),
					Login = calendarEntity.GetTypedColumnValue<string>("Login"),
					Password = calendarEntity.GetTypedColumnValue<string>("Password"),
					SenderEmailAddress = senderEmailAddress,
					ServiceUrl = calendarEntity.GetTypedColumnValue<string>("ServiceUrl"),
					SyncEnabled = calendarEntity.GetTypedColumnValue<bool>("ImportAppointments") ||
						calendarEntity.GetTypedColumnValue<bool>("ExportActivities")
				};
				calendar.SetOwner(
					calendarEntity.GetTypedColumnValue<Guid>("OwnerId"),
					calendarEntity.GetTypedColumnValue<string>("OwnerName"),
					senderEmailAddress
				);
				calendar.SetCalendarSettings(calendarSettings);
				result.Add(calendar);
			}
			return result;
		}

		private EntitySchemaQuery GetCalendarEsq() {
			var esq = new EntitySchemaQuery(_userConnection.EntitySchemaManager, "ActivitySyncSettings");
			esq.UseAdminRights = false;
			esq.PrimaryQueryColumn.IsAlwaysSelect = true;
			esq.AddColumn("ActivitySyncPeriod");
			esq.AddColumn("ImportAppointmentsAll");
			esq.AddColumn("ImportAppointmentsCalendars");
			esq.AddColumn("ExportActivities");
			esq.AddColumn("ImportAppointments");
			esq.AddColumn("MailboxSyncSettings").Name = "Settings";
			esq.AddColumn("MailboxSyncSettings.SysAdminUnit.Contact").Name = "Owner";
			esq.AddColumn("MailboxSyncSettings.SenderEmailAddress").Name = "SenderEmailAddress";
			esq.AddColumn("MailboxSyncSettings.UserName").Name = "Login";
			esq.AddColumn("MailboxSyncSettings.UserPassword").Name = "Password";
			esq.AddColumn("MailboxSyncSettings.MailServer.ExchangeEmailAddress").Name = "ServiceUrl";
			esq.AddColumn("MailboxSyncSettings.MailServer.OAuthApplication").Name = "OAuthApplication";
			esq.AddColumn("MailboxSyncSettings.MailServer.OAuthApplication.ClientClassName").Name = "OAuthClassName";
			return esq;
		}

		private void AddOwnerFilter(Guid ownerId, EntitySchemaQuery esq) {
			var ownerFiler = esq.CreateFilterWithParameters(FilterComparisonType.Equal,
				"MailboxSyncSettings.SysAdminUnit.Contact", ownerId);
			esq.Filters.Add(ownerFiler);
		}

		private void AddEmailFilter(EntitySchemaQuery esq, string email) {
			var esqFilter = esq.CreateFilterWithParameters(FilterComparisonType.Equal, "MailboxSyncSettings.SenderEmailAddress", email);
			esq.Filters.Add(esqFilter);
		}

		private List<Calendar> GetCalendars(Action<EntitySchemaQuery> filterAction, bool importRequired = true, bool exportRequired = true) {
			var esq = GetCalendarEsq();
			AddEnabledCalendarFilters(esq, importRequired, exportRequired);
			filterAction(esq);
			return ConvertEntitiesToModels(esq);
		}

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="ICalendarRepository.GetCalendar(Guid, bool, bool)"/>
		public Calendar GetCalendar(Guid ownerId, bool importRequired = true, bool exportRequired = true) {
			return GetCalendars((esq)=> AddOwnerFilter(ownerId, esq), importRequired, exportRequired).FirstOrDefault();
		}

		/// <inheritdoc cref="ICalendarRepository.GetCalendar(string, bool, bool)"/>
		public Calendar GetCalendar(string senderEmailAddress, bool importRequired = true, bool exportRequired = true) {
			if (senderEmailAddress.IsNullOrEmpty()) {
				return default;
			}
			return GetCalendars((esq) => AddEmailFilter(esq, senderEmailAddress), importRequired, exportRequired).FirstOrDefault();
		}

		/// <inheritdoc cref="ICalendarRepository.GetCalendar(Guid, string, bool, bool)"/>
		public Calendar GetCalendar(Guid ownerId, string sessionId, bool importRequired = true, bool exportRequired = true) {
			_sessionId = sessionId;
			return GetCalendar(ownerId, importRequired, exportRequired);
		}

		/// <inheritdoc cref="ICalendarRepository.GetAllCalendars(Guid, string)"/>
		public List<Calendar> GetAllCalendars(Guid ownerId, string sessionId) {
			_sessionId = sessionId;
			var ownerCalendars = GetCalendars((esq) => AddOwnerFilter(ownerId, esq), false, false);
			return ownerCalendars;
		}

		#endregion

	}

	#endregion

}
