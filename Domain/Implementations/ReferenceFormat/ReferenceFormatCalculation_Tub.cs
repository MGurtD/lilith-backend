using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatCalculation_Tub : IReferenceFormatCalculation
    {
        public decimal Calculate(ReferenceDimensions referenceDimensions)
        {
            double result, diamInt, diamExt;
            if ((referenceDimensions.Diameter > 0) && (referenceDimensions.Thickness > 0) && (referenceDimensions.Height > 0) && (referenceDimensions.Density > 0))
            {
                diamExt = (double) referenceDimensions.Diameter;
                diamInt = (double) (referenceDimensions.Diameter - referenceDimensions.Thickness);
                var height = (double) referenceDimensions.Height;
                var density = (double) referenceDimensions.Density;

                result = (Math.Pow(diamExt / 2, 2) - Math.Pow(diamInt / 2, 2)) * Math.PI * height * density;
            } 
            else 
            {
                throw new Exception($"All values must be greater than 0");
            }            
            return (decimal) result;
        }
    }
}
