using Application.Contracts;
using Application.Contracts.Sales;
using Application.Persistance;
using Application.Services;
using Application.Services.Sales;
using Domain.Entities.Sales;

namespace Api.Services.Sales
{
    public class BudgetService : IBudgetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IExerciseService _exerciseService;
        private readonly ILogger<BudgetService> _logger;

        public BudgetService(IUnitOfWork unitOfWork, IExerciseService exerciseService, ILogger<BudgetService> logger)
        {
            _unitOfWork = unitOfWork;
            _exerciseService = exerciseService;
            _logger = logger;
        }        
        
        public async Task<Budget?> GetById(Guid id)
        {
            var budget = await _unitOfWork.Budgets.Get(id);
            return budget;
        }

        public IEnumerable<Budget> GetBetweenDates(DateTime startDate, DateTime endDate)
        {
            var budgets = _unitOfWork.Budgets.Find(p => p.Date >= startDate && p.Date <= endDate);
            return budgets;
        }
        public IEnumerable<Budget> GetBetweenDatesAndCustomer(DateTime startDate, DateTime endDate, Guid customerId)
        {
            var budgets = _unitOfWork.Budgets.Find(p => p.Date >= startDate && p.Date <= endDate && p.CustomerId == customerId);
            return budgets;
        }

        public async Task<GenericResponse> Create(CreateHeaderRequest createRequest)
        {
            var counterObj = await _exerciseService.GetNextCounter(createRequest.ExerciseId, "budget");
            if (!counterObj.Result || counterObj.Content == null) return new GenericResponse(false, new List<string>() { "Error al crear el comptador" });
            var budget = new Budget
            {
                Id = createRequest.Id,
                Number = counterObj.Content.ToString()!,
                Date = createRequest.Date,
                ExerciseId = createRequest.ExerciseId,
                CustomerId = createRequest.CustomerId
            };

            // Estat inicial
            if (createRequest.InitialStatusId.HasValue)
            {
                budget.StatusId = createRequest.InitialStatusId;
            }
            else
            {
                var lifecycle = _unitOfWork.Lifecycles.Find(l => l.Name == "Budget").FirstOrDefault();
                if (lifecycle == null)
                    return new GenericResponse(false, new List<string>() { "El cicle de vida 'Budget' no existeix" });
                if (!lifecycle.InitialStatusId.HasValue)
                    return new GenericResponse(false, new List<string>() { "El cicle de vida 'Budget' no té estat inicial" });
                budget.StatusId = lifecycle.InitialStatusId;
            }

            await _unitOfWork.Budgets.Add(budget);
            return new GenericResponse(true, budget);
        }

        public async Task<GenericResponse> Update(Budget budget)
        {
            budget.Details.Clear();

            var existingBudget = await _unitOfWork.Budgets.Get(budget.Id);
            if (existingBudget == null)
            {
                return new GenericResponse(false, $"La comanda amb ID {budget.Id} no existeix");
            }

            var statusPending = await _unitOfWork.Lifecycles.GetStatusByName("Budget", "Pendent d'acceptar");
            var statusAccept = await _unitOfWork.Lifecycles.GetStatusByName("Budget", "Acceptat");

            if (statusPending == null || statusAccept == null)
            {
                return new GenericResponse(false, "No s'han trobat els estats 'Pendent d'acceptar' i 'Acceptat' al cicle de vida 'Budget'");
            }

            if (existingBudget.StatusId == statusPending.Id && budget.StatusId == statusAccept.Id)
            {
                budget.AcceptanceDate = DateTime.Now;                
            }

            await _unitOfWork.Budgets.Update(budget);
            return new GenericResponse(true);
        }

        public async Task<GenericResponse> Remove(Guid id)
        {
            var budget = _unitOfWork.Budgets.Find(p => p.Id == id).FirstOrDefault();
            if (budget == null)
            {
                return new GenericResponse(false, $"La comanda amb ID {id} no existeix");
            }
            else
            {
                await _unitOfWork.Budgets.Remove(budget);
                return new GenericResponse(true, new List<string> { });
            }
        }
        
        public async Task<GenericResponse> AddDetail(BudgetDetail detail)
        {
            await _unitOfWork.Budgets.Details.Add(detail);
            return new GenericResponse(true, detail);
        }
        public async Task<GenericResponse> UpdateDetail(BudgetDetail detail)
        {
            await _unitOfWork.Budgets.Details.Update(detail);
            return new GenericResponse(true, detail);
        }
        public async Task<GenericResponse> RemoveDetail(Guid id)
        {
            var detail = _unitOfWork.Budgets.Details.Find(d => d.Id == id).FirstOrDefault();
            if (detail == null) return new GenericResponse(false, $"El detall de comanda amb ID {id} no existeix");
            await _unitOfWork.Budgets.Details.Remove(detail);

            return new GenericResponse(true, detail);
        }

        public async Task<BudgetReportResponse?> GetByIdForReporting(Guid id)
        {
             var budget = await GetById(id);
            if (budget is null) return null;

            var customer = await _unitOfWork.Customers.Get(budget.CustomerId);
            if (customer is null) return null;

            var site = (await _unitOfWork.Sites.FindAsync(s => !s.Disabled)).FirstOrDefault();
            if (site == null) return null;

            budget.Details = budget.Details.OrderBy(d => d.Reference!.Code).ToList();
            var salesOrderReport = new BudgetReportResponse
            {
                Budget = budget,
                Customer = customer,
                Site = site,
                Total = budget.Details.Sum(d => d.Amount),
            };
            return salesOrderReport;
        }

        public async Task<GenericResponse> RejectOutdatedBudgets()
        {
            var status = await _unitOfWork.Lifecycles.GetStatusByName("Budget", "Pendent d'acceptar");
            var rejectedstatus = await _unitOfWork.Lifecycles.GetStatusByName("Budget", "Rebutjat");
            if (status == null || rejectedstatus == null)
            {
                return new GenericResponse(false, "No s'han trobat els estats 'Pendent d'acceptar' i 'Rebutjat' al cicle de vida 'Budget'");
            }

            var budgets =  await _unitOfWork.Budgets.FindAsync(b => b.StatusId == status.Id && b.Date.AddDays(30) <= DateTime.UtcNow);           
            foreach (var budget in budgets)
            {
                budget.StatusId = rejectedstatus.Id;
                budget.AutoRejectedDate = DateTime.UtcNow;
                budget.Notes = "Pressupost rebutjat automaticament en data: " + DateTime.UtcNow.ToString();
                await _unitOfWork.Budgets.Update(budget);
                
            }
            return (new GenericResponse(true));
        }
    }
}
