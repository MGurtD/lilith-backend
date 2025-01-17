using Application.Services;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Api.Services
{
    public class DueDateService : IDueDateService
    {
        public List<DueDate> GenerateDueDates(PaymentMethod paymentMethod, DateTime date, decimal netAmount)
        {
            var dueDates = new List<DueDate>();

            // Factura de pagament immediat
            if (paymentMethod.PaymentDay == 0 && paymentMethod.DueDays == 0)
            {
                var due = new DueDate()
                {
                    Amount = netAmount,
                    Date = date,
                };
                dueDates.Add(due);
                return dueDates;
            }

            // Factura amb venciment
            for (var i = 0; i < paymentMethod.NumberOfPayments; i++)
            {
                var dueDateAmount = netAmount / paymentMethod.NumberOfPayments;

                var dueDays = paymentMethod.Frequency > 0 ? paymentMethod.Frequency : paymentMethod.DueDays;
                if (dueDays % 30 == 0)
                {
                    date = date.AddMonths(dueDays / 30);
                }
                else
                {
                    date = date.AddDays(dueDays);
                }

                if (paymentMethod.PaymentDay > 0)
                {
                    if (paymentMethod.PaymentDay >= date.Day)
                    {
                        // Mentre no ens passem el día de pagament, ens quedem al mateix mes associant el dia de pagament
                        date = new DateTime(date.Year, date.Month, paymentMethod.PaymentDay);
                    } else
                    {
                        // Al passar-nos al dia de pagament, ens anem al mes següent
                        date = new DateTime(date.Month == 12 ? date.Year + 1 : date.Year,
                                            date.Month == 12 ? 1 : date.Month + 1,
                                            paymentMethod.PaymentDay);
                    }                    
                }

                var due = new DueDate()
                {
                    Amount = decimal.Round(dueDateAmount, 2),
                    Date = date,
                };
                dueDates.Add(due);
            }

            return dueDates;
        }

    }
}
