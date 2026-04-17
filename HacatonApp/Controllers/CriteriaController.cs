using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HacatonApp.Controllers
{
    public class CriteriaController : Controller
    {
        // GET: CriteriaController
        public ActionResult Index()
        {
            return View();
        }

        // GET: CriteriaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: CriteriaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: CriteriaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CriteriaController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: CriteriaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: CriteriaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: CriteriaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
