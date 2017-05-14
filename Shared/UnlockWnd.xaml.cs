using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Forms;

#if __IOS__
using LocalAuthentication;
using Foundation;
#endif

namespace SafeNotebooks
{
	public partial class UnlockWnd : ContentPage
	{
		public enum TState
		{
			Splash,
			Unlocking,
			Locked,
			Unlocked,
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

		public void SplashMode()
		{
			State = TState.Splash;
		}

		public void UnlockingMode()
		{
			State = TState.Unlocking;
		}

		public void TryToUnlock()
		{
			UnlockingMode();
			if (!RuntimePlatformTryToUnlock(_UnlockedCorrectly, _NotUnlocked))
			{
				CrossPlatformTryToUnlock(_UnlockedCorrectly, _NotUnlocked);
			}
		}

		public event EventHandler UnlockedCorrectly = null;

		//

		bool RuntimePlatformTryToUnlock(Action Unlocked, Action NotUnlocked)
		{
#if __IOS__
			// TODO: do tools?

			var myReason = new NSString("To add a new chore");

			var context = new LAContext();
			NSError AuthError;

			LAPolicy policy = LAPolicy.DeviceOwnerAuthenticationWithBiometrics;
			if (!context.CanEvaluatePolicy(policy, out AuthError))
				policy = LAPolicy.DeviceOwnerAuthentication;
			if (context.CanEvaluatePolicy(policy, out AuthError))
			{
				var replyHandler = new LAContextReplyHandler((success, error) =>
				{

					Device.BeginInvokeOnMainThread(() =>
					{
						if (success)
						{
							Debug.WriteLine("You are logged in!");
							Unlocked();
						}
						else
						{
							Debug.WriteLine($"Not auth! -> {error}");
							NotUnlocked();
						}
					});

				});

				context.EvaluatePolicy(policy, myReason, replyHandler);

				return true;
			}
			else
			{
				Debug.WriteLine($"Error! -> {AuthError}");
				return false;
			}
#endif
		}

		void CrossPlatformTryToUnlock(Action Unlocked, Action NotUnlocked)
		{
			State = TState.Locked;
		}

		void _UnlockedCorrectly()
		{
			State = TState.Unlocked;
			UnlockedCorrectly?.Invoke(this, null);
		}

		void _NotUnlocked()
		{
			State = TState.Locked;
		}

		//

		void UnlockBtn_Clicked(object sender, System.EventArgs e)
		{
			UnlockingMode();
			TryToUnlock();
		}
	}
}
