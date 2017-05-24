using System;
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
	}

}
