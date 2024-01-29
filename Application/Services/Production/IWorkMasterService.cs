using Application.Contracts.Purchase;
using Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities.Production;

namespace Application.Services.Production
{
    public interface IWorkMasterService
    {
        Task<GenericResponse> Calculate(WorkMaster workMaster);
    }
}
