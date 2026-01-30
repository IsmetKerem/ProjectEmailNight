# 📧 EmailNight - Modern Email Management System

<div align="center">

![EmailNight Banner](https://via.placeholder.com/800x400/0f172a/7c3aed?text=EmailNight)

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=for-the-badge&logo=dotnet)](https://docs.microsoft.com/aspnet/core)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![Gemini AI](https://img.shields.io/badge/Gemini-AI-4285F4?style=for-the-badge&logo=google)](https://ai.google.dev/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)

**Yapay zeka destekli, modern ve şık bir email yönetim uygulaması**

[Demo](#demo) • [Özellikler](#-özellikler) • [Kurulum](#-kurulum) • [Teknolojiler](#-teknolojiler) • [Ekran Görüntüleri](#-ekran-görüntüleri)

</div>

---

## 🎯 Proje Hakkında

EmailNight, ASP.NET Core 8 MVC kullanılarak geliştirilen modern bir email yönetim uygulamasıdır. Gmail benzeri kullanıcı deneyimi sunarken, **Gemini AI** entegrasyonu ile akıllı özellikler barındırır.

### ✨ Öne Çıkan Özellikler

- 🤖 **AI Email Özetleme** - Uzun emailleri saniyeler içinde özetler
- 🏷️ **Akıllı Kategorileme** - AI ile otomatik email sınıflandırma
- 💬 **AI Yanıt Önerisi** - Farklı tonlarda otomatik yanıt oluşturma
- 🔍 **Gelişmiş Arama** - Gönderen, konu, içerik ve kategoride arama
- 📎 **Dosya Ekleri** - Drag & drop ile dosya yükleme (Max 25MB)
- ⭐ **Yıldızlama** - Önemli emailleri işaretleme
- 📊 **Dashboard** - Anlık istatistikler ve grafikler

---

## 🚀 Özellikler

### 📬 Email Yönetimi
| Özellik | Açıklama |
|---------|----------|
| Gelen Kutusu | Tüm gelen emailleri görüntüleme |
| Gönderilenler | Gönderilen emailleri takip |
| Yıldızlı | Önemli emailleri bir arada görme |
| Taslaklar | Yarım kalan emailleri kaydetme |
| Kategoriler | Birincil, Sosyal, Promosyon, İş |

### 🤖 AI Özellikleri (Gemini API)
- **Otomatik Özetleme**: Uzun emailleri 2 cümleye indirir
- **Akıllı Kategorileme**: Email içeriğine göre kategori belirler
- **Yanıt Önerisi**: 4 farklı tonda (Profesyonel, Samimi, Kısa, Resmi) yanıt oluşturur

### 🔐 Güvenlik
- ASP.NET Core Identity ile kimlik doğrulama
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

### Frontend
- **UI**: Custom CSS (Dark Theme)
- **Icons**: Font Awesome 6
- **Charts**: Chart.js
- **Rich Text Editor**: Quill.js

### Araçlar
- Visual Studio 2022 / JetBrains Rider
- SQL Server Management Studio
- Git & GitHub

---

## 📦 Kurulum

### Gereksinimler
- .NET 8 SDK
- SQL Server 2019+
- Gemini API Key (opsiyonel, AI özellikleri için)

### Adımlar

1. **Repoyu klonla**
```bash
git clone https://github.com/kullaniciadi/EmailNight.git
cd EmailNight
```

2. **Veritabanı bağlantısını ayarla**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmailNightDb;Trusted_Connection=true;TrustServerCertificate=true;"
  },
  "GeminiSettings": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
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
<img width="1510" height="859" alt="Screenshot 2026-01-29 at 17 42 30" src="https://github.com/user-attachments/assets/873f4cf0-37ce-470f-b5a1-a8af6c0ede39" />


### Gelen Kutusu
<img width="1510" height="859" alt="Screenshot 2026-01-29 at 17 42 39" src="https://github.com/user-attachments/assets/4d12e2da-ded9-4169-b294-f6212297888e" />


### Email Detay & AI Özet
<img width="1510" height="859" alt="Screenshot 2026-01-29 at 17 44 21" src="https://github.com/user-attachments/assets/cfbef3bf-dd82-472b-addd-a4d9d4041183" />


### Email Yazma (Quill Editor)
<img width="1510" height="859" alt="Screenshot 2026-01-29 at 17 49 46" src="https://github.com/user-attachments/assets/dbce77cd-9838-43ab-bfda-0c8ceecca330" />


### Profil
<img width="1510" height="859" alt="Screenshot 2026-01-29 at 17 44 29" src="https://github.com/user-attachments/assets/0a102992-ed4a-44c9-8842-b748c6aa7591" />



---

## 📁 Proje Yapısı
```
EmailNight/
├── Controllers/
│   ├── AccountController.cs    # Giriş/Kayıt
│   ├── DashboardController.cs  # Ana panel
│   ├── EmailController.cs      # Email CRUD
│   └── ProfileController.cs    # Profil yönetimi
├── Entities/
│   ├── AppUser.cs              # Kullanıcı entity
│   ├── Email.cs                # Email entity
│   ├── EmailCategory.cs        # Kategori entity
│   └── EmailAttachment.cs      # Ek dosya entity
├── Models/
│   ├── EmailListViewModel.cs
│   ├── EmailDetailViewModel.cs
│   ├── ComposeViewModel.cs
│   └── ProfileViewModel.cs
├── Services/
│   ├── IEmailService.cs
│   ├── EmailService.cs
│   ├── IAIService.cs
│   └── GeminiAIService.cs
├── Views/
│   ├── Account/
│   ├── Dashboard/
│   ├── Email/
│   ├── Profile/
│   └── Shared/
└── wwwroot/
    └── emailthema/
        ├── css/style.css
        └── js/script.js
```

---

## 🔮 Gelecek Özellikler

- [ ] 📱 Mobil uygulama (MAUI)
- [ ] 🔔 Gerçek zamanlı bildirimler (SignalR)
- [ ] 📅 Zamanlanmış gönderim
- [ ] 🏷️ Özel etiketler
- [ ] 📧 SMTP/IMAP entegrasyonu
- [ ] 🌍 Çoklu dil desteği

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
[![GitHub](https://img.shields.io/badge/GitHub-100000?style=for-the-badge&logo=github&logoColor=white)](https://github.com/ismetkeremeren)
[![Email](https://img.shields.io/badge/Email-D14836?style=for-the-badge&logo=gmail&logoColor=white)](mailto:ismetkeremeren@gmail.com)

</div>

---

<div align="center">

⭐ Bu projeyi beğendiyseniz yıldız vermeyi unutmayın!

Made with ❤️ and ☕

</div>
