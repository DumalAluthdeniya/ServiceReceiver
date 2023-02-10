using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Web.Administration;
using ServiceReceiver.Models;
using System.Diagnostics;
using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;
using System.DirectoryServices;

namespace ServiceReceiver.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IConfiguration _configuration;

		public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}

		public IActionResult Index()
		{
			var services = ServiceController.GetServices().ToList();
			var definedServices = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["Services"].Split(',').ToList();
			var ser = new List<Service>();
			services.FindAll(s => definedServices.Contains(s.ServiceName)).ForEach(f =>
			{
				ser.Add(new Service()
				{
					Name = f.ServiceName,
					Status = f.Status.ToString(),
				});
			});

			ServerManager manager = new ServerManager();
			var webSite = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["Site"];
			var ApplicationName = "/" + new ConfigurationBuilder().AddJsonFile("appsettings.json").Build()["ApplicationName"];
			Site defaultSite = manager.Sites[webSite];
			var app = defaultSite.Applications.Where(p => p.Path == ApplicationName).FirstOrDefault();
			
			var pool = new AppPool()
			{
				Name = app.ApplicationPoolName,
				AppPoolStatus = defaultSite.State.ToString()
			};

			return View(new ViewModel()
			{
				Services = ser,
				AppPool = pool,
			});
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
}