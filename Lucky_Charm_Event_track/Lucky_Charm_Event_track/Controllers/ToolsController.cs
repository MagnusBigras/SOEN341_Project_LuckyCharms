using Lucky_Charm_Event_track.Helpers;
using Lucky_Charm_Event_track.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace Lucky_Charm_Event_track.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToolsController : ControllerBase
    {
        private readonly WebAppDBContext _dbContext;

        public ToolsController(WebAppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/tools/export-csv/{eventId}
        // Calls the helper that writes the CSV to AttendeesList/event_{eventId}.csv
        [HttpGet("export-csv/{eventId}")]
        public IActionResult ExportAttendeesCSV(int eventId)
        {
            try
            {
                // Ensure directory & file are created by helper
                CSVCreationHelper.CreateAttendeeListCSV(_dbContext, eventId);

                string directory = "AttendeesList";
                string filename = $"event_{eventId}.csv";
                string filepath = Path.Combine(directory, filename);

                if (!System.IO.File.Exists(filepath))
                    return NotFound(new { success = false, error = "CSV file could not be created" });

                // Read file bytes and return as downloadable CSV
                byte[] fileBytes = System.IO.File.ReadAllBytes(filepath);
                return File(fileBytes, "text/csv", filename);
            }
            catch (System.Exception ex)
            {
                // log if you have a logger. Return generic message for security in prod.
                return StatusCode(500, new { success = false, error = "Error generating CSV", detail = ex.Message });
            }
        }

        // POST: api/tools/validate-qr
        // Accepts form-data with key "qrCodeImage" (IFormFile)
        [HttpPost("validate-qr")]
        public IActionResult ValidateQRCode([FromForm] IFormFile qrCodeImage)
        {
            if (qrCodeImage == null)
                return BadRequest(new { success = false, error = "qrCodeImage file is required (form field name: qrCodeImage)." });

            try
            {
                // Your helper returns bool: true = validated & marked checked-in, false = invalid or already used
                bool result = QRCodeGeneratorHelper.VerifyQRCode(_dbContext, qrCodeImage);

                if (!result)
                {
                    // Could be invalid decode, not found, or already checked-in
                    return BadRequest(new { success = false, error = "Invalid or already used ticket." });
                }

                return Ok(new { success = true, message = "Ticket validated successfully." });
            }
            catch (System.Exception ex)
            {
                // If exception occurs (e.g., DB null ref), return 500 so client knows
                return StatusCode(500, new { success = false, error = "Error validating QR", detail = ex.Message });
            }
        }

        // Optional: generate QR image PNG from payload
        // GET: api/tools/generate-qr?payload=someText
        [HttpGet("generate-qr")]
        public IActionResult GenerateQRCode([FromQuery] string payload)
        {
            if (string.IsNullOrWhiteSpace(payload))
                return BadRequest(new { success = false, error = "payload query parameter is required." });

            try
            {
                byte[] png = QRCodeGeneratorHelper.GenerateQRCode(payload);
                return File(png, "image/png");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, error = "Error generating QR", detail = ex.Message });
            }
        }
    }
}
