using aspnet_frontend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace aspnet_frontend.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Please upload an image file.";
                return View();
            }

            var client = _httpClientFactory.CreateClient();

            // Convert uploaded image to byte stream
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var form = new MultipartFormDataContent();
            var streamContent = new StreamContent(memoryStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Adjust if needed
            form.Add(streamContent, "file", file.FileName);

            try
            {
                var response = await client.PostAsync("http://127.0.0.1:5000/predict", form);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Error from backend API.";
                    return View();
                }

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PredictionResult>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Convert uploaded image to base64 to display in the view without saving
                var base64Image = Convert.ToBase64String(memoryStream.ToArray());
                ViewBag.UploadedImage = $"data:image/jpeg;base64,{base64Image}";
                ViewBag.Prediction = result?.Prediction;
                ViewBag.Confidence = result?.Confidence;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Could not connect to backend API: " + ex.Message;
            }

            return View();
        }
    }
}
