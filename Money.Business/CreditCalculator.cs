using System;
using System.Collections.Generic;
using System.Linq;
using Money.Business.Models;
using Action = Money.Business.Models.Action;


namespace Money.Business
{
    public class CreditCalculator
    {
        private readonly DataContext _context;
        public CreditCalculator(DataContext context)
        {
            _context = context;
        }
        public IEnumerable<Action> GetPaymentsForCredit(Credit credit)
        {
            var currentActions = _context.GetActions().Where(x => x.Credit == credit.Id && x.Date <= DateTime.Now).ToList();
            var sum = GetCreditSum(credit, currentActions);
            var date = currentActions.Any() ? currentActions.Max(x => x.Date) : credit.Date;
            var dailyPercent = credit.Interest / 365;
            var payments = _context.GetPayments().OrderBy(x => x.Date).Where(x => x.Credit == credit.Id && x.Date > DateTime.Now);
            foreach (var payment in payments)
            {
                var days = (payment.Date - date).TotalDays;
                var action = new Action
                {
                    Date = payment.Date,
                    Description = $"Платёж по кредиту '{credit.Description}' за {payment.Date.Month}",
                    Type = ActionType.Spending,
                    Value = credit.PaymentValue > sum ? sum : credit.PaymentValue
                };
                yield return action;
                decimal percents = Math.Round(sum * ((decimal)days * dailyPercent / 100), 2);
                sum -= (credit.PaymentValue - percents);
                if (sum < 0) break;
                date = payment.Date;

            }
        }

        private decimal GetCreditSum(Credit credit, List<Action> actions)
        {
            var sum = credit.Sum;
            var date = credit.Date;
            var dailyPercent = credit.Interest / 365;
            foreach (var action in actions)
            {
                var days = (action.Date - date).TotalDays;
                decimal percents = Math.Round(sum * ((decimal)days * dailyPercent / 100), 2);
                sum -= action.Value - percents;
            }
            return sum;
        }

    }
}
