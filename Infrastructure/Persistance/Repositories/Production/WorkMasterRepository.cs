using Application.Persistance.Repositories.Production;
using Domain.Entities.Production;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistance.Repositories.Production
{
    public class WorkMasterRepository : Repository<WorkMaster, Guid>, IWorkMasterRepository
    {
        public IWorkMasterPhaseRepository Phases { get; }

        public WorkMasterRepository(ApplicationDbContext context) : base(context)
        {
            Phases = new WorkMasterPhaseRepository(context);
        }

        public override async Task<IEnumerable<WorkMaster>> GetAll()
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .AsNoTracking()
                        .ToListAsync();
        }

        public override async Task<WorkMaster?> Get(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include(d => d.Phases)
                            .ThenInclude(d => d.Details)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<bool> Copy(WorkMasterCopy workMasterCopy)
        {
            using var transaction = await context.Database.BeginTransactionAsync();
            
            try
            {
                // Get the source WorkMaster with all related entities
                var sourceWorkMaster = await dbSet
                    .Include(wm => wm.Reference)
                    .Include(wm => wm.Phases)
                        .ThenInclude(p => p.Details)
                    .Include(wm => wm.Phases)
                        .ThenInclude(p => p.BillOfMaterials)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(wm => wm.Id == workMasterCopy.WorkmasterId);

                if (sourceWorkMaster == null)
                    return false;

                Guid targetReferenceId;

                // Create new reference if needed
                if (workMasterCopy.ReferenceId == null)
                {
                    targetReferenceId = Guid.NewGuid();
                    var newReference = new Domain.Entities.Shared.Reference
                    {
                        Id = targetReferenceId,
                        Code = workMasterCopy.ReferenceCode,
                        Description = sourceWorkMaster.Reference!.Description,
                        Cost = sourceWorkMaster.Reference.Cost,
                        Price = sourceWorkMaster.Reference.Price,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                        Disabled = sourceWorkMaster.Reference.Disabled,
                        Version = sourceWorkMaster.Reference.Version,
                        TaxId = sourceWorkMaster.Reference.TaxId,
                        Production = sourceWorkMaster.Reference.Production,
                        Purchase = sourceWorkMaster.Reference.Purchase,
                        Sales = sourceWorkMaster.Reference.Sales,
                        ReferenceTypeId = sourceWorkMaster.Reference.ReferenceTypeId,
                        ReferenceFormatId = sourceWorkMaster.Reference.ReferenceFormatId,
                        WorkMasterCost = sourceWorkMaster.Reference.WorkMasterCost,
                        IsService = sourceWorkMaster.Reference.IsService,
                        LastCost = sourceWorkMaster.Reference.LastCost,
                        CustomerId = sourceWorkMaster.Reference.CustomerId,
                        CategoryName = sourceWorkMaster.Reference.CategoryName,
                        TransportAmount = sourceWorkMaster.Reference.TransportAmount
                    };

                    context.Set<Domain.Entities.Shared.Reference>().Add(newReference);
                }
                else
                {
                    targetReferenceId = workMasterCopy.ReferenceId.Value;
                }

                // Create new WorkMaster
                var newWorkMaster = new WorkMaster
                {
                    Id = Guid.NewGuid(),
                    ReferenceId = targetReferenceId,
                    BaseQuantity = sourceWorkMaster.BaseQuantity,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow,
                    Disabled = sourceWorkMaster.Disabled,
                    externalCost = sourceWorkMaster.externalCost,
                    machineCost = sourceWorkMaster.machineCost,
                    materialCost = sourceWorkMaster.materialCost,
                    operatorCost = sourceWorkMaster.operatorCost,
                    Mode = workMasterCopy.Mode
                };

                dbSet.Add(newWorkMaster);

                // Copy phases with their details and bill of materials
                foreach (var sourcePhase in sourceWorkMaster.Phases)
                {
                    var newPhase = new WorkMasterPhase
                    {
                        Id = Guid.NewGuid(),
                        Code = sourcePhase.Code,
                        Description = sourcePhase.Description,
                        WorkMasterId = newWorkMaster.Id,
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow,
                        Disabled = sourcePhase.Disabled,
                        OperatorTypeId = sourcePhase.OperatorTypeId,
                        PreferredWorkcenterId = sourcePhase.PreferredWorkcenterId,
                        WorkcenterTypeId = sourcePhase.WorkcenterTypeId,
                        Comment = sourcePhase.Comment,
                        ExternalWorkCost = sourcePhase.ExternalWorkCost,
                        IsExternalWork = sourcePhase.IsExternalWork,
                        ServiceReferenceId = sourcePhase.ServiceReferenceId,
                        TransportCost = sourcePhase.TransportCost,
                        ProfitPercentage = sourcePhase.ProfitPercentage
                    };

                    context.Set<WorkMasterPhase>().Add(newPhase);

                    // Copy phase details
                    foreach (var sourceDetail in sourcePhase.Details)
                    {
                        var newDetail = new WorkMasterPhaseDetail
                        {
                            Id = Guid.NewGuid(),
                            WorkMasterPhaseId = newPhase.Id,
                            MachineStatusId = sourceDetail.MachineStatusId,
                            EstimatedTime = sourceDetail.EstimatedTime,
                            IsCycleTime = sourceDetail.IsCycleTime,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedOn = DateTime.UtcNow,
                            Disabled = sourceDetail.Disabled,
                            Comment = sourceDetail.Comment,
                            Order = sourceDetail.Order,
                            EstimatedOperatorTime = sourceDetail.EstimatedOperatorTime
                        };

                        context.Set<WorkMasterPhaseDetail>().Add(newDetail);
                    }

                    // Copy bill of materials
                    foreach (var sourceBom in sourcePhase.BillOfMaterials)
                    {
                        var newBom = new WorkMasterPhaseBillOfMaterials
                        {
                            Id = Guid.NewGuid(),
                            WorkMasterPhaseId = newPhase.Id,
                            ReferenceId = sourceBom.ReferenceId,
                            Quantity = sourceBom.Quantity,
                            Width = sourceBom.Width,
                            CreatedOn = DateTime.UtcNow,
                            UpdatedOn = DateTime.UtcNow,
                            Disabled = sourceBom.Disabled,
                            Diameter = sourceBom.Diameter,
                            Height = sourceBom.Height,
                            Length = sourceBom.Length,
                            Thickness = sourceBom.Thickness
                        };

                        context.Set<WorkMasterPhaseBillOfMaterials>().Add(newBom);
                    }
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<WorkMaster?> GetFullById(Guid id)
        {
            return await dbSet
                        .Include(d => d.Reference)
                        .Include("Phases.Details")
                        .Include("Phases.BillOfMaterials")
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}
