using Application.Persistance;
using Application.Persistance.Repositories.Purchase;
using Application.Services;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Shared
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReferenceController(IUnitOfWork unitOfWork, IReferenceService referenceService) : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IReferenceService _referenceService = referenceService;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.References.GetAll();

            return Ok(entities.OrderBy(r => r.Code));
        }

        [Route("Sales")]
        [HttpGet]
        public IActionResult GetAllSales(Guid customerId)
        {
            var entities = _unitOfWork.References.Find(r => r.Sales).OrderBy(r => r.Code);
            return Ok(entities);
        }
        [Route("Sales/Customer/{id:guid}")]
        [HttpGet]
        public IActionResult GetAllSalesByCustomerId(Guid id)
        {
            var entities = _unitOfWork.References.Find(r => r.Sales && r.CustomerId == id).OrderBy(r => r.Code);
            return Ok(entities);
        }

        [Route("Purchase")]
        [HttpGet]
        public IActionResult GetAllPurchase()
        {
            var entities = _unitOfWork.References.Find(r => r.Purchase).OrderBy(r => r.Code);
            return Ok(entities);
        }        

        [Route("Production")]
        [HttpGet]
        public IActionResult GetAllProduction()
        {
            var entities = _unitOfWork.References.Find(r => r.Production).OrderBy(r => r.Code);
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.References.Get(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("{id:guid}/Suppliers")]
        [HttpGet]
        public IActionResult GetReferenceSuppliers(Guid id)
        {
            var entities = _unitOfWork.Suppliers.GetReferenceSuppliers(id);
            return Ok(entities);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Reference request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.References.Find(r => request.Code == r.Code && request.Version == r.Version).Any();
            if (!exists)
            {
                request.Code = await GetReferenceCode(request);
                await _unitOfWork.References.Add(request);

                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict($"Referencia {request.Code} existent");
            }
        }

        private async Task<string> GetReferenceCode(Reference reference) {
            if (reference.ReferenceTypeId.HasValue) {
                var referenceType = await _unitOfWork.ReferenceTypes.Get(reference.ReferenceTypeId.Value);
                if (referenceType is not null && !reference.Code.Contains($" ({referenceType.Name})")) {
                    var codeParts = reference.Code.Split(" (");
                    if (codeParts.Length >= 1) {
                        return $"{codeParts[0]} ({referenceType.Name})";
                    } else {
                        return $"{reference.Code} ({referenceType.Name})";
                    }
                }
            }

            return reference.Code;
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Reference request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.References.Exists(request.Id);
            if (!exists)
                return NotFound();

            request.Code = await GetReferenceCode(request);
            await _unitOfWork.References.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.References.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            var canDelete = _referenceService.CanDelete(id);
            if (canDelete.Result)
            {
                await _unitOfWork.References.Remove(entity);
                return Ok(entity);
            }
            else
            {
                return BadRequest(canDelete.Errors[0]);
            }
        }

        [Route("Formats")]
        [HttpGet]
        public async Task<IActionResult> GetReferenceFormats()
        {
            var entities = await _unitOfWork.ReferenceFormats.GetAll();
            return Ok(entities);
        }
    }
}
