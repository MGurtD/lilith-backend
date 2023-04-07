using Microsoft.AspNetCore.Mvc;
using Application.Persistance;
using Domain.Entities;
using AutoMapper;
using Api.Mapping.Dtos;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SiteController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> Create(SiteDto request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var site = _mapper.Map<Site>(request);

            // Validate existence of the unique user key
            var dbSite = _unitOfWork.Sites.Find(e => e.Name == site.Name).SingleOrDefault();
            if (dbSite is not null)
            {
                return BadRequest($"Site with name {request.Name} already exists");
            }

            site.CreatedOn = DateTime.UtcNow;

            await _unitOfWork.Sites.Add(site);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var site = await _unitOfWork.Sites.GetAll();
            var dtos = _mapper.Map<IEnumerable<SiteDto>>(site);
            
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var sites = await _unitOfWork.Sites.Get(id);
            var dto = _mapper.Map<SiteDto>(sites);

            return Ok(dto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, SiteDto request)
        {
            // Validation the incoming request
            if (id != request.Id)
                return BadRequest();            
            if (!ModelState.IsValid) 
                return BadRequest(ModelState.ValidationState);

            var sitesDb = _unitOfWork.Sites.Find(e => e.Id == id).FirstOrDefault();
            if (sitesDb is null)
                return NotFound();

            var sites = _mapper.Map<Site>(request);
            sites.CreatedOn = sitesDb.CreatedOn;
            sites.UpdatedOn = DateTime.UtcNow;

            await _unitOfWork.Sites.Update(sites);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var site = await _unitOfWork.Sites.Get(id);
            if (site is null)
                return NotFound();

            await _unitOfWork.Sites.Remove(site);

            var dto = _mapper.Map<SiteDto>(site);
            return Ok(dto);
        }

    }
}