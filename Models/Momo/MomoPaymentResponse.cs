using Newtonsoft.Json;

namespace GYM_Manage.Models.Momo
{
    public class MomoPaymentResponse
    {
        [JsonProperty("payUrl")]
        public string PayUrl { get; set; }

        // Bạn có thể thêm các field khác nếu cần debug:
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("errorCode")]
        public int ErrorCode { get; set; }
    }


}
