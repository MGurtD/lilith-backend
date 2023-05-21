using Api.Mapping.Dtos;
using Application.Dtos;
using Application.Persistance;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NLog.Web.LayoutRenderers;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SupplierTypeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SupplierTypeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(SupplierTypeDto request)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.SupplierTypes.Find(r => request.Name == r.Name).Any();
            if(!exists)
            {
                var supplierType = _mapper.Map<SupplierType>(request);
                await _unitOfWork.SupplierTypes.Add(supplierType);

                return Ok(supplierType);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.SupplierTypes.GetAll();
            var dtos = _mapper.Map<IEnumerable<SupplierTypeDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.SupplierTypes.Get(id);
            if (entity is not null)
            {
                var dto = _mapper.Map<SupplierTypeDto>(entity);
                return Ok(dto);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, SupplierType request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.SupplierTypes.Exists(request.Id);
            if (!exists)
                return NotFound();

            var entity = _mapper.Map<SupplierType>(request);
            await _unitOfWork.SupplierTypes.Update(entity);  
            return Ok(entity);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.SupplierTypes.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.SupplierTypes.Remove(entity);
            return Ok(entity);
        }
    }
}
