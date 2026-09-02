namespace IntegrationV2.Files.cs.Domains.MeetingDomain.Model
{
	using System;
	using System.Collections.Generic;

	#region Class: Contact

	public class Contact
	{

		#region Properties: Public

		public string Name { get; private set; }

		public Guid Id { get; private set; }

		public string EmailAddress { get; private set; }

		#endregion

		#region Constructors: Public

		public Contact() : this(Guid.Empty, string.Empty) { }

		public Contact(Guid id, string name) {
			Id = id;
			Name = name;
		}

		public Contact(Guid id, string name, string email) {
			Id = id;
			Name = name;
			EmailAddress = email;
		}

		#endregion

		#region Methods: Public

		/// <summary>
		/// Set email address.
		/// </summary>
		/// <param name="email">Email address.</param>
		public void SetEmail(string email) {
			EmailAddress = email;
		}

		public override string ToString() {
			return $"[\"{Id}\" \"{Name}\"]";
		}

		#endregion

	}

	#endregion

}
