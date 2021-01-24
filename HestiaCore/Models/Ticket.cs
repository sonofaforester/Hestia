using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HestiaCore.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public string Assignee { get; set; }
        public DateTime Date { get; set; }
        public string NextState { get; set; }
        public string NextStateName { get; set; }
    }
}
