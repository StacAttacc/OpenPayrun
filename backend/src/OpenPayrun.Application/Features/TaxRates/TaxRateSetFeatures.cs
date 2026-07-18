using MediatR;
using Microsoft.EntityFrameworkCore;
using OpenPayrun.Application.Interfaces;
using OpenPayrun.Domain.Entities;
using OpenPayrun.Domain.Enums;

namespace OpenPayrun.Application.Features.TaxRates;

public record BracketDto(decimal? UpperBound, decimal Rate);

public record TaxRateSetResponse(
    int Id,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    decimal QppExemption,
    decimal QppYmpe,
    decimal QppYampe,
    decimal QppBaseRate,
    decimal QppAdditionalTier1Rate,
    decimal QppTier1Rate,
    decimal QppTier2Rate,
    decimal QppTier1MaxEmployee,
    decimal QppTier2MaxEmployee,
    decimal EiEmployeeRate,
    decimal EiEmployerMultiplier,
    decimal EiMaxInsurableEarnings,
    decimal EiMaxEmployeePremium,
    decimal QpipEmployeeRate,
    decimal QpipEmployerRate,
    decimal QpipMaxInsurableEarnings,
    decimal QpipMaxEmployeePremium,
    decimal QpipMaxEmployerPremium,
    decimal FederalBasicPersonalAmount,
    decimal FederalEmploymentAmount,
    decimal FederalLowestRate,
    decimal QuebecFederalAbatement,
    decimal QuebecBasicPersonalAmount,
    decimal QuebecWorkerDeductionMax,
    decimal QuebecWorkerDeductionRate,
    decimal QuebecLowestRate,
    decimal FssqSmallEmployerRate,
    decimal FssqLargeEmployerRate,
    BracketDto[] FederalBrackets,
    BracketDto[] QuebecBrackets
);

public record TaxRateSetBody(
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    decimal QppExemption,
    decimal QppYmpe,
    decimal QppYampe,
    decimal QppBaseRate,
    decimal QppAdditionalTier1Rate,
    decimal QppTier1Rate,
    decimal QppTier2Rate,
    decimal QppTier1MaxEmployee,
    decimal QppTier2MaxEmployee,
    decimal EiEmployeeRate,
    decimal EiEmployerMultiplier,
    decimal EiMaxInsurableEarnings,
    decimal EiMaxEmployeePremium,
    decimal QpipEmployeeRate,
    decimal QpipEmployerRate,
    decimal QpipMaxInsurableEarnings,
    decimal QpipMaxEmployeePremium,
    decimal QpipMaxEmployerPremium,
    decimal FederalBasicPersonalAmount,
    decimal FederalEmploymentAmount,
    decimal FederalLowestRate,
    decimal QuebecFederalAbatement,
    decimal QuebecBasicPersonalAmount,
    decimal QuebecWorkerDeductionMax,
    decimal QuebecWorkerDeductionRate,
    decimal QuebecLowestRate,
    decimal FssqSmallEmployerRate,
    decimal FssqLargeEmployerRate,
    BracketDto[] FederalBrackets,
    BracketDto[] QuebecBrackets
);

// GET /api/tax-rates
public record GetTaxRateSetsQuery : IRequest<TaxRateSetResponse[]>;

public class GetTaxRateSetsHandler(IAppDbContext db)
    : IRequestHandler<GetTaxRateSetsQuery, TaxRateSetResponse[]>
{
    public async Task<TaxRateSetResponse[]> Handle(GetTaxRateSetsQuery _, CancellationToken ct)
    {
        var sets = await db.TaxRateSets
            .Include(r => r.Brackets)
            .OrderBy(r => r.EffectiveFrom)
            .ToListAsync(ct);

        return sets.Select(Mapper.ToResponse).ToArray();
    }
}

// POST /api/tax-rates
public record CreateTaxRateSetCommand(TaxRateSetBody Body) : IRequest<TaxRateSetResponse>;

public class CreateTaxRateSetHandler(IAppDbContext db)
    : IRequestHandler<CreateTaxRateSetCommand, TaxRateSetResponse>
{
    public async Task<TaxRateSetResponse> Handle(CreateTaxRateSetCommand req, CancellationToken ct)
    {
        var entity = Mapper.ApplyBody(new TaxRateSet(), req.Body);
        db.TaxRateSets.Add(entity);
        await db.SaveChangesAsync(ct);
        return Mapper.ToResponse(entity);
    }
}

// PUT /api/tax-rates/{id}
public record UpdateTaxRateSetCommand(int Id, TaxRateSetBody Body) : IRequest<TaxRateSetResponse>;

public class UpdateTaxRateSetHandler(IAppDbContext db)
    : IRequestHandler<UpdateTaxRateSetCommand, TaxRateSetResponse>
{
    public async Task<TaxRateSetResponse> Handle(UpdateTaxRateSetCommand req, CancellationToken ct)
    {
        var entity = await db.TaxRateSets
            .Include(r => r.Brackets)
            .FirstOrDefaultAsync(r => r.Id == req.Id, ct)
            ?? throw new KeyNotFoundException($"TaxRateSet {req.Id} not found.");

        entity.Brackets.Clear();
        Mapper.ApplyBody(entity, req.Body);
        await db.SaveChangesAsync(ct);
        return Mapper.ToResponse(entity);
    }
}

// DELETE /api/tax-rates/{id}
public record DeleteTaxRateSetCommand(int Id) : IRequest;

public class DeleteTaxRateSetHandler(IAppDbContext db)
    : IRequestHandler<DeleteTaxRateSetCommand>
{
    public async Task Handle(DeleteTaxRateSetCommand req, CancellationToken ct)
    {
        var entity = await db.TaxRateSets
            .Include(r => r.Brackets)
            .FirstOrDefaultAsync(r => r.Id == req.Id, ct)
            ?? throw new KeyNotFoundException($"TaxRateSet {req.Id} not found.");

        db.TaxRateSets.Remove(entity);
        await db.SaveChangesAsync(ct);
    }
}

// Shared helpers
file static class Mapper
{
public static TaxRateSet ApplyBody(TaxRateSet e, TaxRateSetBody b)
{
    e.EffectiveFrom = b.EffectiveFrom;
    e.EffectiveTo = b.EffectiveTo;
    e.QppExemption = b.QppExemption;
    e.QppYmpe = b.QppYmpe;
    e.QppYampe = b.QppYampe;
    e.QppBaseRate = b.QppBaseRate;
    e.QppAdditionalTier1Rate = b.QppAdditionalTier1Rate;
    e.QppTier1Rate = b.QppTier1Rate;
    e.QppTier2Rate = b.QppTier2Rate;
    e.QppTier1MaxEmployee = b.QppTier1MaxEmployee;
    e.QppTier2MaxEmployee = b.QppTier2MaxEmployee;
    e.EiEmployeeRate = b.EiEmployeeRate;
    e.EiEmployerMultiplier = b.EiEmployerMultiplier;
    e.EiMaxInsurableEarnings = b.EiMaxInsurableEarnings;
    e.EiMaxEmployeePremium = b.EiMaxEmployeePremium;
    e.QpipEmployeeRate = b.QpipEmployeeRate;
    e.QpipEmployerRate = b.QpipEmployerRate;
    e.QpipMaxInsurableEarnings = b.QpipMaxInsurableEarnings;
    e.QpipMaxEmployeePremium = b.QpipMaxEmployeePremium;
    e.QpipMaxEmployerPremium = b.QpipMaxEmployerPremium;
    e.FederalBasicPersonalAmount = b.FederalBasicPersonalAmount;
    e.FederalEmploymentAmount = b.FederalEmploymentAmount;
    e.FederalLowestRate = b.FederalLowestRate;
    e.QuebecFederalAbatement = b.QuebecFederalAbatement;
    e.QuebecBasicPersonalAmount = b.QuebecBasicPersonalAmount;
    e.QuebecWorkerDeductionMax = b.QuebecWorkerDeductionMax;
    e.QuebecWorkerDeductionRate = b.QuebecWorkerDeductionRate;
    e.QuebecLowestRate = b.QuebecLowestRate;
    e.FssqSmallEmployerRate = b.FssqSmallEmployerRate;
    e.FssqLargeEmployerRate = b.FssqLargeEmployerRate;

    int sort = 1;
    foreach (var br in b.FederalBrackets)
        e.Brackets.Add(new TaxBracket { BracketType = BracketType.Federal, UpperBound = br.UpperBound, Rate = br.Rate, SortOrder = sort++ });
    sort = 1;
    foreach (var br in b.QuebecBrackets)
        e.Brackets.Add(new TaxBracket { BracketType = BracketType.Quebec, UpperBound = br.UpperBound, Rate = br.Rate, SortOrder = sort++ });

    return e;
}

public static TaxRateSetResponse ToResponse(TaxRateSet e) => new(
    e.Id, e.EffectiveFrom, e.EffectiveTo,
    e.QppExemption, e.QppYmpe, e.QppYampe,
    e.QppBaseRate, e.QppAdditionalTier1Rate, e.QppTier1Rate, e.QppTier2Rate,
    e.QppTier1MaxEmployee, e.QppTier2MaxEmployee,
    e.EiEmployeeRate, e.EiEmployerMultiplier, e.EiMaxInsurableEarnings, e.EiMaxEmployeePremium,
    e.QpipEmployeeRate, e.QpipEmployerRate, e.QpipMaxInsurableEarnings,
    e.QpipMaxEmployeePremium, e.QpipMaxEmployerPremium,
    e.FederalBasicPersonalAmount, e.FederalEmploymentAmount, e.FederalLowestRate, e.QuebecFederalAbatement,
    e.QuebecBasicPersonalAmount, e.QuebecWorkerDeductionMax, e.QuebecWorkerDeductionRate, e.QuebecLowestRate,
    e.FssqSmallEmployerRate, e.FssqLargeEmployerRate,
    e.Brackets.Where(b => b.BracketType == BracketType.Federal).OrderBy(b => b.SortOrder)
              .Select(b => new BracketDto(b.UpperBound, b.Rate)).ToArray(),
    e.Brackets.Where(b => b.BracketType == BracketType.Quebec).OrderBy(b => b.SortOrder)
              .Select(b => new BracketDto(b.UpperBound, b.Rate)).ToArray()
);
}
