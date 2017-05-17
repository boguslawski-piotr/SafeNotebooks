using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Forms;
using pbXForms;
using System.Threading.Tasks;

//#if __IOS__
#if __UNIFIED__
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
            PK_Focused(this, new FocusEventArgs(PIN, PIN.IsFocused));
        }

        public void SplashMode()
        {
            State = TState.Splash;
            PIN.IsVisible = false;
            PK_Focused(this, new FocusEventArgs(PIN, false));
        }

        public void UnlockingMode()
        {
            State = TState.Unlocking;
        }

        public void TryToUnlock()
        {
            UnlockingMode();

            if (App.Settings.UnlockUsingSystem)
            {
                if (RuntimePlatformTryToUnlock())
                {
                    return;
                };
            }

            CrossPlatformTryToUnlock();
        }

        public event EventHandler UnlockedCorrectly = null;

        //

        bool RuntimePlatformTryToUnlock()
        {
            return App.CredentialsManager.AuthenticateDeviceOwner("TODO: some explanation", _RuntimePlatformUnlockedCorrectly, _RuntimePlatformNotUnlocked);
        }

        void _RuntimePlatformUnlockedCorrectly()
        {
            CrossPlatformTryToUnlock();
        }

        void _RuntimePlatformNotUnlocked(string Error)
        {
            State = TState.Locked;
        }

        void CrossPlatformTryToUnlock()
        {
            if (App.Settings.UnlockUsingPin)
            {
                UnlockingMode();

                PIN.IsVisible = true;
                PIN.Focus();
            }
            else
                _UnlockedCorrectly();
        }

        async void CrossPlatformCheckPK()
        {
            string _PIN = PIN.Text ?? "";
            PIN.Text = "";

            if (_PIN == "1")
            {
                _UnlockedCorrectly();
            }
            else
            {
                PIN.Focus();

				// TODO: zrobić animację wstrząśnięcia ;)
                await PIN.ScaleTo(0.3);
                await PIN.ScaleTo(1);
            }
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
            if (PIN.IsVisible)
            {
                CrossPlatformCheckPK();
            }
            else
                TryToUnlock();
        }

        void PK_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
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
            	Logo.IsVisible = DeviceEx.Orientation != DeviceOrientation.Landscape;
            }
            else
            {
                _View.VerticalOptions = LayoutOptions.CenterAndExpand;
                _View.Padding = new Thickness(0);
				Logo.IsVisible = true;
            }
			BatchCommit();
        }

        void PK_Completed(object sender, System.EventArgs e)
        {
            if (PIN.Text != null && PIN.Text.Length > 0)
                CrossPlatformCheckPK();
        }

    }
}
