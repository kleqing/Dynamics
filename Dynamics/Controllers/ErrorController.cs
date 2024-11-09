using Microsoft.AspNetCore.Mvc;

namespace Dynamics.Controllers;

public class ErrorController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        Console.WriteLine("In the error controller");
        return View();
    }
    
    public IActionResult PageNotFound()
    {
        string originalPath = "unknown";
        if (HttpContext.Items.ContainsKey("originalPath"))
        {
            originalPath = HttpContext.Items["originalPath"] as string;
        }
        return View();
    }
}