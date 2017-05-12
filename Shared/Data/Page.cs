using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public class Page : Item
	{
		public ObservableCollection<Note> Notes = null;

		public Page()
		{
			Notes = new ObservableCollection<Note>();
		}

		public void AddNote(Note note)
		{
			note.Parent = this;
			Notes.Add(note);
		}
	}
}
