using Microsoft.AspNetCore.Mvc;
using Application.Persistance;
using Domain.Entities;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciceController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExerciceController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Exercice request)
        {
            // Validation the incoming request
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);


            // Validate existence of the unique user key
            var exists = _unitOfWork.Exercices.Find(r => request.Name == r.Name).Count() > 0;
            if (!exists)
            {
                await _unitOfWork.Exercices.Add(request);

                return Ok(request);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var exercice = await _unitOfWork.Exercices.GetAll();
            return Ok(exercice);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var exercice = await _unitOfWork.Exercices.Get(id);
            if (exercice is not null)
            {
                return Ok(exercice);
            } 
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid Id, [FromBody]Exercice request)
        {
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.Exercices.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.Exercices.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var exercice = await _unitOfWork.Exercices.Get(id);
            if (exercice is not null)
            {
                await _unitOfWork.Exercices.Remove(exercice);
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }

    }
}