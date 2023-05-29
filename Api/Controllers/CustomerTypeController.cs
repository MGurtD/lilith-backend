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
    public class CustomerTypeController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CustomerTypeController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CustomerTypeDto request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);
            
            var exists = _unitOfWork.CustomerTypes.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                var customerType = _mapper.Map<CustomerType>(request);
                await _unitOfWork.CustomerTypes.Add(customerType);

                return Ok(customerType);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.CustomerTypes.GetAll();
            var dtos = _mapper.Map<IEnumerable<CustomerTypeDto>>(entities);
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.CustomerTypes.Get(id);
            if (entity is not null)
            {
                var dto = _mapper.Map<CustomerTypeDto>(entity);
                return Ok(dto);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, CustomerType request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.CustomerTypes.Exists(request.Id);
            if (!exists)
                return NotFound();

            var entity = _mapper.Map<CustomerType>(request);
            await _unitOfWork.CustomerTypes.Update(entity);
            return Ok(entity);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.CustomerTypes.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.CustomerTypes.Remove(entity);
            return Ok(entity);
        }
    }
}
