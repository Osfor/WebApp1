using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public partial class Currency
    {
        public Currency()
        {
            Rate = new HashSet<Rate>();
        }

        public string Id { get; set; }
        public string Iso { get; set; }
        public string Name { get; set; }
        public int Nominal { get; set; }

        public virtual ICollection<Rate> Rate { get; set; }
    }
}
