using EcommerceML.ML;
using EcommerceML.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace EcommerceML.Controllers
{
    public class RecommendationController : Controller
    {
        // GET: /Recommendation/
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Recommendation/GetRecommendations
        [HttpPost]
        public IActionResult GetRecommendations(RecommendationInputModel input)
        {
            if (input.UserId == 0)
            {
                ModelState.AddModelError("", "Por favor ingresa un UserId válido (>0).");
                return View("Index");
            }

            List<uint> recommendedProducts = RecommendationModel.RecommendProducts(input.UserId, 5);

            ViewBag.UserId = input.UserId;
            ViewBag.RecommendedProducts = recommendedProducts;

            return View("Index");
        }
    }
}
