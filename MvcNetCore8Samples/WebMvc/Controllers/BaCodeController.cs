using Microsoft.AspNetCore.Mvc;
using System.DrawingCore.Imaging;
using ZXing.ZKWeb;

namespace WebMvc.Controllers;

public class BaCodeController : Controller
{
    private readonly ILogger<BaCodeController> _logger;
    public BaCodeController(ILogger<BaCodeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Generation(string id)
    {
        var barcode = new BarcodeWriter()
        {
            Format = ZXing.BarcodeFormat.CODE_128,
            Options = new ZXing.Common.EncodingOptions
            {
                Width = 300,
                Height = 100,
                GS1Format = true,
                Margin = 10
            }
        };
        var bitmap = barcode.Write(id);
        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return File(ms.ToArray(), "image/png");
    }
}