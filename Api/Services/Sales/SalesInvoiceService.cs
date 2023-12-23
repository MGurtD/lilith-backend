using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services;
using Application.Services.Sales;
using Domain.Entities;
using Domain.Entities.Production;
using Domain.Entities.Sales;
using FastReport;

namespace Api.Services.Sales
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
        private readonly IDeliveryNoteService _deliveryNoteService;
        private readonly string LifecycleName = "SalesInvoice";
        private readonly IExerciseService _exerciseService;

        public SalesInvoiceService(IUnitOfWork unitOfWork, IDueDateService dueDateService, ISalesOrderService salesOrderService, IDeliveryNoteService deliveryNoteService, IExerciseService exerciseService)
        {
            _unitOfWork = unitOfWork;
            _dueDateService = dueDateService;
            _deliveryNoteService = deliveryNoteService;
            _exerciseService = exerciseService;
        }

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

        public async Task<GenericResponse> Create(CreateHeaderRequest createInvoiceRequest)
        {
            var response = await ValidateCreateInvoiceRequest(createInvoiceRequest);
            if (!response.Result) return response;

            var invoiceEntities = (InvoiceEntities)response.Content!;
            var counterObj = await _exerciseService.GetNextCounter(invoiceEntities.Exercise.Id, "salesinvoice");
            if (counterObj == null) return new GenericResponse(false, new List<string>() { "Error al crear el comptador" });
            var counter = counterObj.Content.ToString();
            var invoice = new SalesInvoice
            {
                Id = createInvoiceRequest.Id,
                InvoiceNumber = counter,
                InvoiceDate = createInvoiceRequest.Date
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

            return new GenericResponse(true, invoice);
        }

        private async Task<GenericResponse> ValidateCreateInvoiceRequest(CreateHeaderRequest createInvoiceRequest)
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

        public async Task<GenericResponse> Update(SalesInvoice invoice)
        {
            var currentInvoice = await _unitOfWork.SalesInvoices.Get(invoice.Id);
            if (currentInvoice == null) return new GenericResponse(false, $"La factura amb ID {invoice.Id} no existeix");

            // Recuperar estat actual y nou
            var lifecycle = await _unitOfWork.Lifecycles.GetByName(LifecycleName);
            var currentStatus = lifecycle!.Statuses!.FirstOrDefault(s => s.Id == currentInvoice.StatusId);
            var updatedStatus = lifecycle!.Statuses!.FirstOrDefault(s => s.Id == invoice.StatusId);

            // Netejar dependencies per evitar col·lisions de EF
            currentInvoice = null;
            invoice.SalesInvoiceDetails.Clear();
            invoice.SalesInvoiceImports.Clear();
            invoice.SalesInvoiceDueDates.Clear();

            // Actualizar la factura y l'albarà d'entrega relacionat
            await _unitOfWork.SalesInvoices.Update(invoice);
            await GenerateDueDates(invoice);
            await UpdateRelatedDeliveryNote(invoice.Id, currentStatus!, updatedStatus!);

            return new GenericResponse(true, invoice);
        }

        private async Task UpdateRelatedDeliveryNote(Guid invoiceId, Status currentStatus, Status updatedStatus)
        {
            // Accions relacionades amb l'albarà d'entrega
            if (currentStatus != null && updatedStatus != null && currentStatus.Id != updatedStatus.Id)
            {
                if (updatedStatus.Name == "Cobrada")
                {
                    await _deliveryNoteService.Invoice(invoiceId);
                }
                else if (currentStatus.Name == "Cobrada")
                {
                    await _deliveryNoteService.UnInvoice(invoiceId);
                }
            }
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var invoice = _unitOfWork.SalesInvoices.Find(p => p.Id == id).FirstOrDefault();
            if (invoice == null) return new GenericResponse(false, new List<string> { $"La factura amb ID {id} no existeix" });

            var invoiceDeliveryNotes = _unitOfWork.DeliveryNotes.Find(d => d.SalesInvoiceId == id);
            if (invoiceDeliveryNotes != null && invoiceDeliveryNotes.Any())
            {
                foreach (var note in invoiceDeliveryNotes)
                {
                    note.SalesInvoiceId = null;
                    _unitOfWork.DeliveryNotes.UpdateWithoutSave(note);
                }
                await _unitOfWork.CompleteAsync();
            }

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

        #region Details
        public async Task<GenericResponse> AddDeliveryNote(Guid id, DeliveryNote deliveryNote)
        {
            var invoice = _unitOfWork.SalesInvoices.Find(i => i.Id == id).FirstOrDefault();
            if (invoice == null) return new GenericResponse(false, $"La factura amb ID {id} no existeix");
            var tax = _unitOfWork.Taxes.Find(t => t.Percentatge == 21).FirstOrDefault();
            if (tax == null) return new GenericResponse(false, $"No existeix l'import IVA 21%");

            // Crear les lines de la factura segons les línes de l'albarà
            var deliveryNoteDetails = _unitOfWork.DeliveryNotes.Details.Find(d => d.DeliveryNoteId == deliveryNote.Id);
            var invoiceDetails = new List<SalesInvoiceDetail>();
            foreach (var deliveryNoteDetail in deliveryNoteDetails.ToList())
            {
                var salesInvoiceDetail = new SalesInvoiceDetail
                {
                    SalesInvoiceId = id
                };
                salesInvoiceDetail.TaxId = tax.Id;
                salesInvoiceDetail.SetDeliveryNoteDetail(deliveryNoteDetail);
                invoice.SalesInvoiceDetails.Add(salesInvoiceDetail);
                invoiceDetails.Add(salesInvoiceDetail);
            }
            await _unitOfWork.SalesInvoices.InvoiceDetails.AddRange(invoiceDetails);
            deliveryNoteDetails = null;
            invoiceDetails = null;

            // Actualizar taules relacionades
            await UpdateImportsAndHeaderAmounts(invoice);

            // Associar la factura per evitar la selecció de l'albarà d'entrega a altre factures
            deliveryNote.SalesInvoiceId = id;
            await _deliveryNoteService.Update(deliveryNote);

            return new GenericResponse(true, invoiceDetails);
        }

        public async Task<GenericResponse> RemoveDeliveryNote(Guid id, DeliveryNote deliveryNote)
        {
            var invoice = _unitOfWork.SalesInvoices.Find(i => i.Id == id).FirstOrDefault();
            if (invoice == null) return new GenericResponse(false, $"La factura amb ID {id} no existeix");
            var detailIds = _unitOfWork.DeliveryNotes.Details.Find(d => d.DeliveryNoteId == deliveryNote.Id).Select(d => d.Id).ToList();

            var deliveryNoteDetails = _unitOfWork.SalesInvoices.InvoiceDetails.Find(d => d.DeliveryNoteDetailId != null && detailIds.Contains(d.DeliveryNoteDetailId.Value));
            // Eliminar els detalls associats a l'albarà
            await _unitOfWork.SalesInvoices.InvoiceDetails.RemoveRange(deliveryNoteDetails);

            // Actualizar taules relacionades
            await UpdateImportsAndHeaderAmounts(invoice);

            // Alliberar l'albarà perquè sigui assignable de nou a una factura
            deliveryNote.SalesInvoiceId = null;
            await _deliveryNoteService.Update(deliveryNote);

            return new GenericResponse(true, deliveryNoteDetails);
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
            var memoryDetail = invoice.SalesInvoiceDetails.First(d => d.Id == detail.Id);
            invoice.SalesInvoiceDetails.Remove(memoryDetail);

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
