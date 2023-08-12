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
                var dueDate = date.AddDays(paymentMethod.Frequency > 0 ? paymentMethod.Frequency : paymentMethod.DueDays);

                if (paymentMethod.PaymentDay > 0 && dueDate.Month > date.Month)
                {
                    // Al passar-nos al dia de pagament, ens anem al mes següent
                    dueDate = new DateTime(dueDate.Month == 12 ? dueDate.Year + 1 : dueDate.Year,
                                           dueDate.Month == 12 ? 1 : dueDate.Month + 1,
                                           paymentMethod.PaymentDay);
                }

                var due = new DueDate()
                {
                    Amount = decimal.Round(dueDateAmount, 2),
                    Date = dueDate,
                };
                dueDates.Add(due);
            }

            return dueDates;
        }
    }
}
