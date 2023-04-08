using Microsoft.AspNetCore.Mvc;
using Application.Persistance;
using Domain.Entities;
using AutoMapper;
using Api.Mapping.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Server.IIS.Core;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnterpriseController : ControllerBase
    {
        private readonly ILogger<EnterpriseController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public EnterpriseController(ILogger<EnterpriseController> logger, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(EnterpriseDto requestEnterprise)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var enterprise = _mapper.Map<Enterprise>(requestEnterprise);

            // Validate existence of the unique user key
            var dbEnterprise = _unitOfWork.Enterprises.Find(e => e.Name == enterprise.Name).SingleOrDefault();
            if (dbEnterprise is not null)
            {
                throw new KeyNotFoundException($"Enterprise with name {requestEnterprise.Name} already exists");
            }

            enterprise.CreatedOn = DateTime.UtcNow;
            enterprise.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.Enterprises.Add(enterprise);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var enterprises = await _unitOfWork.Enterprises.GetAll();
            var dtos = _mapper.Map<IEnumerable<EnterpriseDto>>(enterprises);
            
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var enterprises = await _unitOfWork.Enterprises.Get(id);
            var dto = _mapper.Map<EnterpriseDto>(enterprises);

            return Ok(dto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, EnterpriseDto requestEnterprise)
        {
            // Validation the incoming request
            if (id != requestEnterprise.Id)
                return BadRequest();            
            if (!ModelState.IsValid) 
                return BadRequest(ModelState.ValidationState);

            var enterpriseDb = _unitOfWork.Enterprises.Find(e => e.Id == id).FirstOrDefault();
            if (enterpriseDb is null)
            {
                throw new KeyNotFoundException($"Enterprise with id {id} does not exists");
            }

            var enterprise = _mapper.Map<Enterprise>(requestEnterprise);
            enterprise.CreatedOn = enterpriseDb.CreatedOn;
            enterprise.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.Enterprises.Update(enterprise);
            return Ok(requestEnterprise);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var enterprise = await _unitOfWork.Enterprises.Get(id);
            if (enterprise is null)
            {
                throw new KeyNotFoundException($"Enterprise with id {id} does not exists");
            }

            await _unitOfWork.Enterprises.Remove(enterprise);

            var dto = _mapper.Map<EnterpriseDto>(enterprise);
            return Ok(dto);
        }

    }
}