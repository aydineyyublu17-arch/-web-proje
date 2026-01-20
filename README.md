# PrintMarket - Matbaa Makineleri Pazaryeri

## Proje Amacı
Bu proje, matbaa ve baskı sektöründeki işletmelerin ihtiyaç duyduğu sanayi tipi baskı makinelerinin (Ofset, Dijital, Serigrafi vb.) alım-satım süreçlerini dijitalleştirmeyi amaçlamaktadır. Geleneksel yöntemlerle (telefon, katalog vb.) yürütülen makine ticaretini modern, filtrelenebilir ve yönetilebilir bir e-ticaret altyapısına taşıyarak, doğru makinenin doğru alıcıyla buluşmasını sağlar.

## Hedef Kullanıcı Kitlesi
*   **Matbaa İşletmeleri:** Makine parkurunu yenilemek veya genişletmek isteyen KOBİ'ler ve büyük ölçekli matbaalar.
*   **Makine Tedarikçileri & Satıcılar:** İkinci el veya sıfır makine satışı yapan firmalar.
*   **Sektör Profesyonelleri:** Belirli teknik özelliklere sahip (renk sayısı, baskı hızı vb.) makine arayan teknik yöneticiler.

## Senaryo / Kullanım Amacı
PrintMarket, "Pazaryeri" (Marketplace) modeliyle çalışacak şekilde kurgulanmıştır.

1.  **Ürün Listeleme ve Arama:** Ziyaretçiler ana sayfada kategorilere, markalara, fiyata veya makine durumuna (Sıfır/2. El) göre detaylı arama yapabilir. Örneğin, "Heidelberg marka, 2020 model üzeri, 4 renkli ofset makinesi" gibi spesifik aramalar gerçekleştirilebilir.
2.  **Yönetim Paneli (Admin & Satıcı):** İşletme sahipleri sisteme giriş yaparak yeni makine ilanı oluşturabilir, stok durumunu güncelleyebilir ve gelen siparişleri yönetebilir.
3.  **Sipariş Süreci:** Alıcılar ilgilendikleri makineleri sepet mantığıyla sipariş edebilir.
4.  **Operasyonel Takip:** "Yönetim Kokpiti" (Dashboard) üzerinden toplam sipariş sayısı, bekleyen işlemler ve ürün istatistikleri görsel olarak takip edilebilir. Siparişler "Yumuşak Silme" (Soft Delete) özelliği ile güvenli bir şekilde arşivlenebilir veya iptal edilebilir.

## Kullanılan Teknolojiler

Bu proje, modern web geliştirme standartlarına uygun olarak **MVC (Model-View-Controller)** mimarisiyle geliştirilmiştir.

*   **Programlama Dili:** C#
*   **Framework:** ASP.NET Core MVC (NET 10.0 Preview / 8.0)
*   **Veritabanı:** SQL Server Express (Entity Framework Core Code-First Yaklaşımı)
*   **Frontend:** HTML5, CSS3, Bootstrap 5, JavaScript (jQuery & AJAX)
*   **Diğer Araçlar:** 
    *   **Entity Framework Core:** Veritabanı Migrations ve LINQ sorguları için.
    *   **SweetAlert2:** Kullanıcı bildirimleri ve onay pencereleri için.
    *   **FontAwesome:** İkon setleri için.

## Tanıtım Videosu

Projenin çalışır halini, MVC yapısını ve temel özelliklerini anlatan tanıtım videosuna aşağıdaki linkten ulaşabilirsiniz:

YouTube Video Linki
https://youtu.be/jeogtgN0E5A

---

### Kurulum (Development)

Projeyi yerel ortamda çalıştırmak için:

1.  Repository'yi klonlayın.
2.  `appsettings.json` içerisindeki Connection String'i kendi SQL Server instance'ınıza göre düzenleyin.
3.  Package Manager Console üzerinden `Update-Database` komutunu çalıştırarak veritabanını oluşturun.
4.  Projeyi derleyip çalıştırın.
