using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public class ItemWithItems : Item
	{
		public ObservableCollectionEx<Item> ObservableItems { get; protected set; }

		[Serializable]
		public struct SortParameters
		{
			public bool ByName;
			public bool ByDate;
			public bool ByColor;
			public bool Descending;

			public SortParameters(bool byName, bool byDate, bool byColor, bool descending)
			{
				ByName = byName;
				ByDate = byDate;
				ByColor = byColor;
				Descending = descending;
			}

			public void Clear()
			{
				ByName = ByDate = ByColor = false;
			}
		}

		public static readonly SortParameters DefaultSortParams = new SortParameters(false, false, true, false);

		[Serializable]
		class IWIData
		{
			public SortParameters SortParams = DefaultSortParams;
		}

		IWIData iwidata;

		public SortParameters SortParams
		{
			get => iwidata.SortParams;
			set => SetValue(ref iwidata.SortParams, value, false);
		}

		bool _SelectModeForItemsEnabled;
		public virtual bool SelectModeForItemsEnabled
		{
			get => _SelectModeForItemsEnabled;
			set {
				_SelectModeForItemsEnabled = value;
				if (ObservableItems != null)
					foreach (var item in ObservableItems)
						item.SelectModeEnabled = value;
			}
		}

		protected override string SerializeNotEncryptedData()
		{
			return base.SerializeNotEncryptedData() + NotebooksManager.Serializer.ToString(iwidata, "iwid");
		}

		protected override void DeserializeNotEncryptedData(string d)
		{
			base.DeserializeNotEncryptedData(d);
			iwidata = NotebooksManager.Serializer.FromString<IWIData>(d, "iwid");
		}

		protected override void InternalNew()
		{
			iwidata = new IWIData();
			base.InternalNew();
		}

		public override async Task<bool> SaveAllAsync(bool force = false)
		{
			if (!await base.SaveAllAsync(force))
				return false;
			if (ObservableItems != null)
			{
				foreach (var i in ObservableItems.ToList())
					if (!await i.SaveAllAsync(force))
						return false;
			}
			return true;
		}

		protected void CreateObservableItems()
		{
			ObservableItems = new ObservableCollectionEx<Item>();
			NotebooksManager?.OnItemObservableItemsCreated(this);
		}

		object addItemLock = new object();

		public void AddItem(Item item)
		{
			item.SelectModeEnabled = SelectModeForItemsEnabled;
			lock (addItemLock)
			{
				if (ObservableItems == null)
					CreateObservableItems();
				
				ObservableItems.Add(item);
				if (ObservableItems.Count % 5 == 0)
					SortItems();
			}

			NotebooksManager?.OnItemAdded(item, this);
		}

		public virtual void SortItems()
		{
			bool desc = SortParams.Descending;
			Comparison<Item> f = (x, y) =>
			{
				int cc = (desc ? 1 : -1) * string.Compare(x.ComparableColor, y.ComparableColor, StringComparison.Ordinal);
				if (cc == 0)
					cc = (desc ? -1 : 1) * string.Compare(x.NameForLists, y.NameForLists, StringComparison.CurrentCulture);
				return cc;
			};

			if (SortParams.ByName)
				f = (x, y) => (desc ? -1 : 1) * string.Compare(x.NameForLists, y.NameForLists, StringComparison.CurrentCulture);
			else if (SortParams.ByDate)
				f = (x, y) => (desc ? -1 : 1) * (x.ModifiedOn > y.ModifiedOn ? 1 : x.ModifiedOn < y.ModifiedOn ? -1 : 0);

			ObservableItems?.Sort(f);
			NotebooksManager?.OnItemObservableItemsSorted(this);
		}
	}
}
