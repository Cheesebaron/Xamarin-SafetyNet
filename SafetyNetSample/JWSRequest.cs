using System.Text.Json.Serialization;

namespace SafetyNetSample
{
    public class JWSRequest
    {
        [JsonPropertyName("signedAttestation")]
        public string SignedAttestation { get; set; }
    }
}
