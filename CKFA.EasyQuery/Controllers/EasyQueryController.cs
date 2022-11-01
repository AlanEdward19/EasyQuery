using CKFA.Domain.Entities;
using CKFA.Domain.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CKFA.Presentation.Controllers
{
    [Route("getEasyQuery")]
    [ApiController]
    public class EasyQueryController : ControllerBase
    {
        private readonly IEasyQueryRepository _easyQueryRepository;

        public EasyQueryController(IEasyQueryRepository easyQueryRepository)
        {
            _easyQueryRepository = easyQueryRepository;
        }

        [HttpGet]
        public async Task<IActionResult> EasyQuery([FromQuery] Input input)
        {
            var query = await _easyQueryRepository.EasyQuery(input.DatabaseLanguage, input.Fields, input.FieldsValues,input.DatabaseName,
                input.TableName);

            return Ok(query);
        }
    }
}
