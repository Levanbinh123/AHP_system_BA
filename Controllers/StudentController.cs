
using ClosedXML.Excel;
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
  #region Import Excel → JSON → DB
        [HttpPost("students-excel-json")]
        public async Task<IActionResult> ImportStudentsAsJson(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            var studentsList = new List<Student>();
            var errors = new List<string>();

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null || worksheet.LastRowUsed() == null)
                return BadRequest("Excel file is empty or invalid");

            int rowCount = worksheet.LastRowUsed().RowNumber();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    string studentCode = worksheet.Cell(row, 1).GetString()?.Trim() ?? "";
                    if (string.IsNullOrEmpty(studentCode)) continue;

                    var student = new Student
                    {
                        StudentCode = studentCode,
                        Name = worksheet.Cell(row, 2).GetString()?.Trim(),
                        ClassName = worksheet.Cell(row, 3).GetString()?.Trim(),
                        Email = worksheet.Cell(row, 4).GetString()?.Trim(),
                        Performances = new List<StudentPerformance>
                        {
                            new StudentPerformance
                            {
                                TestScore = worksheet.Cell(row, 5).GetDoubleOrDefault(),
                                Attendance = worksheet.Cell(row, 6).GetDoubleOrDefault(),
                                StudyHours = worksheet.Cell(row, 7).GetDoubleOrDefault(),
                                CreatedDate = DateTime.Now
                            }
                        }
                    };

                    studentsList.Add(student);
                }
                catch (Exception ex)
                {
                    errors.Add($"Row {row}: {ex.Message}");
                }
            }

            // Lưu vào database
            foreach (var s in studentsList)
            {
                var existing = await _context.Students
                    .Include(x => x.Performances)
                    .FirstOrDefaultAsync(x => x.StudentCode == s.StudentCode);

                if (existing == null)
                {
                    _context.Students.Add(s);
                }
                else
                {
                    existing.Performances ??= new List<StudentPerformance>();
                    existing.Performances.AddRange(s.Performances);
                }
            }

            await _context.SaveChangesAsync();

            // Lấy lại dữ liệu thực tế từ DB để trả JSON
            var savedStudents = await _context.Students
                .Include(s => s.Performances)
                .Where(s => studentsList.Select(x => x.StudentCode).Contains(s.StudentCode))
                .ToListAsync();

            var json = System.Text.Json.JsonSerializer.Serialize(savedStudents, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            if (errors.Any())
                return Ok(new { message = "Imported with some errors", count = savedStudents.Count, errors, dataJson = json });

            return Ok(new { message = "Imported students successfully", count = savedStudents.Count, dataJson = json });
        }
        #endregion

        #region Export Excel từ DB
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportStudents()
        {
            var students = await _context.Students
                .Include(s => s.Performances)
                .ToListAsync();

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Students");

            // Header
            worksheet.Cell(1, 1).Value = "Student Code";
            worksheet.Cell(1, 2).Value = "Name";
            worksheet.Cell(1, 3).Value = "Class";
            worksheet.Cell(1, 4).Value = "Email";
            worksheet.Cell(1, 5).Value = "Test Score";
            worksheet.Cell(1, 6).Value = "Attendance";
            worksheet.Cell(1, 7).Value = "Study Hours";

            int row = 2;
            foreach (var s in students)
            {
                var perf = s.Performances.FirstOrDefault(); // hoặc nối tất cả performances
                worksheet.Cell(row, 1).Value = s.StudentCode;
                worksheet.Cell(row, 2).Value = s.Name;
                worksheet.Cell(row, 3).Value = s.ClassName;
                worksheet.Cell(row, 4).Value = s.Email;
                worksheet.Cell(row, 5).Value = perf?.TestScore ?? 0;
                worksheet.Cell(row, 6).Value = perf?.Attendance ?? 0;
                worksheet.Cell(row, 7).Value = perf?.StudyHours ?? 0;
                row++;
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            string fileName = $"Students-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        #endregion
    }
        

public static class XLCellExtensions
{
    public static double GetDoubleOrDefault(this IXLCell cell)
    {
        return double.TryParse(cell.GetString(), out var value) ? value : 0;
    }
}