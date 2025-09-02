using Application.Contracts;
using Application.Persistance;
using Application.Services;
using Domain.Entities;

namespace Api.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocalizationService _localizationService;

        public ExerciseService(IUnitOfWork unitOfWork, ILocalizationService localizationService)
        {
            _unitOfWork = unitOfWork;
            _localizationService = localizationService;
        }

        public Exercise? GetExerciceByDate(DateTime dateTime)
        {
            var exercice = _unitOfWork.Exercices.Find(e => dateTime >= e.StartDate && dateTime <= e.EndDate).FirstOrDefault();
            return exercice;
        }

        public async Task<GenericResponse> GetNextCounter(Guid exerciseId, string counterName)
        {
            var exercise = await _unitOfWork.Exercices.Get(exerciseId);
            var counter = "";
            if (exercise == null)
            {
                return new GenericResponse(false, _localizationService.GetLocalizedString("ExerciseNotFound"));
            }

            var prefix = exercise.Name[^2..];
            int newcounter;
            switch (counterName.ToLower())
            {
                case "purchaseorder":
                    counter = prefix + exercise.PurchaseOrderCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.PurchaseOrderCounter = newcounter.ToString()[^3..];
                    break;
                case "purchaseinvoice":
                    counter = prefix + exercise.PurchaseInvoiceCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.PurchaseInvoiceCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "salesinvoice":
                    counter = prefix + exercise.SalesInvoiceCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.SalesInvoiceCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "salesorder":
                    counter = prefix + exercise.SalesOrderCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.SalesOrderCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "receipt":
                    counter = prefix + exercise.ReceiptCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.ReceiptCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "deliverynote":
                    counter = prefix + exercise.DeliveryNoteCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.DeliveryNoteCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "budget":
                    counter = prefix + exercise.BudgetCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.BudgetCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "workorder":
                    counter = prefix + exercise.WorkOrderCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.WorkOrderCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                default:
                    return new GenericResponse(false, _localizationService.GetLocalizedString("ExerciseCounterNotFound", counterName));
            }

            await _unitOfWork.Exercices.Update(exercise);
            return new GenericResponse(true, newcounter);
        }
    }
}
