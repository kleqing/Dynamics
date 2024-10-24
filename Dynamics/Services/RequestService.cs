using System.Linq.Expressions;
using AutoMapper;
using Dynamics.Models.Models;
using Dynamics.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Dynamics.Services;

public class RequestService : IRequestService
{
    private readonly IMapper _mapper;

    public RequestService(IMapper mapper)
    {
        _mapper = mapper;
    }
    public RequestOverviewDto MapRequestToRequestOverviewDto(Request request)
    {
        var requestDto = _mapper.Map<RequestOverviewDto>(request);
        // Get the first attachment:
        if (request.Attachment == null) return requestDto;
        var firstImg = request.Attachment.Split(",")[0];
        requestDto.FirstImageAttachment = firstImg;
        return requestDto;
    }
    
    public List<RequestOverviewDto> MapToListRequestOverviewDto(List<Request> requests)
    {
        var resultDtos = new List<RequestOverviewDto>();
        foreach (var request in requests)
        {
            var requestDto = _mapper.Map<RequestOverviewDto>(request);
            // Get the first attachment, if null we just skip those
            if (request.Attachment != null)
            {
                var firstImg = request.Attachment.Split(",")[0];
                requestDto.FirstImageAttachment = firstImg;
            }
            // Get only the first address (the city)
            var location = request.Location.Split(",");
            var city = location[0];
            if (location.Length == 4)
            {
                city = location[3];
            }
            requestDto.Location = city;
            resultDtos.Add(requestDto);
        }

        return resultDtos;
    }
}