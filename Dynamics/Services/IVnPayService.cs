using Dynamics.Models.Dto;

namespace Dynamics.Services;

public interface IVnPayService
{
    string CreatePaymentUrl(HttpContext context, VnPayCreatePaymentDto model);
    VnPayResponseDto ExtractPaymentResult(IQueryCollection collection);
    /**
     * Init the pay request with some information <br />
     * Make sure the pay request dto has: <br />
     * FromId, ResourceId, TargetId, target type <br />
     * Amount, message?
     */
}