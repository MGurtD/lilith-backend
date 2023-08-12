using Application.Contracts;
using Domain.Entities;
using Domain.Entities.Shared;

namespace Application.Services
{
    public interface IDueDateService
    {
        List<DueDate> GenerateDueDates(PaymentMethod paymentMethod, DateTime date, decimal totalAmount);
    }
}
