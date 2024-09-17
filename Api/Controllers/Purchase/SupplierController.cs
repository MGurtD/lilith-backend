using Application.Persistance;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SupplierController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Supplier request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.Suppliers.Find(r => request.ComercialName == r.ComercialName).Any();
            if (!exists)
            {
                await _unitOfWork.Suppliers.Add(request);

                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict($"Proveïdor {request.ComercialName} existent");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.Suppliers.GetAll();
            return Ok(entities.OrderBy(e => e.ComercialName));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.Suppliers.Get(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Supplier request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.Suppliers.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.Suppliers.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.Suppliers.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.Suppliers.Remove(entity);
            return Ok(entity);
        }

        #region Contacts
        [Route("Contact")]
        [HttpPost]
        public async Task<IActionResult> CreateContact(SupplierContact request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var supplier = await _unitOfWork.Suppliers.Get(request.SupplierId);
            if (supplier is not null)
            {
                await _unitOfWork.Suppliers.AddContact(request);
                return Ok(request);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Contact/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateContact(Guid id, SupplierContact request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var contact = _unitOfWork.Suppliers.GetContactById(id);
            if (contact is not null)
            {
                contact.FirstName = request.FirstName;
                contact.LastName = request.LastName;
                contact.Email = request.Email;
                contact.Phone = request.Phone;
                contact.PhoneExtension = request.PhoneExtension;
                contact.Charge = request.Charge;
                contact.Disabled = request.Disabled;
                contact.Default = request.Default;
                contact.Observations = request.Observations;

                await _unitOfWork.Suppliers.UpdateContact(contact);
                return Ok(contact);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Contact/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveContact(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var contact = _unitOfWork.Suppliers.GetContactById(id);
            if (contact is not null)
            {
                await _unitOfWork.Suppliers.RemoveContact(contact);
                return Ok(contact);
            }
            else
            {
                return NotFound();
            }
        }
        #endregion

        #region References
        [Route("Reference/{supplierReferenceId:guid}")]
        [HttpGet]
        public IActionResult GetReference(Guid supplierReferenceId)
        {
            var references = _unitOfWork.Suppliers.GetSupplierReferenceById(supplierReferenceId);
            return Ok(references);
        }

        [Route("{id:guid}/Reference")]
        [HttpGet]
        public IActionResult GetReferences(Guid id)
        {
            var references = _unitOfWork.Suppliers.GetSupplierReferences(id);
            return Ok(references);
        }

        [Route("Reference/GetSupplierByReference/{referenceId:guid}")]
        [HttpGet]
        public IActionResult GetSuppliersByReference(Guid referenceId)
        {
            var suppliers = _unitOfWork.Suppliers.GetReferenceSuppliers(referenceId);
            return Ok(suppliers);
        }

        [Route("Reference")]
        [HttpPost]
        public async Task<IActionResult> CreateReference(SupplierReference request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var supplier = await _unitOfWork.Suppliers.Get(request.SupplierId);
            if (supplier is null) return NotFound("El proveïdor no existeix");

            var reference = await _unitOfWork.References.Get(request.ReferenceId);
            if (reference is null) return NotFound("La referencia no existeix");

            var references = _unitOfWork.Suppliers.GetSupplierReferences(request.SupplierId);
            var exists = references.Where(r => r.ReferenceId == request.ReferenceId).Count() > 0;
            if (!exists)
            {
                await _unitOfWork.Suppliers.AddSupplierReference(request);
                return Ok(request);
            }
            else
            {
                return ValidationProblem("La referencia indicada ja existeix al proveïdor");
            }
        }

        [Route("Reference/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateReference(Guid id, SupplierReference request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var supplierReference = await _unitOfWork.Suppliers.GetSupplierReferenceById(id);
            if (supplierReference is not null)
            {
                supplierReference.UpdatedOn = DateTime.Now;
                supplierReference.SupplierCode = request.SupplierCode;
                supplierReference.SupplierDescription = request.SupplierDescription;
                supplierReference.SupplierPrice = request.SupplierPrice;
                supplierReference.SupplyDays = request.SupplyDays;

                await _unitOfWork.Suppliers.UpdateSupplierReference(supplierReference);
                return Ok(supplierReference);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Reference/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveReference(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var supplierReference = await _unitOfWork.Suppliers.GetSupplierReferenceById(id);
            if (supplierReference is not null)
            {
                await _unitOfWork.Suppliers.RemoveSupplierReference(supplierReference);
                return Ok(supplierReference);
            }
            else
            {
                return NotFound();
            }
        }
        #endregion


    }
}
