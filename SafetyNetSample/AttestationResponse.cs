using System.Text.Json.Serialization;

namespace SafetyNetSample
{
    public class AttestationResponse
    {
        [JsonPropertyName("isValidSignature")]
        public bool IsValidSignature { get; set; }
    }
}
