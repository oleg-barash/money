using System;

namespace Money.Business.Models
{
    public class Credit
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal Summ { get; set; }
        public int Duration { get; set; }
        public double Interest { get; set; }
        public DateTime Date { get; set; }
        public decimal PaymentValue { get; set; }
    }
}
