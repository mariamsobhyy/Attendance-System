
using AttendanceBehaviour_Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AttendanceBehaviour_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceRiskController : ControllerBase
    {
        private readonly AttendanceRiskService _riskService;

        public AttendanceRiskController(AttendanceRiskService riskService)
        {
            _riskService = riskService;
        }

        [HttpGet("student/{studentId:long}")]
        public async Task<IActionResult> GetStudentRisk(long studentId)
        {
            var result = await _riskService.GetStudentRiskAsync(studentId);

            if (result == null)
                return NotFound(new { message = "Student not found." });

            return Ok(result);
        }

        [HttpGet("at-risk-students")]
        public async Task<IActionResult> GetAtRiskStudents()
        {
            var result = await _riskService.GetAtRiskStudentsAsync();
            return Ok(result);
        }


        [HttpGet("student/{studentId:long}/details")]
public async Task<IActionResult> GetStudentRiskDetails(long studentId)
{
    var result = await _riskService.GetStudentRiskDetailsAsync(studentId);

    if (result == null)
        return NotFound(new { message = "Student not found." });

    return Ok(result);
}
    }
}