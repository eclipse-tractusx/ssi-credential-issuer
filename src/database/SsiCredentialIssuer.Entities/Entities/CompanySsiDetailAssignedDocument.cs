namespace Org.Eclipse.TractusX.SsiCredentialIssuer.Entities.Entities;

public class CompanySsiDetailAssignedDocument
{
    public CompanySsiDetailAssignedDocument(Guid documentId, Guid companySsiDetailId)
    {
        DocumentId = documentId;
        CompanySsiDetailId = companySsiDetailId;
    }

    public Guid DocumentId { get; set; }
    public Guid CompanySsiDetailId { get; set; }

    public virtual Document? Document { get; set; }
    public virtual CompanySsiDetail? CompanySsiDetail { get; set; }
}
