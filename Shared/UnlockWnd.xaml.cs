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

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

		protected override void OnAppearing()
		{
			if (State == TState.Unlocking)
				TryToUnlockAsync();
		}

#pragma warning restore CS4014

		protected override bool OnBackButtonPressed()
		{
			return false;
		}

		public void SetSplashMode()
		{
			State = TState.Splash;

			_Message.Text = "";
			_Message.IsVisible = false;
			_FPIcon.IsVisible = false;
			_UnlockOrCancelBtn.IsVisible = false;
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

		public async Task TryToUnlockAsync()
		{
			SetUnlockingMode();

			try
			{
				if (App.Settings.UnlockUsingDeviceOwnerAuthentication)
				{
					DOAuthentication doa = App.C.SecretsManager.AvailableDOAuthentication;
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
			}
			catch (Exception ex)
			{
				await MainWnd.C.DisplayError(ex);
				return;
			}

#pragma warning disable CS4014

			TryToUnlockUsingPinAsync();

#pragma warning restore CS4014
		}

		public event EventHandler UnlockedCorrectly = null;

		//

		bool TryToUnlockUsingDOAuthentication()
		{
			return App.C.SecretsManager.StartDOAuthentication(T.Localized("AuthenticateDeviceOwnerReason"), OnUnlockedCorrectlyUsingDOAuthentication, OnNotUnlockedUsingDOAuthentication);
		}

#pragma warning disable CS4014

		void OnUnlockedCorrectlyUsingDOAuthentication()
		{
			TryToUnlockUsingPinAsync();
		}

#pragma warning restore CS4014

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

		public static PinDlg CreatePinDlg(double ownerHeight)
		{
			PinDlg dlg = new PinDlg();

			if (ownerHeight < 360)
				dlg.SetCompactSize();

			dlg.BackgroundColor = (Color)Application.Current.Resources["PageBackgroundColor"];
			dlg.PinVisualization.BackgroundColor = (Color)Application.Current.Resources["EntryBackgroundColor"];
			dlg.DelBtn.Text = "";
			dlg.DelBtn.Image = new FileImageSource { File = "ic_action_backspace.png" };
			dlg.OKBtn.Text = "";
			dlg.OKBtn.Image = new FileImageSource { File = "ic_done.png" };

			//for (int i = 0; i < 10; i++)
			//{
			//	PIButton btn = dlg.DigitBtns[i];
			//	if(btn != null)
			//		btn.BackgroundColor = Color.Red;
			//}		

			return dlg;
		}

		async Task TryToUnlockUsingPinAsync()
		{
			if (App.Settings.UnlockUsingPin)
			{
				SetUnlockingMode();

				// Wait for a while in order to process everything during app start/resume
				await Task.Delay(1000);

				// Move logo and app name to the top
				_View.VerticalOptions = LayoutOptions.FillAndExpand;
				_View.Padding = new Thickness(0, Bounds.Height <= 520 ? Metrics.ScreenEdgeMargin : Metrics.AppBarHeightLandscape, 0, 0);

				// Run PIN dialog
				PinDlg dlg = CreatePinDlg(Bounds.Height);
				dlg.Title.Text = T.Localized("PinTitle");

				while (true)
				{
					if (await ModalManager.DisplayModalAsync(dlg, ModalViewsManager.ModalPosition.BottomCenter))
					{
						bool pok = await App.C.SecretsManager.ComparePasswordAsync(App.Name, dlg.Pin);
						if (pok)
						{
							if (App.Settings.UsePinAsMasterPassword)
								await App.C.SecretsManager.CreateCKeyAsync(App.Name, CKeyLifeTime.WhileAppRunning, dlg.Pin);
						}

						dlg.Reset();

						if (pok)
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

#pragma warning disable CS4014

		void UnlockBtn_Clicked(object sender, System.EventArgs e)
		{
			TryToUnlockAsync();
		}

#pragma warning restore CS4014
	}
}
