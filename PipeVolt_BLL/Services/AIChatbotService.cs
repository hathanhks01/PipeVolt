using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PipeVolt_BLL.IServices;
using PipeVolt_DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PipeVolt_BLL.Services
{
    public class AIChatbotService : IAIChatbotService
    {
        private readonly PipeVoltDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _geminiApiKey;
        private readonly string _geminiEndpoint = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

        public AIChatbotService(PipeVoltDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _geminiApiKey = _configuration["OpenAI:ApiKey"]; // Giữ nguyên key từ config
        }

        public async Task<ChatMessage> ProcessUserMessageAsync(int chatRoomId, string userMessage, int senderId, int senderType)
        {
            try
            {
                // Phân tích intent của tin nhắn
                var intent = await AnalyzeMessageIntentAsync(userMessage);
                string botResponse = "";

                switch (intent.Type)
                {
                    case "product_inquiry":
                        botResponse = await GetProductRecommendationAsync(userMessage, senderId);
                        break;
                    case "warranty_check":
                        var warrantyInfo = ExtractWarrantyInfo(userMessage);
                        botResponse = await CheckWarrantyAsync(warrantyInfo.ProductCode, warrantyInfo.SerialNumber);
                        break;
                    case "technical_support":
                        botResponse = await GetTechnicalSupportAsync(userMessage);
                        break;
                    case "installation_guide":
                        var productId = await ExtractProductIdFromMessage(userMessage);
                        botResponse = await GetInstallationGuideAsync(productId);
                        break;
                    default:
                        botResponse = await GetGeneralResponseAsync(userMessage);
                        break;
                }

                // Lưu tin nhắn bot vào database
                var botMessage = new ChatMessage
                {
                    ChatRoomId = chatRoomId,
                    SenderId = 0, // Bot ID
                    SenderType = 2, // Bot type
                    MessageContent = botResponse,
                    MessageType = 0, // Text
                    IsRead = false,
                    SentAt = DateTime.Now
                };

                _context.ChatMessages.Add(botMessage);
                await _context.SaveChangesAsync();

                return botMessage;
            }
            catch (Exception ex)
            {
                // Log error và trả về tin nhắn lỗi
                return new ChatMessage
                {
                    ChatRoomId = chatRoomId,
                    SenderId = 0,
                    SenderType = 2,
                    MessageContent = "Xin lỗi, tôi đang gặp sự cố. Vui lòng thử lại sau hoặc liên hệ nhân viên hỗ trợ.",
                    MessageType = 0,
                    IsRead = false,
                    SentAt = DateTime.Now
                };
            }
        }

        public async Task<string> GetProductRecommendationAsync(string query, int? customerId = null)
        {
            try
            {
                // Lấy lịch sử mua hàng của khách hàng
                List<Product> purchasedProducts = new List<Product>();
                if (customerId.HasValue)
                {
                    purchasedProducts = await _context.OrderDetails
                        .Include(od => od.Order)
                        .Include(od => od.Product)
                        .Where(od => od.Order.CustomerId == customerId.Value)
                        .Select(od => od.Product)
                        .Distinct()
                        .ToListAsync();
                }

                // Lấy danh sách sản phẩm phù hợp
                var products = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .Where(p => p.ProductName.Contains(query) ||
                               p.Description.Contains(query) ||
                               p.Category.CategoryName.Contains(query))
                    .Take(5)
                    .ToListAsync();

                if (!products.Any())
                {
                    return "Xin lỗi, tôi không tìm thấy sản phẩm phù hợp với yêu cầu của bạn. Bạn có thể mô tả chi tiết hơn không?";
                }

                // Tạo prompt cho AI
                var prompt = $@"
                Khách hàng đang tìm: {query}
                
                Lịch sử mua hàng: {string.Join(", ", purchasedProducts.Select(p => p.ProductName))}
                
                Sản phẩm có sẵn:
                {string.Join("\n", products.Select(p => $"- {p.ProductName} ({p.Brand?.BrandName}) - {p.SellingPrice:C} - {p.Description}"))}
                
                Hãy tư vấn sản phẩm phù hợp nhất cho khách hàng, giải thích lý do và đưa ra gợi ý cụ thể.
                ";

                return await CallGeminiAsync(prompt);
            }
            catch (Exception ex)
            {
                return "Xin lỗi, tôi không thể tư vấn sản phẩm lúc này. Vui lòng liên hệ nhân viên để được hỗ trợ tốt hơn.";
            }
        }

        public async Task<string> CheckWarrantyAsync(string productCode, string serialNumber)
        {
            try
            {
                var warranty = await _context.Warranties
                    .Include(w => w.Product)
                    .Include(w => w.Customer)
                    .FirstOrDefaultAsync(w =>
                        w.Product.ProductCode == productCode &&
                        w.SerialNumber == serialNumber);

                if (warranty == null)
                {
                    return $"Không tìm thấy thông tin bảo hành cho sản phẩm {productCode} với serial {serialNumber}. Vui lòng kiểm tra lại thông tin.";
                }

                var remainingDays = (warranty.EndDate?.ToDateTime(TimeOnly.MinValue) - DateTime.Now)?.Days ?? 0;

                if (remainingDays > 0)
                {
                    return $@"
📋 **THÔNG TIN BẢO HÀNH**
🔹 Sản phẩm: {warranty.Product.ProductName}
🔹 Serial: {warranty.SerialNumber}
🔹 Ngày bắt đầu: {warranty.StartDate?.ToString("dd/MM/yyyy")}
🔹 Ngày kết thúc: {warranty.EndDate?.ToString("dd/MM/yyyy")}
🔹 Trạng thái: ✅ Còn bảo hành ({remainingDays} ngày)
🔹 Ghi chú: {warranty.Notes ?? "Không có"}

Sản phẩm của bạn vẫn trong thời gian bảo hành. Nếu có vấn đề, vui lòng mang sản phẩm đến trung tâm bảo hành.
                    ";
                }
                else
                {
                    return $@"
📋 **THÔNG TIN BẢO HÀNH**
🔹 Sản phẩm: {warranty.Product.ProductName}
🔹 Serial: {warranty.SerialNumber}
🔹 Trạng thái: ❌ Hết hạn bảo hành ({Math.Abs(remainingDays)} ngày trước)

Sản phẩm của bạn đã hết hạn bảo hành. Chúng tôi vẫn có thể hỗ trợ sửa chữa với chi phí hợp lý.
                    ";
                }
            }
            catch (Exception ex)
            {
                return "Xin lỗi, tôi không thể kiểm tra thông tin bảo hành lúc này. Vui lòng thử lại sau.";
            }
        }

        public async Task<string> GetTechnicalSupportAsync(string issue, int? productId = null)
        {
            try
            {
                string productInfo = "";
                if (productId.HasValue)
                {
                    var product = await _context.Products
                        .Include(p => p.Brand)
                        .Include(p => p.Category)
                        .FirstOrDefaultAsync(p => p.ProductId == productId.Value);

                    if (product != null)
                    {
                        productInfo = $"Sản phẩm: {product.ProductName} ({product.Brand?.BrandName})";
                    }
                }

                var prompt = $@"
                Bạn là chuyên gia kỹ thuật về thiết bị điện nước.
                
                {productInfo}
                Vấn đề khách hàng gặp phải: {issue}
                
                Hãy:
                1. Phân tích nguyên nhân có thể
                2. Đưa ra các bước khắc phục đơn giản khách hàng có thể tự thực hiện
                3. Cảnh báo các trường hợp cần thợ chuyên nghiệp
                4. Đưa ra lời khuyên an toàn
                
                Trả lời một cách chuyên nghiệp và dễ hiểu.
                ";

                return await CallGeminiAsync(prompt);
            }
            catch (Exception ex)
            {
                return "Xin lỗi, tôi không thể hỗ trợ kỹ thuật lúc này. Vui lòng liên hệ hotline để được hỗ trợ trực tiếp.";
            }
        }

        public async Task<string> GetInstallationGuideAsync(int productId)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Brand)
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (product == null)
                {
                    return "Không tìm thấy thông tin sản phẩm để hướng dẫn lắp đặt.";
                }

                var prompt = $@"
                Hãy tạo hướng dẫn lắp đặt chi tiết cho sản phẩm:
                - Tên: {product.ProductName}
                - Thương hiệu: {product.Brand?.BrandName}
                - Danh mục: {product.Category?.CategoryName}
                - Mô tả: {product.Description}
                
                Bao gồm:
                1. Chuẩn bị trước khi lắp đặt
                2. Dụng cụ cần thiết
                3. Các bước lắp đặt chi tiết
                4. Lưu ý an toàn
                5. Kiểm tra sau lắp đặt
                
                Sử dụng emoji và format đẹp để dễ đọc.
                ";

                return await CallGeminiAsync(prompt);
            }
            catch (Exception ex)
            {
                return "Xin lỗi, tôi không thể cung cấp hướng dẫn lắp đặt lúc này. Vui lòng tham khảo sách hướng dẫn đi kèm sản phẩm.";
            }
        }

        private async Task<MessageIntent> AnalyzeMessageIntentAsync(string message)
        {
            // Simple rule-based intent recognition
            var lowerMessage = message.ToLower();

            if (lowerMessage.Contains("bảo hành") || lowerMessage.Contains("warranty"))
            {
                return new MessageIntent { Type = "warranty_check", Confidence = 0.9 };
            }
            else if (lowerMessage.Contains("lắp đặt") || lowerMessage.Contains("cài đặt") || lowerMessage.Contains("install"))
            {
                return new MessageIntent { Type = "installation_guide", Confidence = 0.8 };
            }
            else if (lowerMessage.Contains("sửa chữa") || lowerMessage.Contains("hỏng") || lowerMessage.Contains("lỗi"))
            {
                return new MessageIntent { Type = "technical_support", Confidence = 0.8 };
            }
            else if (lowerMessage.Contains("sản phẩm") || lowerMessage.Contains("mua") || lowerMessage.Contains("tư vấn"))
            {
                return new MessageIntent { Type = "product_inquiry", Confidence = 0.7 };
            }

            return new MessageIntent { Type = "general", Confidence = 0.5 };
        }

        private async Task<string> CallGeminiAsync(string prompt)
        {
            try
            {
                var systemInstruction = "Bạn là trợ lý AI chuyên về thiết bị điện nước, luôn trả lời bằng tiếng Việt một cách thân thiện và chuyên nghiệp.";
                var fullPrompt = $"{systemInstruction}\n\n{prompt}";

                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = fullPrompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        maxOutputTokens = 500,
                        topP = 0.8,
                        topK = 40
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                var url = $"{_geminiEndpoint}?key={_geminiApiKey}";
                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GeminiResponse>(responseBody);

                    var generatedText = result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
                    return generatedText ?? "Xin lỗi, tôi không thể trả lời lúc này.";
                }

                return "Xin lỗi, hệ thống AI đang bận. Vui lòng thử lại sau.";
            }
            catch (Exception ex)
            {
                return "Xin lỗi, tôi đang gặp sự cố kỹ thuật. Vui lòng liên hệ nhân viên hỗ trợ.";
            }
        }

        private async Task<string> GetGeneralResponseAsync(string query)
        {
            var prompt = $@"
            Khách hàng hỏi: {query}
            
            Bạn là trợ lý của cửa hàng thiết bị điện nước PipeVolt.
            Hãy trả lời thân thiện và hướng dẫn khách hàng đến dịch vụ phù hợp.
            ";

            return await CallGeminiAsync(prompt);
        }

        private async Task<int> ExtractProductIdFromMessage(string message)
        {
            // Logic để extract product ID từ tin nhắn
            // Có thể sử dụng regex hoặc NLP
            return 0;
        }

        private (string ProductCode, string SerialNumber) ExtractWarrantyInfo(string message)
        {
            // Logic để extract thông tin bảo hành từ tin nhắn
            // Sử dụng regex để tìm mã sản phẩm và serial number
            return ("", "");
        }
    }

    // Supporting classes
    public class MessageIntent
    {
        public string Type { get; set; }
        public double Confidence { get; set; }
    }

    // Gemini API Response Classes
    public class GeminiResponse
    {
        public Candidate[] candidates { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
    }

    public class Content
    {
        public Part[] parts { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }
}