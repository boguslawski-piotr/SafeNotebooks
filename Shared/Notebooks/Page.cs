using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SafeNotebooks
{
    public class Page : ItemWithItems
    {
        public Notebook Notebook => Parent as Notebook;

		public const string IdForStoragePrefix = "P-";
		public override string IdForStorage => IdForStoragePrefix + Notebook?.Id + "-" + base.IdForStorage;

		[Serializable]
		class PageData
        {
            public NoteType DefaultNoteType = NoteType.Note;
        }

        PageData pdata;

        protected override string Serialize()
        {
            return base.Serialize() + NotebooksManager.Serializer.ToString(pdata, "pd");
        }

        protected override void Deserialize(string d)
        {
            base.Deserialize(d);
            pdata = NotebooksManager.Serializer.FromString<PageData>(d, "pd");
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
            bool anyNoteLoaded = await NotebooksManager.LoadItemsForItemAsync<Note>(this, pattern, tryToUnlockChildren);

            if(anyNoteLoaded)
                SortItems();

			NotebooksManager.OnPageLoaded(this, anyNoteLoaded);
			return true;
		}

		public async Task<Note> NewNoteAsync()
		{
            Note note = await NotebooksManager.NewItemAsync<Note>(this);
			if (note == null)
				return null;

			AddItem(note);
			SortItems();

			return note;
		}
    }
}
