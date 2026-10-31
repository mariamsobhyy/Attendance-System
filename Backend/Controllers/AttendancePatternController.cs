
using AttendanceBehaviour_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceBehaviour_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendancePatternController : ControllerBase
    {
        private readonly AttendancePatternService _patternService;

        public AttendancePatternController(
            AttendancePatternService patternService)
        {
            _patternService = patternService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatterns()
        {
            var result = await _patternService.GetAllPatternsAsync();

            return Ok(result);
        }
    }
}
