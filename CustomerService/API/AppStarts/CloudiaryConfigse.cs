using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Identity.Client;

public class CloudinaryConfig
{
    public Cloudinary Cloudinary { get; }

    public CloudinaryConfig()
    {
        Account account = new Account(
    "dpbscvwv3",
    "742261457416638",
    "89eQg0LcQcu9AUXLYWgEN-S63Lk");

        Cloudinary cloudinary = new Cloudinary(account);
        Cloudinary = new Cloudinary(account);
        Cloudinary.Api.Secure = true;        
    }
}
public static class ServiceExtensions
{
    public static void AddCloudinary(this IServiceCollection services)
    {
        var cloudinaryConfig = new CloudinaryConfig();
        services.AddSingleton(cloudinaryConfig.Cloudinary);
    }
}

