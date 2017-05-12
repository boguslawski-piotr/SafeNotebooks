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
			n.AddPage(new Page() { Name = "Page 0", });
			Notebooks.Add(n);

			n = new Notebook()
			{
				Name = "Work (better none inside...)",
			};
			n.AddPage(new Page() { Name = "Page 0", });
			n.AddPage(new Page() { Name = "Page 0", });
			Notebooks.Add(n);

		}

		//

	}
}
