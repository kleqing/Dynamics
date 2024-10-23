using System.Linq.Expressions;
using Dynamics.Models.Models;
using Dynamics.Models.Models.Dto;
using Dynamics.Models.Models.DTO;

namespace Dynamics.Services;

public interface IRequestService
{
    RequestOverviewDto MapRequestToRequestOverviewDto(Request request);
    List<RequestOverviewDto> MapToListRequestOverviewDto(List<Request> requests);
    /**
     * Apply modifier query to the current query and execute it.
     */
}