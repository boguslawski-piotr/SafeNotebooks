using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SafeNotebooks
{
	public class Note : Item
	{
		public Page Page => Parent as Page;
		public Notebook Notebook => Page.Notebook;

		public const string IdForStoragePrefix = "O-";
		public override string IdForStorage => IdForStoragePrefix + Page?.Id + "-" + base.IdForStorage;

        //public ISearchableStorage<Attachment> AttachmentsStorage { get; set; }

        protected override void InternalNew()
        {
            base.InternalNew();
        }
	}
}
