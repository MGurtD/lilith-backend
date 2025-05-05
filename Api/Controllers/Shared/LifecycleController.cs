using Application.Persistance;
using Domain.Entities;
using Domain.Entities.Sales;
using Domain.Entities.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class LifecycleController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public LifecycleController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Lifecycle request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.Lifecycles.Find(r => request.Name == r.Name).Any();
            if (!exists)
            {
                await _unitOfWork.Lifecycles.Add(request);

                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict($"Cicle de vida '{request.Name}' existent");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.Lifecycles.GetAll();

            return Ok(entities.OrderBy(e => e.Name));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.Lifecycles.Get(id);
            if (entity is not null)
            {
                return Ok(entity);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("Name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var entity = await _unitOfWork.Lifecycles.GetByName(name);
            if (entity is not null)
                return Ok(entity);
            else
                return NotFound();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, Lifecycle request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.Lifecycles.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.Lifecycles.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.Lifecycles.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.Lifecycles.Remove(entity);
            return Ok(entity);
        }

        [Route("Status")]
        [HttpPost]
        public async Task<IActionResult> CreateStatus(Status request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var entity = await _unitOfWork.Lifecycles.StatusRepository.Get(request.Id);
            if (entity is null)
            {
                await _unitOfWork.Lifecycles.StatusRepository.Add(request);
                return Ok(request);
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("Status/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateStatus(Guid id, Status request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.Lifecycles.StatusRepository.Find(s => s.Id == id).FirstOrDefault();
            if (exists is not null)
            {
                await _unitOfWork.Lifecycles.StatusRepository.Update(request);
                return Ok(request);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Status/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveContact(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var status = _unitOfWork.Lifecycles.StatusRepository.Find(s => s.Id == id).FirstOrDefault();
            if (status is not null)
            {
                await _unitOfWork.Lifecycles.StatusRepository.Remove(status);
                return Ok(status);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("StatusTransition")]
        [HttpPost]
        public async Task<IActionResult> CreateStatusTransition(StatusTransition request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            if (request.StatusId.HasValue && request.StatusToId.HasValue)
            {
                var status = await _unitOfWork.Lifecycles.StatusRepository.Get(request.StatusId.Value);
                var statusTo = await _unitOfWork.Lifecycles.StatusRepository.Get(request.StatusToId.Value);
                if (status is not null && statusTo is not null)
                {
                    await _unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Add(request);
                    return Ok(request);
                }
                else
                {
                    return NotFound("Identificadors de la transició inexistents");
                }
            }
            else
            {
                return BadRequest();
            }
        }

        [Route("StatusTransition/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateStatusTransition(Guid id, StatusTransition request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var transition = _unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Find(t => t.Id == id).FirstOrDefault();
            if (transition is not null)
            {
                await _unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Update(request);
                return Ok(transition);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("StatusTransition/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveStatusTransition(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var transition = _unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Find(t => t.Id == id).FirstOrDefault();
            if (transition is not null)
            {
                await _unitOfWork.Lifecycles.StatusRepository.TransitionRepository.Remove(transition);
                return Ok(transition);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
