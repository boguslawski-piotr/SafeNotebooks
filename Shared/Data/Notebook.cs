using System;
using System.Collections.ObjectModel;

namespace SafeNotebooks
{
	public class Notebook : Item
	{
		public ObservableCollection<Page> Pages = null;

		public Notebook()
		{
			Pages = new ObservableCollection<Page>();
		}

		public void AddPage(Page page)
		{
			page.Parent = this;
			Pages.Add(page);
		}
	}
}
