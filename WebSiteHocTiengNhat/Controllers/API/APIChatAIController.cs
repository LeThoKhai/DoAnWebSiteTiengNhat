using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly string _geminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash-latest:generateContent"; // Cập nhật URL của API Gemini
    private readonly string _apiKey = "AIzaSyDkh-E-q9D9RVA0mzxtPc35WTY9JjxyeoI"; // API Key của bạn

    private readonly HttpClient _httpClient;

    public ChatController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // API endpoint để gửi tin nhắn đến Gemini và nhận phản hồi
    [HttpPost("sendMessage")]
    public async Task<IActionResult> SendMessage([FromBody] string request)
    {
        // Tạo dữ liệu JSON để gửi đi
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = request }
                    }
                }
            }
        };

        var jsonContent = JsonConvert.SerializeObject(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        // Thực hiện yêu cầu POST đến Gemini API
        var response = await _httpClient.PostAsync($"{_geminiApiUrl}?key={_apiKey}", content);

        if (response.IsSuccessStatusCode)
        {
            // Đọc nội dung phản hồi từ Gemini
            var responseData = await response.Content.ReadAsStringAsync();

            // Parse JSON phản hồi để lấy chỉ văn bản trả về
            var jsonResponse = JObject.Parse(responseData);
            var textResponse = jsonResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

            if (!string.IsNullOrEmpty(textResponse))
            {
                return Ok(textResponse); // Trả về chỉ văn bản trả lời
            }

            // Nếu không tìm thấy văn bản trả lời
            return BadRequest("Không có phản hồi hợp lệ từ Gemini.");
        }

        // Trả về lỗi nếu có sự cố trong yêu cầu
        return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
    }
}
