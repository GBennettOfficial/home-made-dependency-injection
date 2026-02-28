# Dependency Injection Container

A lightweight, thread-safe dependency injection (DI) container implementation in C#. This project demonstrates a deep understanding of software design patterns, object lifetime management, and concurrent programming.

## 🎯 Features

- **Three Lifetime Scopes**: Singleton, Scoped, and Transient
- **Thread-Safe**: Uses `ReaderWriterLockSlim` for optimal concurrent access
- **Named Registrations**: Support for keyed service resolution
- **Scope Branching**: Create child scopes that inherit parent registrations
- **Type-Safe**: Full generic type support with compile-time safety
- **Zero Dependencies**: Pure C# implementation without external packages

## 📋 Overview

This dependency injection container provides a clean, intuitive API for managing object dependencies and lifetimes in .NET applications. It's similar in concept to Microsoft's built-in DI container but implemented from scratch to demonstrate understanding of the underlying principles.

### Lifetime Scopes

| Lifetime | Description | Use Case |
|----------|-------------|----------|
| **Singleton** | One instance for the entire application lifetime | Configuration, logging, caching services |
| **Scoped** | One instance per scope (e.g., per HTTP request) | Database contexts, unit of work patterns |
| **Transient** | New instance every time it's requested | Stateless services, lightweight operations |

## 🚀 Getting Started

### Basic Usage

```csharp
using DependencyInjection;

// Create a container
IContainer container = new Container();

// Register services
container.RegisterSingleton(() => new Logger());
container.RegisterScoped(() => new DatabaseContext());
container.RegisterTransient(() => new EmailService());

// Resolve services
var logger = container.Inject<Logger>();
var dbContext = container.Inject<DatabaseContext>();
var emailService = container.Inject<EmailService>();
```

### Named Registrations

When you need multiple implementations of the same type:

```csharp
// Register multiple implementations with keys
container.RegisterSingleton(() => new SqlDatabase(), key: "sql");
container.RegisterSingleton(() => new NoSqlDatabase(), key: "nosql");

// Resolve by key
var sqlDb = container.Inject<IDatabase>(key: "sql");
var noSqlDb = container.Inject<IDatabase>(key: "nosql");
```

### Scoped Services

Perfect for managing per-request or per-operation lifetimes:

```csharp
// Root container
var rootContainer = new Container();
rootContainer.RegisterScoped(() => new RequestContext());

// Create a scope for handling a request
var requestScope = rootContainer.BranchScope();
var context1 = requestScope.Inject<RequestContext>();
var context2 = requestScope.Inject<RequestContext>();
// context1 and context2 are the same instance

// Create another scope for a different request
var anotherRequestScope = rootContainer.BranchScope();
var context3 = anotherRequestScope.Inject<RequestContext>();
// context3 is a different instance from context1/context2
```

### Dependency Chains

Services can depend on other services:

```csharp
container.RegisterSingleton(() => new Logger());
container.RegisterSingleton(() => new DatabaseConfig());
container.RegisterScoped(() => 
    new UserService(
        container.Inject<Logger>(),
        container.Inject<DatabaseConfig>()
    ));

var userService = container.Inject<UserService>();
```

## 🏗️ Architecture

### Thread Safety

The container uses `ReaderWriterLockSlim` to ensure thread-safe access:
- **Read locks** for retrieving already-instantiated services (fast path)
- **Write locks** only when instantiating new singleton/scoped services (slow path)
- Transient services are created under read lock since they don't modify state

### Lazy Initialization

Singleton and scoped services use lazy initialization:
- Factories are registered immediately
- Instances are created only when first requested
- Subsequent requests return the cached instance

### Scope Isolation

When creating a child scope with `BranchScope()`:
- All parent registrations are copied to the child
- Parent singleton instances are shared with the child
- Scoped instances are NOT shared (each scope gets its own)
- Changes to child registrations don't affect the parent

## 🔧 Technical Highlights

This implementation showcases several important software engineering concepts:

1. **Generic Programming**: Type-safe service registration and resolution
2. **Thread Safety**: Proper use of reader-writer locks for concurrent access
3. **SOLID Principles**: Clear separation of concerns with `IContainer` interface
4. **Factory Pattern**: Delegate-based lazy instantiation
5. **Immutability Considerations**: Records for internal keys, defensive copying for scopes
6. **Error Handling**: Clear exceptions for misconfiguration

## 📚 API Reference

### `IContainer` Interface

```csharp
public interface IContainer
{
    // Create a new scope that inherits registrations
    IContainer BranchScope();
    
    // Resolve a service by type and optional key
    T Inject<T>(string key = "");
    
    // Register a service with singleton lifetime
    void RegisterSingleton<T>(Func<T> factory, string key = "");
    
    // Register a service with scoped lifetime
    void RegisterScoped<T>(Func<T> factory, string key = "");
    
    // Register a service with transient lifetime
    void RegisterTransient<T>(Func<T> factory, string key = "");
}
```

## 🎓 Learning Outcomes

Building this container demonstrates understanding of:

- **Dependency Injection Pattern**: Inversion of Control and Dependency Inversion Principle
- **Object Lifetime Management**: Different strategies for object creation and disposal
- **Concurrent Programming**: Thread-safe data structures and locking strategies
- **Generic Types**: Working with `Type` objects and generic constraints
- **.NET Best Practices**: Modern C# features (records, pattern matching, etc.)

## 🔍 Potential Enhancements

Future improvements could include:

- [ ] Automatic constructor injection via reflection
- [ ] Support for `IDisposable` cleanup in scopes
- [ ] Circular dependency detection
- [ ] Interface-to-implementation auto-registration
- [ ] Assembly scanning for auto-registration
- [ ] Performance benchmarking suite
- [ ] XML documentation for IntelliSense
- [ ] Decorator pattern support

## 📝 License

This project is open source and available for educational purposes.

## 🤝 Contributing

This is a learning project, but suggestions and improvements are welcome!

---

**Built with:** .NET 9.0 | C# 13