using System.Globalization;

namespace PrintMarket.Extensions
{
    public static class PriceExtensions
    {
        /// <summary>
        /// Fiyatı standart formata çevirir (Örn: 8.000 ₺ veya 12.500,50 $)
        /// </summary>
        /// <param name="price">Formatlanacak tutar</param>
        /// <param name="currency">Para birimi sembolü (Varsayılan: ₺)</param>
        /// <returns>Formatlanmış fiyat stringi</returns>
        public static string FormatPrice(this decimal price, string currency = "₺")
        {
            var culture = new CultureInfo("tr-TR");
            
            // Eğer küsurat yoksa (Tam sayı ise) ondalık kısmı gösterme (N0)
            // Küsurat varsa (örn 50 kuruş) standart 2 hane göster (N2)
            string formattedPrice = (price % 1 == 0) 
                ? price.ToString("N0", culture) 
                : price.ToString("N2", culture);

            return $"{formattedPrice} {currency}";
        }
    }
}
