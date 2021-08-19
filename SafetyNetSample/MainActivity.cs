using Android.App;
using Android.OS;
using AndroidX.AppCompat.App;
using Android.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace SafetyNetSample
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var safetynetHelper = new SafetyNetHelper(this, "<your_key_here>");
            var (hasPlayServices, errorCode) = safetynetHelper.EnsurePlayServices();
            if (!hasPlayServices)
            {
                safetynetHelper.ShowPlayServicesResolution(errorCode);
            }

            SetContentView(Resource.Layout.activity_main);

            Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            TextView ctsProfileMatch = FindViewById<TextView>(Resource.Id.cts_profile_match_value);
            TextView basicIntegrity = FindViewById<TextView>(Resource.Id.basic_integrity_value);

            var attestation = await safetynetHelper.RequestAttestation();
            ctsProfileMatch.Text = $"{attestation.ctsProfileMatch}";
            basicIntegrity.Text = $"{attestation.basicIntegrity}";
        }
    }
}
