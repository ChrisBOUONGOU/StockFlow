# StockFlow

**StockFlow** is a desktop inventory management application designed for small businesses, retailers and independent operators. Built with **C#**, **.NET 10**, **Avalonia UI** and **SQLite**, it helps users track products and stock movements on Windows, Linux and macOS (subject to Avalonia platform support).

> Interface languages: French and English.

## Features

- **Dashboard**: product count, available units, estimated inventory value and low-stock alerts.
- **Product catalogue**: add, update, search and delete products.
- **Product details**: name, SKU, category, quantity, minimum stock level and unit price.
- **Stock in**: record deliveries and quantity increases.
- **Stock out**: record sales and usage; an operation that exceeds available stock is rejected.
- **Low-stock alerts**: identify products whose quantity is at or below their configured minimum.
- **CSV report**: export the product catalogue for Excel or another compatible spreadsheet.
- **Local storage**: application data is persisted in a SQLite database on the computer.

## Technology stack

| Technology | Purpose |
|---|---|
| C# / .NET 10 | Language and platform |
| Avalonia UI 11 | Cross-platform desktop UI |
| SQLite / Microsoft.Data.Sqlite | Local data storage |
| XAML | Window layout |

## Requirements

- .NET 10 SDK
- An environment supported by Avalonia UI
- Git (to clone the repository)

## Getting started

1. Clone the repository:

   ```bash
   git clone https://github.com/ChrisBOUONGOU/StockFlow.git
   cd StockFlow
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Run the application:

   ```bash
   dotnet run
   ```

To publish a Release build:

```bash
dotnet publish -c Release
```

## Local data

The `stockflow.db` database is created automatically on first launch in the current user's application data directory:

- **Windows**: `%LOCALAPPDATA%\\StockFlow\\stockflow.db`
- **Linux**: `~/.local/share/StockFlow/stockflow.db` (depending on the .NET environment)
- **macOS**: the user's Application Support directory (depending on the .NET environment)

Back up this file regularly to preserve your data.

## Quick usage guide

1. Open **Products** and create a product with a unique SKU.
2. Set its initial quantity, minimum stock level and unit price.
3. Use **Movements** to record stock in and stock out.
4. Check the **Dashboard** to monitor low-stock products.
5. Export the catalogue as CSV from **Reports**.

## Project structure

```text
StockFlow/
├── App.axaml
├── App.axaml.cs
├── MainWindow.axaml
├── MainWindow.axaml.cs
├── Program.cs
├── StockFlow.csproj
├── StockFlow.slnx
├── README.md
└── README.en.md
```

## Current scope

This release provides a first functional foundation: product catalogue management, stock adjustments, low-stock alerts and CSV export. Future versions may add a searchable movement history, supplier and customer management, multi-user authentication, automated backups, printable reports and automated tests.

## Contributing

Suggestions and contributions are welcome. Open an *issue* to report a bug or suggest an improvement.

## License

No open-source license is currently specified in this repository. Please contact the author before reusing or redistributing this project.

---

Developed by **ChrisBOUONGOU** · [GitHub repository](https://github.com/ChrisBOUONGOU/StockFlow)
