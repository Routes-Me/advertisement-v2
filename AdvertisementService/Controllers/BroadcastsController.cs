using Microsoft.AspNetCore.Mvc;

namespace AdvertisementService.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/")]
    public class BroadcastsController : ControllerBase
    {

    }
}
