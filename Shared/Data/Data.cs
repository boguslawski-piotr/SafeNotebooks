using System;
using System.Collections.ObjectModel;

namespace SafeNotebooks
{
	public class Data
	{
		public ObservableCollection<Notebook>  Notebooks = null;

		public Data()
		{
			Notebooks = new ObservableCollection<Notebook>();

			// Initial data...

			Notebook n = new Notebook()
			{
				Name = "Private"
			};
			n.AddPage(new Page() { Name = "Page 0", });
			n.AddPage(new Page() { Name = "Page 1", });
			Notebooks.Add(n);

			n = new Notebook()
			{
				Name = "Work (better none inside...)",
			};
			n.AddPage(new Page() { Name = "Page 0", });
			n.AddPage(new Page() { Name = "Page 1", });
			Notebooks.Add(n);

		}

		//

		public Notebook SelectedNotebook = null;

		public event EventHandler<Notebook> NotebookSelected;

		public void SelectNotebook(Notebook notebook)
		{
			SelectedNotebook = notebook;
			NotebookSelected?.Invoke(this, SelectedNotebook);
		}


		public Page SelectedPage = null;

		public event EventHandler<Page> PageSelected;

		public void SelectPage(Page page)
		{
			SelectedPage = page;
			PageSelected?.Invoke(this, SelectedPage);
		}

	}
}
