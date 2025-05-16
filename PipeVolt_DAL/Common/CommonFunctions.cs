using System;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Http;

namespace PipeVolt_Api.Common
{
    // Định nghĩa lớp CryptoRandom ngay trong cùng không gian tên
    public static class CryptoRandom
    {
        public enum OutputFormat
        {
            Base64,
            Hex
        }

        public static string CreateUniqueId(int length, OutputFormat format = OutputFormat.Base64)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] data = new byte[length * 2]; // Tạo ra nhiều byte hơn để đảm bảo đủ độ dài sau khi chuyển đổi
                rng.GetBytes(data);

                switch (format)
                {
                    case OutputFormat.Hex:
                        return BitConverter.ToString(data).Replace("-", "").Substring(0, length);
                    case OutputFormat.Base64:
                    default:
                        return Convert.ToBase64String(data).Substring(0, length);
                }
            }
        }
    }

    public class CommonFunctions
    {
        public CommonFunctions( )
        {
        }

        public static string PhysicalPath(string relPath)
        {
            return Path.Combine(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "wwwroot", relPath.Replace('/', Path.DirectorySeparatorChar).Trim(Path.DirectorySeparatorChar));
        }

        public static bool IsEmail(string input)
        {
            // Regex kiểm tra email hợp lệ - mở rộng để chấp nhận nhiều tên miền hơn
            return Regex.IsMatch(input.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool IsPhoneNumber(string input)
        {
            // SDT gồm đúng 10 chữ số và bắt đầu bằng 0
            return Regex.IsMatch(input.Trim(), @"^0\d{9}$");
        }

        public static bool IsValidPassword(string password)
        {
            return Regex.IsMatch(password.Trim(), @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$");
        }

        public const string TestImg = "https://photo.znews.vn/w660/Uploaded/mdf_eioxrd/2021_07_06/2.jpg";

        public static string ProcessBase64Image(string encodedData, string folderPath)
        {
            encodedData = encodedData ?? TestImg;
            var ar = encodedData.Split("base64,");
            if (ar.Length != 2) throw new Exception("Invalid image data");
            if (!encodedData.StartsWith("data:image")) throw new Exception("Invalid image data");
            var fileExt = ar[0].Replace("data:image/", "").Replace(";", "").ToLower();
            var fname = DateTime.Now.ToString("yyyy-MM-dd") + "-" + CryptoRandom.CreateUniqueId(6, CryptoRandom.OutputFormat.Hex) + "." + fileExt;
            var imageBytes = Convert.FromBase64String(ar[1]);
            using var imgStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
            var img = System.Drawing.Image.FromStream(imgStream, true);
            folderPath = folderPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            if (!Directory.Exists(PhysicalPath(folderPath)))
            {
                Directory.CreateDirectory(PhysicalPath(folderPath));
            }
            var relPath = Path.Combine(folderPath, fname);
            var filePath = PhysicalPath(relPath);
            ImageFormat fm = ImageFormat.Bmp;
            switch (fileExt)
            {
                case "bmp":
                    fm = ImageFormat.Bmp;
                    break;
                case "jpeg":
                case "jpg":
                    fm = ImageFormat.Jpeg;
                    break;
                case "png":
                    fm = ImageFormat.Png;
                    break;
                case "gif":
                    fm = ImageFormat.Gif;
                    break;
                case "tiff":
                    fm = ImageFormat.Tiff;
                    break;
                default:
                    fm = ImageFormat.Jpeg; // Mặc định là JPEG
                    break;
            }
            using (var imageFile = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                img.Save(imageFile, fm);
                imageFile.Close();
            }
            img.Dispose();
            return relPath;
        }
        public static string UploadFile(IFormFile file, string folderPath)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file uploaded");
            }

            // Tạo đường dẫn đến thư mục lưu file
            string wwwRootPath = Path.Combine(Environment.CurrentDirectory, "wwwroot");
            folderPath = folderPath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
            string fileDirectory = Path.Combine(wwwRootPath, folderPath);

            // Kiểm tra và tạo thư mục nếu chưa tồn tại
            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            // Tạo tên file duy nhất
            string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            string filePath = Path.Combine(fileDirectory, uniqueFileName);

            // Lưu file vào thư mục
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            // Trả về đường dẫn tương đối của file
            return Path.Combine(folderPath, uniqueFileName).Replace("\\", "/");
        }
    }
}