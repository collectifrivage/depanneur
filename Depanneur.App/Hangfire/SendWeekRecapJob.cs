using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Depanneur.App.Entities;
using Depanneur.App.Helpers;
using Depanneur.App.Models;
using FluentEmail.Core;
using Hangfire;
using Microsoft.Extensions.Configuration;

namespace Depanneur.App.Hangfire
{
    public class SendWeekRecapJob
    {
        private readonly DepanneurContext db;
        private readonly IBackgroundJobClient jobs;
        private readonly IConfiguration config;

        public SendWeekRecapJob(DepanneurContext db, IBackgroundJobClient jobs, IConfiguration config)
        {
            this.db = db;
            this.jobs = jobs;
            this.config = config;
        }

        public void Run()
        {
            var users = db.Users.Where(x => x.IsDeleted == false).Select(x => x.Id).ToList();

            foreach (var user in users)
            {
                jobs.Enqueue(() => Run(user));
            }
        }

        public void Run(string userId)
        {
            var user = db.Users.Find(userId);
            var recap = GetUserRecap(user);

            if (recap.RecapTotal > 0 || recap.PreviousTotal > 0)
            {
                SendRecapEmail(user, recap);
            }
        }

        private void SendRecapEmail(User user, UserRecap recap)
        {
            var email = Email
                .From("freud-noreply@sigmund.ca", "Freud")
                .To(user.Email, user.Name)
                .Subject($"[Dépanneur] Votre récapitulatif pour la semaine {recap.RecapWeek}")
                .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Emails/Recap.cshtml", recap);

            email.Send();
        }

        private UserRecap GetUserRecap(User user)
        {
            var todayLocal = TimeZoneInfo.ConvertTime(DateTime.UtcNow, DateExtensions.DefaultTimezone);

            var recap = new UserRecap {
                BaseUrl = config.GetValue<string>("Email:BaseUrl"),
                Name = user.Name,
                RecapWeek = new Week(todayLocal.AddDays(-7)),
                PreviousWeek = new Week(todayLocal.AddDays(-14)),
                CurrentBalance = user.Balance
            };

            recap.Purchases = GetPurchasesSummary(user, recap.RecapWeek, recap.PreviousWeek);
            return recap;
        }

        private List<SummaryItem> GetPurchasesSummary(User user, Week recapWeek, Week previousWeek)
        {
            var recapPurchases = GetPurchases(user, recapWeek);
            var previousPurchases = GetPurchases(user, previousWeek);
            var subscriptions = db.Subscriptions.Where(s => s.UserId == user.Id).Select(s => s.ProductId).ToList();

            var purchases = recapPurchases.ToDictionary(p => p.id, p => new SummaryItem {
                ProductName = p.name,
                RecapCount = p.qty,
                RecapTotal = p.cost,
                IsSubscribed = subscriptions.Contains(p.id)
            });

            foreach (var p in previousPurchases)
            {
                if (!purchases.ContainsKey(p.id)) {
                    purchases.Add(p.id, new SummaryItem {
                        ProductName = p.name,
                        IsSubscribed = subscriptions.Contains(p.id)
                    });
                }

                purchases[p.id].PreviousCount = p.qty;
                purchases[p.id].PreviousTotal = p.cost;
            }

            return purchases.Values.OrderBy(x => x.ProductName).ToList();
        }

        private IEnumerable<(int id, string name, int qty, decimal cost)> GetPurchases(User user, Week week)
        {
            var results = db.Transactions
                .OfType<Purchase>()
                .Where(p => p.User == user)
                .Where(p => p.Timestamp >= week.StartUtc && p.Timestamp < week.EndUtc)
                .GroupBy(p => p.ProductId)
                .Select(g => new {
                    ProductId = g.Key,
                    ProductName = g.Select(x => x.Product.Name).FirstOrDefault(),
                    Quantity = g.Sum(p => p.Quantity),
                    Cost = g.Sum(p => p.Amount)
                })
                .ToList();

            return results.Select(x => (x.ProductId, x.ProductName, x.Quantity, x.Cost));
        }
    }
}