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
        private readonly IProductToolService _productToolService;
        private readonly string _openAiApiKey;
        private readonly string _openAiModel;
        private const string OpenAiEndpoint = "https://api.openai.com/v1/responses";

        public AIChatbotService(
            PipeVoltDbContext context,
            HttpClient httpClient,
            IConfiguration configuration,
            IProductToolService productToolService)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _productToolService = productToolService;
            _openAiApiKey = _configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured.");
            _openAiModel = _configuration["OpenAI:Model"] ?? "gpt-5.4-mini";
        }

        // ─── Public entry points ───────────────────────────────────────────────

        public async Task<string> GetProductRecommendationAsync(string query, int? customerId = null)
        {
            const string systemPrompt =
                "Bạn là trợ lý tư vấn của cửa hàng vật tư điện nước PipeVolt.\n" +
                "Nhiệm vụ: tư vấn sản phẩm phù hợp cho khách hàng dựa trên yêu cầu.\n" +
                "QUAN TRỌNG: Luôn dùng tool search_products để tra cứu sản phẩm trước khi trả lời.\n" +
                "Nếu không tìm thấy sản phẩm phù hợp trong cửa hàng, hãy từ chối lịch sự và KHÔNG gợi ý danh mục khác hay sản phẩm không có trong kết quả tìm kiếm.\n" +
                "Chỉ tư vấn các sản phẩm thực sự có trong kết quả tool trả về.\n" +
                "Trả lời ngắn gọn bằng tiếng Việt.";

            return await RunAgenticLoopAsync(systemPrompt, query);
        }

        public async Task<ChatMessage> ProcessUserMessageAsync(
            int chatRoomId, string userMessage, int senderId, int senderType)
        {
            const string systemPrompt =
                "Bạn là trợ lý hỗ trợ khách hàng của PipeVolt - cửa hàng vật tư điện nước.\n" +
                "Luôn dùng tool search_products để tra cứu sản phẩm trước khi tư vấn.\n" +
                "QUAN TRỌNG:\n" +
                "- Chỉ tư vấn các sản phẩm thực sự có trong kết quả tool trả về.\n" +
                "- Nếu không tìm thấy sản phẩm phù hợp, hãy trả lời: 'Xin lỗi, cửa hàng hiện không có sản phẩm này.' và dừng lại.\n" +
                "- KHÔNG gợi ý danh mục khác, KHÔNG đề xuất sản phẩm thay thế ngoài dữ liệu cửa hàng.\n" +
                "Trả lời ngắn gọn bằng tiếng Việt.";

            string botResponse;
            try
            {
                botResponse = await RunAgenticLoopAsync(systemPrompt, userMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AIChatbot Error] {ex.Message}");
                botResponse = "Xin lỗi, tôi đang gặp sự cố. Vui lòng thử lại hoặc liên hệ nhân viên hỗ trợ.";
            }

            var botMessage = new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = 0,
                SenderType = 1,
                MessageContent = botResponse,
                MessageType = 0,
                IsRead = false,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(botMessage);
            await _context.SaveChangesAsync();
            return botMessage;
        }

        // ─── Agentic loop ──────────────────────────────────────────────────────

        /// <summary>
        /// Sends a request to OpenAI Responses API and loops if the model wants to call tools,
        /// using previous_response_id chaining to submit tool results.
        /// </summary>
        private async Task<string> RunAgenticLoopAsync(
            string systemPrompt,
            string userInput,
            int maxIterations = 5)
        {
            var tools = BuildToolDeclarations();
            string? previousResponseId = null;

            // First request
            var firstRequest = new
            {
                model = _openAiModel,
                instructions = systemPrompt,
                input = userInput,
                tools,
                store = true,
                previous_response_id = (string?)null
            };

            var (responseId, outputItems) = await PostAsync(firstRequest);
            previousResponseId = responseId;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                // Check for text reply
                var textReply = ExtractTextReply(outputItems);
                if (textReply != null)
                    return textReply;

                // Collect all function_call items
                var functionCalls = outputItems
                    .Where(o => o.TryGetProperty("type", out var t) && t.GetString() == "function_call")
                    .ToList();

                if (!functionCalls.Any())
                    break; // No text and no tool calls – bail out

                // Execute each tool and build function_call_output list
                var toolOutputs = new List<object>();
                foreach (var fc in functionCalls)
                {
                    var callId = fc.GetProperty("call_id").GetString()!;
                    var toolName = fc.GetProperty("name").GetString()!;
                    var argsRaw = fc.GetProperty("arguments").GetString() ?? "{}";

                    var args = JsonSerializer.Deserialize<Dictionary<string, object>>(argsRaw);
                    Console.WriteLine($"[Tool Call] {toolName} args={argsRaw}");

                    string toolResult;
                    try
                    {
                        toolResult = await _productToolService.ExecuteToolAsync(toolName, args);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Tool Error] {toolName}: {ex.Message}");
                        toolResult = $"Lỗi khi thực hiện tool {toolName}: {ex.Message}";
                    }

                    toolOutputs.Add(new
                    {
                        type = "function_call_output",
                        call_id = callId,
                        output = toolResult
                    });
                }

                // Submit tool results as next turn
                var followupRequest = new
                {
                    model = _openAiModel,
                    instructions = systemPrompt,
                    input = toolOutputs,   // array of function_call_output items
                    tools,
                    store = true,
                    previous_response_id = previousResponseId
                };

                (responseId, outputItems) = await PostAsync(followupRequest);
                previousResponseId = responseId;
            }

            // If we exhausted iterations without a text reply
            var lastText = ExtractTextReply(outputItems);
            return lastText ?? "Xin lỗi, tôi không thể xử lý yêu cầu này lúc này. Vui lòng thử lại.";
        }

        // ─── HTTP helpers ──────────────────────────────────────────────────────

        private async Task<(string ResponseId, List<JsonElement> OutputItems)> PostAsync(object requestBody)
        {
            var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });

            var request = new HttpRequestMessage(HttpMethod.Post, OpenAiEndpoint);
            request.Headers.Add("Authorization", $"Bearer {_openAiApiKey}");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var httpResponse = await _httpClient.SendAsync(request);

            if (!httpResponse.IsSuccessStatusCode)
            {
                var err = await httpResponse.Content.ReadAsStringAsync();
                Console.WriteLine($"[OpenAI Error] {(int)httpResponse.StatusCode} {err}");
                throw new HttpRequestException($"OpenAI API error {(int)httpResponse.StatusCode}: {err}");
            }

            var body = await httpResponse.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<JsonElement>(body);

            var responseId = result.GetProperty("id").GetString()!;
            var outputItems = result.GetProperty("output").EnumerateArray().ToList();

            return (responseId, outputItems);
        }

        private static string? ExtractTextReply(List<JsonElement> outputItems)
        {
            foreach (var item in outputItems)
            {
                if (!item.TryGetProperty("type", out var typeEl)) continue;
                if (typeEl.GetString() != "message") continue;

                if (!item.TryGetProperty("content", out var contentArr)) continue;

                foreach (var part in contentArr.EnumerateArray())
                {
                    if (part.TryGetProperty("type", out var partType) &&
                        partType.GetString() == "output_text" &&
                        part.TryGetProperty("text", out var textEl))
                    {
                        return textEl.GetString();
                    }
                }
            }
            return null;
        }

        // ─── Tool declarations ─────────────────────────────────────────────────

        /// <summary>
        /// OpenAI Responses API uses a FLAT tool schema (name/description/parameters at top level),
        /// unlike Chat Completions which wraps them inside a "function" object.
        /// </summary>
        private static object[] BuildToolDeclarations()
        {
            return new object[]
            {
                new
                {
                    type = "function",
                    name = "search_products",
                    description = "Tìm kiếm sản phẩm theo từ khóa hoặc danh mục. Dùng khi khách hỏi về sản phẩm cụ thể.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            keyword = new { type = "string", description = "Từ khóa tìm kiếm (tên sản phẩm, loại vật tư)" },
                            categoryId = new { type = "integer", description = "ID danh mục (không bắt buộc)" }
                        },
                        required = new[] { "keyword" }
                    }
                },
                new
                {
                    type = "function",
                    name = "get_product_detail",
                    description = "Lấy thông tin chi tiết một sản phẩm theo ID.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            productId = new { type = "integer", description = "ID sản phẩm" }
                        },
                        required = new[] { "productId" }
                    }
                },
                new
                {
                    type = "function",
                    name = "check_inventory",
                    description = "Kiểm tra tồn kho của sản phẩm. Dùng khi khách hỏi còn hàng không.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            productId = new { type = "integer", description = "ID sản phẩm" }
                        },
                        required = new[] { "productId" }
                    }
                },
                new
                {
                    type = "function",
                    name = "get_categories",
                    description = "Lấy danh sách tất cả danh mục sản phẩm.",
                    parameters = new
                    {
                        type = "object",
                        properties = new { }
                    }
                },
                new
                {
                    type = "function",
                    name = "get_products_by_price_range",
                    description = "Lọc sản phẩm theo khoảng giá.",
                    parameters = new
                    {
                        type = "object",
                        properties = new
                        {
                            minPrice = new { type = "number", description = "Giá thấp nhất (VND)" },
                            maxPrice = new { type = "number", description = "Giá cao nhất (VND)" }
                        },
                        required = new[] { "minPrice", "maxPrice" }
                    }
                }
            };
        }
    }

    // ─── Supporting response classes (kept for compatibility) ──────────────────

    public class GeminiResponse
    {
        public Candidate[]? candidates { get; set; }
    }

    public class Candidate
    {
        public Content? content { get; set; }
    }

    public class Content
    {
        public Part[]? parts { get; set; }
    }

    public class Part
    {
        public string? text { get; set; }
    }
}