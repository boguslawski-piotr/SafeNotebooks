using System;
using System.Collections.ObjectModel;
using pbXSecurity;

namespace SafeNotebooks
{
	public class Data
	{
        public ISecretsManager SecretsManager { get; set; }

        public ObservableCollection<Notebook>  Notebooks = null;

		public Data()
		{
			Notebooks = new ObservableCollection<Notebook>();

		}

		//

		public Notebook SelectedNotebook = null;

		public event EventHandler<Notebook> NotebookSelected;

		public bool SelectNotebook(Notebook notebook, bool remember = true)
		{
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
