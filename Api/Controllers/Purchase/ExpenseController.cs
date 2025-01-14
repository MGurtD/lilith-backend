using Application.Contracts.Purchase;
using Application.Persistance;
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
        public IActionResult GetBetweenDatesAndType(DateTime startTime, DateTime endTime, Guid? expenseTypeId)
        {
            var entities = _unitOfWork.Expenses.Find(e => e.PaymentDate >= startTime && e.PaymentDate <= endTime);
            if (expenseTypeId.HasValue) {
                entities = entities.Where(e => e.ExpenseTypeId == expenseTypeId.Value);
            }
            return Ok(entities.OrderBy(e => e.PaymentDate));
        }
        [HttpGet]
        [Route("Monthly")]
        public IActionResult GetMonthly(int year, int month)
        {
            var entities = _unitOfWork.Expenses.Find(e => e.PaymentDate.Month == month && e.PaymentDate.Year == year);
            
            return Ok(entities.Sum(e => e.Amount));
        }

        [Route("ExpenseType/{id:guid}")]
        [HttpGet]        
        public IActionResult GetByType(Guid id)
        {
            var expenses =  _unitOfWork.Expenses.Find(p => p.ExpenseTypeId == id);
            return Ok(expenses);
        }

        [HttpGet]
        [Route("Consolidated")]
        public IActionResult GetAllConsolidation(DateTime startTime, DateTime endTime, string? type = "", string? typeDetail = "")
        {
            IEnumerable<ConsolidatedExpense> entities = Enumerable.Empty<ConsolidatedExpense>();
            if (!string.IsNullOrWhiteSpace(type) && !string.IsNullOrWhiteSpace(typeDetail))
                entities = _unitOfWork.ConsolidatedExpenses.Find(c => c.PaymentDate >= startTime && c.PaymentDate <= endTime && c.Type == type && c.TypeDetail == typeDetail);
            else if (!string.IsNullOrWhiteSpace(type))
                entities = _unitOfWork.ConsolidatedExpenses.Find(c => c.PaymentDate >= startTime && c.PaymentDate <= endTime && c.Type == type);
            else 
                entities = _unitOfWork.ConsolidatedExpenses.Find(c => c.PaymentDate >= startTime && c.PaymentDate <= endTime);            
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
