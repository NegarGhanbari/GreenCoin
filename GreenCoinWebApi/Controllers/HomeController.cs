using System.Web.Mvc;

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
            return View();
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
