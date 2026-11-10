// See https://aka.ms/new-console-template for more information
using System.Net.Http.Headers;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("Hello, World!");


var textInput = "Downvote đang được Meta thử nghiệm nhằm giảm nội dung rác cho Facebook. Khi càng nhiều người nhấn nút, bình luận đó sẽ ít được hiển thị hơn. Tuy nhiên, công ty chưa mô tả chi tiết cách thức Downvote hoạt động cũng như thời gian triển khai toàn cầu.";
var apiKey = "";
var voiceId = "29vD33N1CtxCmqQRPOHJ";
var folderOutput = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "output");
var apiUrl = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}";
if (!Directory.Exists(folderOutput)) Directory.CreateDirectory(folderOutput);
var outputFileName = Path.Combine(folderOutput, "output.mp3");

using var client = new HttpClient();
client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
client.DefaultRequestHeaders.Add("xi-api-key", apiKey);
var model = new { text = textInput, model_id = "eleven_flash_v2_5", output_format = "mp3_44100_128", language_code = "vi" };
var requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
var response = await client.PostAsync(apiUrl, requestContent);
if (response.IsSuccessStatusCode)
{
    using (var stream = await response.Content.ReadAsStreamAsync())
    using (var fileStream = new FileStream(outputFileName, FileMode.Create, FileAccess.Write, FileShare.None))
    {
        await stream.CopyToAsync(fileStream);
    }
    Console.WriteLine($"Đã chuyển đổi thành công và lưu vào file: {outputFileName}");
}
else
{
    Console.WriteLine($"Lỗi khi gọi API: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
}



Console.ReadKey();