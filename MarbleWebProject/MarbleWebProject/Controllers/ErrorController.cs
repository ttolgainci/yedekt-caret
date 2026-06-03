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
            _telemetryClient.TrackException(exceptionHandlerPathFeature.Error);
            _telemetryClient.TrackEvent("Error.ServerError", new Dictionary<string, string>
            {
                ["originalPath"] = exceptionHandlerPathFeature.Path,
                ["error"] = exceptionHandlerPathFeature.Error.Message
            });
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

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
            return View();
        }
    }
}
