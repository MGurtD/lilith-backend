using Application.Contracts;
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

        // TODO > Test creació de factura!!
        public async Task<GenericResponse> Create(PurchaseInvoice purchaseInvoice)
        {
            // Incrementar el comptador de factures de l'exercici
            if (purchaseInvoice.ExerciceId.HasValue)
            {
                var exercise = _unitOfWork.Exercices.Find(e => e.Id == purchaseInvoice.ExerciceId.Value).FirstOrDefault();
                if (exercise == null || exercise.Disabled)
                {
                    return new GenericResponse(false, new List<string> { $"Exercici invàlid" });
                }

                exercise.PurchaseInvoiceCounter += 1;
                purchaseInvoice.Number = exercise.PurchaseInvoiceCounter;

                await _unitOfWork.PurchaseInvoices.Add(purchaseInvoice);
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

        public async Task<GenericResponse> ChangeStatus(ChangeStatusRequest changeStatusOfPurchaseInvoiceRequest)
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

        #region Imports

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

        #endregion

    }
}
