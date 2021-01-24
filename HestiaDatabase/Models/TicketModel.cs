using System;

namespace HestiaDatabase.Models
{
    public class TicketModel
    {
        public Guid Id { get; set; }
        public string Assignee { get; set; }
        public DateTime Date { get; set; }
        public string NextState { get; set; }
        public string NextStateName { get; set; }
    }
}
