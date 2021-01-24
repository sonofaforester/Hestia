using HestiaCore.Models;
using HestiaDatabase;
using HestiaDatabase.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OptimaJet.Workflow.Core.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace HestiaCore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : HestiaControllerBase
    {
        private readonly HestiaCoreContext context;
        private readonly WorkflowRuntime runtime;

        public TicketsController(HestiaCoreContext context, WorkflowRuntime runtime)
        {
            this.context = context;
            this.runtime = runtime;
        }

        // GET: api/<TicketsController>
        [HttpGet]
        public async Task<IEnumerable<Ticket>> Get()
        {
            return await context.Tickets.Select(t => new Ticket() {
                Id = t.Id,
                NextStateName = t.NextStateName,
                NextState = t.NextState,
                Assignee = t.Assignee,
                Date = t.Date
            }).ToListAsync();
        }

        // GET api/<TicketsController>/5
        [HttpGet("{id}")]
        public async Task<Ticket> Get(Guid id)
        {
            var t = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            return new Ticket()
            {
                Id = t.Id,
                NextStateName = t.NextStateName,
                NextState = t.NextState,
                Assignee = t.Assignee,
                Date = t.Date
            };
        }

        // POST api/<TicketsController>
        [HttpPost]
        public async Task Post([FromBody] Ticket ticket)
        {
            var ticketModel = new TicketModel()
            {
                Id = ticket.Id,
                NextStateName = ticket.NextStateName,
                NextState = ticket.NextState,
                Assignee = ticket.Assignee,
                Date = ticket.Date
            };
            await context.Tickets.AddAsync(ticketModel);

            await context.SaveChangesAsync();

            if (runtime.IsProcessExists(ticketModel.Id))
                return;

            runtime.CreateInstance("SimpleWF", ticketModel.Id);
        }

        // PUT api/<TicketsController>/5
        [HttpPut("{id}")]
        public async Task Put(Guid id, [FromBody] Ticket value)
        {
            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            ticket.Assignee = value.Assignee;

            await context.SaveChangesAsync();
        }

        // DELETE api/<TicketsController>/5
        [HttpDelete("{id}")]
        public async Task Delete(Guid id)
        {
            var ticket = await context.Tickets.FirstOrDefaultAsync(t => t.Id == id);

            if (ticket != null)
                context.Tickets.Remove(ticket);

            await context.SaveChangesAsync();
        }
    }
}
