namespace IntegrationV2.Files.cs.Domains.MeetingDomain {
	public enum SyncAction {
		CreateOrUpdate,
		Delete,
		SendInvite,
		ImportPeriod,
		ExportPeriod,
		UpdateWithInvite,
		DeleteWithInvite,
	}
}
