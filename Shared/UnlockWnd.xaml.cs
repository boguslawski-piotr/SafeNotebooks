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
            _PIN.IsVisible = false;
            PIN_Focused(this, new FocusEventArgs(_PIN, false));
        }

        public bool UnlockingNeeded
        {
            get { return App.Settings.UnlockUsingDeviceOwnerAuthentication || App.Settings.UnlockUsingPin; }
        }

        public void SetUnlockingMode()
        {
            State = TState.Unlocking;
        }

        public void TryToUnlock()
        {
            SetUnlockingMode();

            if (App.Settings.UnlockUsingDeviceOwnerAuthentication)
            {
                if (TryToUnlockUsingDeviceOwnerAuthentication())
                {
                    return;
                };
            }

            TryToUnlockUsingPin();
        }

        public event EventHandler UnlockedCorrectly = null;

        //

        bool TryToUnlockUsingDeviceOwnerAuthentication()
        {
            return App.SecretsManager.AuthenticateDeviceOwner(T.Localized("AuthenticateDeviceOwnerReason"), OnUnlockedCorrectlyUsingDeviceOwnerAuthentication, OnNotUnlockedUsingDeviceOwnerAuthentication);
        }

        void OnUnlockedCorrectlyUsingDeviceOwnerAuthentication()
        {
            TryToUnlockUsingPin();
		}

        void OnNotUnlockedUsingDeviceOwnerAuthentication(string error)
        {
            State = TState.Locked;
        }

        void TryToUnlockUsingPin()
        {
            if (App.Settings.UnlockUsingPin)
            {
                SetUnlockingMode();

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
                                                    : Metrics.AppBarHeightPortrait / (Device.RuntimePlatform == Device.iOS ? 1 : 2)),
                                               0,
                                               0);
                _Logo.IsVisible = DeviceEx.Orientation != DeviceOrientation.Landscape;
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
