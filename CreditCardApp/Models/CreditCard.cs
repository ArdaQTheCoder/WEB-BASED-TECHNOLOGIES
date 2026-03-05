namespace CreditCardApp.Models
{
    public class CreditCard
    {
        public string Owner { get; set; }

        public string CardNumber { get; set; }

        public string CCV { get; set; }

        public int ExpMonth { get; set; }

        public int ExpYear { get; set; }

        public string CardType { get; set; }
    }
}