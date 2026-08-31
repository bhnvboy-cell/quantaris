using Quantaris.Core.Entities.Common;

namespace Quantaris.Core.Entities;

public class QfdModel : BaseEntity, ITenantEntity
{
    public int ProjectId { get; set; }
    public string ModelName { get; set; } = string.Empty;

    public Project Project { get; set; } = null!;
    public ICollection<QfdRequirement> Requirements { get; set; } = new List<QfdRequirement>();
    public ICollection<QfdMatrix> Matrices { get; set; } = new List<QfdMatrix>();
    public ICollection<QfdCorrelation> Correlations { get; set; } = new List<QfdCorrelation>();
}

public class QfdRequirement
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string ReqType { get; set; } = string.Empty;
    public int ReqIndex { get; set; }
    public string ReqName { get; set; } = string.Empty;
    public double Weight { get; set; } = 1.0;

    public QfdModel Model { get; set; } = null!;
}

public class QfdMatrix
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public int CustomerReqId { get; set; }
    public int TechnicalReqId { get; set; }
    public int Rating { get; set; }

    public QfdModel Model { get; set; } = null!;
    public QfdRequirement CustomerReq { get; set; } = null!;
    public QfdRequirement TechnicalReq { get; set; } = null!;
}

public class QfdCorrelation
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public int TechReq1Id { get; set; }
    public int TechReq2Id { get; set; }
    public string Correlation { get; set; } = string.Empty;

    public QfdModel Model { get; set; } = null!;
    public QfdRequirement TechReq1 { get; set; } = null!;
    public QfdRequirement TechReq2 { get; set; } = null!;
}
