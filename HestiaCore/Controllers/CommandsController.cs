using HestiaCore.Models;
using HestiaDatabase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HestiaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommandsController : ControllerBase
    {
        private readonly HestiaCoreContext context;
        private readonly WorkflowRuntime runtime;

        public CommandsController(HestiaCoreContext context, WorkflowRuntime runtime)
        {
            this.context = context;
            this.runtime = runtime;
        }


        [HttpGet("{id}")]
        public async Task<IEnumerable<Command>> Get(Guid id)
        {
            var commands = await runtime.GetAvailableCommandsAsync(id, "mark");

            return commands.Select(c => new Command() { Name = c.CommandName, TicketId = c.ProcessId });
        }
    }
}
