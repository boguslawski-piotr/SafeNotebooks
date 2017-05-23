using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SafeNotebooks
{
	public class Note : Item
	{
		public Page Page => Parent as Page;
		public Notebook Notebook => Page.Notebook;

		//public ISearchableStorage<Attachment> AttachmentsStorage { get; set; }

		
        //

        public const string IdForStoragePrefix = "O-";

		public override string IdForStorage => IdForStoragePrefix + Notebook?.Id + "-" + Page?.Id + "-" + base.IdForStorage;

        public override async Task NewAsync(Item parent)
        {
            await base.NewAsync(parent);
        }

        //     protected override string Serialize()
        //     {
        //         string d = base.Serialize();

        ////d += ",'d2':" + JsonConvert.SerializeObject(data, pbXNet.Settings.Json);

        //return d;
        //}


        //

        public override string DetailForLists => $"{ModifiedOn.ToLocalTime().ToString()}";
	}
}
