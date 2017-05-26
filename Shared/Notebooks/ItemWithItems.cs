using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SafeNotebooks
{
    public static class ItemWithItems
    {
        public static SortParameters DefaultSortParameters = new SortParameters(false, false, true, false);

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
    }

    public class ItemWithItems<T> : Item where T : Item
    {
        public IList<T> Items { get; protected set; } = new List<T>();
        public ObservableCollection<T> ObservableItems { get; protected set; } //= new ObservableCollection<T>();

        public override void Dispose()
        {
            ObservableItems.Clear();
            ObservableItems = null;
            Items.Clear();
            Items = null;
            iwidata = null;
            base.Dispose();
        }


        //

        class IWIData
        {
            public ItemWithItems.SortParameters SortParameters = new ItemWithItems.SortParameters();
        }

        IWIData iwidata;

        public ItemWithItems.SortParameters SortParameters
        {
            get => iwidata.SortParameters;
            set => SetValue(ref iwidata.SortParameters, value); // TODO: to nie powinno skutkowac zmiana daty modyfikacji w Parent
        }


        //

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


        //

        public override async Task NewAsync(Item parent)
        {
            iwidata = new IWIData()
            {
                SortParameters = ItemWithItems.DefaultSortParameters
            };
            await base.NewAsync(parent);
        }

        public override async Task<bool> SaveAllAsync(bool force = false)
		{
            if (!await SaveAsync(force))
                return false;
            foreach (var i in Items)
                if (!await i.SaveAllAsync(force))
                    return false;
            return true;
		}

		public void AddItem(T item)
        {
            item.SelectModeEnabled = SelectModeForItemsEnabled;
            Items.Add(item);
        }


        //

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

        public virtual void SortItems()
        {
            Func<T, object> f = (v) => v.ComparableColor; ;
            bool desc = SortParameters.Descending;

            if (SortParameters.ByColor)
                desc = !desc; // for colors reverse order
            else if (SortParameters.ByName)
                f = (v) => v.NameForLists;
            else if (SortParameters.ByDate)
                f = (v) => v.ModifiedOn;

            IOrderedEnumerable<T> l = null;
            if (desc)
                l = Items.OrderByDescending(f).ThenBy((v) => v.NameForLists);
            else
                l = Items.OrderBy(f).ThenBy((v) => v.NameForLists);

            ObservableItems = new ObservableCollection<T>(l);
        }

    }
}
