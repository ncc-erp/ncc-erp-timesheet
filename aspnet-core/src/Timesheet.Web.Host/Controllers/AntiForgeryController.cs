using Microsoft.AspNetCore.Antiforgery;
using Ncc.Controllers;

namespace Ncc.Web.Host.Controllers
{
    public class AntiForgeryController : TimesheetControllerBase
    {
        private readonly IAntiforgery _antiforgery;

        public AntiForgeryController(IAntiforgery antiforgery)
        {
            _antiforgery = antiforgery;
        }

        public void GetToken()
        {
            _antiforgery.SetCookieTokenAndHeader(HttpContext);
        }
    }
}
