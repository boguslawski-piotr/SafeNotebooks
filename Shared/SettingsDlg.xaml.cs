using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using pbXForms;
using pbXNet;
using Xamarin.Forms;

namespace SafeNotebooks
{
	public partial class SettingsDlg : SettingsContentView
	{
		public SettingsDlg()
		{
			BindingContext = App.C.Settings;
			InitializeComponent();
			InitializeUI();
		}

		public void InitializeUI()
		{
			InitializeSettings();
		}

		public void OK_Clicked(object sender, System.EventArgs e)
		{
			OnOK();
		}

		//

		void InitializeSettings()
		{
			UnlockUsingDOAuthentication.IsEnabled = (DOAuthentication.Type != DOAuthenticationType.NotAvailable);

			if (!App.C.Settings.UnlockUsingPin)
				App.C.Settings.UsePinAsMasterPassword = false;

			UnlockUsingPin.Toggled -= UnlockUsingPin_Toggled;
			UsePinAsMasterPassword.Toggled -= UsePinAsMasterPassword_Toggled;

			// Unfortunately because of asynchronus operations it is not quite clear what and when is done
			// and simple: unregister from event, change observable value (IsToggled), register to event again
			// doesn't work every time :(. So the following guards are used.

			if (UnlockUsingPin.IsToggled != App.C.Settings.UnlockUsingPin)
			{
				LockEvent(_UsePinAsMasterPassword_Toggled_Guard);
				UnlockUsingPin.IsToggled = App.C.Settings.UnlockUsingPin;
			}

			UsePinAsMasterPassword.IsEnabled = UnlockUsingPin.IsToggled;
			if (UsePinAsMasterPassword.IsToggled != App.C.Settings.UsePinAsMasterPassword)
			{
				LockEvent(_UsePinAsMasterPassword_Toggled_Guard);
				UsePinAsMasterPassword.IsToggled = App.C.Settings.UsePinAsMasterPassword;
			}

			UnlockUsingPin.Toggled += UnlockUsingPin_Toggled;
			UsePinAsMasterPassword.Toggled += UsePinAsMasterPassword_Toggled;
		}

		const int _UnlockUsingPin_Toggled_Guard = 0;
		const int _UsePinAsMasterPassword_Toggled_Guard = 1;

		volatile bool[] _eventsGuards = { false, false, };

		void LockEvent(int i)
		{
			_eventsGuards[i] = true;

			Device.StartTimer(TimeSpan.FromMilliseconds(1000), () =>
			{
				_eventsGuards[i] = false;
				return false;
			});
		}

		bool UnlockEvent(int i)
		{
			bool rc = _eventsGuards[i];
			_eventsGuards[i] = false;

			if (rc)
				InitializeSettings();

			return rc;
		}


		//

		async Task TurnOnUnlockUsingPin()
		{
			// The design of the function is a bit strange to be sure to clear the entire memory where the pin could be stored as soon as possible.

			// Before we can turn on UnlockUsingPin option we need to ask user for pin.
			// Twice, just to be sure... ;)

			PinDlg pinDlg = UnlockWnd.CreatePinDlg(Wnd.C.Bounds.Height);
			pinDlg.Title.Text = T.Localized("NewPinTitle");

			bool rc = await Wnd.C.ModalManager.DisplayModalAsync(pinDlg, ModalViewsManager.ModalPosition.BottomCenter);
			if (rc)
			{
				Password pin = new Password(pinDlg.Pin);
				pinDlg.Reset();

				pinDlg.Title.Text = T.Localized("ConfirmNewPinTitle");
				rc = await Wnd.C.ModalManager.DisplayModalAsync(pinDlg, ModalViewsManager.ModalPosition.BottomCenter);
				if (rc)
					rc = pin.Equals(pinDlg.Pin);

				pin.Dispose();

				if (rc)
				{
					// Everything is in order, pin confirmed and we can save it (sort of ;))
					App.C.Settings.UnlockUsingPin = true;
					App.C.SecretsManager.AddOrUpdatePassword(App.Name, SecretLifeTime.Infinite, pinDlg.Pin);
				}

				pinDlg.Reset();

				if (rc)
					return;
				else
				{
					// Pin was not confirmed.
					// TODO: komunikat dla uzytkownika, ze piny sie nie zgadzaja (ale NIE return!)
				}
			}

			pinDlg.Reset();
			await TurnOffUnlockUsingPin();
		}

		async Task<bool> TurnOffUnlockUsingPin()
		{
			if (App.C.Settings.UsePinAsMasterPassword && !await TurnOffUsePinAsMasterPassword())
				return false;

			App.C.Settings.UnlockUsingPin = false;
			App.C.SecretsManager.DeletePassword(App.Name);

			return true;
		}

		async void UnlockUsingPin_Toggled(object sender, Xamarin.Forms.ToggledEventArgs e)
		{
			if (UnlockEvent(_UnlockUsingPin_Toggled_Guard))
				return;

			if (e.Value)
				await TurnOnUnlockUsingPin();
			else
				await TurnOffUnlockUsingPin();

			InitializeSettings();
		}

		async Task TurnOnUsePinAsMasterPassword()
		{
			if (App.C.Settings.UnlockUsingPin == true)
			{
				PinDlg pinDlg = UnlockWnd.CreatePinDlg(Wnd.C.Bounds.Height);
				pinDlg.Title.Text = T.Localized("PinTitle");

				bool rc = await Wnd.C.ModalManager.DisplayModalAsync(pinDlg, ModalViewsManager.ModalPosition.BottomCenter);
				if (rc)
					rc = App.C.SecretsManager.ComparePassword(App.Name, pinDlg.Pin);

				pinDlg.Reset();

				if (rc)
				{
					App.C.SecretsManager.CreateCKey(App.Name, SecretLifeTime.WhileAppRunning, pinDlg.Pin);

					// encrypt all data on low level

					App.C.Settings.UsePinAsMasterPassword = rc;
				}
				else
				{
					// bad pin message
				}
			}
		}

		async Task<bool> TurnOffUsePinAsMasterPassword()
		{
			// We should ask the user if he really wants to do it, because it involves decrypting the data.
			if (!await Application.Current.MainPage.DisplayAlert("Q", "Really?", "Yes", "No")) // TODO: zastapic wlasnym dialogiem
				return false;

			// Decrypt all data on low level...

			App.C.Settings.UsePinAsMasterPassword = false;
			App.C.SecretsManager.DeleteCKey(App.Name);

			return true;
		}

		async void UsePinAsMasterPassword_Toggled(object sender, Xamarin.Forms.ToggledEventArgs e)
		{
			if (UnlockEvent(_UsePinAsMasterPassword_Toggled_Guard))
				return;

			if (e.Value)
				await TurnOnUsePinAsMasterPassword();
			else
				await TurnOffUsePinAsMasterPassword();

			InitializeSettings();
		}
	}
}
