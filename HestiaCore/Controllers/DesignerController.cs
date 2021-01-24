using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OptimaJet.Workflow;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace HestiaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DesignerController : ControllerBase
    {
        private readonly WorkflowRuntime workflowRuntime;

        public DesignerController(WorkflowRuntime workflowRuntime)
        {
            this.workflowRuntime = workflowRuntime;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var pars = new NameValueCollection();
            foreach (var q in Request.Query)
            {
                pars.Add(q.Key, q.Value.First());
            }

            var res = this.workflowRuntime.DesignerAPI(pars, out bool hasError, null, true);

            if (pars["operation"].ToLower() == "downloadscheme" && !hasError)
                return File(Encoding.UTF8.GetBytes(res), "text/xml");
            if (pars["operation"].ToLower() == "downloadschemebpmn" && !hasError)
                return File(Encoding.UTF8.GetBytes(res), "text/xml");

            return Content(res);
        }

        [HttpPost]
        public IActionResult Post()
        {
            Stream filestream = null;
            if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                filestream = Request.Form.Files[0].OpenReadStream();

            var pars = new NameValueCollection();
            foreach (var q in Request.Query)
            {
                pars.Add(q.Key, q.Value.First());
            }

            var parsKeys = pars.AllKeys;

            foreach (var key in Request.Form.Keys)
            {
                if (!parsKeys.Contains(key))
                {
                    pars.Add(key, Request.Form[key]);
                }
            }
            
            var res = this.workflowRuntime.DesignerAPI(pars, out bool hasError, filestream, true);

            if (pars["operation"].ToLower() == "downloadscheme" && !hasError)
                return File(Encoding.UTF8.GetBytes(res), "text/xml");
            if (pars["operation"].ToLower() == "downloadschemebpmn" && !hasError)
                return File(Encoding.UTF8.GetBytes(res), "text/xml");

            return Content(res);
        }

    }
}
