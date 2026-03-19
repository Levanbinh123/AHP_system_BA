using System.ComponentModel.DataAnnotations;

public class AHPMatrix
{
    [Key]
    public int Id { get; set; }

    public string CriteriaName { get; set; }

    public string MatrixJson { get; set; }

    public int AHPCriteriaId { get; set; }

    public DateTime CreatedDate { get; set; }
}