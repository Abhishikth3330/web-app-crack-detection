using Microsoft.AspNetCore.Mvc;

namespace aspnet_frontend.Models
{
    public class PredictionResult
    {
        public string Prediction { get; set; }
        public float Confidence { get; set; }
    }
}
