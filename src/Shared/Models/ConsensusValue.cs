using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class ConsensusValue
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public double Value { get; set; }
        public bool IsConsensus { get; set; }
    }
}
