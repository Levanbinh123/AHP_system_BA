using DSSStudentRisk.Data;
using DSSStudentRisk.Models;
using DSSStudentRisk.Service;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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
            input.Attendance_Study
        );

        if (!IsValid(result.test) ||
            !IsValid(result.attendance) ||
            !IsValid(result.study) ||
            !IsValid(result.cr))
        {
            return BadRequest("Invalid AHP result");
        }

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
            success = result.cr < 0.1,
            data = input
        });
    }

    // ================== CALCULATE CRITERIA MATRIX ==================
    [HttpPost("criteria")]
    public async Task<IActionResult> CalculateCriteria(AhpMatrixRequest request)
    {
        var result = _ahp.CalculateMatrix(request.CriteriaName, request.Matrix);

        var criteria = _context.AHPCriteria.FirstOrDefault(x => x.IsActive);
        if (criteria == null)
            return BadRequest("No active criteria");

        // Check existing matrix → update nếu đã có
        var existingMatrix = _context.AHPMatrices
            .FirstOrDefault(x => x.AHPCriteriaId == criteria.Id && x.CriteriaName == request.CriteriaName);

        if (existingMatrix != null)
        {
            existingMatrix.MatrixJson = JsonSerializer.Serialize(request.Matrix);
            existingMatrix.CreatedDate = DateTime.Now;
        }
        else
        {
            var matrixEntity = new AHPMatrix
            {
                CriteriaName = request.CriteriaName,
                MatrixJson = JsonSerializer.Serialize(request.Matrix),
                AHPCriteriaId = criteria.Id,
                CreatedDate = DateTime.Now
            };
            _context.AHPMatrices.Add(matrixEntity);
        }

        await _context.SaveChangesAsync();
        return Ok(result);
    }

    // ================== CALCULATE ALTERNATIVE WEIGHTS ==================
    [HttpPost("alternative")]
    public async Task<IActionResult> CalculateAlternative(AhpMatrixRequest request)
    {
        var result = _ahp.CalculateMatrix(request.CriteriaName, request.Matrix);

        var criteria = _context.AHPCriteria.FirstOrDefault(x => x.IsActive);
        if (criteria == null)
            return BadRequest("No active criteria");

        // Check existing alternative weight → update nếu đã có
        var existingAlt = _context.AHPAlternativeWeights
            .FirstOrDefault(x => x.AHPCriteriaId == criteria.Id && x.CriteriaName == request.CriteriaName);

        if (existingAlt != null)
        {
            existingAlt.A1 = result.Weights[0];
            existingAlt.A2 = result.Weights[1];
            existingAlt.A3 = result.Weights[2];
            existingAlt.CreatedDate = DateTime.Now;
        }
        else
        {
            var altEntity = new AHPAlternativeWeight
            {
                CriteriaName = request.CriteriaName,
                A1 = result.Weights[0],
                A2 = result.Weights[1],
                A3 = result.Weights[2],
                AHPCriteriaId = criteria.Id,
                CreatedDate = DateTime.Now
            };
            _context.AHPAlternativeWeights.Add(altEntity);
        }

        // Also save the matrix
        var existingMatrix = _context.AHPMatrices
            .FirstOrDefault(x => x.AHPCriteriaId == criteria.Id && x.CriteriaName == request.CriteriaName);

        if (existingMatrix != null)
        {
            existingMatrix.MatrixJson = JsonSerializer.Serialize(request.Matrix);
            existingMatrix.CreatedDate = DateTime.Now;
        }
        else
        {
            var matrixEntity = new AHPMatrix
            {
                CriteriaName = request.CriteriaName,
                MatrixJson = JsonSerializer.Serialize(request.Matrix),
                AHPCriteriaId = criteria.Id,
                CreatedDate = DateTime.Now
            };
            _context.AHPMatrices.Add(matrixEntity);
        }

        await _context.SaveChangesAsync();
        return Ok(result);
    }

    // ================== CALCULATE FINAL RESULT ==================
    [HttpGet("final")]
    public async Task<IActionResult> FinalResult()
    {
        var criteria = _context.AHPCriteria.FirstOrDefault(x => x.IsActive);
        if (criteria == null)
            return BadRequest("No criteria");

        var test = _context.AHPAlternativeWeights
            .FirstOrDefault(x => x.CriteriaName == "TestScore" && x.AHPCriteriaId == criteria.Id);
        var att = _context.AHPAlternativeWeights
            .FirstOrDefault(x => x.CriteriaName == "Attendance" && x.AHPCriteriaId == criteria.Id);
        var study = _context.AHPAlternativeWeights
            .FirstOrDefault(x => x.CriteriaName == "StudyHours" && x.AHPCriteriaId == criteria.Id);

        if (test == null || att == null || study == null)
            return BadRequest("Missing alternative weights");

        double a1 = test.A1 * criteria.TestWeight +
                    att.A1 * criteria.AttendanceWeight +
                    study.A1 * criteria.StudyWeight;

        double a2 = test.A2 * criteria.TestWeight +
                    att.A2 * criteria.AttendanceWeight +
                    study.A2 * criteria.StudyWeight;

        double a3 = test.A3 * criteria.TestWeight +
                    att.A3 * criteria.AttendanceWeight +
                    study.A3 * criteria.StudyWeight;

        string best = "A1";
        double max = a1;
        if (a2 > max) { max = a2; best = "A2"; }
        if (a3 > max) { max = a3; best = "A3"; }

        var existing = _context.AHPFinalResults
            .FirstOrDefault(x => x.AHPCriteriaId == criteria.Id);

        if (existing != null)
        {
            existing.A1 = a1;
            existing.A2 = a2;
            existing.A3 = a3;
            existing.BestAlternative = best;
            existing.CreatedDate = DateTime.Now;
        }
        else
        {
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
        }

        await _context.SaveChangesAsync();

        return Ok(new { a1, a2, a3, best });
    }

    // ================== GET FULL REPORT ==================
    [HttpGet("report")]
    public IActionResult GetFullReport()
    {
        var criteria = _context.AHPCriteria.FirstOrDefault(x => x.IsActive);
        if (criteria == null)
            return BadRequest("No criteria");

        var matrices = _context.AHPMatrices
            .Where(x => x.AHPCriteriaId == criteria.Id)
            .GroupBy(x => x.CriteriaName) // Lấy ma trận mới nhất mỗi criteria
            .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
            .ToList();

        var alternatives = _context.AHPAlternativeWeights
            .Where(x => x.AHPCriteriaId == criteria.Id)
            .GroupBy(x => x.CriteriaName)
            .Select(g => g.OrderByDescending(x => x.CreatedDate).First())
            .ToList();

        var final = _context.AHPFinalResults
            .FirstOrDefault(x => x.AHPCriteriaId == criteria.Id);

        return Ok(new
        {
            criteriaWeights = new
            {
                criteria.TestWeight,
                criteria.AttendanceWeight,
                criteria.StudyWeight
            },
            cr = criteria.ConsistencyRatio,
            matrices = matrices.Select(x => new
            {
                x.CriteriaName,
                matrix = JsonSerializer.Deserialize<List<List<double>>>(x.MatrixJson)
            }),
            alternativeWeights = alternatives.Select(x => new
            {
                x.CriteriaName,
                weights = new[] { x.A1, x.A2, x.A3 }
            }),
            finalScores = new[] { final?.A1 ?? 0, final?.A2 ?? 0, final?.A3 ?? 0 },
            best = final?.BestAlternative ?? ""
        });
    }

    private bool IsValid(double v)
    {
        return !(double.IsNaN(v) || double.IsInfinity(v));
    }
}