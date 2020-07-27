using System;
using System.Collections.Generic;

namespace WebApp.Models
{
    public partial class Rate
    {
        public string CurrencyId { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public virtual Currency Currency { get; set; }
    }
}
