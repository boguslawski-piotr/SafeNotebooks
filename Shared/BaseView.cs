﻿using System;
using System.Collections;
using System.Collections.Generic;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
    public abstract class BaseView : ContentViewEx
    {

        //

        protected async void SortBtn_Clicked(string title, ItemWithItems item, ListView ListCtl, bool detailView = false)
        {
            if (item == null)
                return;
            
			SortParametersDlg dlg = new SortParametersDlg(T.Localized("HowToSort") + " " + title + "?", item.SortParams);
			if (await MainWnd.Current.ModalManager.DisplayModalAsync(dlg, MainWnd.Current.IsSplitView && !detailView ? ModalViewsManager.ModalPosition.BottomLeft : ModalViewsManager.ModalPosition.BottomCenter))
			{
				item.SortParams = dlg.SortParams;
				item.SortItems();
				Device.BeginInvokeOnMainThread(() => BaseView.ListViewScrollToFirst(ListCtl));
			}
		}


		//

		protected void InitializeSearchBarFor(ListView ListCtl)
		{
            if (ListCtl.HeaderElement is StackLayout SearchBar)
            {
                SearchBar.BackgroundColor = ListCtl.BackgroundColor;
                if(SearchBar.Children[0] is VisualElement l1)
                    l1.BackgroundColor = ListCtl.BackgroundColor;
            }

		}

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

		public abstract void SearchQuery_TextChanged(string text);


		//

		public static void ListViewScrollTo(ListView ListCtl, Item item)
		{
			ListCtl.ScrollTo(item, ScrollToPosition.Center, false);
		}

		public static void ListViewScrollToFirst(ListView ListCtl)
		{
			IEnumerator e = ListCtl.ItemsSource.GetEnumerator();
			e.Reset();
			object o = null;
			if (e.MoveNext())
				o = e.Current;
			if (o != null)
				ListCtl.ScrollTo(o, ScrollToPosition.Start, false);
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

