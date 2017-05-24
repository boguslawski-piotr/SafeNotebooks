﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SafeNotebooks
{
    public class Page : ItemWChildren<Note>
    {
        public Notebook Notebook => Parent as Notebook;

        //

        public async Task<Note> NewNoteAsync()
        {
            Note note = new Note() { NotebooksManager = NotebooksManager };
            if (!await NotebooksManager.NewItemHelperAsync(note, this))
                return null;

			AddItem(note);
			Sort();

			return note;
        }

        public async Task AddNoteAsync(Note note)
        {
            await note.ChangeParentAsync(this);
		
            AddItem(note);
			Sort();
		}


        //

        class PageData
        {
            public NoteType DefaultNoteType = NoteType.Note;
        }

        PageData pdata;


        //

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


        //

        public const string IdForStoragePrefix = "P-";

        public override string IdForStorage => IdForStoragePrefix + Notebook?.Id + "-" + base.IdForStorage;

        public override async Task NewAsync(Item parent)
        {
            pdata = new PageData();
            await base.NewAsync(parent);
        }

        public override async Task<bool> LoadAsync(bool tryToUnlockChildren)
        {
            if (!await base.LoadAsync(true))
                return false;

            string pattern = Note.IdForStoragePrefix + Notebook.Id + "-" + Id + "-\\w*";
            bool anyNoteLoaded = await NotebooksManager.LoadChildrenForItemHelperAsync<Note>(this, pattern, tryToUnlockChildren);

            //if(anyNoteLoaded)
                Sort();

			NotebooksManager.OnPageLoaded(this, anyNoteLoaded);
			return true;
		}


        //

        public override string DetailForLists => $"{Storage?.Name}, {ModifiedOn.ToLocalTime().ToString()}";
    }
}
