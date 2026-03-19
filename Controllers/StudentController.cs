
using DSSStudentRisk.Data;
using DSSStudentRisk.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml;// nếu dùng EPPlus < 8
using OfficeOpenXml; // EPPlus 8+
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
    //excel
 // Import Excel sinh viên + điểm học tập
    [HttpPost("students-excel")]
    public async Task<IActionResult> ImportStudents(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // EPPlus 8+ License
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var stream = file.OpenReadStream();
        using var package = new ExcelPackage(stream);

        var worksheet = package.Workbook.Worksheets.FirstOrDefault();
        if (worksheet == null || worksheet.Dimension == null)
            return BadRequest("Excel file is empty or invalid");

        int rowCount = worksheet.Dimension.Rows;
        if (rowCount < 2)
            return BadRequest("Excel file has no data");

        var errors = new List<string>();

        for (int row = 2; row <= rowCount; row++) // row 1 là header
        {
            try
            {
                string studentCode = worksheet.Cells[row, 1]?.Text.Trim();
                if (string.IsNullOrEmpty(studentCode))
                    continue; // bỏ qua dòng rỗng

                string name = worksheet.Cells[row, 2]?.Text.Trim();
                string className = worksheet.Cells[row, 3]?.Text.Trim();
                string email = worksheet.Cells[row, 4]?.Text.Trim();

                // Lấy điểm học tập nếu Excel có cột 5,6,7
                double testScore = double.TryParse(worksheet.Cells[row, 5]?.Text, out var t) ? t : 0;
                double attendance = double.TryParse(worksheet.Cells[row, 6]?.Text, out var a) ? a : 0;
                double studyHours = double.TryParse(worksheet.Cells[row, 7]?.Text, out var s) ? s : 0;

                // Kiểm tra sinh viên đã tồn tại
                var existing = await _context.Students
                    .Include(s => s.Performances)
                    .FirstOrDefaultAsync(s => s.StudentCode == studentCode);

                if (existing == null)
                {
                    var student = new Student
                    {
                        StudentCode = studentCode,
                        Name = name,
                        ClassName = className,
                        Email = email,
                        Performances = new List<StudentPerformance>
                        {
                            new StudentPerformance
                            {
                                TestScore = testScore,
                                Attendance = attendance,
                                StudyHours = studyHours,
                                CreatedDate = DateTime.Now
                            }
                        }
                    };
                    _context.Students.Add(student);
                }
                else
                {
                    existing.Performances ??= new List<StudentPerformance>();
                    existing.Performances.Add(new StudentPerformance
                    {
                        TestScore = testScore,
                        Attendance = attendance,
                        StudyHours = studyHours,
                        CreatedDate = DateTime.Now
                    });
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Row {row}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync();

        if (errors.Any())
            return Ok(new { message = "Imported with some errors", errors });

        return Ok(new { message = "Imported students successfully" });
    }
    
    
}