﻿using Application.Persistance;
using Domain.Entities.Purchase;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers.Purchase
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExpenseController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExpenseController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Expenses request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState.ValidationState);

            await _unitOfWork.Expenses.Add(request);
            //TODO add control to create recurring expenses
            if (request.Recurring)
            {
                request.RelatedExpenseId = request.Id.ToString();
                DateTime paymentDay;
                while (request.PaymentDate < request.EndDate)
                {
                    paymentDay = request.PaymentDate;
                    paymentDay = paymentDay.AddMonths(request.Frecuency);
                    if (request.PaymentDate.Day != request.PaymentDay)
                    {
                        paymentDay = paymentDay.AddDays(request.PaymentDay - request.PaymentDate.Day);
                    }

                    //TODO generar un nou ID, i asociar l'ID al related
                    Guid g = Guid.NewGuid();
                    request.Id = g;
                    request.PaymentDate = paymentDay;
                    await _unitOfWork.Expenses.Add(request);
                }
            }
            return Ok(request);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var entities = await _unitOfWork.Expenses.GetAll();
            return Ok(entities.OrderBy(e => e.PaymentDay));
        }
        [Route("ExpenseType/{id:guid}")]
        [HttpGet]        
        public async Task<IActionResult> GetByType(Guid id)
        {
            var expenses =  _unitOfWork.Expenses.Find(p => p.ExpenseTypeId == id);
            return Ok(expenses);
        }

        [HttpGet]
        [Route("Consolidated")]
        public IActionResult GetAllConsolidation(DateTime startTime, DateTime endTime)
        {
            var entities = _unitOfWork.ConsolidatedExpenses.Find(c => c.PaymentDate >= startTime && c.PaymentDate <= endTime);
            return Ok(entities);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await _unitOfWork.Expenses.Get(id);
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
        public async Task<IActionResult> Update(Guid Id, Expenses request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);
            if (Id != request.Id)
                return BadRequest();

            var exists = await _unitOfWork.Expenses.Exists(request.Id);
            if (!exists)
                return NotFound();

            await _unitOfWork.Expenses.Update(request);
            return Ok(request);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.ValidationState);

            var entity = _unitOfWork.Expenses.Find(e => e.Id == id).FirstOrDefault();
            if (entity is null)
                return NotFound();

            if (entity.Recurring)
            {
                var parentId = string.IsNullOrEmpty(entity.RelatedExpenseId) ? entity.Id : Guid.Parse(entity.RelatedExpenseId);
                var relatedParent = _unitOfWork.Expenses.Find(e => e.Id == parentId).FirstOrDefault();
                if (relatedParent is not null)
                    await _unitOfWork.Expenses.Remove(relatedParent);

                var relatedExpenses = _unitOfWork.Expenses.Find(e => e.RelatedExpenseId == parentId.ToString());
                await _unitOfWork.Expenses.RemoveRange(relatedExpenses);
            }
            else
            {
                await _unitOfWork.Expenses.Remove(entity);
            }

            return Ok(entity);
        }

    }
}
