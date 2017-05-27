using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SafeNotebooks
{
    public class Page : ItemWithItems<Note>
    {
        public Notebook Notebook => Parent as Notebook;

		public const string IdForStoragePrefix = "P-";
		public override string IdForStorage => IdForStoragePrefix + Notebook?.Id + "-" + base.IdForStorage;

		class PageData
        {
            public NoteType DefaultNoteType = NoteType.Note;
        }

        PageData pdata;

        public override void Dispose()
        {
            pdata = null;
			base.Dispose();
		}

		protected override string Serialize()
        {
            return base.Serialize() +
                ",'pd':" + JsonConvert.SerializeObject(pdata, pbXNet.Settings.JsonSerializer);
        }

        protected override void Deserialize(JObject d)
        {
            base.Deserialize(d);
            pdata = JsonConvert.DeserializeObject<PageData>(d["pd"].ToString(), pbXNet.Settings.JsonSerializer);
        }

        protected override void InternalNew()
        {
            pdata = new PageData();
            base.InternalNew();
        }

        protected override async Task<bool> InternalLoadAsync(bool tryToUnlockChildren)
        {
            if (!await base.InternalLoadAsync(true))
                return false;

			string pattern = Note.IdForStoragePrefix + Id + "-\\w*";
            bool anyNoteLoaded = await NotebooksManager.LoadChildrenForItemHelperAsync<Note>(this, pattern, tryToUnlockChildren);

            if(anyNoteLoaded)
                SortItems();

			NotebooksManager.OnPageLoaded(this, anyNoteLoaded);
			return true;
		}

		public async Task<Note> NewNoteAsync()
		{
			Note note = new Note() { NotebooksManager = NotebooksManager };
			if (!await NotebooksManager.NewItemHelperAsync(note, this))
				return null;

			AddItem(note);
			SortItems();

			return note;
		}

		public async Task AddNoteAsync(Note note)
		{
			await note.ChangeParentAsync(this);

			AddItem(note);
			SortItems();
		}

		public override void SortItems()
		{
			base.SortItems();
            NotebooksManager.OnNotesSorted(this);
		}
    }
}
