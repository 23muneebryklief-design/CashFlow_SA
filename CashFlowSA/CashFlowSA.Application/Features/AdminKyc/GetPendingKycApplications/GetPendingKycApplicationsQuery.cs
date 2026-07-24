using MediatR;

namespace CashFlowSA.Application.Features.AdminKyc.GetPendingKycApplications
{
    public class GetPendingKycApplicationsQuery : IRequest<List<PendingKycApplicationDto>>
    {
    }
}
