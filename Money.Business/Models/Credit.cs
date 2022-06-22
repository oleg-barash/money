using System;

namespace Money.Business.Models
{
    public class Credit
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Sum { get; set; }
        public int Duration { get; set; }
        public decimal Interest { get; set; }
        public DateTime Date { get; set; }
        public decimal PaymentValue { get; set; }
    }
}
