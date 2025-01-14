﻿/* Copyright 2023-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace MongoDB.EntityFrameworkCore.FunctionalTests.Utilities;

internal static class SingleEntityDbContext
{
    public static SingleEntityDbContext<T> Create<T>(IMongoDatabase database, string collectionName) where T:class =>
        new (new DbContextOptionsBuilder<SingleEntityDbContext<T>>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options, collectionName);

    public static SingleEntityDbContext<T> Create<T>(IMongoCollection<T> collection) where T:class =>
        new (new DbContextOptionsBuilder<SingleEntityDbContext<T>>()
            .UseMongoDB(collection.Database.Client, collection.Database.DatabaseNamespace.DatabaseName)
            .Options, collection.CollectionNamespace.CollectionName);
}

internal class SingleEntityDbContext<T> : DbContext where T:class
{
    private readonly string _collectionName;

    public DbSet<T> Entitites { get; init; }

    public SingleEntityDbContext(DbContextOptions options, string collectionName)
        : base(options)
    {
        this._collectionName = collectionName;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<T>().ToCollection(_collectionName);
    }
}
