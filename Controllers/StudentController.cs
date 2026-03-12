using DSSStudentRisk.Data;
using DSSStudentRisk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace DSSStudentRisk.Controllers;
[ApiController]
[Route("api/student")]
public class StudentController : ControllerBase
{

    private readonly AppDbContext _context;

    public StudentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Student s)
    {
        _context.Students.Add(s);

        await _context.SaveChangesAsync();

        return Ok(s);
    }
   [HttpGet]
   public async Task<ActionResult<List<Student>>> GetAllStudent()
    {
        var students=await _context.Students.Include(s=>s.Performances).ToListAsync();
    
        return Ok(students);
    }
}