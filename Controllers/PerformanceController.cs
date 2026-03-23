using DSSStudentRisk.Data;
using DSSStudentRisk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var scores=await _context.StudentPerformances.ToListAsync();
        return Ok(scores);
    }

}