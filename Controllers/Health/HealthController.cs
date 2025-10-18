using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace NoteApp.Controllers.Health
{
    [ApiController]
    [Route("health")] // GET /health
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() => Ok("ok");

        // GET /health/db
        [HttpGet("db")]
        public IActionResult Db([FromServices] IDbConnection db)
        {
            try
            {
                if (db.State != ConnectionState.Open)
                {
                    db.Open();
                }

                using var cmd = db.CreateCommand();
                cmd.CommandText = "SELECT 1";
                var _ = cmd.ExecuteScalar();

                return Ok(new { status = "ok", db = "up" });
            }
            catch (Exception ex)
            {
                return StatusCode(503, new { status = "degraded", db = "down", error = ex.Message });
            }
            finally
            {
                if (db.State == ConnectionState.Open)
                {
                    db.Close();
                }
            }
        }
    }
}
