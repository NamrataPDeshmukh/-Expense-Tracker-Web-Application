﻿using Expense_Tracker_Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Threading.Tasks;

namespace Expense_Tracker_Application.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            //last 7 days transactions
            DateTime StartDate = DateTime.Today.AddDays(-6);
            DateTime EndDate = DateTime.Today;
            List<Transaction> SelectedTransactions = await _context.Transactions
                .Include(x=>x.Category)
                .Where(y => y.Date>= StartDate && y.Date<=EndDate)
                .ToListAsync();

            //Total Income
            int TotalIncome = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .Sum(j => j.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");


            //Total TotalExpense
            int TotalExpense = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .Sum(j => j.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");

            //Balance
            int Balance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-IN");
            culture.NumberFormat.CurrencyNegativePattern = 1;
            ViewBag.Balance = string.Format(culture, "{0:C0}", Balance);

            //Doughnut chart - Expense By Category
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon + " " + k.First().Category.Title,
                    amount = k.Sum(j => j.Amount),
                    formattedAmount = k.Sum(j => j.Amount).ToString("C0"),
                })
                .OrderByDescending(l=>l.amount)
                .ToList();

            //Spline chart- Income vs Expense
            //Income
            List<SplineCharData> IncomeSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Income")
                .GroupBy(j => j.Date)
                .Select(k => new SplineCharData()
                {
                    day = k.First().Date.ToString(),
                    
                    income = k.Sum(l => l.Amount)
                })
                .ToList();

            //Expense
            List<SplineCharData> ExpenseSummary = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(j => j.Date)
                .Select(k => new SplineCharData()
                {
                    day = k.First().Date.ToString(),
                    expense = k.Sum(l => l.Amount)
                })
                .ToList();

               // Combine Income & Expense
string[] Last7Days = Enumerable.Range(0, 7)
    .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
    .ToArray();
   ViewBag.SplineChartData = from day in Last7Days
                                                               
          join income in IncomeSummary on day equals income.day into dayIncomeJoined
          from income in dayIncomeJoined.DefaultIfEmpty()
           join expense in ExpenseSummary on day equals expense.day into expenseJoined
            from expense in expenseJoined.DefaultIfEmpty()
           select new
          {
            day = day,
          income = income == null ? 0 : income.income,
           expense = expense == null ? 0 : expense.expense,
              };

            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();

            return View();
        }
    }
    public class SplineCharData
    {
        public string day;
        public int income;
        public int expense;
    }
}
