using Application.Contracts;
using Application.Persistance;
using Domain.Entities;
using Domain.Entities.Purchase;

namespace Application.Services
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PurchaseInvoiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<PurchaseInvoice> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate);
            return invoices;
        }

        public IEnumerable<PurchaseInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate && p.PurchaseInvoiceStatusId == statusId);
            return invoices;
        }

        public IEnumerable<PurchaseInvoice> GetBetweenDatesAndSupplier(DateTime startDate, DateTime endDate, Guid supplierId)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate && p.SupplierId == supplierId);
            return invoices;
        }

        public async Task<IEnumerable<PurchaseInvoice>> GetByExercise(Guid exerciseId)
        {
            var exercice = await _unitOfWork.Exercices.Get(exerciseId);
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.ExerciceId == exerciseId);
            return invoices;
        }

        public async Task<GenericResponse> Create(PurchaseInvoice purchaseInvoice)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.Get(purchaseInvoice.PaymentMethodId);
            if (paymentMethod == null || paymentMethod.Disabled)
            {
                return new GenericResponse(false, new List<string> { $"Métode de pagament inválid" });
            }

            await _unitOfWork.PurchaseInvoices.Add(purchaseInvoice);

            // Generació de venciments
            var dueDates = GenerateDueDates(purchaseInvoice, paymentMethod);
            await _unitOfWork.PurchaseInvoiceDueDates.AddRange(dueDates);

            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> ChangeStatus(Guid id, Guid toStatusId)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.Get(id);
            if (invoice == null)
            {
                return new GenericResponse(false, new List<string> { $"La factura amb ID {id} no existeix" });
            }

            var status = await _unitOfWork.PurchaseInvoiceStatuses.Get(toStatusId);
            if (status == null || status.Disabled)
            {
                return new GenericResponse(false, new List<string> { $"L'estat de factura amb ID {id} no existeix o está deshabilitat" });
            }

            invoice.PurchaseInvoiceStatusId = toStatusId;
            await _unitOfWork.PurchaseInvoices.Update(invoice);

            return new GenericResponse(true, new List<string> { });
        }

        private List<PurchaseInvoiceDueDate> GenerateDueDates(PurchaseInvoice purchaseInvoice, PaymentMethod paymentMethod)
        {
            var purchaseInvoiceDueDates = new List<PurchaseInvoiceDueDate>();
            if (paymentMethod is not null)
            {
                for (var i = 0; i < paymentMethod.Frequency; i++)
                {
                    var dueDateAmount = purchaseInvoice.NetAmount / paymentMethod.NumberOfPayments;
                    var dueDate = purchaseInvoice.PurchaseInvoiceDate.AddDays(paymentMethod.DueDays);

                    if (dueDate.Day > paymentMethod.PaymentDay)
                    {
                        dueDate = new DateTime(dueDate.Month == 12 ? dueDate.Year + 1 : dueDate.Year,
                                               dueDate.Month == 12 ? 1 : dueDate.Month + 1,
                                               paymentMethod.PaymentDay);
                    }

                    var purchaseInvoiceDueDate = new PurchaseInvoiceDueDate()
                    {
                        PurchaseInvoiceId = purchaseInvoice.Id,
                        Amount = dueDateAmount,
                        DueDate = dueDate,
                    };
                    purchaseInvoiceDueDates.Add(purchaseInvoiceDueDate);
                }
            }
            return purchaseInvoiceDueDates;
        }

    }
}
