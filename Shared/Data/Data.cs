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

		public bool SelectNotebook(Notebook notebook, bool remember = true)
		{
			if (SelectedNotebook == notebook)
				return true;
			
			// TODO: unlock if protected

			// TODO: load additional data (fields that are encrypted and pages basic data)

			SelectedNotebook = notebook;
			NotebookSelected?.Invoke(this, SelectedNotebook);

			return true;
		}

		//

		public Page SelectedPage = null;

		public event EventHandler<Page> PageSelected;

		public bool SelectPage(Page page, bool remember = true)
		{
			if (SelectedPage == page)
				return true;
			
			// TODO: unlock if protected

			// TODO: load additional data (fields that are encrypted and notes basic data)

			SelectedPage = page;
			PageSelected?.Invoke(this, SelectedPage);

			return true;
		}

		//

		// TODO: notes

	}
}
