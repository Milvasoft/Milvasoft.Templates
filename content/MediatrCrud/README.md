# 🧩 MediatR CRUD Template

This is a `dotnet new` template that generates MediatR-based CRUD commands and queries following the CQRS pattern.

---

## 🚀 Tech Stack

- .NET 10
- MediatR
- FluentValidation
- Milvonion (Clean Architecture) solution layout

---

## 📦 Installation

```bash
dotnet new install Milvasoft.Templates
```

---

## ⚙️ Usage

Run the command from the solution root (the folder of `{{projectName}}.slnx`); the files land in the solution's own folders:

```bash
dotnet new mediatr-crud -n Product --projectName MyApp --pluralName Products --entityKeyType int
```

### 🧠 Parameters

| Parameter         | Description                                              |
|-------------------|----------------------------------------------------------|
| `-n`              | Singular entity name (e.g., `Product`, `Category`)       |
| `--projectName`   | Root namespace of the application (e.g., `MyApp`)        |
| `--pluralName`    | Plural name used in namespaces (e.g., `Products`)        |
| `--entityKeyType` | Key type of the entity (`int`, `long`, `Guid`)           |

---

## 📁 Generated Files

```
src/{{projectName}}.Api/Controllers/{{pluralName}}Controller.cs
src/{{projectName}}.Application/Dtos/{{name}}Dtos/{{name}}DetailDto.cs, {{name}}ListDto.cs
src/{{projectName}}.Application/Features/{{pluralName}}/Create{{name}}/Create{{name}}Command.cs, ...Handler.cs, ...Validator.cs
src/{{projectName}}.Application/Features/{{pluralName}}/Update{{name}}/...
src/{{projectName}}.Application/Features/{{pluralName}}/Delete{{name}}/...
src/{{projectName}}.Application/Features/{{pluralName}}/Get{{name}}Detail/...
src/{{projectName}}.Application/Features/{{pluralName}}/Get{{name}}List/...
```

Every namespace matches its folder, so the architecture tests of the solution pass once the steps below are done.

---

## ✅ Before the first build

The generated code uses these, add them once per entity (the names below are for `-n Product --pluralName Products`):

- Entity: `src/MyApp.Domain/Product.cs` (`[Table(TableNames.Products)]`, e.g. `FullAuditableEntity<int>` with a `Name` property), `TableNames.Products` and `DbSet<Product> Products` in `MyAppDbContext`.
- Message keys: `MessageKey.ProductNotFound = nameof(ProductNotFound)` and `MessageKey.Product = "Global.Product"`, map `ProductNotFound` to 404 in `ResponseStatusCodes`.
- Activities: `UserActivity.CreateProduct`, `UpdateProduct`, `DeleteProduct` with explicit, never reused values (and `[Description]` + XML doc like the other members).
- Permissions: `PermissionCatalog.ProductManagement` with `List`, `Detail`, `Create`, `Update`, `Delete`.
- Resx (both `SharedResource.tr-TR.resx` and `SharedResource.en-US.resx`): `ProductNotFound`, `Global.Product`, `ProductManagement`, `PG.ProductManagement`. `MissingResxKeyFinder` (and the `ResxTests` unit test) reports any key that is still missing.
- A new migration for the table (`dotnet ef migrations add AddProducts --project src/MyApp.Api`).

---

## 🧹 Uninstall

```bash
dotnet new uninstall Milvasoft.Templates
```

---

## 💡 Notes

- Ideal for Milvonion projects.
- The detail, update and delete handlers return `MessageKey.{{name}}NotFound` as an error (404 through `ResponseStatusCodes`) when the record does not exist.
- The list handler clamps the page size (`ClampRowCount`, default 20, at most `GlobalConstant.MaxRowCount`).
- The update and delete validators do not apply the seed record protection (`NotBeDefaultData`): only the seed tables (`Users`, `Roles`, `Permissions`, `Languages`) keep their ids below 21.
