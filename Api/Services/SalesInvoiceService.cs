using Application.Contracts;
using Application.Contracts.Purchase;
using Application.Persistance;
using Application.Services;
using Domain.Entities.Sales;

namespace Api.Services
{
    public class SalesInvoiceService : ISalesInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDueDateService _dueDateService;

        public SalesInvoiceService(IUnitOfWork unitOfWork, IDueDateService dueDateService)
        {
            _unitOfWork = unitOfWork;
            _dueDateService = dueDateService;
        }

        #region Invoice        

        public async Task<SalesInvoice?> GetById(Guid id)
        {
            var invoices = await _unitOfWork.SalesInvoices.Get(id);
            return invoices;
        }
        public IEnumerable<SalesInvoice> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var invoice = _unitOfWork.SalesInvoices.Find(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate);
            return invoice;
        }
        public IEnumerable<SalesInvoice> GetBetweenDatesAndStatus(DateTime startDate, DateTime endDate, Guid statusId)
        {
            var invoices = _unitOfWork.SalesInvoices.Find(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate && p.StatusId == statusId);
            return invoices;
        }

        public IEnumerable<SalesInvoice> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var invoices = _unitOfWork.SalesInvoices.Find(p => p.InvoiceDate >= startDate && p.InvoiceDate <= endDate && p.CustomerId == customerId);
            return invoices;
        }

        public IEnumerable<SalesInvoice> GetByCustomer(Guid customerId)
        {
            var invoices = _unitOfWork.SalesInvoices.Find(p => p.CustomerId == customerId);
            return invoices;
        }

        public IEnumerable<SalesInvoice> GetByStatus(Guid statusId)
        {
            var invoices = _unitOfWork.SalesInvoices.Find(p => p.StatusId == statusId);
            return invoices;
        }

        public IEnumerable<SalesInvoice> GetByExercise(Guid exerciseId)
        {
            var invoices = _unitOfWork.SalesInvoices.Find(p => p.ExerciseId == exerciseId);
            return invoices;
        }

        public async Task<GenericResponse> Create(SalesInvoice invoice)
        {
            if (invoice == null)
                return new GenericResponse(false, new List<string> { $"Comanda invalida" });
            if (!invoice.ExerciseId.HasValue)
                return new GenericResponse(false, new List<string> { $"Exercici invàlid" });

            var exercise = _unitOfWork.Exercices.Find(e => e.Id == invoice.ExerciseId.Value).FirstOrDefault();
            if (exercise == null)
                return new GenericResponse(false, new List<string> { $"Exercici invàlid" });

            // Recuperar número de factura de l'exercici
            invoice.InvoiceNumber = exercise.SalesInvoiceCounter;
            await _unitOfWork.SalesInvoices.Add(invoice);

            // Incrementar el comptador de comandes de l'exercici
            exercise.SalesInvoiceCounter += 1;
            await _unitOfWork.Exercices.Update(exercise);

            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> Update(SalesInvoice invoices)
        {
            // Netejar dependencies per evitar col·lisions de EF
            invoices.SalesInvoiceDetails.Clear();
            invoices.SalesInvoiceImports.Clear();
            invoices.SalesInvoiceDueDates.Clear();

            await _unitOfWork.SalesInvoices.Update(invoices);
            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var invoice = _unitOfWork.SalesInvoices.Find(p => p.Id == id).FirstOrDefault();
            if (invoice == null)
                return new GenericResponse(false, new List<string> { $"La factura amb ID {id} no existeix" });
            else
                await _unitOfWork.SalesInvoices.Remove(invoice);
                return new GenericResponse(true, new List<string> { });

        }

        public Task<GenericResponse> RecreateDueDates(SalesInvoice SalesInvoice)
        {
            throw new NotImplementedException();
        }
        public Task<GenericResponse> ChangeStatus(ChangeStatusRequest changeStatusRequest)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Details

        public async Task<GenericResponse> AddDetail(SalesInvoiceDetail invoiceDetail)
        {
            await _unitOfWork.SalesInvoices.InvoiceDetails.Add(invoiceDetail);
            return new GenericResponse(true, new List<string> { });
        }
        public async Task<GenericResponse> UpdateDetail(SalesInvoiceDetail invoiceDetail)
        {
            var detail = _unitOfWork.SalesInvoices.InvoiceDetails.Find(p => p.Id == invoiceDetail.Id).FirstOrDefault();
            if (detail == null)
                return new GenericResponse(false, new List<string> { $"La detall de factura amb ID {invoiceDetail.Id} no existeix" });

            await _unitOfWork.SalesInvoices.InvoiceDetails.Update(invoiceDetail);
            return new GenericResponse(true, new List<string> { });
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = _unitOfWork.SalesInvoices.InvoiceDetails.Find(p => p.Id == id).FirstOrDefault();

            if (detail == null)
                return new GenericResponse(false, new List<string> { $"La detall de factura amb ID {id} no existeix" });

            await _unitOfWork.SalesInvoices.InvoiceDetails.Remove(detail);
            return new GenericResponse(true, new List<string> { });
        }
        #endregion

        #region Imports
        public async Task<GenericResponse> AddImport(SalesInvoiceImport import)
        {
            await _unitOfWork.SalesInvoices.InvoiceImports.Add(import);
            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> UpdateImport(SalesInvoiceImport import)
        {
            await _unitOfWork.SalesInvoices.InvoiceImports.Update(import);
            return new GenericResponse(true, new List<string> { });
        }

        public async Task<GenericResponse> RemoveImport(Guid id)
        {
            var import = _unitOfWork.SalesInvoices.InvoiceImports.Find(p => p.Id == id).FirstOrDefault();

            if (import == null)
                return new GenericResponse(false, new List<string> { $"La detall de factura amb ID {id} no existeix" });

            await _unitOfWork.SalesInvoices.InvoiceImports.Remove(import);
            return new GenericResponse(true, new List<string> { });
        }
        #endregion

    }
}
