using DSSStudentRisk.Data;
using DSSStudentRisk.Models;
using DSSStudentRisk.Service;
using Microsoft.AspNetCore.Mvc;
namespace DSSStudentRisk.Controllers;
[ApiController]
[Route("api/risk")]
public class RiskController : ControllerBase
{

    private readonly AppDbContext _context;
    private readonly RiskService _risk;

    public RiskController(AppDbContext context)
    {
        _context = context;
        _risk = new RiskService();
    }

    [HttpPost("{studentId}")]
    public async Task<IActionResult> Calculate(int studentId)
    {

        var perf = _context.StudentPerformances
            .FirstOrDefault(x => x.StudentId == studentId);

        var criteria = _context.AHPCriteria
            .FirstOrDefault(x => x.IsActive);

        double score = _risk.CalculateRisk(
            perf.TestScore,
            perf.Attendance,
            perf.StudyHours,
            criteria.TestWeight,
            criteria.AttendanceWeight,
            criteria.StudyWeight);

        var result = new RiskResult
        {
            StudentId = studentId,
            RiskScore = score,
            RiskLevel = _risk.GetLevel(score),
            CalculatedDate = DateTime.Now
        };

        _context.RiskResults.Add(result);

        await _context.SaveChangesAsync();

        return Ok(result);
    }
    [HttpPost("calculate-all")]
    public async Task<IActionResult> CalculateAll()
    {
       var criteria = _context.AHPCriteria
        .FirstOrDefault(x => x.IsActive);
        if (criteria == null)
        {
            return BadRequest("Chưa có trọng số AHP");
        }
        var performances=_context.StudentPerformances.ToList();
        foreach(var perf in performances)
        {
            double score=_risk.CalculateRisk(
                perf.TestScore,
                 perf.Attendance,
            perf.StudyHours,
            criteria.TestWeight,
            criteria.AttendanceWeight,
            criteria.StudyWeight);
            var result= new RiskResult
            {
                StudentId=perf.StudentId,
                RiskScore=score,
                RiskLevel=_risk.GetLevel(score),
                CalculatedDate=DateTime.Now
            };
            _context.RiskResults.Add(result);
        }
        await _context.SaveChangesAsync();
       
    return Ok(new
    {
        message = "Calculated risk for all students",
        total = performances.Count
    });
    }
    [HttpGet("results")]
    public IActionResult GetResults()
    {
        var results=_context.RiskResults.OrderByDescending(x=>x.RiskScore).ToList();
        return Ok(results);
    }
    [HttpGet("top-risk")]
    public IActionResult GetTop10Risk()
    {
        var results=_context.RiskResults.OrderByDescending(x=>x.RiskScore).
        Take(10).ToList();
        return Ok(results);
    }
}