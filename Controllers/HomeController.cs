using BugTrackerPro.Extentions;
using BugTrackerPro.Models;
using BugTrackerPro.Models.ViewModels;
using BugTrackerPro.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BugTrackerPro.Controllers;
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IBTProCompanyInfoService _companyInfoService;

    public HomeController(ILogger<HomeController> logger, IBTProCompanyInfoService companyInfoService)
    {
        _logger = logger;
        _companyInfoService = companyInfoService;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> Dashboard()
    {
        DashboardViewModel model = new();
        int companyId = User.Identity!.GetCompanyId()!.Value;

        model.Company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId);
        model.Projects = (await _companyInfoService.GetAllProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();
        model.Tickets = model.Projects.SelectMany(p => p.Tickets!).Where(t => t.Archived == false).ToList();
        model.Members = model.Company.Members!.ToList();

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
