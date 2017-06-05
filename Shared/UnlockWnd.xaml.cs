using System;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class UnlockWnd : ContentPageEx
	{
		public enum TState
		{
			Splash,
			Unlocking,
			Unlocked,
			Locked,
		}

		public TState State = TState.Splash;

		public UnlockWnd()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			if (State == TState.Unlocking)
				TryToUnlock();
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
		}

		public void SetSplashMode()
		{
			State = TState.Splash;

			_Message.Text = "";
			_Message.IsVisible = false;
			_FPIcon.IsVisible = false;
			_UnlockOrCancelBtn.IsVisible = false;
		}

		public static bool UnlockingNeeded
		{
			get { return App.Settings.UnlockUsingDeviceOwnerAuthentication || App.Settings.UnlockUsingPin; }
		}

		public void SetUnlockingMode()
		{
			State = TState.Unlocking;

			_Message.Text = "";
			_Message.IsVisible = false;
			_FPIcon.IsVisible = false;
			_UnlockOrCancelBtn.Text = "";
			_UnlockOrCancelBtn.IsVisible = false;
		}

		public void TryToUnlock()
		{
			SetUnlockingMode();

			if (App.Settings.UnlockUsingDeviceOwnerAuthentication)
			{
				DOAuthentication doa = App.SecretsManager.AvailableDOAuthentication;

				if (doa != DOAuthentication.None && TryToUnlockUsingDOAuthentication())
				{
					if (doa == DOAuthentication.Fingerprint)
					{
						_FPIcon.IsVisible = true;
						_Message.Text = T.Localized("ScanFingerprint");
					}
					else if (doa == DOAuthentication.Password)
						_Message.Text = T.Localized("EnterSystemPassword");
					else
						_Message.Text = T.Localized("UseSomeDOA");
					_Message.IsVisible = true;
					return;
				};
			}

			TryToUnlockUsingPinAsync(true);
		}

		public event EventHandler UnlockedCorrectly = null;

		//

		bool TryToUnlockUsingDOAuthentication()
		{
			return App.SecretsManager.StartDOAuthentication(T.Localized("AuthenticateDeviceOwnerReason"), OnUnlockedCorrectlyUsingDOAuthentication, OnNotUnlockedUsingDOAuthentication);
		}

		void OnUnlockedCorrectlyUsingDOAuthentication()
		{
			TryToUnlockUsingPinAsync(false);
		}

		void OnNotUnlockedUsingDOAuthentication(string error, bool hint)
		{
			State = TState.Locked;

			_Message.Text = error;
			if (!hint)
			{
				_FPIcon.IsVisible = false;
				_UnlockOrCancelBtn.Text = T.Localized("TryAgain");
				_UnlockOrCancelBtn.IsVisible = true;
			}
		}

		async Task TryToUnlockUsingPinAsync(bool appStarting)
		{
			// TODO: nie uzywac tego ustawienia, rozpoznawac po tym czy passwd jest w systemie
			if (App.Settings.UnlockUsingPin)
			{
				SetUnlockingMode();

				// Wait for a while in order to process everything during app start
				if(appStarting)
					await Task.Delay(1000);

				// Move logo and app name to the top
				_View.VerticalOptions = LayoutOptions.FillAndExpand;
				_View.Padding = new Thickness(0, Metrics.AppBarHeightLandscape, 0, 0);

				// Run PIN dialog
				PinDlg dlg = new PinDlg();
				dlg.Content.WidthRequest = 240;
				dlg.BackgroundColor = (Color)Application.Current.Resources["PageBackgroundColor"];
				dlg.Title.Text = T.Localized("PinTitle");
				dlg.PinVisualization.BackgroundColor = (Color)Application.Current.Resources["EntryBackgroundColor"];
				dlg.DelBtn.Text = "";
				dlg.DelBtn.Image = new FileImageSource { File = "ic_action_backspace.png" };
				dlg.OKBtn.Text = "";
				dlg.OKBtn.Image = new FileImageSource { File = "ic_done.png" };

				//for (int i = 0; i < 10; i++)
				//{
				//	PIButton btn = dlg.Btn[i];
				//	if(btn != null)
				//		btn.BackgroundColor = Color.Red;
				//}

				while (true)
				{
					if (await ModalManager.DisplayModalAsync(dlg, Bounds.Height <= 640 || Device.Idiom == TargetIdiom.Phone ? ModalViewsManager.ModalPosition.BottomCenter : ModalViewsManager.ModalPosition.Center))
					{
						bool pok = await App.SecretsManager.ComparePasswordAsync(App.Name, dlg.Pin);
						dlg.Reset();

						if(pok)
						{
							OnUnlockedCorrectly();
							break;
						}

					}
				}
			}
			else
				OnUnlockedCorrectly();
		}

		void OnUnlockedCorrectly()
		{
			State = TState.Unlocked;
			UnlockedCorrectly?.Invoke(this, null);
		}

		void OnNotUnlocked()
		{
			State = TState.Locked;
		}

		//

		void UnlockBtn_Clicked(object sender, System.EventArgs e)
		{
			TryToUnlock();
		}
	}
}
