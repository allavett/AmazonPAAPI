using System.Web.Mvc;
using AmazonPAAPI.Models;

namespace AmazonPAAPI.Controllers
{
    public class ItemSearchController : Controller
    {
        //
        // GET: /ItemSearch/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Search(string searchIndex = null, string searchKeywords = null, int pageNr = 1)
        {
            ItemSearch results = new ItemSearch(searchIndex, searchKeywords, pageNr);
            return View(results);
        }
	}
}