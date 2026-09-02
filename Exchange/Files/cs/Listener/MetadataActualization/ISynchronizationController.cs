namespace BPMSoft.Configuration
{

	using BPMSoft.Core;

	#region Interface: ISynchronizationController

	/// <summary>
	/// Classes that implements this interface can be selected as synchronization handlers.
	/// </summary>
	public interface ISynchronizationController : IJobExecutor
	{
	}

	#endregion

}