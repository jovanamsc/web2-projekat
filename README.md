# TravelPlanner

Aplikacija za planiranje putovanja. Microsoft Service Fabric backend, React frontend.

## Servisi

- **Gateway** (5100) — ulazna tacka, proksira sve zahtjeve
- **UserService** (5101) — auth, korisnici
- **TravelService** (5102) — planovi, destinacije, aktivnosti, ceklista, dijeljenje
- **BudgetService** (5103) — troskovi
- **ShareService** (5104) — share tokeni, stateful, cuva stanje u ReliableCollection

Svaki servis ima svoju bazu: `TravelPlanner_Users`, `TravelPlanner_Travel`, `TravelPlanner_Budget`.

## Pokretanje

### Preduslovi

- .NET 8 SDK
- Node.js 20+
- SQL Server Express
- Service Fabric SDK + Visual Studio 2022

### SQL Server

Namjestite staticni port 1433 u SQL Server Configuration Manager:

```
SQL Server Network Configuration → Protocols for SQLEXPRESS → TCP/IP
→ IP Addresses → IPAll → TCP Dynamic Ports: (ocistiti), TCP Port: 1433
```

Restartujte SQL Server servis.

### appsettings.Local.json

Svaki servis cita `appsettings.Local.json` koji nije u gitu. Kreirajte ih:

`UserService/appsettings.Local.json`:
```json
{
  "ConnectionStrings": {
    "UserDb": "Server=tcp:127.0.0.1,1433;Database=TravelPlanner_Users;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": { "Key": "TravelPlannerSecretKey2024VeryLongAndSecure!@#$%", "Issuer": "TravelPlanner", "Audience": "TravelPlannerApp" },
  "AdminSeed": { "Email": "admin@travelplanner.com", "Password": "Admin123!" }
}
```

`TravelService/appsettings.Local.json`:
```json
{
  "ConnectionStrings": {
    "TravelDb": "Server=tcp:127.0.0.1,1433;Database=TravelPlanner_Travel;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": { "Key": "TravelPlannerSecretKey2024VeryLongAndSecure!@#$%", "Issuer": "TravelPlanner", "Audience": "TravelPlannerApp" }
}
```

`BudgetService/appsettings.Local.json`:
```json
{
  "ConnectionStrings": {
    "BudgetDb": "Server=tcp:127.0.0.1,1433;Database=TravelPlanner_Budget;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": { "Key": "TravelPlannerSecretKey2024VeryLongAndSecure!@#$%", "Issuer": "TravelPlanner", "Audience": "TravelPlannerApp" }
}
```

`Gateway/appsettings.Local.json`:
```json
{
  "Jwt": { "Key": "TravelPlannerSecretKey2024VeryLongAndSecure!@#$%", "Issuer": "TravelPlanner", "Audience": "TravelPlannerApp" }
}
```

ShareService ne treba lokalni config.

### Backend

1. Pokrenite Service Fabric Local Cluster Manager iz System Tray → Start Local Cluster
2. Otvorite `backend/TravelPlanner.sln`, pritisnite F5

Migracije se primjenjuju automatski pri pokretanju. Admin nalog se kreira pri prvom pokretanju (`admin@travelplanner.com` / `Admin123!`).

### Frontend

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

http://localhost:5173
