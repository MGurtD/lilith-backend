﻿using Domain.Entities.Warehouse;

namespace Domain.Implementations.ReferenceFormat
{
    public class ReferenceFormatCalculation_Rodo : IReferenceFormatCalculation
    {
        public decimal Calculate(ReferenceDimensions referenceDimensions)
        {
            double result;
            double factor = 0.000001;
            if ((referenceDimensions.Diameter > 0) && (referenceDimensions.Height > 0) && (referenceDimensions.Density > 0))
            {
                var diameter = (double) referenceDimensions.Diameter;
                var height = (double) referenceDimensions.Height;
                var density = (double) referenceDimensions.Density;
                var radius = diameter / 2;
                var volume = Math.Pow(radius, 2) * Math.PI * (height);

                result = volume * density * factor;
            } 
            else 
            {
                throw new Exception($"El diàmetre, l'alçada i la densitat han de ser superiors a 0 per calcular el format rodó");
            }            
            return (decimal) result;
        }
    }
}
