using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EcommerceML.ML
{
    public class InteractionData
    {
        [KeyType(count: 1000)]
        public uint UserId { get; set; }

        [KeyType(count: 1000)]
        public uint ProductId { get; set; }

        public float Label { get; set; }
    }

    public class ProductPrediction
    {
        public float Score { get; set; }

        public uint ProductId { get; set; }
    }

    public class RecommendationModel
    {
        private static readonly string _modelPath = Path.Combine("MLModels", "recommendationModel.zip");
        private static MLContext _mlContext = new MLContext();
        private static ITransformer _trainedModel;
        private static PredictionEngine<InteractionData, ProductPrediction> _predictionEngine;

        private static List<InteractionData> _trainingData = new List<InteractionData>
        {
            new InteractionData { UserId = 1, ProductId = 1, Label = 5 },
            new InteractionData { UserId = 1, ProductId = 2, Label = 3 },
            new InteractionData { UserId = 2, ProductId = 2, Label = 4 },
            new InteractionData { UserId = 2, ProductId = 3, Label = 2 },
            new InteractionData { UserId = 3, ProductId = 1, Label = 1 },
            new InteractionData { UserId = 3, ProductId = 3, Label = 5 },
        };

        public static void Train()
        {
            var data = _mlContext.Data.LoadFromEnumerable(_trainingData);

            var options = new MatrixFactorizationTrainer.Options
            {
                MatrixColumnIndexColumnName = nameof(InteractionData.UserId),
                MatrixRowIndexColumnName = nameof(InteractionData.ProductId),
                LabelColumnName = nameof(InteractionData.Label),
                NumberOfIterations = 20,
                ApproximationRank = 100
            };

            var pipeline = _mlContext.Recommendation().Trainers.MatrixFactorization(options);

            _trainedModel = pipeline.Fit(data);

            Directory.CreateDirectory("MLModels");
            _mlContext.Model.Save(_trainedModel, data.Schema, _modelPath);
        }

        public static void Load()
        {
            if (!File.Exists(_modelPath))
                Train();

            DataViewSchema schema;
            _trainedModel = _mlContext.Model.Load(_modelPath, out schema);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<InteractionData, ProductPrediction>(_trainedModel);
        }

        public static List<uint> RecommendProducts(uint userId, int numberOfProducts = 3)
        {
            if (_predictionEngine == null)
                Load();

            // Productos con IDs del 1 al 5
            var allProducts = Enumerable.Range(1, 5).Select(i => (uint)i);

            var scoredProducts = new List<(uint ProductId, float Score)>();

            foreach (var productId in allProducts)
            {
                var input = new InteractionData { UserId = userId, ProductId = productId };
                var prediction = _predictionEngine.Predict(input);
                scoredProducts.Add((productId, prediction.Score));
            }

            return scoredProducts.OrderByDescending(x => x.Score)
                .Take(numberOfProducts)
                .Select(x => x.ProductId)
                .ToList();
        }
    }
}
