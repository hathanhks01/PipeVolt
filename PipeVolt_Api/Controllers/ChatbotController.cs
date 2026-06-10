using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PipeVolt_BLL.IServices;
using PipeVolt_BLL.Services;
using System.ComponentModel.DataAnnotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private readonly IAIChatbotService _chatbotService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatbotController(IAIChatbotService chatbotService, IHubContext<ChatHub> hubContext)
        {
            _chatbotService = chatbotService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Gửi tin nhắn tới chatbot và nhận phản hồi
        /// </summary>
        /// <param name="request">Thông tin tin nhắn từ người dùng</param>
        /// <returns>Phản hồi từ chatbot</returns>
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessageRequest request)
        {
            try
            {
                // Validate dữ liệu đầu vào
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Gọi service xử lý tin nhắn và tạo phản hồi từ AI
                var botResponse = await _chatbotService.ProcessUserMessageAsync(
                    request.ChatRoomId,
                    request.Message,
                    request.SenderId,
                    request.SenderType
                );

                // Broadcast bot response to all clients in the room via SignalR
                await _hubContext.Clients.Group($"ChatRoom_{request.ChatRoomId}")
                    .SendAsync("ReceiveMessage", new
                    {
                        messageId = botResponse.MessageId,
                        chatRoomId = botResponse.ChatRoomId,
                        senderId = botResponse.SenderId,
                        senderType = botResponse.SenderType,
                        senderName = "Trợ lý AI",
                        messageContent = botResponse.MessageContent,
                        messageType = botResponse.MessageType,
                        isRead = botResponse.IsRead,
                        sentAt = botResponse.SentAt
                    });

                // Trả về phản hồi thành công
                return Ok(new
                {
                    success = true,
                    message = "Tin nhắn đã được xử lý thành công",
                    data = new
                    {
                        messageId = botResponse.MessageId,
                        content = botResponse.MessageContent,
                        sentAt = botResponse.SentAt,
                        senderId = botResponse.SenderId,
                        senderType = botResponse.SenderType
                    }
                });
            }
            catch (Exception ex)
            {
                // Log lỗi và trả về thông báo lỗi chung
                return StatusCode(500, new
                {
                    success = false,
                    message = "Đã xảy ra lỗi khi xử lý tin nhắn",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Tư vấn sản phẩm dựa trên yêu cầu của khách hàng
        /// </summary>
        /// <param name="request">Thông tin yêu cầu tư vấn sản phẩm</param>
        /// <returns>Gợi ý sản phẩm từ AI</returns>
        [HttpPost("product-recommendation")]
        public async Task<IActionResult> GetProductRecommendation([FromBody] ProductRecommendationRequest request)
        {
            try
            {
                // Validate dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest("Vui lòng nhập yêu cầu tư vấn sản phẩm");
                }

                // Gọi service để AI tư vấn sản phẩm dựa trên query và lịch sử mua hàng
                var recommendation = await _chatbotService.GetProductRecommendationAsync(
                    request.Query,
                    request.CustomerId
                );

                // Trả về kết quả tư vấn
                return Ok(new
                {
                    success = true,
                    message = "Tư vấn sản phẩm thành công",
                    data = new
                    {
                        query = request.Query,
                        customerId = request.CustomerId,
                        recommendation = recommendation
                    }
                });
            }
            catch (Exception ex)
            {
                // Xử lý lỗi và trả về thông báo lỗi
                return StatusCode(500, new
                {
                    success = false,
                    message = "Không thể tư vấn sản phẩm lúc này",
                    error = ex.Message
                });
            }
        }

       

        /// <summary>
        /// Kiểm tra trạng thái hoạt động của chatbot
        /// </summary>
        /// <returns>Trạng thái chatbot</returns>
        [HttpGet("health-check")]
        public IActionResult HealthCheck()
        {
            // Endpoint để kiểm tra chatbot có hoạt động bình thường không
            return Ok(new
            {
                success = true,
                message = "Chatbot đang hoạt động bình thường",
                timestamp = DateTime.Now,
                version = "1.0.0"
            });
        }
    }

    // ===== REQUEST MODELS =====
    // Các model để nhận dữ liệu từ client

    /// <summary>
    /// Model cho request gửi tin nhắn tới chatbot
    /// </summary>
    public class ChatMessageRequest
    {
        [Required(ErrorMessage = "ChatRoomId là bắt buộc")]
        public int ChatRoomId { get; set; }

        [Required(ErrorMessage = "Tin nhắn không được để trống")]
        [StringLength(1000, ErrorMessage = "Tin nhắn không được vượt quá 1000 ký tự")]
        public string Message { get; set; }

        [Required(ErrorMessage = "SenderId là bắt buộc")]
        public int SenderId { get; set; }

        [Required(ErrorMessage = "SenderType là bắt buộc")]
        public int SenderType { get; set; } // 1: Customer, 2: Employee
    }

    /// <summary>
    /// Model cho request tư vấn sản phẩm
    /// </summary>
    public class ProductRecommendationRequest
    {
        [Required(ErrorMessage = "Query tư vấn là bắt buộc")]
        [StringLength(500, ErrorMessage = "Query không được vượt quá 500 ký tự")]
        public string Query { get; set; }

        public int? CustomerId { get; set; } // Optional để lấy lịch sử mua hàng
    }

    /// <summary>
    /// Model cho request kiểm tra bảo hành
    /// </summary>
    public class WarrantyCheckRequest
    {
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc")]
        [StringLength(50, ErrorMessage = "Mã sản phẩm không được vượt quá 50 ký tự")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Số serial là bắt buộc")]
        [StringLength(50, ErrorMessage = "Số serial không được vượt quá 50 ký tự")]
        public string SerialNumber { get; set; }
    }

    /// <summary>
    /// Model cho request hỗ trợ kỹ thuật
    /// </summary>
    public class TechnicalSupportRequest
    {
        [Required(ErrorMessage = "Mô tả vấn đề là bắt buộc")]
        [StringLength(1000, ErrorMessage = "Mô tả vấn đề không được vượt quá 1000 ký tự")]
        public string Issue { get; set; }

        public int? ProductId { get; set; } // Optional để lấy thông tin sản phẩm cụ thể
    }
}
