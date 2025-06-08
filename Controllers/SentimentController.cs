using EcommerceML.ML;
using EcommerceML.Models;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceML.Controllers
{
    //esto es el controller
    public class SentimentController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Analyze(SentimentInputModel input)
        {
            if (string.IsNullOrWhiteSpace(input.Opinion))
            {
                ModelState.AddModelError("", "Por favor ingresa una opinión válida.");
                return View("Index");
            }

            var prediction = SentimentModel.Predict(input.Opinion);

            ViewBag.Prediction = prediction.Prediction ? "Positivo :)" : "Negativo :(";
            ViewBag.Score = prediction.Score;

            return View("Index");
        }
    }
}
