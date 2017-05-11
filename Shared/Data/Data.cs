using System;
using System.Collections.ObjectModel;

namespace SafeNotebooks
{
	public class Data
	{
		public static string MsgPageSelected = "MsgPageSelected";

		public ObservableCollection<Notebook>  Notebooks = null;

		public Data()
		{
			Notebooks = new ObservableCollection<Notebook>();

			// Initial data...

			Notebooks.Add(
				new Notebook() 
				{ 
					Name = "Private", 
					Pages = new ObservableCollection<Page>()
					{
						new Page() { Name = "P Page 1", },
						new Page() { Name = "P Page 2", },
					}
				});

			Notebooks.Add(
				new Notebook()
				{
					Name = "Work", 
					Pages = new ObservableCollection<Page>()
					{
						new Page() { Name = "W Page 1", },
						new Page() { Name = "W Page 2", },
					}
				});
		}

		//

	}
}
