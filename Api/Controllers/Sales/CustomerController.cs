using Application.Persistance;
using Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Sales
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public CustomerController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var exists = _unitOfWork.Customers.Find(r => request.ComercialName == r.ComercialName).Any();
            if (!exists)
            {
                await _unitOfWork.Customers.Add(request);

                var location = Url.Action(nameof(GetById), new { id = request.Id }) ?? $"/{request.Id}";
                return Created(location, request);
            }
            else
            {
                return Conflict($"Client {request.ComercialName} existent");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.Customers.GetAll();

            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.Customers.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, Customer request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.Customers.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.Customers.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.Customers.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            await _unitOfWork.Customers.Remove(entity);
            return Ok(entity);
        }

        [Route("Contact")]
        [HttpPost]
        public async Task<IActionResult> CreateContact(CustomerContact request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var supplier = await _unitOfWork.Customers.Get(request.CustomerId);
            if (supplier is not null)
            {
                await _unitOfWork.Customers.AddContact(request);
                return Ok(request);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Contact/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateContact(Guid id, CustomerContact request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var contact = _unitOfWork.Customers.GetContactById(id);
            if (contact is not null)
            {
                contact.FirstName = request.FirstName;
                contact.LastName = request.LastName;
                contact.Email = request.Email;
                contact.PhoneNumber = request.PhoneNumber;
                contact.Charge = request.Charge;
                contact.Disabled = request.Disabled;
                contact.Main = request.Main;


                await _unitOfWork.Customers.UpdateContact(contact);
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

            var contact = _unitOfWork.Customers.GetContactById(id);
            if (contact is not null)
            {
                await _unitOfWork.Customers.RemoveContact(contact);
                return Ok(contact);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Address")]
        [HttpPost]
        public async Task<IActionResult> CreateAddress(CustomerAddress request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var customer = await _unitOfWork.Customers.Get(request.CustomerId);
            if (customer is not null)
            {
                await _unitOfWork.Customers.AddAddress(request);
                return Ok(request);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Address/{id:guid}")]
        [HttpPut]
        public async Task<IActionResult> UpdateAddress(Guid id, CustomerAddress request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var address = _unitOfWork.Customers.GetAddressById(id);
            if (address is not null)
            {
                address.Name = request.Name;
                address.Address = request.Address;
                address.Country = request.Country;
                address.Region = request.Region;
                address.PostalCode = request.PostalCode;
                address.City = request.City;
                address.Disabled = request.Disabled;
                address.Main = request.Main;
                address.Observations = request.Observations;

                await _unitOfWork.Customers.UpdateAddress(address);
                return Ok(address);
            }
            else
            {
                return NotFound();
            }
        }

        [Route("Address/{id:guid}")]
        [HttpDelete]
        public async Task<IActionResult> RemoveAddress(Guid id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            var address = _unitOfWork.Customers.GetAddressById(id);
            if (address is not null)
            {
                await _unitOfWork.Customers.RemoveAddress(address);
                return Ok(address);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
