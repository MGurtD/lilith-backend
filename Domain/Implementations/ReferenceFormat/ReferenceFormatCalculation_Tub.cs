using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatCalculation_Tub : IReferenceFormatCalculation
    {
        public decimal Calculate(ReferenceDimensions referenceDimensions)
        {
            decimal result, diamInt, diamExt;

            if ((referenceDimensions.Diameter > 0) && (referenceDimensions.Thickness > 0) && (referenceDimensions.Height > 0) && (referenceDimensions.Density > 0))
            {
                diamExt = referenceDimensions.Diameter;
                diamInt = referenceDimensions.Diameter - referenceDimensions.Thickness;
                result = ((decimal) Math.Pow((double)(diamExt/2),2.0) - (decimal)Math.Pow((double)(diamInt/2),2.0))*(decimal) Math.PI * referenceDimensions.Height * referenceDimensions.Density;
            } else {
                throw new Exception($"All values must be greater than 0");
            }
            
            return result;
        }
    }
}
