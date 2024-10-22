using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Hosting.Server.Features;

namespace Dynamics.Utility
{
    public static class Util
    {
        
        public static string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
        public static string UploadImage(IFormFile image, string folder)
        {
            try
            {
                // Get the ID of the user to mark it as filename 
                string fileName = Path.GetRandomFileName();
                string fileNameExtension = Path.GetExtension(image.FileName);
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName + fileNameExtension);
                using (var myfile = new FileStream(fullPath, FileMode.Create))
                {
                    image.CopyTo(myfile);
                }
                string imagePath = Path.Combine(folder, fileName + fileNameExtension);
                return "/" + imagePath.Replace('\\', '/');
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        // upload multiple images for request controller
        public static string UploadMultiImage(List<IFormFile> images, string folder, Guid id)
        {
            try
            {
                // List to hold the individual image paths
                List<string> imagesPath = new List<string>();
                // Process each image
                foreach (var image in images)
                {
                    if (image != null && image.Length > 0)
                    {
                        // create folder for each individual request
                        string folderName = id.ToString();
                        string filenameExtension = image.FileName;
                        bool folderExists = Directory.Exists(folder);
                        if(!folderExists) { Directory.CreateDirectory(@"wwwroot\" + folder); }
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder,
                            filenameExtension);
                        using (var myFile = new FileStream(fullPath, FileMode.Create))
                        {
                            image.CopyTo(myFile);
                        }

                        string imagePath = Path.Combine(folder,filenameExtension);
                        imagesPath.Add("/" + imagePath.Replace('\\', '/'));
                    }
                }

                // Return all image paths joined by ","
                return string.Join(",", imagesPath);
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        
        public static bool DeleteImage(string imagePath)
        {
            try
            {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('\\'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        public async static Task<string> UploadImages(List<IFormFile> images, string folder)
        {
            string imagesPath = "";
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif",".webp" }; 
            try
            {
                if(images.Count == 0)
                {
                    return "No file";
                }
                foreach (var image in images)
                {
                    var fileExtension = Path.GetExtension(image.FileName).ToLower();
                    if (!allowedExtensions.Contains(Path.GetExtension(image.FileName).ToLower()))
                    {
                        return "Wrong extension";
                    }

                    string fileName = Path.GetRandomFileName() + fileExtension;
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);
                    using (var myfile = new FileStream(fullPath, FileMode.Create))
                    {
                        await image.CopyToAsync(myfile);
                    }
                     imagesPath += ("/" + Path.Combine(folder, fileName) + ", ").Replace('\\', '/');                       
                }
                //imagesPath = imagesPath.TrimEnd(',', ' ');
                return imagesPath;
            }
            catch (Exception e)
            {
                return string.Empty;
            }

        }
        public async static Task<string> UploadFiles(List<IFormFile> files, string folder)
        {
            string filePath = "";
            var allowedExtensions = new[] { ".txt",".doc",".xls" ,".xlsx", ".docx", ".pdf",".csv" };
            try
            {
                if (files.Count == 0)
                {
                    return "No file";
                }
                foreach (var file in files)
                {
                    var fileExtension = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                    {
                        return "Wrong extension";
                    }

                    string fileName = Path.GetRandomFileName() + fileExtension;
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folder, fileName);
                    using (var myfile = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(myfile);
                    }
                    filePath += ("/" + Path.Combine(folder, fileName) + ", ").Replace('\\', '/');
                }
                filePath = filePath.TrimEnd(',', ' ');
                return filePath;
            }
            catch (Exception e)
            {
                return string.Empty;
            }

        }

    }
}
