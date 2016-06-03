using System.Web.Mvc;
using GreenCoinWebApi.Models;

namespace GreenCoinWebApi.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Transfer()
        {
            //Get balance
            return View();
        }

        public ActionResult CreateWallet()
        {
            var model = new CreateWalletRequest() {UserName = "negar"};
            return View(model);
        }

        public ActionResult ViewBalance()
        {
            return View();
        }

        public ActionResult AddCoin()
        {
            return View();
        }

    }
}
