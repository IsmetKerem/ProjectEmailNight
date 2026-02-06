# 📧 EmailNight - Modern Email Management System

<div align="center">

<img width="1489" height="861" alt="1" src="https://github.com/user-attachments/assets/684cac05-f5cb-4ab9-b912-3a3def980a32" />

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)](https://docs.microsoft.com/aspnet/core)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Gemini AI](https://img.shields.io/badge/Gemini-AI-4285F4?style=for-the-badge&logo=google)](https://ai.google.dev/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-512BD4?style=for-the-badge&logo=dotnet)](https://docs.microsoft.com/aspnet/signalr)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

**Yapay zeka destekli, gerçek zamanlı bildirimler ve SMTP/IMAP entegrasyonu ile modern email yönetim uygulaması**

[Demo](#demo) • [Özellikler](#-özellikler) • [Kurulum](#-kurulum) • [Teknolojiler](#-teknolojiler) • [Ekran Görüntüleri](#-ekran-görüntüleri)

</div>

---

## 🎯 Proje Hakkında

EmailNight, ASP.NET Core 8 MVC kullanılarak geliştirilen modern bir email yönetim uygulamasıdır. Gmail benzeri kullanıcı deneyimi sunarken, **Gemini AI** entegrasyonu ile akıllı özellikler, **SignalR** ile gerçek zamanlı bildirimler ve **SMTP/IMAP** ile gerçek email gönderim/alma yeteneği barındırır.

### ✨ Öne Çıkan Özellikler

- 🤖 **AI Email Özetleme** - Uzun emailleri saniyeler içinde özetler
- 🏷️ **Akıllı Kategorileme** - AI ile otomatik email sınıflandırma
- 💬 **AI Yanıt Önerisi** - Farklı tonlarda otomatik yanıt oluşturma
- 🔔 **Gerçek Zamanlı Bildirimler** - SignalR ile anlık email bildirimleri
- ⏰ **Zamanlanmış Gönderim** - Email zamanlama ve Hangfire ile job yönetimi
- 📧 **SMTP/IMAP Entegrasyonu** - Gmail ile gerçek email gönderme/alma
- 📊 **Gelişmiş Dashboard** - Heatmap, grafikler ve AI öngörüleri
- 🔍 **Gelişmiş Arama** - Gönderen, konu, içerik ve kategoride arama
- 📎 **Dosya Ekleri** - Drag & drop ile dosya yükleme
- ⭐ **Yıldızlama** - Önemli emailleri işaretleme

---

## 🚀 Özellikler

### 📬 Email Yönetimi
| Özellik | Açıklama |
|---------|----------|
| Gelen Kutusu | Tüm gelen emailleri görüntüleme |
| Gönderilenler | Gönderilen emailleri takip |
| Yıldızlı | Önemli emailleri bir arada görme |
| Taslaklar | Yarım kalan emailleri kaydetme |
| Zamanlanmış | İleri tarihli email planlama |
| Çöp Kutusu | Silinen emailleri yönetme |
| Kategoriler | Birincil, Sosyal, Promosyon, İş, Spam |

### 🤖 AI Özellikleri (Gemini API)
- **Otomatik Özetleme**: Uzun emailleri 2 cümleye indirir
- **Akıllı Kategorileme**: Email içeriğine göre kategori belirler
- **Yanıt Önerisi**: 4 farklı tonda (Profesyonel, Samimi, Kısa, Resmi) yanıt oluşturur
- **AI Öngörüleri**: Dashboard'da email trafiği analizi ve öneriler

### 📊 Gelişmiş Dashboard
- **Email Trafiği Grafiği**: Son 7 günlük gelen/gönderilen trend
- **Kategori Dağılımı**: Doughnut chart ile görselleştirme
- **Aktivite Haritası**: 7x24 saatlik heatmap
- **Haftalık Karşılaştırma**: Bu hafta vs geçen hafta analizi
- **Performans Metrikleri**: Yanıt süresi, okunma oranı, yanıtlama oranı
- **AI Öngörüleri**: En yoğun saat, trend analizi

### 🔔 Gerçek Zamanlı Bildirimler (SignalR)
- Yeni email geldiğinde anlık toast notification
- Okunmamış email sayacı güncelleme
- Notification dropdown ile son emailler
- Kullanıcı online durumu takibi

### ⏰ Zamanlanmış Gönderim (Hangfire)
- İleri tarihli email zamanlama
- Hızlı seçenekler (Yarın Sabah, Yarın Öğlen, Gelecek Pazartesi)
- Zamanlanmış email iptal etme
- Background job dashboard

### 📧 SMTP/IMAP Entegrasyonu
- Gmail SMTP ile gerçek email gönderimi
- Gmail IMAP ile email alma
- Background service ile otomatik senkronizasyon
- Harici kullanıcı desteği

### 🔐 Güvenlik
- ASP.NET Core Identity ile kimlik doğrulama
- Role-based authorization (Admin, User)
- XSS koruması (HTML sanitization)
- CSRF token doğrulama
- Güvenli dosya yükleme

---

## 🛠️ Teknolojiler

### Backend
- **Framework**: ASP.NET Core 8 MVC
- **ORM**: Entity Framework Core 8
- **Veritabanı**: SQL Server 2022
- **Authentication**: ASP.NET Core Identity
- **AI**: Google Gemini API
- **Real-time**: SignalR
- **Job Scheduling**: Hangfire
- **Email**: MailKit (IMAP), System.Net.Mail (SMTP)

### Frontend
- **UI**: Custom CSS (Cyan/Dark Theme)
- **Icons**: Font Awesome 6
- **Charts**: Chart.js
- **Rich Text Editor**: Quill.js
- **Notifications**: Custom Toast System

### Araçlar
- Visual Studio 2022 / JetBrains Rider
- SQL Server Management Studio / DataGrip
- Git & GitHub

---

## 📦 Kurulum

### Gereksinimler
- .NET 8 SDK
- SQL Server 2019+
- Gemini API Key (opsiyonel, AI özellikleri için)
- Gmail App Password (opsiyonel, SMTP/IMAP için)

### Adımlar

1. **Repoyu klonla**
```bash
git clone https://github.com/IsmetKerem/ProjectEmailNight.git
cd ProjectEmailNight
```

2. **Veritabanı ve API ayarlarını yapılandır**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmailNightDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "GeminiSettings": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  },
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "EnableSsl": true,
    "UserName": "your.email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your.email@gmail.com",
    "FromName": "EmailNight"
  },
  "ImapSettings": {
    "Host": "imap.gmail.com",
    "Port": 993,
    "EnableSsl": true,
    "UserName": "your.email@gmail.com",
    "Password": "your-app-password"
  }
}
```

3. **Migration'ları uygula**
```bash
dotnet ef database update
```

4. **Uygulamayı çalıştır**
```bash
dotnet run
```

5. **Tarayıcıda aç**
```
https://localhost:5001
```

---

## 📸 Ekran Görüntüleri

### Dashboard
<img width="1489" height="861" alt="1" src="https://github.com/user-attachments/assets/684cac05-f5cb-4ab9-b912-3a3def980a32" />
<img width="1489" height="861" alt="2" src="https://github.com/user-attachments/assets/caaa967c-72f5-4915-a9c2-468ddbd58e55" />


### Gelen Kutusu
<img width="1489" height="861" alt="3" src="https://github.com/user-attachments/assets/2091fff3-72c4-48aa-805b-a6009369f6c3" />


### Email Detay & AI Özet
<img width="1489" height="861" alt="4" src="https://github.com/user-attachments/assets/4ea8e868-876f-4698-920d-b8d159e51609" />


### Email Yazma & Zamanlama
<img width="1489" height="861" alt="5" src="https://github.com/user-attachments/assets/234f8819-80c8-4876-906e-50d55b86c81c" />


### Profil
<img width="1489" height="861" alt="6" src="https://github.com/user-attachments/assets/3147af8f-4129-4e1f-b6cc-c770597c59dc" />


---

## 📁 Proje Yapısı
```
EmailNight/
├── Controllers/
│   ├── AccountController.cs    # Giriş/Kayıt
│   ├── AdminController.cs      # Admin paneli
│   ├── DashboardController.cs  # Ana panel & istatistikler
│   ├── EmailController.cs      # Email CRUD & SMTP
│   └── ProfileController.cs    # Profil yönetimi
├── Entities/
│   ├── AppUser.cs              # Kullanıcı entity
│   ├── Email.cs                # Email entity
│   ├── EmailCategory.cs        # Kategori entity
│   └── EmailAttachment.cs      # Ek dosya entity
├── Hubs/
│   └── NotificationHub.cs      # SignalR hub
├── Models/
│   ├── DashboardViewModel.cs   # Dashboard verileri
│   ├── EmailListViewModel.cs
│   ├── EmailDetailViewModel.cs
│   ├── ComposeViewModel.cs
│   ├── SmtpSettings.cs         # SMTP config
│   └── ImapSettings.cs         # IMAP config
├── Services/
│   ├── IEmailService.cs
│   ├── EmailService.cs
│   ├── IAIService.cs
│   ├── GeminiAIService.cs
│   ├── ISmtpEmailService.cs    # SMTP gönderim
│   ├── SmtpEmailService.cs
│   ├── IImapEmailService.cs    # IMAP alma
│   ├── ImapEmailService.cs
│   ├── EmailSyncService.cs     # Background sync
│   ├── IScheduledEmailService.cs
│   ├── ScheduledEmailService.cs
│   └── NotificationService.cs  # SignalR notifications
├── Views/
│   ├── Account/
│   ├── Admin/
│   ├── Dashboard/
│   ├── Email/
│   ├── Profile/
│   └── Shared/
└── wwwroot/
    └── emailthema/
        ├── css/style.css       # Cyan/Dark theme
        └── js/script.js
```

---

## 🆕 v2.0 Yeni Özellikler

- ✅ 🔔 **SignalR ile gerçek zamanlı bildirimler**
- ✅ ⏰ **Hangfire ile zamanlanmış email gönderimi**
- ✅ 📧 **Gmail SMTP/IMAP entegrasyonu**
- ✅ 📊 **Gelişmiş dashboard (heatmap, performans metrikleri)**
- ✅ 🤖 **AI öngörüleri ve trend analizi**
- ✅ 🎨 **Yeni Cyan/Mavi tema**
- ✅ 👥 **Admin paneli ve kullanıcı yönetimi**
- ✅ 🗑️ **Çöp kutusu ve toplu işlemler**

---

## 🔮 Gelecek Özellikler

- [ ] 📱 Mobil uygulama (.NET MAUI)
- [ ] 🏷️ Özel kullanıcı etiketleri
- [ ] 🔒 End-to-end encryption
- [ ] 🌍 Çoklu dil desteği (i18n)
- [ ] 📅 Takvim entegrasyonu
- [ ] 🔗 CRM entegrasyonu

---

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/AmazingFeature`)
3. Commit yapın (`git commit -m 'Add some AmazingFeature'`)
4. Push yapın (`git push origin feature/AmazingFeature`)
5. Pull Request açın

---

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

## 👨‍💻 Geliştirici

<div align="center">

**İsmet Kerem Eren**

[![LinkedIn](https://img.shields.io/badge/LinkedIn-0077B5?style=for-the-badge&logo=linkedin&logoColor=white)](https://linkedin.com/in/ismetkeremeren)
[![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/IsmetKerem)
[![Email](https://img.shields.io/badge/Email-D14836?style=for-the-badge&logo=gmail&logoColor=white)](mailto:1ismetkerem@gmail.com)

</div>

---

<div align="center">

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!

Made with ❤️ and ☕

</div>
```
