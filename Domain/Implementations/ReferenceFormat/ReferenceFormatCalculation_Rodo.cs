using Domain.Entities.Shared;
using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatCalculation_Rodo : IReferenceFormatCalculation
    {
        public decimal Calculate(ReferenceDimensions referenceDimensions)
        {
            decimal result;
            if ((referenceDimensions.Diameter > 0) && (referenceDimensions.Height > 0) && (referenceDimensions.Density > 0))
            {
                result = (referenceDimensions.Diameter/2)*(referenceDimensions.Diameter/2)*(decimal)(Math.PI)*(referenceDimensions.Height/1000)*(referenceDimensions.Density);
            }else{
                result = 0;
                throw new Exception($"All values must be greater than 0");
            }
            
            return result;
        }
    }
}
