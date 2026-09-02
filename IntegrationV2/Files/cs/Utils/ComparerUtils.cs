namespace IntegrationV2.Files.cs.Utils
{
	using IntegrationV2.Files.cs.Domains.MeetingDomain.DTO;
	using IntegrationV2.Files.cs.Domains.MeetingDomain.Model;
	using System;
	using System.Collections.Generic;

	#region Class: ComparerUtils

	public static class ComparerUtils
	{

		#region Methods: Public

		/// <summary>
		/// Gets implementation of <see cref="IEqualityComparer{T}"/> interface.
		/// </summary>
		/// <typeparam name="T">Type that implements <see cref="IEqualityComparer{T}"/> interface.</typeparam>
		/// <returns>Implementation of <see cref="IEqualityComparer{T}"/> interface.</returns>
		public static IEqualityComparer<T> GetComparer<T>() where T: new() {
			Type type = typeof(T);
			switch (true) {
				case true when typeof(Contact).IsAssignableFrom(type):
					return new ContactComparer() as IEqualityComparer<T>;
				case true when typeof(MeetingDto).IsAssignableFrom(type):
					return new MeetingDtoComparer() as IEqualityComparer<T>;
				default:
					if (new T() is IEqualityComparer<T> defaultComparer) {
						return defaultComparer;
					} else {
						throw new InvalidCastException($"{type.FullName} does not inherit IEqualityComparer<T>.");
					}
			}
		}

		#endregion

	}

	#endregion

	#region Class: MeetingDtoComparer

	class ContactComparer: IEqualityComparer<Contact>
	{

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		public ContactComparer() { }

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="IEqualityComparer{Contact}.Equals(Contact, Contact)"/>
		public bool Equals(Contact x, Contact y) {
			return x.Id == y.Id;
		}

		/// <inheritdoc cref="IEqualityComparer{Contact}.GetHashCode(Contact, Contact)"/>
		public int GetHashCode(Contact obj) {
			return obj.Id.GetHashCode();
		}

		#endregion

	}

	#endregion

	#region Class: MeetingDtoComparer

	class MeetingDtoComparer: IEqualityComparer<MeetingDto>
	{

		#region Constructors: Public

		/// <summary>
		/// .ctor.
		/// </summary>
		public MeetingDtoComparer() { }

		#endregion

		#region Methods: Public

		/// <inheritdoc cref="IEqualityComparer{MeetingDto}.Equals(MeetingDto, MeetingDto)"/>
		public bool Equals(MeetingDto x, MeetingDto y) {
			return x.ICalUid == y.ICalUid && x.RemoteId == y.RemoteId;
		}

		/// <inheritdoc cref="IEqualityComparer{MeetingDto}.GetHashCode(MeetingDto)"/>
		public int GetHashCode(MeetingDto obj) {
			return obj.ICalUid.GetHashCode() & obj.RemoteId.GetHashCode();

		}

		#endregion

	}

	#endregion

}
