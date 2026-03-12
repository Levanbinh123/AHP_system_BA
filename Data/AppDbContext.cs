using DSSStudentRisk.Models;
using Microsoft.EntityFrameworkCore;
namespace DSSStudentRisk.Data;
public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }

    public DbSet<StudentPerformance> StudentPerformances { get; set; }

    public DbSet<AHPCriteria> AHPCriteria { get; set; }

    public DbSet<RiskResult> RiskResults { get; set; }
}