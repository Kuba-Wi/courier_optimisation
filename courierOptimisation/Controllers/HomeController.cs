﻿using courierOptimisation.Models;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using System.Web;

namespace courierOptimisation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IndexModel _model;
        private PathsFinder _pathsFinder;

        public HomeController(ILogger<HomeController> logger,
            IndexModel model, PathsFinder pathsFinder)
        {
            _logger = logger;
            _model = model;
            _pathsFinder = pathsFinder;
        }

        public IActionResult Index()
        {
            return View(_model);
        }

        public IActionResult Test()
        {
            _model.DistanceMatrix = DistanceMatrixHelper.GenerateDistanceMatrix(_model.Points);
            _pathsFinder._distanceMatrix = _model.DistanceMatrix;
            _pathsFinder._clientsWeights = _model.Weights;
            _pathsFinder.traverseSolutions();
            _model.Paths = _pathsFinder.bestPaths;
            _model.PathsToJson();
            _model.InitCost = _pathsFinder.InitCost;
            _model.Cost = _pathsFinder.FinalCost;
            return View("Index", _model);
        }

        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            _model.Paths.Clear();
            _model.Points.Clear();
            List<string> fileContent = new();

            if (file != null)
            {
                string? line;
                using (var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8))
                {
                    while((line = reader.ReadLine()) != null)
                    {
                        fileContent.Add(line);
                    }
                }
            }
            _model.Points = DistanceMatrixHelper.ConvertStringsToPoints(fileContent);
            _model.Weights = DistanceMatrixHelper.ConvertStringsToWeights(fileContent);
            _model.PointsToJson();
            _model.PathsToJson();
            return View("Index", _model);
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