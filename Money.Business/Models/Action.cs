using System;

namespace Money.Business.Models
{
    public enum ActionType
    {
        Incoming,
        Spending
    }

    public enum Category
    {
        NotSpecified,
        Necessary,
        Health,
        Bills,
        Planned,
        Salary,
        Gift
    }

    public class Action
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public ActionType Type { get; set; }
        public decimal Value { get; set; }
        public string Description { get; set; }
        public int? Credit { get; set; }
        public Category Category { get; set; }
    }
}
