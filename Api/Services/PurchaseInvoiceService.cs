using Application.Contracts.Auth;
using Application.Contracts.Purchase;
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

        public async Task<PurchaseInvoice?> GetById(Guid id)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.Get(id);
            return invoice;
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

        public IEnumerable<PurchaseInvoice> GetBetweenDatesAndExcludeStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var invoices = _unitOfWork.PurchaseInvoices.Find(p => p.PurchaseInvoiceDate >= startDate && p.PurchaseInvoiceDate <= endDate && p.PurchaseInvoiceStatusId != statusId);
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

        public async Task<IEnumerable<PurchaseInvoiceDueDate>?> GetPurchaseInvoiceDueDates(PurchaseInvoice purchaseInvoice)
        {
            // Recuperar m�tode de pagament
            var paymentMethod = await _unitOfWork.PaymentMethods.Get(purchaseInvoice.PaymentMethodId);
            if (paymentMethod == null || paymentMethod.Disabled)
            {
                return null;
            }

            var purchaseInvoiceDueDates = new List<PurchaseInvoiceDueDate>();

            // Factura de pagament immediat
            if (paymentMethod.PaymentDay == 0 && paymentMethod.DueDays == 0) {
                var purchaseInvoiceDueDate = new PurchaseInvoiceDueDate()
                {
                    PurchaseInvoiceId = purchaseInvoice.Id,
                    Amount = purchaseInvoice.NetAmount,
                    DueDate = purchaseInvoice.PurchaseInvoiceDate,
                };
                purchaseInvoiceDueDates.Add(purchaseInvoiceDueDate);
                return purchaseInvoiceDueDates;
            }
            
            // Factura amb venciment
            for (var i = 0; i < paymentMethod.NumberOfPayments; i++)
            {
                var dueDateAmount = purchaseInvoice.NetAmount / paymentMethod.NumberOfPayments;
                var dueDate = purchaseInvoice.PurchaseInvoiceDate.AddDays(paymentMethod.Frequency > 0 ? paymentMethod.Frequency : paymentMethod.DueDays + 1);

                if (paymentMethod.PaymentDay > 0 && paymentMethod.PaymentDay > dueDate.Day)
                {
                    dueDate = new DateTime(dueDate.Month == 12 ? dueDate.Year + 1 : dueDate.Year,
                                           dueDate.Month == 12 ? 1 : dueDate.Month + 1,
                                           paymentMethod.PaymentDay);
                }

                var purchaseInvoiceDueDate = new PurchaseInvoiceDueDate()
                {
                    PurchaseInvoiceId = purchaseInvoice.Id,
                    Amount = decimal.Round(dueDateAmount, 2),
                    DueDate = dueDate,
                };
                purchaseInvoiceDueDates.Add(purchaseInvoiceDueDate);
            }

            return purchaseInvoiceDueDates;
        }

        public async Task<GenericResponse> Create(PurchaseInvoice purchaseInvoice)
        {
            purchaseInvoice.Number = _unitOfWork.PurchaseInvoices.GetNextNumber();
            await _unitOfWork.PurchaseInvoices.Add(purchaseInvoice);   

            // Incrementar el comptador de factures de l'exercici
            if (purchaseInvoice.ExerciceId.HasValue)
            {
                var exercise = _unitOfWork.Exercices.Find(e => e.Id == purchaseInvoice.ExerciceId.Value).FirstOrDefault();
                if (exercise == null || exercise.Disabled)
                {
                    return new GenericResponse(false, new List<string> { $"Exercici inv�lid" });
                }

                exercise.PurchaseInvoiceCounter += 1;
                await _unitOfWork.Exercices.Update(exercise);
            }

            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> Update(PurchaseInvoice purchaseInvoice)
        {
            purchaseInvoice.PurchaseInvoiceDueDates = null;

            await _unitOfWork.PurchaseInvoices.Update(purchaseInvoice);
            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> RecreateDueDates(PurchaseInvoice requestInvoice)
        {
            var dbInvoice = await _unitOfWork.PurchaseInvoices.Get(requestInvoice.Id);
            if (dbInvoice != null && dbInvoice.PurchaseInvoiceDueDates != null && dbInvoice.PurchaseInvoiceDueDates.Count > 0)
            {
                await _unitOfWork.PurchaseInvoiceDueDates.RemoveRange(dbInvoice.PurchaseInvoiceDueDates);
            }

            if (requestInvoice.PurchaseInvoiceDueDates != null)
            {
                foreach (var purchaseInvoiceDueDate in requestInvoice.PurchaseInvoiceDueDates)
                    await _unitOfWork.PurchaseInvoiceDueDates.Add(purchaseInvoiceDueDate);
            }

            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> ChangeStatuses(ChangeStatusOfPurchaseInvoicesRequest changeStatusOfPurchaseInvoicesRequest)
        {
            var statusToId = changeStatusOfPurchaseInvoicesRequest.StatusToId;
            var status = await _unitOfWork.PurchaseInvoiceStatuses.Get(statusToId);
            if (status == null || status.Disabled)
            {
                return new GenericResponse(false, new List<string> { $"L'estat de factura amb ID {statusToId} no existeix o est� deshabilitat" });
            }

            var purchaseInvoices = _unitOfWork.PurchaseInvoices.Find(pi => changeStatusOfPurchaseInvoicesRequest.Ids.Contains(pi.Id));
            foreach (var invoice in purchaseInvoices)
            {
                invoice.PurchaseInvoiceStatusId = statusToId;
                _unitOfWork.PurchaseInvoices.UpdateWithoutSave(invoice);
            }
            await _unitOfWork.CompleteAsync();

            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> ChangeStatus(ChangeStatusOfPurchaseInvoiceRequest changeStatusOfPurchaseInvoiceRequest)
        {
            var invoice = await _unitOfWork.PurchaseInvoices.Get(changeStatusOfPurchaseInvoiceRequest.Id);
            if (invoice == null)
            {
                return new GenericResponse(false, new List<string> { $"La factura amb ID {changeStatusOfPurchaseInvoiceRequest.Id} no existeix" });
            }

            var status = await _unitOfWork.PurchaseInvoiceStatuses.Get(changeStatusOfPurchaseInvoiceRequest.StatusToId);
            if (status == null || status.Disabled)
            {
                return new GenericResponse(false, new List<string> { $"L'estat de factura amb ID {changeStatusOfPurchaseInvoiceRequest.StatusToId} no existeix o est� deshabilitat" });
            }

            invoice.PurchaseInvoiceStatusId = changeStatusOfPurchaseInvoiceRequest.StatusToId;
            await _unitOfWork.PurchaseInvoices.Update(invoice);

            return new GenericResponse(true, new List<string> { });
        }        

        public async Task<GenericResponse> Remove(Guid id)
        {
            var invoice = _unitOfWork.PurchaseInvoices.Find(p => p.Id == id).FirstOrDefault();
            if (invoice == null)
            {
                return new GenericResponse(false, new List<string> { $"La factura amb ID {id} no existeix" });
            }   
            else
            {
                await _unitOfWork.PurchaseInvoices.Remove(invoice);
                return new GenericResponse(true, new List<string> { });
            }

        }

        public async Task<GenericResponse> AddImport(PurchaseInvoiceImport import)
        {
            await _unitOfWork.PurchaseInvoices.AddImport(import);
            return new GenericResponse(true, new List<string> { });
        }

            public async Task<GenericResponse> UpdateImport(PurchaseInvoiceImport import)
        {
            await _unitOfWork.PurchaseInvoices.UpdateImport(import);
            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> RemoveImport(Guid id)
        {
            await _unitOfWork.PurchaseInvoices.RemoveImport(id);
            return new GenericResponse(true, new List<string> { });
        }

    }
}
