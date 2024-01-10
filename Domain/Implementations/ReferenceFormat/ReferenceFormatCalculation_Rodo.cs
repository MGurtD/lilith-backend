using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatCalculation_Rodo : IReferenceFormatCalculation
    {
        public decimal Calculate(ReferenceDimensions referenceDimensions)
        {
            double result;
            if ((referenceDimensions.Diameter > 0) && (referenceDimensions.Height > 0) && (referenceDimensions.Density > 0))
            {
                var diameter = (double) referenceDimensions.Diameter;
                var height = (double) referenceDimensions.Height;
                var density = (double) referenceDimensions.Density;

                result = Math.Pow(diameter / 2, 2) * Math.PI * (height / 1000) * density;
            } 
            else 
            {
                throw new Exception($"All values must be greater than 0");
            }            
            return (decimal) result;
        }
    }
}
