using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Depanneur.App.Controllers
{
    [Authorize]
    public class AppController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
