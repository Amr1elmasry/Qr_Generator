using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Drawing;
using ZXing.QrCode;
using ZXing;
using Qr_Generator.Models;
using NuGet.Protocol;
using static System.Net.Mime.MediaTypeNames;
using Image = System.Drawing.Image;

namespace Qr_Generator.Controllers
{
    public class GeneratorController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        public GeneratorController(ApplicationDbContext dbContext, IWebHostEnvironment webHostEnvironment)
        {
            _dbContext = dbContext;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Generate(int id)
        {
            var user = _dbContext.Users.SingleOrDefault(m => m.Id == id);
            if (user == null) return View(nameof(Index));
            string userSer = JsonConvert.SerializeObject( new UserToSerialize { Id = user.Id,  Name = user.Name });
            var writer = new QRCodeWriter();
            var resultBit = writer.encode(userSer, ZXing.BarcodeFormat.QR_CODE, 150, 150);
            var matrix = resultBit;
            int scale = 2;
            Bitmap result = new Bitmap(matrix.Width * scale, matrix.Height * scale);
            for (int x = 0; x < matrix.Width; x++)
            {
                for (int y = 0; y < matrix.Height; y++)
                {
                    Color pixel = matrix[x, y] ? Color.Black : Color.White;
                    for (int i = 0; i < scale; i++)
                    {
                        for (int j = 0; j < scale; j++)
                        {
                            result.SetPixel(x * scale + i, y * scale + j, pixel);
                        }
                    }
                }
            }
            string webRootPath = _webHostEnvironment.WebRootPath;
            string name = webRootPath + "\\Images\\" + user.Id + ".png";
            result.Save(name);
            user.QrCode = ImageToByte(result);
            _dbContext.Update(user);
            _dbContext.SaveChanges();
            System.IO.File.Delete(name);
            string imreBase64Data = Convert.ToBase64String(user.QrCode);
            string imgDataURL = string.Format("data:image/png;base64,{0}", imreBase64Data);
            ViewBag.URL = imgDataURL;
            return View(nameof(Index));
        }


        [HttpPost]
        public IActionResult ReadQR(IFormFile QrImage)
        {
            if(QrImage is not null && QrImage.Length < 2097152)
            {
                var reader = new BarcodeReaderGeneric();
                using (var memoryStream = new MemoryStream())
                {
                    QrImage.CopyToAsync(memoryStream);
                    var image = Image.FromStream(memoryStream);
                    
                    LuminanceSource source = new ZXing.Windows.Compatibility.BitmapLuminanceSource((Bitmap)image);
                    var read = reader.Decode(source);

                    if (read != null)
                    {
                        var obj = JsonConvert.DeserializeObject<UserToSerialize>(read.Text);
                        var user = _dbContext.Users.SingleOrDefault(u => u.Id == obj.Id && u.Name == obj.Name);
                        if (user is not null) return RedirectToAction("Profile", "Users", new { id = user.Id });
                    }
                }
            }
            
            return RedirectToAction("Index","Users");
        }
    }
}
