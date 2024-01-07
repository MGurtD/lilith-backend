using Domain.Entities.Shared;
using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatCalculation_Placa : IReferenceFormatCalculation
    {
        public decimal Calculate(ReferenceDimensions referenceDimensions)
        {
            decimal result;
            if ((referenceDimensions.Width > 0) && (referenceDimensions.Length > 0) && (referenceDimensions.Height > 0) && (referenceDimensions.Density > 0))
            {
                result = referenceDimensions.Width / 1000 * (referenceDimensions.Length / 1000) * referenceDimensions.Height * referenceDimensions.Density;
            }else{
                result = 0;
                throw new Exception($"All values must be greater than 0");
            }
            
            return result;
        }
    }
}
