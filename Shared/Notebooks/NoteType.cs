using System;

namespace SafeNotebooks
{
	public enum NoteType
	{
		/// <summary>
		/// The note. Name, Note (rich text), Zero or more attachments (any file).
		/// </summary>
		Note,

		/// <summary>
		/// The checklist. Name, List (TODO: jakie atrybuty każda pozycja?)
		/// </summary>
		Checklist,

		/// <summary>
		/// The secret. Name, Type (service, payment card, document?, other), 
		/// </summary>
		Secret,

		/// <summary>
		/// The user defined.
		/// </summary>
		UserDefined = 128,
	}
}
