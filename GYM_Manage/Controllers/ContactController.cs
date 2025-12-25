using Microsoft.AspNetCore.Mvc;

namespace GYM_Manage.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
