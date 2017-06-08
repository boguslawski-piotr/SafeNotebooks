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
			InitializeComponent();

			InitializeSettings();
			InitializeUI();
		}

		public void InitializeUI()
		{
			HeaderHeightInLandscape = Metrics.AppBarHeightLandscape;
			HeaderHeightInPortrait = Metrics.AppBarHeightPortrait;
		}

		void Handle_Clicked(object sender, System.EventArgs e)
		{
			App.Settings.TryToUnlockItemItems = !App.Settings.TryToUnlockItemItems;
			ApplyBindings();
		}


		//

		void InitializeSettings()
		{
			if (!App.Settings.UnlockUsingPin)
				App.Settings.UsePinAsMasterPassword = false;

			UnlockUsingPin.Toggled -= UnlockUsingPin_Toggled;
			UsePinAsMasterPassword.Toggled -= UsePinAsMasterPassword_Toggled;

			// Unfortunately because of asynchronus operations it is not quite clear what and when is done
			// and simple: unregister from event, change observable value (IsToggled), register to event again
			// doesn't work every time :(. So the following guards are used.

			if (UnlockUsingPin.IsToggled != App.Settings.UnlockUsingPin)
			{
				LockEvent(_UsePinAsMasterPassword_Toggled_Guard);
				UnlockUsingPin.IsToggled = App.Settings.UnlockUsingPin;
			}

			UsePinAsMasterPassword.IsEnabled = UnlockUsingPin.IsToggled;
			if (UsePinAsMasterPassword.IsToggled != App.Settings.UsePinAsMasterPassword)
			{
				LockEvent(_UsePinAsMasterPassword_Toggled_Guard);
				UsePinAsMasterPassword.IsToggled = App.Settings.UsePinAsMasterPassword;
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

		async Task<bool> TurnOnUnlockUsingPin()
		{
			// The design of the function is a bit strange to be sure to clear the entire memory where the pin could be stored as soon as possible.

			// Before we can turn on UnlockUsingPin option we need to ask user for pin.
			// Twice, just to be sure... ;)

			PinDlg pinDlg = UnlockWnd.CreatePinDlg(MainWnd.Current.Bounds.Height);
			pinDlg.Title.Text = T.Localized("NewPinTitle");
			bool rc = await MainWnd.Current.ModalManager.DisplayModalAsync(pinDlg, ModalViewsManager.ModalPosition.BottomCenter);
			if (rc)
			{
				char[] pin = pinDlg.Pin.MakeACopy();
				pinDlg.Reset();

				pinDlg.Title.Text = T.Localized("ConfirmNewPinTitle");
				rc = await MainWnd.Current.ModalManager.DisplayModalAsync(pinDlg, ModalViewsManager.ModalPosition.BottomCenter);
				if (rc)
					rc = pin.SequenceEqual(pinDlg.Pin);

				pin.FillWithDefault();

				if (rc)
				{
					// Everything is in order, pin confirmed and we can save it (sort of ;))
					App.Settings.UnlockUsingPin = true;
					await App.SecretsManager.AddOrUpdatePasswordAsync(App.Name, pinDlg.Pin);
				}

				pinDlg.Reset();

				if (rc)
					return true;
				else
				{
					// Pin was not confirmed.
					// TODO: komunikat dla uzytkownika, ze piny sie nie zgadzaja (ale NIE return!)
				}
			}

			pinDlg.Reset();
			await TurnOffUnlockUsingPin();
			return false;
		}

		async Task<bool> TurnOffUnlockUsingPin()
		{
			if (App.Settings.UsePinAsMasterPassword && !await TurnOffUsePinAsMasterPassword())
				return false;

			App.Settings.UnlockUsingPin = false;
			await App.SecretsManager.DeletePasswordAsync(App.Name);

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

		async Task<bool> TurnOnUsePinAsMasterPassword()
		{
			bool rc = false;

			if (App.Settings.UnlockUsingPin == true)
			{
				PinDlg pinDlg = UnlockWnd.CreatePinDlg(MainWnd.Current.Bounds.Height);
				pinDlg.Title.Text = T.Localized("PinTitle");
				rc = await MainWnd.Current.ModalManager.DisplayModalAsync(pinDlg, ModalViewsManager.ModalPosition.BottomCenter);
				if (rc)
					rc = await App.SecretsManager.ComparePasswordAsync(App.Name, pinDlg.Pin);

				pinDlg.Reset();

				if (rc)
				{
					await App.SecretsManager.CreateCKeyAsync(App.Name, CKeyLifeTime.WhileAppRunning, pinDlg.Pin);

					// encrypt all data on low level

					App.Settings.UsePinAsMasterPassword = rc;
				}
				else
				{
					// bad pin message
				}
			}

			return false;
		}

		async Task<bool> TurnOffUsePinAsMasterPassword()
		{
			// We should ask the user if he really wants to do it, because it involves decrypting the data.
			if (!await Application.Current.MainPage.DisplayAlert("Q", "Really?", "Yes", "No")) // TODO: zastapic wlasnym dialogiem
				return false;

			// Decrypt all data on low level...

			App.Settings.UsePinAsMasterPassword = false;
			await App.SecretsManager.DeleteCKeyAsync(App.Name);

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
