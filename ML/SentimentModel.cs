using Microsoft.ML;
using Microsoft.ML.Data;
using System;
using System.IO;

namespace EcommerceML.ML
{
    public class SentimentModel
    {
        private static readonly string _modelPath = Path.Combine("MLModels", "SentimentModel.zip");
        private static MLContext _mlContext = new MLContext(seed: 0);
        private static ITransformer _trainedModel;

        // Clase para datos de entrada (solo texto)
        private class SentimentData
        {
            public string Opinion { get; set; }
        }

        // Clase para datos con Label para entrenamiento
        private class SentimentDataWithLabel
        {
            public string Opinion { get; set; }
            public bool Label { get; set; }
        }

        // Clase para la predicción
        public class SentimentPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }

            public float Score { get; set; }
        }

        public static void Train()
        {
            var trainingData = new[]
            {
                new SentimentDataWithLabel { Opinion = "Me encanta este producto, es fantástico!", Label = true },
                new SentimentDataWithLabel { Opinion = "No me gustó, muy malo.", Label = false },
                new SentimentDataWithLabel { Opinion = "Muy bueno y de calidad.", Label = true },
                new SentimentDataWithLabel { Opinion = "Es terrible, no lo recomiendo.", Label = false }
            };

            var trainingDataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentDataWithLabel.Opinion))
                .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

            _trainedModel = pipeline.Fit(trainingDataView);

            Directory.CreateDirectory("MLModels");
            _mlContext.Model.Save(_trainedModel, trainingDataView.Schema, _modelPath);
        }

        public static void Load()
        {
            if (_trainedModel == null)
            {
                if (File.Exists(_modelPath))
                {
                    _trainedModel = _mlContext.Model.Load(_modelPath, out _);
                }
                else
                {
                    Train();
                }
            }
        }

        public static SentimentPrediction Predict(string opinion)
        {
            Load();

            var predEngine = _mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(_trainedModel);

            var input = new SentimentData { Opinion = opinion };

            return predEngine.Predict(input);
        }
    }
}
