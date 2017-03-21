using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeAnalyze.Business;
using Microsoft.AspNetCore.Mvc;

namespace CodeAnalyze.Controllers
{
    [Route("api/[controller]")]
    public class HookController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]dynamic payload)
        {
            var cancelationToken = HttpContext.RequestAborted;

            if (payload.action != "opened")
            {
                return Ok();
            }
            
            List<RemoteFile> files = await RemoteFileReader.ReadAll(payload, cancelationToken);
            var errors = new List<ErrorMessage>();
            foreach (var file in files)
            {
                if (cancelationToken.IsCancellationRequested)
                {
                    return StatusCode(408);
                }
                errors.AddRange(await CodeValidator.ValidateFile(file, cancelationToken));
            }

            await ReviewSender.Send(errors, payload.pull_request.url.ToString(), cancelationToken);
            return Ok();
        }
    }
}
