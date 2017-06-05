using System;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class UnlockWnd : ContentPage
	{
		public enum TState
		{
			Splash,
			Unlocking,
			Unlocked,
			Locked,
		}

		public TState State = TState.Splash;

		PINBehavior TruePIN;

		public UnlockWnd()
		{
			InitializeComponent();

			TruePIN = new PINBehavior();
			_PIN.Behaviors.Add(TruePIN);
		}

		protected override void OnAppearing()
		{
			if (State == TState.Unlocking)
				TryToUnlock();
		}

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);
			PIN_Focused(this, new FocusEventArgs(_PIN, _PIN.IsFocused));
		}

		public void SetSplashMode()
		{
			State = TState.Splash;

			_Message.Text = "";
			_Message.IsVisible = false;
			_FPIcon.IsVisible = false;
			_PIN.IsVisible = false;
			_UnlockOrCancelBtn.IsVisible = false;
			PIN_Focused(this, new FocusEventArgs(_PIN, false));
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

			TryToUnlockUsingPin();
		}

		public event EventHandler UnlockedCorrectly = null;

		//

		bool TryToUnlockUsingDOAuthentication()
		{
			return App.SecretsManager.StartDOAuthentication(T.Localized("AuthenticateDeviceOwnerReason"), OnUnlockedCorrectlyUsingDOAuthentication, OnNotUnlockedUsingDOAuthentication);
		}

		void OnUnlockedCorrectlyUsingDOAuthentication()
		{
			TryToUnlockUsingPin();
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

		void TryToUnlockUsingPin()
		{
			if (App.Settings.UnlockUsingPin)
			{
				SetUnlockingMode();

				_Message.Text = "";
				_Message.IsVisible = false;
				_FPIcon.IsVisible = false;
				_UnlockOrCancelBtn.Text = T.Localized("Unlock");
				_UnlockOrCancelBtn.IsVisible = true;
				_PIN.IsVisible = true;
				_PIN.Focus();
			}
			else
				OnUnlockedCorrectly();
		}

		async Task CheckPIN()
		{
			if (await App.SecretsManager.ComparePasswordAsync(App.Name, TruePIN.Text ?? ""))
			{
				OnUnlockedCorrectly();
			}
			else
			{
				Color textColor = _PIN.TextColor;
				_PIN.TextColor = Color.Red;
				await _PIN.ScaleTo(0.3);
				await _PIN.ScaleTo(1);
				_PIN.TextColor = textColor;
				_PIN.Focus();
			}

			_PIN.Text = "";
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
			if (_PIN.IsVisible)
			{
				CheckPIN();
			}
			else
				TryToUnlock();
		}

		void PIN_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
		{
			if (Device.Idiom == TargetIdiom.Desktop)
				return;

			BatchBegin();

			if (e.IsFocused)
			{
				_View.VerticalOptions = LayoutOptions.FillAndExpand;
				_View.Padding = new Thickness(0,
											   (DeviceEx.Orientation == DeviceOrientation.Landscape
													? Metrics.AppBarHeightLandscape / (Device.Idiom == TargetIdiom.Tablet ? 1 : 4)
													: Metrics.AppBarHeightPortrait),
											   0,
											   0);
				_Logo.IsVisible = DeviceEx.Orientation != DeviceOrientation.Landscape || Device.Idiom != TargetIdiom.Phone;
			}
			else
			{
				_View.VerticalOptions = LayoutOptions.CenterAndExpand;
				_View.Padding = new Thickness(0);
				_Logo.IsVisible = true;
			}

			BatchCommit();
		}

		void PIN_Completed(object sender, System.EventArgs e)
		{
			if (!string.IsNullOrEmpty(_PIN.Text))
				CheckPIN();
		}

	}
}
