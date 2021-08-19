using System;
using Android.App;
using Android.Gms.Common;
using System.Threading.Tasks;
using Android.Gms.SafetyNet;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace SafetyNetSample
{
    class SafetyNetHelper
    {
        private const string attestationVerificationUrl = "https://www.googleapis.com/androidcheck/v1/attestations/verify?key=";
        private readonly Activity context;
        private readonly string attestationApiKey;
        private readonly HttpClient httpClient;

        public SafetyNetHelper(Activity context, string attestationApiKey)
        {
            this.context = context;
            this.attestationApiKey = attestationApiKey;
            httpClient = new HttpClient();
        }

        public (bool available, int errorCode) EnsurePlayServices()
        {
            var code = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(context, 13000000);

            if (code == ConnectionResult.Success)
            {
                return (true, 0);
            }

            return (false, code);
        }

        public void ShowPlayServicesResolution(int errorCode)
        {
            var instance = GoogleApiAvailability.Instance;

            if (instance.IsUserResolvableError(errorCode))
            {
                instance.ShowErrorDialogFragment(context, errorCode, 1234);
            }
        }

        public async Task<(bool ctsProfileMatch, bool basicIntegrity)> RequestAttestation()
        {
            SafetyNetClient client = SafetyNetClass.GetClient(context);
            var nonce = Nonce.Generate(24);

            try
            {
                var response = await client.AttestAsync(nonce, attestationApiKey).ConfigureAwait(false);
                var result = response.JwsResult;
                var validSignature = await VerifyAttestationOnline(result).ConfigureAwait(false);
                if (validSignature)
                {
                    var jwtToken = new JwtSecurityToken(result);
                    var cts = jwtToken.Claims.First(claim => claim.Type == "ctsProfileMatch").Value;
                    var basicIntegrity = jwtToken.Claims.First(claim => claim.Type == "ctsProfileMatch").Value;

                    return (bool.Parse(cts), bool.Parse(basicIntegrity));
                }
            }
            catch (Exception)
            {
                // handle errors here
            }

            return (false, false);
        }

        // This should actually be done by your server, not by the App!
        public async Task<bool> VerifyAttestationOnline(string attestation)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"{attestationVerificationUrl}{attestationApiKey}");
            var data = new JWSRequest { SignedAttestation = attestation };
            var json = JsonSerializer.Serialize(data);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            using var responseData = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var attestationResponse = await JsonSerializer.DeserializeAsync<AttestationResponse>(responseData).ConfigureAwait(false);

            return attestationResponse.IsValidSignature;
        }
    }
}
