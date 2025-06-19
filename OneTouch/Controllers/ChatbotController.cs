using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace OneTouch.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChatbotController> _logger;

        public ChatbotController(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<ChatbotController> logger)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest(new { error = "Message không được để trống" });
                }

                // Kiểm tra knowledge base trước
                var knowledgeResponse = GetKnowledgeBaseResponse(request.Message);
                if (!string.IsNullOrEmpty(knowledgeResponse))
                {
                    return Ok(new { response = knowledgeResponse });
                }

                // Nếu không tìm thấy trong knowledge base, gọi AI
                var aiResponse = await CallGeminiAPI(request.Message);
                return aiResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing chat request");
                return StatusCode(500, new { error = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }

        private string GetKnowledgeBaseResponse(string message)
        {
            var lowerMessage = message.ToLower().Trim();

            // Knowledge Base - Thông tin cơ bản về phòng khám
            var knowledgeBase = new Dictionary<string, string>
            {
                // Thông tin liên hệ
                ["số điện thoại"] = "Số điện thoại OneTouch Medical: 0123-456-789",
                ["điện thoại"] = "Số điện thoại OneTouch Medical: 0123-456-789",
                ["liên hệ"] = "Liên hệ OneTouch Medical:\n📞 Điện thoại: 0123-456-789\n📧 Email: info@onetouchmedical.com\n🏥 Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM",

                // Giá dịch vụ
                ["giá khám tổng quát"] = "Giá khám tổng quát: 200.000 VNĐ",
                ["phí khám"] = "Bảng giá dịch vụ:\n• Khám tổng quát: 200.000 VNĐ\n• Khám chuyên khoa: 300.000 VNĐ\n• Xét nghiệm máu: 150.000 VNĐ\n• Siêu âm: 250.000 VNĐ",
                ["bảng giá"] = "Bảng giá dịch vụ:\n• Khám tổng quát: 200.000 VNĐ\n• Khám chuyên khoa: 300.000 VNĐ\n• Xét nghiệm máu: 150.000 VNĐ\n• Siêu âm: 250.000 VNĐ",

                // Giờ làm việc
                ["giờ làm việc"] = "Giờ làm việc OneTouch Medical:\n• Thứ 2-6: 8:00 - 17:00\n• Thứ 7: 8:00 - 12:00\n• Chủ nhật: Nghỉ",
                ["thời gian"] = "Giờ làm việc OneTouch Medical:\n• Thứ 2-6: 8:00 - 17:00\n• Thứ 7: 8:00 - 12:00\n• Chủ nhật: Nghỉ",

                // Địa chỉ
                ["địa chỉ"] = "Địa chỉ OneTouch Medical: 123 Đường ABC, Quận XYZ, TP.HCM",
                ["ở đâu"] = "OneTouch Medical tọa lạc tại: 123 Đường ABC, Quận XYZ, TP.HCM",

                // Dịch vụ
                ["dịch vụ"] = "Dịch vụ OneTouch Medical:\n• Khám tổng quát\n• Khám chuyên khoa\n• Xét nghiệm\n• Siêu âm\n• Chụp X-quang\n• Tư vấn sức khỏe",

                // Đặt lịch
                ["đặt lịch"] = "Để đặt lịch khám, vui lòng:\n📞 Gọi: 0123-456-789\n💻 Truy cập website: www.onetouchmedical.com\n🏥 Đến trực tiếp phòng khám",

                // Chào hỏi
                ["xin chào"] = "Xin chào! Tôi là trợ lý AI của OneTouch Medical. Tôi có thể giúp bạn tìm hiểu về dịch vụ, giá cả, đặt lịch khám. Bạn cần hỗ trợ gì?",
                ["hello"] = "Xin chào! Tôi là trợ lý AI của OneTouch Medical. Tôi có thể giúp bạn tìm hiểu về dịch vụ, giá cả, đặt lịch khám. Bạn cần hỗ trợ gì?",
                ["chào"] = "Chào bạn! Có gì tôi có thể giúp đỡ không?"
            };

            // Tìm kiếm exact match
            if (knowledgeBase.ContainsKey(lowerMessage))
            {
                return knowledgeBase[lowerMessage];
            }

            // Tìm kiếm partial match
            foreach (var item in knowledgeBase)
            {
                if (lowerMessage.Contains(item.Key))
                {
                    return item.Value;
                }
            }

            return string.Empty;
        }

        private async Task<IActionResult> CallGeminiAPI(string message)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("Gemini API Key is missing");
                return StatusCode(500, new { error = "Cấu hình API key bị thiếu" });
            }

            var client = _httpClientFactory.CreateClient("Gemini");
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            // Enhanced system prompt với context về phòng khám
            var systemPrompt = @"Bạn là trợ lý AI của OneTouch Medical - một phòng khám y tế hiện đại tại TP.HCM.

THÔNG TIN PHÒNG KHÁM:
- Tên: OneTouch Medical
- Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM
- Điện thoại: 0123-456-789
- Email: info@onetouchmedical.com
- Giờ làm việc: T2-T6: 8:00-17:00, T7: 8:00-12:00, CN: Nghỉ

DỊCH VỤ & GIÁ:
- Khám tổng quát: 200.000 VNĐ
- Khám chuyên khoa: 300.000 VNĐ
- Xét nghiệm máu: 150.000 VNĐ
- Siêu âm: 250.000 VNĐ

NHIỆM VỤ:
1. Trả lời các câu hỏi về dịch vụ y tế
2. Hướng dẫn đặt lịch khám
3. Tư vấn sức khỏe cơ bản (không thay thế bác sĩ)
4. Luôn khuyến khích khách hàng đến khám trực tiếp khi cần thiết

HẠN CHẾ:
- KHÔNG chẩn đoán bệnh
- KHÔNG kê đơn thuốc
- KHÔNG thay thế ý kiến bác sĩ
- Luôn đề xuất gặp bác sĩ cho các vấn đề nghiêm trọng

Hãy trả lời ngắn gọn, thân thiện và chuyên nghiệp.";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = $"{systemPrompt}\n\nCâu hỏi của khách hàng: {message}" }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.3, // Giảm temperature để có câu trả lời ổn định hơn
                    maxOutputTokens = 300,
                    topK = 40,
                    topP = 0.95
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_MEDIUM_AND_ABOVE" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_MEDIUM_AND_ABOVE" }
                }
            };

            try
            {
                var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var response = await client.PostAsync(apiUrl,
                    new StringContent(jsonContent, Encoding.UTF8, "application/json"));

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonSerializer.Deserialize<GeminiResponse>(responseContent,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (responseData?.Candidates?.Length > 0 &&
                        responseData.Candidates[0]?.Content?.Parts?.Length > 0)
                    {
                        var aiResponse = responseData.Candidates[0].Content.Parts[0].Text;
                        return Ok(new { response = aiResponse });
                    }
                }

                return BadRequest(new { error = "Không nhận được phản hồi từ AI" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return StatusCode(500, new { error = "Lỗi kết nối AI service" });
            }
        }

        // API để thêm knowledge base (cho admin)
        [HttpPost("knowledge")]
        public async Task<IActionResult> AddKnowledge([FromBody] KnowledgeRequest request)
        {
            // TODO: Implement database storage for knowledge base
            // Hiện tại chỉ là placeholder
            return Ok(new { message = "Knowledge added successfully" });
        }

        // API để training từ conversation history
        [HttpPost("train")]
        public async Task<IActionResult> TrainFromHistory([FromBody] TrainingRequest request)
        {
            // TODO: Implement training logic from conversation history
            return Ok(new { message = "Training completed" });
        }
    }

    // Existing classes remain the same...
    public class ChatRequest
    {
        public string Message { get; set; } = string.Empty;
    }

    public class KnowledgeRequest
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    public class TrainingRequest
    {
        public ConversationHistory[] Conversations { get; set; } = Array.Empty<ConversationHistory>();
    }

    public class ConversationHistory
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public Part[]? Parts { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    public class GeminiErrorResponse
    {
        [JsonPropertyName("error")]
        public GeminiError? Error { get; set; }
    }

    public class GeminiError
    {
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        public int Code { get; set; }
    }
}