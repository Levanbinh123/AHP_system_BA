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
                if (perf == null)
            return NotFound("Student performance not found");
        var criteria = _context.AHPCriteria
            .FirstOrDefault(x => x.IsActive);
        if (criteria == null)
            return BadRequest("No active AHP criteria");

        var final = _context.AHPFinalResults
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefault();
             if (final == null)
            return BadRequest("AHP final result not calculated");
        double score = _risk.CalculateRisk(
            perf.TestScore,
            perf.Attendance,
            perf.StudyHours,
            criteria.TestWeight,
            criteria.AttendanceWeight,
            criteria.StudyWeight);

        // so sánh với A1 A2 A3
        double d1 = Math.Abs(score - final.A1);
        double d2 = Math.Abs(score - final.A2);
        double d3 = Math.Abs(score - final.A3);

          string level = _risk.GetLevel(score, final.A1, final.A2);

        var result = new RiskResult
        {
            StudentId = studentId,
            RiskScore = score,
            RiskLevel = level,
            CalculatedDate = DateTime.Now
        };

        _context.RiskResults.Add(result);

        await _context.SaveChangesAsync();

        return Ok(result);
    }
    [HttpPost("calculate-all")]
    public async Task<IActionResult> CalculateAll()
    {
        var criteria = _context.AHPCriteria.FirstOrDefault(x => x.IsActive);
        if (criteria == null)
            return BadRequest("Chưa có trọng số AHP");

        var final = _context.AHPFinalResults
            .OrderByDescending(x => x.CreatedDate)
            .FirstOrDefault();

        if (final == null)
            return BadRequest("AHP final result not calculated");

        var performances = _context.StudentPerformances.ToList();

        int added = 0, updated = 0;

        foreach (var perf in performances)
        {
            double newScore = _risk.CalculateRisk(
                perf.TestScore,
                perf.Attendance,
                perf.StudyHours,
                criteria.TestWeight,
                criteria.AttendanceWeight,
                criteria.StudyWeight
            );

            string newLevel = _risk.GetLevel(newScore, final.A1, final.A2);

            var existing = _context.RiskResults
                .FirstOrDefault(r => r.StudentId == perf.StudentId);

            if (existing == null)
            {
               
                var result = new RiskResult
                {
                    StudentId = perf.StudentId,
                    RiskScore = newScore,
                    RiskLevel = newLevel,
                    CalculatedDate = DateTime.Now
                };
                _context.RiskResults.Add(result);
                added++;
            }
            else
            {
            
                if (existing.RiskScore != newScore)
                {
                    existing.RiskScore = newScore;
                    existing.RiskLevel = newLevel;
                    existing.CalculatedDate = DateTime.Now;
                    updated++;
                }
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Calculated risk for all students (NO DUPLICATE)",
            added,
            updated,
            total = performances.Count
        });
    }
 
    [HttpGet("results")]    public IActionResult GetResults()
    {
        var results = _context.RiskResults
            .OrderByDescending(x => x.RiskScore)
            .ToList();
        return Ok(results);
    }
  [HttpGet("top-risk")]
    public IActionResult GetTop10Risk()
    {
        var results = _context.RiskResults
            .OrderByDescending(x => x.RiskScore)
            .Take(10)
            .ToList();

        return Ok(results);
    }
    [HttpGet("summary")]
    public IActionResult GetRiskSummary()
    {
        var latestResults=_context.RiskResults
        .GroupBy(x=>x.StudentId)
        .Select(g=>g.OrderByDescending(x=>x.CalculatedDate).First()).ToList();
        var summary=new
        {
            low=latestResults.Count(x=>x.RiskLevel=="Low Risk"),
             medium = latestResults.Count(x => x.RiskLevel == "Medium Risk"),
        high = latestResults.Count(x => x.RiskLevel == "High Risk"),
        total = latestResults.Count
        };
        return Ok(summary);
    }
}