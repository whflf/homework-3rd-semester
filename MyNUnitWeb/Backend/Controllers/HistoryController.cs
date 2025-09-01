using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Threading.Tasks;

[ApiController]
[Route("api/history")]
public class HistoryController : ControllerBase
{
    private const string HistoryFilePath = "dedicated/history.json";

    [HttpGet]
    public async Task<IActionResult> GetHistory()
    {
        if (!System.IO.File.Exists(HistoryFilePath))
        {
            return NotFound(new { message = "History file not found." });
        }

        try
        {
            var historyContent = await System.IO.File.ReadAllTextAsync(HistoryFilePath);
            return Ok(historyContent);
        }
        catch (IOException ex)
        {
            return StatusCode(500, new { message = "Failed to read the history file.", error = ex.Message });
        }
    }
}
