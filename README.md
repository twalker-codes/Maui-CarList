# 🚗 CarListApp - Cross-Platform Car Management Application

A demo .NET MAUI application showcasing cross-platform development for iOS, Android, macOS, and Windows using C# and Visual Studio. This project highlights key concepts such as MVVM, data binding, dependency injection, API integration, authentication, and troubleshooting techniques to build modern mobile and desktop applications.

---

## 🚀 Features

• **Cross-Platform Support**
  - iOS, Android, macOS, and Windows compatibility
  - Responsive UI design
  - Native platform integration

• **Authentication & Authorization**
  - JWT-based authentication
  - Role-based access control
  - Secure token storage

• **Car Management**
  - View car listings
  - Add, edit, and delete cars
  - Search and filter capabilities

• **Modern Architecture**
  - MVVM design pattern
  - Dependency injection
  - Secure data storage
  - RESTful API integration

---

## 🛠️ Tech Stack

### Backend
• ASP.NET Core 8.0
• Entity Framework Core
• SQLite Database
• JWT Authentication
• Serilog Logging

### Frontend
• .NET MAUI
• XAML UI
• Community Toolkit MVVM
• SQLite local storage
• Serilog logging

---

## ⚙️ Setup & Installation

### Prerequisites
• .NET 8.0 SDK
• Visual Studio 2022 or later (with .NET MAUI workload)
• SQLite

### API Setup

## 📡 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET    | /cars    | Get all cars |
| GET    | /cars/{id} | Get car by ID |
| POST   | /cars    | Create new car |
| PUT    | /cars/{id} | Update car |
| DELETE | /cars/{id} | Delete car |
| POST   | /login   | Authenticate user |

## 📦 Key NuGet Packages

### API Packages
| Package | Version | Description |
|---------|---------|-------------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.13 | JWT authentication |
| Microsoft.EntityFrameworkCore.Sqlite | 9.0.2 | SQLite database provider |
| Serilog.AspNetCore | 9.0.0 | Structured logging |

### MAUI Packages
| Package | Version | Description |
|---------|---------|-------------|
| CommunityToolkit.Mvvm | 8.4.0 | MVVM architecture support |
| CommunityToolkit.Maui | 7.0.1 | MAUI UI components |
| sqlite-net-pcl | 1.9.172 | SQLite local storage |

## 🔐 Authentication

The application uses JWT-based authentication with two default users:

• **Administrator**
  - Username: admin@localhost.com
  - Password: P@ssword1

• **Regular User**
  - Username: user@localhost.com
  - Password: P@ssword1

## 🎨 Theming

The application supports:
• Light/Dark mode switching
• Custom primary color selection
• Theme persistence per user
• Dynamic theme updates

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📜 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙏 Acknowledgments

• Microsoft .NET MAUI Team
• Community Toolkit Contributors
• Font Awesome for icons
