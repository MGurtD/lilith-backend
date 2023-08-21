using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;

namespace Api.Services
{
    internal struct InvoiceEntities
    {
        internal Customer Customer;
        internal Exercise Exercise;
        internal Site Site;
    }

    public class SalesInvoiceService : ISalesInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDueDateService _dueDateService;

        public SalesInvoiceService(IUnitOfWork unitOfWork, IDueDateService dueDateService)
        {
            _unitOfWork = unitOfWork;
            _dueDateService = dueDateService;
        }

        public async Task<GenericResponse> Create(CreateInvoiceRequest createInvoiceRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createInvoiceRequest);
            if (!response.Result) return response;

            var invoiceEntities = (InvoiceEntities)response.Content!;
            var invoice = new SalesInvoice
            {
                Id = createInvoiceRequest.Id,
                InvoiceNumber = invoiceEntities.Exercise.SalesInvoiceCounter + 1,
                InvoiceDate = createInvoiceRequest.InvoiceDate
            };

            var lifecycle = _unitOfWork.Lifecycles.Find(l => l.Name == "SalesInvoice").FirstOrDefault();
            if (lifecycle == null)
                return new GenericResponse(false, new List<string>() { "El cicle de vida 'SalesInvoice' no existeix" });
            if (!lifecycle.InitialStatusId.HasValue)
                return new GenericResponse(false, new List<string>() { "El cicle de vida 'SalesInvoice' no té estat inicial" });

            invoice.ExerciseId = invoiceEntities.Exercise.Id;
            invoice.StatusId = lifecycle.InitialStatusId;
            invoice.SetCustomer(invoiceEntities.Customer);
            invoice.SetSite(invoiceEntities.Site);

            await _unitOfWork.SalesInvoices.Add(invoice);

            // Incrementar el comptador de comandes de l'exercici
            invoiceEntities.Exercise.SalesInvoiceCounter += 1;
            await _unitOfWork.Exercices.Update(invoiceEntities.Exercise);

            return new GenericResponse(true, invoice);
        }

        private async Task<GenericResponse> ValidateCreateInvoiceRequest(CreateInvoiceRequest createInvoiceRequest)
        {
            var exercise = await _unitOfWork.Exercices.Get(createInvoiceRequest.ExerciseId);
            if (exercise == null) return new GenericResponse(false, new List<string>() { "L'exercici no existex" });

            var customer = await _unitOfWork.Customers.Get(createInvoiceRequest.CustomerId);
            if (customer == null) return new GenericResponse(false, new List<string>() { "El client no existeix" });
            if (!customer.IsValidForSales())
                return new GenericResponse(false, new List<string>() { "El client no és válid per a crear una factura. Revisa el nom fiscal, el número de compte i el NIF" });
            if (customer.MainAddress() == null)
                return new GenericResponse(false, new List<string>() { "El client no té direccions donades d'alta. Si us plau, creí una direcció." });

            var site = _unitOfWork.Sites.Find(s => s.Name == "Local Torelló").FirstOrDefault();
            if (site == null) 
                return new GenericResponse(false, new List<string>() { "La seu 'Temges' no existeix" });
            if (!site.IsValidForSales()) 
                return new GenericResponse(false, new List<string>() { "Seu 'Temges' no és válida per crear una factura. Revisi les dades de facturació." });

            InvoiceEntities invoiceEntities;
            invoiceEntities.Exercise = exercise;
            invoiceEntities.Customer = customer;
            invoiceEntities.Site = site;
            return new GenericResponse(true, invoiceEntities);
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

        public async Task<GenericResponse> Update(SalesInvoice invoice)
        {
            // Netejar dependencies per evitar col·lisions de EF
            invoice.SalesInvoiceDetails.Clear();
            invoice.SalesInvoiceImports.Clear();
            invoice.SalesInvoiceDueDates.Clear();

            await _unitOfWork.SalesInvoices.Update(invoice);
            await GenerateDueDates(invoice);
            return new GenericResponse(true, invoice);
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

        private async Task<GenericResponse> GenerateDueDates(SalesInvoice invoice)
        {
            var paymentMethod = await _unitOfWork.PaymentMethods.Get(invoice.PaymentMethodId);
            if (paymentMethod == null)
                return new GenericResponse(false, new List<string>() { "El métode de pagament no existeix" });
            var dbInvoice = await _unitOfWork.SalesInvoices.Get(invoice.Id);
            if (dbInvoice == null)
                return new GenericResponse(false, new List<string>() { "La factura no existeix" });

            // Esborrar venciments actuals
            var currentDueDates = _unitOfWork.SalesInvoices.InvoiceDueDates.Find(d => d.SalesInvoiceId == invoice.Id);
            if (currentDueDates.Any())
                await _unitOfWork.SalesInvoices.InvoiceDueDates.RemoveRange(currentDueDates);

            // Generar nous venciments
            var newDueDates = new List<SalesInvoiceDueDate>();
            if (invoice.NetAmount > 0)
            {   
                var dueDates = _dueDateService.GenerateDueDates(paymentMethod, invoice.InvoiceDate, invoice.NetAmount);
                foreach (var dueDate in dueDates)
                {
                    newDueDates.Add(new SalesInvoiceDueDate()
                    {
                        SalesInvoiceId = invoice.Id,
                        Amount = dueDate.Amount,
                        DueDate = dueDate.Date
                    });
                }
                if (dueDates.Any()) await _unitOfWork.SalesInvoices.InvoiceDueDates.AddRange(newDueDates);
            }

            return new GenericResponse(true, newDueDates);
        }
        #endregion

        #region Details
        public async Task<GenericResponse> AddDetailsFromOrderDetails(SalesInvoice invoice, IEnumerable<SalesOrderDetail> salesOrderDetails)
        {
            var salesInvoiceDetails = new List<SalesInvoiceDetail>();
            foreach (var salesOrderDetail in salesOrderDetails)
            {
                var salesInvoiceDetail = new SalesInvoiceDetail()
                {
                    SalesInvoiceId = invoice.Id
                };
                salesInvoiceDetail.SetOrderDetail(salesOrderDetail);
                salesInvoiceDetails.Add(salesInvoiceDetail);
            }

            await _unitOfWork.SalesInvoices.InvoiceDetails.AddRange(salesInvoiceDetails);

            // TODO : Actualizar salesOrderDetails.IsInvoiced + Actualizar estat de la comanda (Programar-ho al servei de comandes)

            return await UpdateImportsAndHeaderAmounts(invoice);
        }

        public async Task<GenericResponse> AddDetail(SalesInvoiceDetail invoiceDetail)
        {
            var invoice = await _unitOfWork.SalesInvoices.Get(invoiceDetail.SalesInvoiceId);
            if (invoice == null) return new GenericResponse(false, new List<string>() { $"La factura amb ID {invoiceDetail.SalesInvoiceId} no existeix" });

            await _unitOfWork.SalesInvoices.InvoiceDetails.Add(invoiceDetail);

            // Generar imports i actualizar imports de la capçalera
            invoice.SalesInvoiceDetails.Add(invoiceDetail);
            return await UpdateImportsAndHeaderAmounts(invoice);
        }
        public async Task<GenericResponse> UpdateDetail(SalesInvoiceDetail invoiceDetail)
        {
            var invoice = await _unitOfWork.SalesInvoices.Get(invoiceDetail.SalesInvoiceId);
            if (invoice == null) return new GenericResponse(false, new List<string>() { $"La factura amb ID {invoiceDetail.SalesInvoiceId} no existeix" });
            var detail = _unitOfWork.SalesInvoices.InvoiceDetails.Find(p => p.Id == invoiceDetail.Id).FirstOrDefault();
            if (detail == null) return new GenericResponse(false, new List<string> { $"La detall de factura amb ID {invoiceDetail.Id} no existeix" });

            await _unitOfWork.SalesInvoices.InvoiceDetails.Update(invoiceDetail);

            // Generar imports i actualizar imports de la capçalera
            return await UpdateImportsAndHeaderAmounts(invoice);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = _unitOfWork.SalesInvoices.InvoiceDetails.Find(p => p.Id == id).FirstOrDefault();
            if (detail == null) return new GenericResponse(false, new List<string> { $"La detall de factura amb ID {id} no existeix" });
            var invoice = await _unitOfWork.SalesInvoices.Get(detail.SalesInvoiceId);
            if (invoice == null) return new GenericResponse(false, new List<string>() { $"La factura amb ID {detail.SalesInvoiceId} no existeix" });

            await _unitOfWork.SalesInvoices.InvoiceDetails.Remove(detail);

            // Generar imports i actualizar imports de la capçalera
            return await UpdateImportsAndHeaderAmounts(invoice);
        }
        #endregion

        #region Imports
        /// <summary>
        /// - Suma els imports de cada impost de les lineas de la factura
        /// - Crea un registre per impost a SalesOrderImports
        /// - Calcula els imports de la capçalera (Tax, Base, Gross, Net)
        /// </summary>
        /// <param name="invoice">SalesInvoice</param>
        private async Task<GenericResponse> UpdateImportsAndHeaderAmounts(SalesInvoice invoice)
        {
            await RemoveImports(invoice);            

            // Obtenir sumatori d'imports agrupat per impost
            var invoiceImports = invoice.SalesInvoiceDetails
                .GroupBy(d => d.TaxId)
                .Select(d => new SalesInvoiceImport()
                {
                    SalesInvoiceId = invoice.Id,
                    TaxId = d.First().TaxId,
                    BaseAmount = d.Sum(d => d.Amount),
                }).ToList();
            // Aplicar impostos
            foreach (var invoiceImport in invoiceImports)
            {
                Tax? tax = await _unitOfWork.Taxes.Get(invoiceImport.TaxId);
                if (tax != null)
                {
                    invoiceImport.TaxAmount = tax.ApplyTax(invoiceImport.BaseAmount);
                    invoiceImport.NetAmount = invoiceImport.BaseAmount + invoiceImport.TaxAmount;
                }
            }
            await _unitOfWork.SalesInvoices.InvoiceImports.AddRange(invoiceImports);

            invoice.SalesInvoiceImports = new List<SalesInvoiceImport>(invoiceImports);
            invoice.CalculateAmountsFromImports();
            return await Update(invoice);
        }
        private async Task RemoveImports(SalesInvoice invoice)
        {
            var salesImports = _unitOfWork.SalesInvoices.InvoiceImports.Find(i => i.SalesInvoiceId == invoice.Id);
            if (salesImports.Any())
                await _unitOfWork.SalesInvoices.InvoiceImports.RemoveRange(salesImports);
        }

        #endregion

    }
}
