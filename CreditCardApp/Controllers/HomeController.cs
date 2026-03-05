using System.Diagnostics;
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using CreditCardApp.Models;

namespace CreditCardApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(CreditCard card)
        {
            if (string.IsNullOrWhiteSpace(card.CardNumber))
            {
                ViewBag.Result = "No card number provided.";
                return View();
            }

            // Strip spaces and dashes before any validation
            string cleanNumber = card.CardNumber.Replace(" ", "").Replace("-", "");

            if (!cleanNumber.All(char.IsDigit))
            {
                ViewBag.Result = "Credit card number is in invalid format.";
                return View();
            }

            string cardType = GetCardType(cleanNumber);

            if (cardType == "Unknown")
            {
                ViewBag.Result = "Unknown card type.";
                return View();
            }

            if (!IsValidLength(cleanNumber, cardType))
            {
                ViewBag.Result = "Credit card number has an inappropriate number of digits.";
                return View();
            }

            if (!LuhnCheck(cleanNumber))
            {
                ViewBag.Result = "Credit card number is invalid.";
                return View();
            }

            if (!ValidateCCV(card.CCV, cardType))
            {
                ViewBag.Result = "Invalid CCV.";
                return View();
            }

            if (!ValidateExpiration(card.ExpMonth, card.ExpYear))
            {
                ViewBag.Result = "Card has expired.";
                return View();
            }

            ViewBag.CardType = cardType;
            ViewBag.Result = $"Card is valid. Type: {cardType}";
            return View();
        }

        string GetCardType(string number)
        {
            // Amex check before Visa since both can start with non-4 digits
            if (number.Length >= 2)
            {
                int firstTwo = int.Parse(number.Substring(0, 2));

                if (firstTwo == 34 || firstTwo == 37)
                    return "Amex";

                if (firstTwo >= 51 && firstTwo <= 55)
                    return "MasterCard";
            }

            if (number.Length >= 6)
            {
                int firstSix = int.Parse(number.Substring(0, 6));
                if (firstSix >= 222100 && firstSix <= 272099)
                    return "MasterCard";
            }

            if (number.StartsWith("4"))
                return "Visa";

            return "Unknown";
        }

        bool IsValidLength(string number, string cardType)
        {
            return cardType switch
            {
                "Visa"       => number.Length == 16,
                "MasterCard" => number.Length == 16,
                "Amex"       => number.Length == 15,
                _            => false
            };
        }

        bool LuhnCheck(string number)
        {
            if (!number.All(char.IsDigit)) return false;

            int sum = 0;
            bool alternate = false;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int n = number[i] - '0';

                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        bool ValidateCCV(string ccv, string cardType)
        {
            if (string.IsNullOrWhiteSpace(ccv)) return false;
            if (!ccv.All(char.IsDigit))         return false;

            int expectedLength = cardType == "Amex" ? 4 : 3;
            return ccv.Length == expectedLength;
        }

        bool ValidateExpiration(int month, int year)
        {
            if (month < 1 || month > 12) return false;
            if (year < 1)                return false;

            // Pad 2-digit years (e.g. 25 → 2025)
            if (year < 100) year += 2000;

            DateTime expiry = new DateTime(year, month, 1)
                .AddMonths(1)
                .AddDays(-1);

            return expiry >= DateTime.Today;
        }
    }
}