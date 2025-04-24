# 🧩 MediatR CRUD Template

This is a `dotnet new` template that generates MediatR-based CRUD commands and queries following the CQRS pattern.

---

## 🚀 Tech Stack

- .NET 6/7/8+
- MediatR
- FluentValidation
- Clean Architecture-ready structure

---

## 📦 Installation

```bash
dotnet new install Milvasoft.Templates
```

---

## ⚙️ Usage

Run the following command to generate CRUD files for a given entity:

```bash
dotnet new mediatr-crud -n Product --projectName=MyApp --pluralName=Products -o src/YourApp.Application/Features/Products
```

### 🧠 Parameters

| Parameter         | Description                                              |
|-------------------|----------------------------------------------------------|
| `-n`              | Singular entity name (e.g., `Product`, `Category`)       |
| `--projectName`   | Root namespace of the application (e.g., `MyApp`)        |
| `--pluralName`    | Plural name used in namespaces (e.g., `Products`)        |

---

## 📁 Generated Files

- `Create{{name}}Command.cs`
- `Create{{name}}CommandHandler.cs`
- `Create{{name}}CommandValidator.cs`
- `Update{{name}}Command.cs`
- `Update{{name}}CommandHandler.cs`
- `Update{{name}}CommandValidator.cs`
- `Delete{{name}}Command.cs`
- `Delete{{name}}CommandHandler.cs`
- `Delete{{name}}CommandValidator.cs`
- `Get{{name}}ListQuery.cs`
- `Get{{name}}ListQueryHandler.cs`
- `Get{{name}}ListQueryValidator.cs`
- `Get{{name}}DetailQuery.cs`
- `Get{{name}}DetailQueryHandler.cs`
- `Get{{name}}DetailQueryValidator.cs`

Each file is placed under the appropriate folder and namespace:

```
{{projectName}}.Application.Features.{{pluralName}}.CreateEntity
```

---

## 🧹 Uninstall

```bash
dotnet new uninstall MediatrCrud
```

---

## 💡 Notes

- Ideal for Milvonion projects.