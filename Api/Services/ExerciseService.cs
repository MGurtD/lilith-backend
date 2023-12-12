using Application.Contracts;
using Application.Persistance;
using Application.Services;
using System.Xml.Linq;

namespace Api.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExerciseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GenericResponse>GetNextCounter(Guid exerciseId, string counterName)
        {
            var exercise = await _unitOfWork.Exercices.Get(exerciseId);
            var result = false;
            var counter = "";
            var newcounter = 0;
            if (exercise == null)
            {
                return new GenericResponse(result, counter);
            }
            
            var prefix = exercise.Name.Substring(exercise.Name.Length - 2);


            switch (counterName.ToLower())
            {
                case "purchaseinvoice":
                    result = true;
                    counter = prefix + exercise.PurchaseInvoiceCounter.PadLeft(3,'0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.PurchaseInvoiceCounter = newcounter.ToString().Substring(newcounter.ToString().Length-3);
                    break;
                case "salesinvoice":
                    result = true;
                    counter = prefix + exercise.SalesInvoiceCounter.PadLeft(3,'0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.SalesInvoiceCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "salesorder":
                    result = true;
                    counter = prefix + exercise.SalesOrderCounter.PadLeft(3,'0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.SalesOrderCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "receipt":
                    result = true;
                    counter = prefix + exercise.ReceiptCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.ReceiptCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
                case "deliverynote":
                    result = true;
                    counter = prefix + exercise.DeliveryNoteCounter.PadLeft(3, '0');
                    newcounter = int.Parse(counter) + 1;
                    exercise.DeliveryNoteCounter = newcounter.ToString().Substring(newcounter.ToString().Length - 3);
                    break;
            }
            await _unitOfWork.Exercices.Update(exercise);
            return new GenericResponse(result, new List<string>() { "" }, counter);
        }
    }
}
