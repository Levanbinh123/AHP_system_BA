using DSSStudentRisk.Data;
using DSSStudentRisk.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
namespace DSSStudentRisk.Controllers;
[ApiController]
[Route("api/performance")]
public class PerformanceController : ControllerBase
{

    private readonly AppDbContext _context;

    public PerformanceController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(StudentPerformance p)
    {

        p.CreatedDate = DateTime.Now;

        _context.StudentPerformances.Add(p);

        await _context.SaveChangesAsync();

        return Ok(p);
    }
    [HttpGet("{studentId}")]//lay lan nhap diem gannhatF
    public IActionResult GetPerfomentByStudentId(int studentId)
    {
            var latest = _context.StudentPerformances
        .Where(x => x.StudentId == studentId)
        .OrderByDescending(x => x.CreatedDate)
        .FirstOrDefault();
        if(latest==null)
        return NotFound();
        return Ok(latest);
    }
    [HttpGet]
public IActionResult GetPerfoment()
{
    var latest = _context.StudentPerformances
        .GroupBy(x => x.StudentId)
        .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
        .ToList();

    return Ok(latest);
}

}