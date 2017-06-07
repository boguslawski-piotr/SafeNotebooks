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

		void InitializeSettings()
		{
			UsePinAsMasterPassword.IsEnabled = App.Settings.UnlockUsingPin;
		}


		//

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
			return rc;
		}


		//

		async void UnlockUsingPin_Toggled(object sender, Xamarin.Forms.ToggledEventArgs e)
		{
			if (UnlockEvent(_UnlockUsingPin_Toggled_Guard))
				return;

			if (e.Value)
			{
				// The design of the function is a bit strange to be sure to clear the entire memory where the pin could be stored.

				// Before we can turn on UnlockUsingPin option we need to ask user for pin.
				// Twice, just to be sure... ;)

				PinDlg dlg = UnlockWnd.CreatePinDlg(Bounds.Height);
				dlg.Title.Text = T.Localized("NewPinTitle");

				bool rc = await MainWnd.Current.ModalManager.DisplayModalAsync(dlg, ModalViewsManager.ModalPosition.BottomCenter);
				if (rc)
				{
					char[] pin1 = dlg.Pin.MakeACopy();
					dlg.Reset();

					dlg.Title.Text = T.Localized("ConfirmNewPinTitle");
					rc = await MainWnd.Current.ModalManager.DisplayModalAsync(dlg, ModalViewsManager.ModalPosition.BottomCenter);
					if (rc)
						rc = pin1.SequenceEqual(dlg.Pin);

					pin1.FillWithDefault();

					if (rc)
						// Everything is in order, pin confirmed and we can save it (sort of ;))
						await App.SecretsManager.AddOrUpdatePasswordAsync(App.Name, dlg.Pin);

					dlg.Reset();

					if (rc)
					{
						InitializeSettings();
						return;
					}
					else
					{
						// Pin was not confirmed.
						// TODO: komunikat dla uzytkownika, ze piny sie nie zgadzaja (ale NIE return!)
					}
				}

				dlg.Reset();
			}
			else
			{
				// Pin was just turned off and now we have to check if it was used as the master password...
				if (App.Settings.UsePinAsMasterPassword)
				{
					// ... and if so, do the right thing like decrypt all data on low level.
					UsePinAsMasterPassword.IsToggled = false;
					return;
				}
			}

			// Unfortunately because of asynchronus operations it is not quite clear what and when is done
			// and simple: unregister from event, change observable value (IsToggled), register to event again
			// doesn't work every time :(. So the following guard is used.

			LockEvent(_UnlockUsingPin_Toggled_Guard);
			UnlockUsingPin.IsToggled = false;

			InitializeSettings();

			await App.SecretsManager.DeletePasswordAsync(App.Name);
		}

		async void UsePinAsMasterPassword_Toggled(object sender, Xamarin.Forms.ToggledEventArgs e)
		{
			if (UnlockEvent(_UsePinAsMasterPassword_Toggled_Guard))
				return;

			if (e.Value)
			{
				Debug.Assert(App.Settings.UnlockUsingPin == true);

				// ask for pin (again... but only once)

				// store ckey

				// encrypt all data on low level
			}
			else
			{
				// Now we should ask the user if he wants to do it, because it involves decrypting the data.
				if (!await Application.Current.MainPage.DisplayAlert("Q", "Really?", "Yes", "No")) // TODO: zastapic wlasnym dialogiem
				{
					// He doesn't ;) We need to rollback everything. 

					// Unfortunately because of asynchronus operations it is not quite clear what and when is done
					// and simple: unregister from event, change observable value (IsToggled), register to event again
					// doesn't work every time :(. So the following guards are used.

					LockEvent(_UsePinAsMasterPassword_Toggled_Guard);
					UsePinAsMasterPassword.IsToggled = true;

					if (!UnlockUsingPin.IsToggled)
					{
						LockEvent(_UnlockUsingPin_Toggled_Guard);
						UnlockUsingPin.IsToggled = true;
					}
				}
				else
				{
					// Decrypt all data on low level...

					await App.SecretsManager.DeleteCKeyAsync(App.Name);
					await App.SecretsManager.DeletePasswordAsync(App.Name);
				}
			}

			InitializeSettings();
		}
	}
}
