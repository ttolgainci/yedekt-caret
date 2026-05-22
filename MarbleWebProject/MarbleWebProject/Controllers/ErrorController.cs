using MarbleWebProject.Infrastructure;
using MarbleWebProject.Models;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MarbleWebProject.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {
        private readonly TelemetryClient _telemetryClient;
        public ErrorController(TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        [Route("500")]
        public IActionResult AppError()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                _telemetryClient.TrackException(exceptionHandlerPathFeature.Error);
                _telemetryClient.TrackEvent("Error.ServerError", new Dictionary<string, string>
                {
                    ["originalPath"] = exceptionHandlerPathFeature.Path ?? "",
                    ["error"] = exceptionHandlerPathFeature.Error.Message
                });
            }

            var correlation =
                HttpContext.Items[CorrelationIdDefaults.HttpContextItemKey] as string
                ?? CorrelationIdAmbient.Current;
            return View(new ErrorViewModel
            {
                Heading = "Sunucu hatası",
                Message = "Beklenmeyen bir hata oluştu. Destek ile görüşürken aşağıdaki kimlikleri paylaşabilirsiniz.",
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                CorrelationId = correlation
            });
        }

        [Route("404")]
        public IActionResult PageNotFound(int code)
        {
            string originalPath = "unknown";
            if (HttpContext.Items.ContainsKey("originalPath"))
            {
                originalPath = HttpContext.Items["originalPath"] as string;
            }
            _telemetryClient.TrackEvent("Error.PageNotFound", new Dictionary<string, string>
            {
                ["originalPath"] = originalPath
            });
            ViewData["CorrelationId"] =
                HttpContext.Items[CorrelationIdDefaults.HttpContextItemKey] as string
                ?? CorrelationIdAmbient.Current;
            return View();
        }
    }
}
