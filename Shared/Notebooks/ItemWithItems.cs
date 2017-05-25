using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace SafeNotebooks
{
    public static class ItemWithItems
    {
        public class SortParameters
        {
            public bool ByName;
            public bool ByDate;
            public bool ByColor = true;
            public bool Descending;

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
            base.Dispose();
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


        public void AddItem(T item)
        {
            item.SelectModeEnabled = SelectModeForItemsEnabled;
            Items.Add(item);
        }

        // TODO: zapisywac wraz z obiektem
        public ItemWithItems.SortParameters SortParameters { get; set; } = new ItemWithItems.SortParameters();

        public virtual void SortItems()
        {
            Func<T, object> f = null;
            bool desc = SortParameters.Descending;

            if (SortParameters.ByColor)
            {
                f = (v) => v.ComparableColor;
                desc = !desc; // for colors reverse order
            }
            else if (SortParameters.ByName)
                f = (v) => v.NameForLists;
            else if (SortParameters.ByDate)
                f = (v) => v.ModifiedOn;

            IOrderedEnumerable<T> l = null;
            if (desc)
                l = Items.OrderByDescending(f);
            else
                l = Items.OrderBy(f);

            ObservableItems = new ObservableCollection<T>(l);
        }

    }
}
