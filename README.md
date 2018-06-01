# reliable-db-connection-wrapper

.NET DbConnection wrapper for retries, making a standard DbConnection more reliable to transient failure.

[![Build History](https://buildstats.info/travisci/chart/mediatechsolutions/reliable-db-connection-wrapper?branch=master)](https://travis-ci.org/mediatechsolutions/reliable-db-connection-wrapper)
[![NuGet Version](https://buildstats.info/nuget/ReliableDbConnectionWrapper?includePreReleases=true)](https://www.nuget.org/packages/ReliableDbConnectionWrapper)
[![Build Status](https://travis-ci.org/mediatechsolutions/reliable-db-connection-wrapper.svg?branch=master)](https://travis-ci.org/mediatechsolutions/reliable-db-connection-wrapper)


## Why ReliableDbConnectionWrapper

Network services can fail or become temporarily unreachable. This is especially true when running code on cloud providers. 

It is extremely valuable to add resilience to the database connection in an already existing project without having to change the database access code. In addition, it is desirable to be able to use ORMs or micro-ORMs (like Dapper, NPoco or PetaPoco) while adding fault tolerance without interfering with those frameworks.

Microsoft itself developed a ReliableSqlConnection which was an implementation of a SqlConnection with retries. ReliableSqlConnection is part of Microsoft's Transient Fault Handling Framework (https://docs.microsoft.com/aspnet/aspnet/overview/developing-apps-with-windows-azure/building-real-world-cloud-apps-with-windows-azure/transient-fault-handling). The drawbacks of ReliableSqlConnection are:

* It is based on ADO.NET 1.0 and it has been deprecated. 
* As such:
    * It is not maintained anymore.
    * You don't get the advantages of the latest versions of ADO.NET.
    * You cannot use code that relies on the DbConnection abstraction (like database instrumentations for metrics and insights).

ReliableDbConnectionWrapper is a wrapper over `DbConnection` which allows .NET applications to apply configurable retries to the database operation: opening connections, executing commands and managing database transactions.

## How to use it

In order to automatically add retries to all your database queries you have to reference the ReliableDbConnectionWrapper Nuget package, [![ReliableDbConnectionWrapper](https://img.shields.io/nuget/v/ReliableDbConnectionWrapper?style=flat)](https://www.nuget.org/packages/ReliableDbConnectionWrapper), in your project and wrap your database connection (`DbConnection`) into a `ReliableDbConnectionWrapper`. 

```csharp
wrappedConnection = new ReliableDbConnectionWrapper(new SqlConnection(_connectionString), retryPolicy);
wrappedConnection.Open();
```
The opening of the database connection is retried.

Having the database connection wrapped into a `ReliableDbConnectionWrapper` it is possible to create and execute database commands, that are automatically retried:
```csharp
var dbCommand = wrappedConnection.CreateCommand();
dbCommand.CommandText = "SELECT 1";
dbCommand.Execute();
```

Be careful to create the database commands via the wrapped `ReliableDbConnectionWrapper` because otherwise they will not be retried:
```csharp
var dbCommand = new SqlCommand("SELECT 1")
dbCommand.Connection = wrappedConnection;
dbCommand.ExecuteReader(); // THIS WILL NOT BE RETRIED
```

In order to properly retry the exceptions you need to provide a configured [Polly](https://github.com/App-vNext/Polly) retry policy to the constructor of `ReliableDbConnectionWrapper`:
```csharp
var retryPolicy = Policy
    .Handle<SqlException>(ex => ex.Number == 1205)
    .WaitAndRetry(5, retryAttempt => TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt))); 
wrappedConnection = new ReliableDbConnectionWrapper(new SqlConnection(_connectionString), retryPolicy);
```

You are absolutely free to configure Polly so you can decide what exceptions or even error codes you want to retry, how many times you want to retry and how much time you want to wait between retries.