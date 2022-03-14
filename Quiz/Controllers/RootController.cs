using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Quiz.Models;

namespace Quiz.Controllers;

public class RootController : Controller
{
    private readonly ILogger<RootController> _logger;

    public RootController(ILogger<RootController> logger)
    {
        _logger = logger;
    }

    /**
     * serves the home page
     */
    public IActionResult Index()
    {
        return View();
    }

    /**
     * serves the how to play page
     */
    public IActionResult Instructions()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
    }
}