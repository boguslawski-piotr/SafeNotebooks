using System;
using System.Collections;
using System.Collections.Generic;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public static class ViewsCommonLogic
	{
		public static void SearchQuery_Focused(Layout<View> SearchBar, Entry SearchQuery, FlatButton CancelSearchBtn)
		{
			CancelSearchBtn.IsVisible = true;
			SearchQuery.Placeholder = "";

			Thickness t = (Thickness)App.Current.Resources["SearchBarPadding"];
			t.Right = 0;
			SearchBar.Padding = t;
		}

		public static void SearchQuery_Unfocused(Layout<View> SearchBar, Entry SearchQuery, FlatButton CancelSearchBtn)
		{
			CancelSearchBtn.IsVisible = false;
			SearchQuery.Placeholder = T.Localized("SearchQueryPlaceholder");
			SearchBar.Padding = (Thickness)App.Current.Resources["SearchBarPadding"];
		}

		public static void CancelSearchBtn_Clicked(Layout<View> SearchBar, Entry SearchQuery, FlatButton CancelSearchBtn)
		{
			SearchQuery.Text = "";
			SearchQuery.Unfocus();
		}


        public static void ListViewScrollTo(ListView ListCtl, Item item)
		{
			ListCtl.ScrollTo(item, ScrollToPosition.Center, false);
		}

		public static void ListViewSetItemsSource(ListView ListCtl, IEnumerable l)
		{
			if (l == null)
			{
				ListCtl.ItemsSource = l;
				return;
			}

			ListCtl.BeginRefresh();

			IEnumerator e = l.GetEnumerator();
			e.Reset();
			object o = null;
			if (e.MoveNext())
				o = e.Current;

			ListCtl.ItemsSource = l;

			if (o != null)
				ListCtl.ScrollTo(o, ScrollToPosition.Start, false);

			ListCtl.EndRefresh();
		}
	}

}
