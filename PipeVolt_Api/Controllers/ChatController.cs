using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PipeVolt_BLL.IServices;
using PipeVolt_BLL.Services;
using PipeVolt_DAL.DTOS;

namespace PipeVolt_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IChatService chatService, IHubContext<ChatHub> hubContext)
        {
            _chatService = chatService;
            _hubContext = hubContext;
        }

        /// <summary>
        /// Tạo mới một phòng chat.
        /// </summary>
        /// <param name="createDto">Thông tin phòng chat cần tạo.</param>
        /// <returns>Phòng chat vừa được tạo.</returns>
        [HttpPost("rooms")]
        public async Task<ActionResult<ChatRoomDto>> CreateChatRoom([FromBody] CreateChatRoomDto createDto)
        {
            try
            {
                var result = await _chatService.CreateChatRoomAsync(createDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách phòng chat của một khách hàng.
        /// </summary>
        /// <param name="customerId">ID khách hàng.</param>
        /// <returns>Danh sách phòng chat của khách hàng.</returns>
        [HttpGet("rooms/customer/{customerId}")]
        public async Task<ActionResult<List<ChatRoomDto>>> GetChatRoomsForCustomer(int customerId)
        {
            try
            {
                var result = await _chatService.GetChatRoomsForCustomerAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách phòng chat của một nhân viên.
        /// </summary>
        /// <param name="employeeId">ID nhân viên.</param>
        /// <returns>Danh sách phòng chat của nhân viên.</returns>
        [HttpGet("rooms/employee/{employeeId}")]
        public async Task<ActionResult<List<ChatRoomDto>>> GetChatRoomsForEmployee(int employeeId)
        {
            try
            {
                var result = await _chatService.GetChatRoomsForEmployeeAsync(employeeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy tất cả các phòng chat.
        /// </summary>
        /// <returns>Danh sách tất cả phòng chat.</returns>
        [HttpGet("rooms")]
        public async Task<ActionResult<List<ChatRoomDto>>> GetAllChatRooms()
        {
            try
            {
                var result = await _chatService.GetAllChatRoomsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gửi tin nhắn vào phòng chat.
        /// </summary>
        /// <param name="messageDto">Thông tin tin nhắn gửi đi.</param>
        /// <returns>Tin nhắn đã gửi.</returns>
        [HttpPost("messages")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageDto messageDto)
        {
            try
            {
                var result = await _chatService.SendMessageAsync(messageDto);

                // Gửi tin nhắn real-time qua SignalR
                await _hubContext.Clients.Group($"ChatRoom_{messageDto.ChatRoomId}")
                    .SendAsync("ReceiveMessage", result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách tin nhắn trong một phòng chat (có phân trang).
        /// </summary>
        /// <param name="chatRoomId">ID phòng chat.</param>
        /// <param name="page">Trang hiện tại (mặc định 1).</param>
        /// <param name="pageSize">Số lượng tin nhắn mỗi trang (mặc định 50).</param>
        /// <returns>Danh sách tin nhắn.</returns>
        [HttpGet("rooms/{chatRoomId}/messages")]
        public async Task<ActionResult<List<ChatMessageDto>>> GetChatMessages(
            int chatRoomId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            try
            {
                var result = await _chatService.GetChatMessagesAsync(chatRoomId, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu một tin nhắn là đã đọc.
        /// </summary>
        /// <param name="messageId">ID tin nhắn.</param>
        [HttpPut("messages/{messageId}/read")]
        public async Task<ActionResult> MarkMessageAsRead(int messageId)
        {
            try
            {
                await _chatService.MarkMessageAsReadAsync(messageId);

                // Thông báo tin nhắn đã được đọc qua SignalR
                await _hubContext.Clients.All.SendAsync("MessageRead", messageId);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Đánh dấu tất cả tin nhắn trong phòng chat là đã đọc cho một user.
        /// </summary>
        /// <param name="chatRoomId">ID phòng chat.</param>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="userType">Loại người dùng (khách hàng/nhân viên).</param>
        [HttpPut("rooms/{chatRoomId}/read-all")]
        public async Task<ActionResult> MarkAllMessagesAsRead(
            int chatRoomId,
            [FromQuery] int userId,
            [FromQuery] int userType)
        {
            try
            {
                await _chatService.MarkAllMessagesAsReadAsync(chatRoomId, userId, userType);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Lấy số lượng tin nhắn chưa đọc trong phòng chat cho một user.
        /// </summary>
        /// <param name="chatRoomId">ID phòng chat.</param>
        /// <param name="userId">ID người dùng.</param>
        /// <param name="userType">Loại người dùng (khách hàng/nhân viên).</param>
        /// <returns>Số lượng tin nhắn chưa đọc.</returns>
        [HttpGet("rooms/{chatRoomId}/unread-count")]
        public async Task<ActionResult<int>> GetUnreadMessageCount(
            int chatRoomId,
            [FromQuery] int userId,
            [FromQuery] int userType)
        {
            try
            {
                var result = await _chatService.GetUnreadMessageCountAsync(chatRoomId, userId, userType);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Gán nhân viên vào phòng chat.
        /// </summary>
        /// <param name="chatRoomId">ID phòng chat.</param>
        /// <param name="employeeId">ID nhân viên.</param>
        /// <returns>Phòng chat sau khi đã gán nhân viên.</returns>
        [HttpPut("rooms/{chatRoomId}/assign")]
        public async Task<ActionResult<ChatRoomDto>> AssignEmployeeToChat(
            int chatRoomId,
            [FromBody] int employeeId)
        {
            try
            {
                var result = await _chatService.AssignEmployeeToChatAsync(chatRoomId, employeeId);
                if (result == null)
                    return NotFound();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Đóng phòng chat.
        /// </summary>
        /// <param name="chatRoomId">ID phòng chat.</param>
        [HttpPut("rooms/{chatRoomId}/close")]
        public async Task<ActionResult> CloseChatRoom(int chatRoomId)
        {
            try
            {
                var result = await _chatService.CloseChatRoomAsync(chatRoomId);
                if (!result)
                    return NotFound();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
