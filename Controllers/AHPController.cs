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
static Dictionary<string,double[]> alternativeWeights=new();

    static double[] criteriaWeights;
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
////buoc5 `

///Bước 6: Tính độ ưu tiên của các phương án theo từng tiêu chí. 
     // tính trọng số tiêu chí///tinh roi
    [HttpPost("criteria")]
    public IActionResult CalculateCriteria([FromBody] AhpMatrixRequest request)
    {
        var result=_ahp.CalculateMatrix(request.CriteriaName,request.Matrix);

        criteriaWeights=result.Weights;

        return Ok(result);
    }


    // tính trọng số phương án theo từng tiêu chí
    [HttpPost("alternative")]
    public IActionResult CalculateAlternative([FromBody] AhpMatrixRequest request)
    {

        var result=_ahp.CalculateMatrix(request.CriteriaName,request.Matrix);
         var criteria = _context.AHPCriteria.FirstOrDefault(x => x.IsActive);
            if(criteria == null)
            
        return BadRequest("No active criteria");
         var entity = new AHPAlternativeWeight
    {
        CriteriaName = request.CriteriaName,
        A1 = result.Weights[0],
        A2 = result.Weights[1],
        A3 = result.Weights[2],
        AHPCriteriaId = criteria.Id,
        CreatedDate = DateTime.Now
    };
     _context.AHPAlternativeWeights.Add(entity);
    _context.SaveChanges();
        return Ok(result);
    }
      //3️ tính ranking cuối
    [HttpGet("final")]
    public IActionResult FinalResult()
    {

        var criteria = _context.AHPCriteria.FirstOrDefault(x => x.IsActive);

    if(criteria == null)
        return BadRequest("No active criteria");
        var test = _context.AHPAlternativeWeights
        .FirstOrDefault(x => x.CriteriaName == "TestScore" && x.AHPCriteriaId == criteria.Id);

    var att = _context.AHPAlternativeWeights
        .FirstOrDefault(x => x.CriteriaName == "Attendance" && x.AHPCriteriaId == criteria.Id);

    var study = _context.AHPAlternativeWeights
        .FirstOrDefault(x => x.CriteriaName == "StudyHours" && x.AHPCriteriaId == criteria.Id);
 if(test == null || att == null || study == null)
        return BadRequest("Alternative weights not calculated");

    double a1 =
        test.A1 * criteria.TestWeight +
        att.A1 * criteria.AttendanceWeight +
        study.A1 * criteria.StudyWeight;

    double a2 =
        test.A2 * criteria.TestWeight +
        att.A2 * criteria.AttendanceWeight +
        study.A2 * criteria.StudyWeight;

    double a3 =
        test.A3 * criteria.TestWeight +
        att.A3 * criteria.AttendanceWeight +
        study.A3 * criteria.StudyWeight;

    string best = "A1";
    double max = a1;

    if(a2 > max)
    {
        max = a2;
        best = "A2";
    }

    if(a3 > max)
    {
        max = a3;
        best = "A3";
    }

    var final = new AhpFinalResult
    {
        A1 = a1,
        A2 = a2,
        A3 = a3,
        BestAlternative = best,
        AHPCriteriaId = criteria.Id,
        CreatedDate = DateTime.Now
    };

    _context.AHPFinalResults.Add(final);
    _context.SaveChanges();

    return Ok(final);

    }
}