using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using pbXNet;

namespace SafeNotebooks
{
    public class ItemWithItems : Item
    {
        public List<Item> Items { get; protected set; }

        public ObservableCollection<Item> ObservableItems { get; protected set; }

        protected void CreateObservableItems(IEnumerable<Item> l)
        {
            ObservableItems = new ObservableCollection<Item>(l);
            NotebooksManager?.OnItemObservableItemsCreated(this);
        }

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
            return base.SerializeNotEncryptedData() +
                ",'iwid':" + JsonConvert.SerializeObject(iwidata, pbXNet.Settings.JsonSerializer);
        }

        protected override void DeserializeNotEncryptedData(JObject d)
        {
            base.DeserializeNotEncryptedData(d);
            iwidata = JsonConvert.DeserializeObject<IWIData>(d["iwid"].ToString(), pbXNet.Settings.JsonSerializer);
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
            if (Items != null)
            {
                foreach (var i in Items.ToList())
                    if (!await i.SaveAllAsync(force))
                        return false;
            }
            return true;
        }

        object addItemLock = new object();

        public void AddItem(Item item)
        {
            item.SelectModeEnabled = SelectModeForItemsEnabled;
            lock (addItemLock)
            {
                if (Items == null)
                    Items = new List<Item>();

                Items.Add(item);

                // This is only for UI to show loading/adding in bulk progress...
                if (ObservableItems == null)
                    CreateObservableItems(Items);
                else
                    ObservableItems.Add(item);
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
