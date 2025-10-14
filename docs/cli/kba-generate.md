# rvr generate - RVR CLI

Générer du code automatiquement.

## Syntaxe

```bash
rvr generate <command> [arguments] [options]
kba gen <command> [arguments] [options]   # alias
kba g <command> [arguments] [options]     # alias
```

## Commandes Disponibles

| Commande | Description |
|----------|-------------|
| `aggregate` | Générer un aggregate root |
| `crud` | Générer CRUD operations |
| `command` | Générer CQRS command |
| `query` | Générer CQRS query |

---

## aggregate

Générer un aggregate root complet avec toutes les couches.

### Syntaxe

```bash
rvr generate aggregate <name> <module>
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Nom de l'aggregate |
| `module` | Nom du module |

### Exemple

```bash
rvr generate aggregate Product Catalog
```

### Files Generated

```
src/
├── Domain/
│   └── Catalog/
│       ├── Aggregates/
│       │   └── Product.cs
│       └── Events/
│           ├── ProductCreatedEvent.cs
│           └── ProductUpdatedEvent.cs
├── Application/
│   └── Catalog/
│       ├── DTOs/
│       │   ├── ProductDto.cs
│       │   ├── CreateProductRequest.cs
│       │   └── UpdateProductRequest.cs
│       └── Services/
│           └── IProductService.cs
└── Infrastructure/
    └── Catalog/
        ├── Configurations/
        │   └── ProductConfiguration.cs
        └── Repositories/
            └── ProductRepository.cs
```

---

## crud

Générer CRUD operations complètes.

### Syntaxe

```bash
rvr generate crud <name> [options]
```

### Arguments

| Argument | Description |
|----------|-------------|
| `name` | Nom de l'entité |

### Options

| Option | Description |
|--------|-------------|
| `--props` | Properties (Name:type,Name:type) |

### Exemples

```bash
# CRUD simple
rvr generate crud User

# CRUD avec properties
rvr generate crud Product --props "Name:string,Price:decimal,Description:string"

# CRUD avec types complexes
rvr generate crud Order --props "CustomerId:int,Items:List<OrderItem>,Total:decimal"
```

### Files Generated

```
src/
├── Domain/
│   └── Entities/
│       └── User.cs
├── Application/
│   └── Users/
│       ├── DTOs/
│       │   ├── UserDto.cs
│       │   ├── CreateUserRequest.cs
│       │   └── UpdateUserRequest.cs
│       ├── Services/
│       │   └── IUserService.cs
│       └── Handlers/
│           ├── CreateUserHandler.cs
│           ├── GetUserHandler.cs
│           ├── UpdateUserHandler.cs
│           └── DeleteUserHandler.cs
├── Infrastructure/
│   └── Users/
│       ├── Configurations/
│       │   └── UserConfiguration.cs
│       └── Repositories/
│           └── UserRepository.cs
└── Api/
    └── Controllers/
        └── UsersController.cs
```

---

## command

Générer une CQRS command.

### Syntaxe

```bash
rvr generate command <name>
```

### Exemple

```bash
rvr generate command CreateUser
```

### Files Generated

```
src/Application/
└── Users/
    ├── Commands/
    │   ├── CreateUserCommand.cs
    │   └── CreateUserCommandHandler.cs
    └── DTOs/
        └── CreateUserResult.cs
```

### Code Generated

```csharp
// CreateUserCommand.cs
public record CreateUserCommand(
    string UserName,
    string Email,
    string Password
) : IRequest<CreateUserResult>;

// CreateUserCommandHandler.cs
public class CreateUserCommandHandler 
    : IRequestHandler<CreateUserCommand, CreateUserResult>
{
    private readonly IUserRepository _repository;

    public CreateUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateUserResult> Handle(
        CreateUserCommand request, 
        CancellationToken cancellationToken)
    {
        var user = new User(request.UserName, request.Email, request.Password);
        await _repository.AddAsync(user, cancellationToken);
        
        return new CreateUserResult(user.Id);
    }
}
```

---

## query

Générer une CQRS query.

### Syntaxe

```bash
rvr generate query <name>
```

### Exemple

```bash
rvr generate query GetUserById
```

### Files Generated

```
src/Application/
└── Users/
    ├── Queries/
    │   ├── GetUserByIdQuery.cs
    │   └── GetUserByIdQueryHandler.cs
    └── DTOs/
        └── UserDto.cs
```

### Code Generated

```csharp
// GetUserByIdQuery.cs
public record GetUserByIdQuery(int Id) : IRequest<UserDto>;

// GetUserByIdQueryHandler.cs
public class GetUserByIdQueryHandler 
    : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(
        GetUserByIdQuery request, 
        CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id, cancellationToken);
        return _mapper.Map<UserDto>(user);
    }
}
```

---

## Voir aussi

- [rvr new](kba-new.md) - Créer un projet
- [rvr ai generate](kba-ai.md) - Génération avec AI
- [CQRS Pattern](../quickstart.md) - Pattern CQRS
