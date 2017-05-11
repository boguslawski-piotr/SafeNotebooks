using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public class Page : Item
	{
		public ObservableCollection<Note> Items = null;

		public Page()
		{
			Items = new ObservableCollection<Note>();
		}
	}
}
