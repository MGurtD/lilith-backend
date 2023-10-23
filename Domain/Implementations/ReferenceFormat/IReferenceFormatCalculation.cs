using Domain.Entities.Shared;

namespace Domain.Implementations.ReferenceFormat
{
    public interface IReferenceFormatCalculation
    {
        decimal Calculate(Reference reference);
    }
}
