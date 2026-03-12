using DSSStudentRisk.Data;
using DSSStudentRisk.Models;
using DSSStudentRisk.Service;
using Microsoft.AspNetCore.Mvc;
namespace DSSStudentRisk.Controllers;
[ApiController]
[Route("api/ahp")]
public class AHPController : ControllerBase
{

    private readonly AppDbContext _context;
    private readonly AHPService _ahp;

    public AHPController(AppDbContext context)
    {
        _context = context;
        _ahp = new AHPService();
    }

    [HttpPost]
    public async Task<IActionResult> Create(AHPCriteria input)
    {

        var result = _ahp.Calculate(
            input.Test_Attendance,
            input.Test_Study,
            input.Attendance_Study);
               // kiểm tra NaN
            if (!IsValid(result.test) ||
        !IsValid(result.attendance) ||
        !IsValid(result.study) ||
        !IsValid(result.cr))
    {
        return BadRequest("AHP calculation returned invalid number (NaN)");
    }
        if (result.cr >= 0.1)
            return BadRequest("Consistency Ratio > 0.1");
            
        input.TestWeight = result.test;
        input.AttendanceWeight = result.attendance;
        input.StudyWeight = result.study;
        input.ConsistencyRatio = result.cr;
        input.IsActive = true;
        input.CreatedDate = DateTime.Now;

        _context.AHPCriteria.Add(input);

        await _context.SaveChangesAsync();

         return Ok(new
    {
        success = true,
        message = result.cr >= 0.1 
            ? "Consistency Ratio > 0.1 (matrix chưa nhất quán)"
            : "AHP calculated successfully",
        data = new
        {
            testWeight = result.test,
            attendanceWeight = result.attendance,
            studyWeight = result.study,
            consistencyRatio = result.cr,
            isConsistent = result.cr < 0.1
        }
    });
    }
    bool IsValid(double v)
{
    return !(double.IsNaN(v) || double.IsInfinity(v));
}
}