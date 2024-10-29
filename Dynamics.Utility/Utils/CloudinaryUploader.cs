using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.HttpSys;

namespace Dynamics.Utility;

public class CloudinaryUploader
{
    private string CLOUDINARY_URL;
    private Cloudinary cloudinary;

    public CloudinaryUploader()
    {
        CLOUDINARY_URL = "cloudinary://999623478143232:kevxumf-9F3oYg9drXSre13APdc@dv1zgaj5y";
        cloudinary = new Cloudinary(CLOUDINARY_URL);
        cloudinary.Api.Secure = true;
    }

    /**
     * Upload to the cloud and return an URL connect to it
     */
    public async Task<string?> UploadImageAsync(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
        {
            return "Wrong file extension";
        }
        var uploadParams = new ImageUploadParams()
        {
            File = new FileDescription(file.FileName, file.OpenReadStream()),
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = true,
        };
        var uploadResult = await cloudinary.UploadAsync(uploadParams);
        if (uploadResult.Error == null)
        {
            return uploadResult.Url.AbsoluteUri;
        }

        // Debug and check for the message if error happens on server side
        return null;
    }

    public async Task<string> UploadMultiImagesAsync(List<IFormFile> files, bool? checkValid = true)
    {
        if (checkValid == true && files.Count == 0) return "No file";

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp",".svg" };
        List<string> result = new List<string>();
        foreach (var file in files)
        {
            if (checkValid == true && !allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
            {
                return "Wrong extension";
            }

            var img = await UploadImageAsync(file);
            if (img != null)
            {
                result.Add(img);
            }
        }

        return string.Join(",", result);
    }
}