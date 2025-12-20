# CSharp-Microservices
.NET / C# microservices labs: Clean Architecture, Postgres, gRPC Gateway, Kafka
# Microservices Labs - ITMO University

Лабораторные работы по разработке микросервисных приложений на C# и .NET 9.

## Технологический стек

- **Backend**: C# (.NET 9), ASP.NET Core
- **Архитектура**: Hexagonal Architecture, Microservices
- **Коммуникация**: gRPC, HTTP REST, Kafka
- **База данных**: PostgreSQL, Npgsql
- **Миграции**: FluentMigrator
- **Инфраструктура**: Docker, Docker Compose
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Конфигурация**: Microsoft.Extensions.Configuration, Custom Configuration Providers
- **HTTP клиенты**: Refit, HttpClientFactory
- **Документация**: Swagger/OpenAPI
- **Консоль**: Spectre.Console

## Лабораторные работы

### Lab 1: Dependency Injection & Configuration
Основы работы с DI контейнером и системой конфигурации .NET.

**Ключевые концепции:**
- Dependency Injection паттерны
- Microsoft.Extensions.Configuration
- Options Pattern

### Lab 2: HTTP клиенты и Custom Configuration Provider
Разработка клиента для внешнего сервиса и кастомного провайдера конфигураций.

**Реализовано:**
- HTTP клиент с ручной реализацией и через Refit
- Пагинированная выгрузка данных
- Custom ConfigurationProvider с периодическим обновлением
- Консольное приложение с динамическим отображением контента (Spectre.Console)
- Интеграция с внешним сервисом конфигураций

**Технологии:**
- Refit для декларативных HTTP клиентов
- IHttpClientFactory
- Microsoft.Extensions.Configuration
- PeriodicTimer для автообновления
- Spectre.Console для UI

### Lab 3: Product Service (Hexagonal Architecture + gRPC)
Полноценный микросервис товаров и заказов с гексагональной архитектурой.

**Функциональность:**
- **Товары**: создание, пагинированный поиск с фильтрацией
- **Заказы**: полный жизненный цикл (создание → обработка → завершение/отмена)
- **Позиции заказов**: добавление, soft delete
- **История заказов**: аудит всех операций с полиморфной сериализацией

**Архитектура:**
- Hexagonal Architecture (Ports & Adapters)
- gRPC Presentation Layer
- HTTP Gateway с REST API
- Repository Pattern
- SQL миграции через FluentMigrator

**Технологии:**
- gRPC с oneof для полиморфизма
- PostgreSQL + Npgsql (raw SQL)
- Транзакции для атомарных операций
- Custom gRPC Interceptors
- Middleware для обработки ошибок
- Swagger документация с полиморфными моделями

### Lab 4: Event-Driven Architecture с Kafka
Интеграция микросервиса с системой обработки заказов через Kafka.

**Реализовано:**
- Producer для топика `order_creation`
- Consumer для топика `order_processing`
- Батчовая обработка сообщений
- Обработка событий жизненного цикла заказа
- REST эндпоинты для сервиса обработки заказов

**Event Flow:**
```
Создание заказа → Processing → Согласование → 
Упаковка → Доставка → Завершение/Отмена
```

**Технологии:**
- Apache Kafka
- Protobuf для сериализации
- Background Services
- Kafka UI для мониторинга
- Options Pattern для конфигурации

## Архитектурные решения

### Hexagonal Architecture
```
Presentation (gRPC/HTTP) → Application (Services) → Domain → Infrastructure (Repositories)
```

### Dependency Injection
- Extension методы для регистрации компонентов
- Модульная структура DI конфигурации
- Lifetime management (Singleton, Scoped, Transient)

### Configuration Management
- Multi-source конфигурация (JSON, Environment, Custom Provider)
- Options Pattern с валидацией
- Динамическое обновление через IOptionsMonitor

## Запуск проектов

Каждая лабораторная содержит docker-compose файл для развертывания зависимостей:
```bash
# Lab 2
cd src/lab-2
docker-compose up -d

# Lab 3
cd src/lab-3
docker-compose up -d

# Lab 4
cd src/lab-4
docker-compose up -d
```

## Особенности реализации

- **Чистая архитектура**: разделение concerns, независимость от фреймворков
- **Тестируемость**: использование интерфейсов и DI
- **Полиморфизм**: в БД через JSON, в gRPC через oneof, в HTTP через наследование
- **Обработка ошибок**: централизованная через interceptors/middleware
- **Транзакционность**: ACID гарантии для критичных операций
- **Масштабируемость**: event-driven подход, пагинация

## Учебные цели

Проект демонстрирует практические навыки в:
- Проектировании микросервисных архитектур
- Работе с распределенными системами
- Реализации сложных бизнес-процессов
- Интеграции различных технологий .NET экосистемы
- Применении best practices enterprise разработки
