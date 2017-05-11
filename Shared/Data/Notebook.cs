using System;
using System.Collections.ObjectModel;

namespace SafeNotebooks
{
	public class Notebook : Item
	{
		public ObservableCollection<Page> Pages = null;

		public Notebook()
		{
		}

	}
}
