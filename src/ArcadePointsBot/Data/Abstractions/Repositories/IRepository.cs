using ArcadePointsBot.Domain;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ArcadePointsBot.Data.Abstractions.Repositories;

public interface IEntityRepository<T, TId> : IReadRepository<T>, IWriteRepository<T, TId> where T : IEntity<TId>
{
    /// <summary>
    /// Finds an entity from data store.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    T? Find(TId id);

    /// <summary>
    /// Asynchronously finds an entity from data store.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task<T?> FindAsync(TId id);
}

public interface IRepository<T, TId> where T : IEntity<TId>
{
    IUnitOfWork UnitOfWork { get; }
}

public interface IWriteRepository<T, TId> : IRepository<T, TId> where T : IEntity<TId>
{
    /// <summary>
    /// Adds the entity to the data store
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <returns>The changed entity</returns>
    void Add(T entity);

    /// <summary>
    /// Asynchronously adds the entity to the data store
    /// </summary>
    /// <param name="entity">The entity to add</param>
    ValueTask AddAsync(T entity);

    /// <summary>
    /// Adds a range of entities to the data store
    /// </summary>
    /// <param name="entities">The collection of entities to add</param>
    void AddRange(IEnumerable<T> entities);

    /// <summary>
    /// Asychronously adds a range of entities to the data store
    /// </summary>
    /// <param name="entities">The collection of entities to add</param>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Deletes an entity from the data store
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    void Delete(T entity);

    /// <summary>
    /// Deletes a range of entities from the data store
    /// </summary>
    /// <param name="entities">The collection of entities to delete</param>
    void DeleteRange(IEnumerable<T> entities);

    /// <summary>
    /// Updates an entity in the data store
    /// </summary>
    /// <param name="entity">The entity to update</param>
    void Update(T entity);

    /// <summary>
    /// Updates a range of entities in the data store
    /// </summary>
    /// <param name="entities">The collection of entities to update</param>
    void UpdateRange(IEnumerable<T> entities);

    public Task<int> ExecuteUpdate(IEnumerable<T> entities, Expression<Func<SetPropertyCalls<T>, SetPropertyCalls<T>>> setPropertyCalls);
}

/// <summary>
/// Generic repository interface for read operations
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IReadRepository<T>
{
    /// <summary>
    /// Checks if the data store contains any entity that satisfies the given predicate
    /// </summary>
    /// <param name="predicate">Condition</param>
    /// <returns>True/false on whether an entity exists that satisfies the predicate</returns>
    bool Exists(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Asynchronously checks if the data store contains any entity that satisfies the given predicate
    /// </summary>
    /// <param name="predicate">Condition</param>
    /// <returns>True/false on whether an entity exists that satisfies the predicate</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Retrieves all entities from the data store as <see cref="IQueryable{T}"/>
    /// </summary>
    /// <returns>Query</returns>
    IQueryable<T> GetAll();

    /// <summary>
    /// Retrieves all entities that satisfy the given condition from the data store as <see cref="IQueryable{T}"/> 
    /// </summary>
    /// <param name="predicate">Condition</param>
    /// <returns>Query</returns>
    IQueryable<T> GetBy(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Retrieves the first entity from data store that satisfies the given predicate
    /// </summary>
    /// <param name="predicate">Condition</param>
    /// <returns>Entity</returns>
    T? GetFirst(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Asynchronously retrieves the first entity from the data store that satisfies the given predicate
    /// </summary>
    /// <param name="predicate">Condition</param>
    /// <returns>Entity</returns>
    Task<T?> GetFirstAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Retrieves a single entity from data store that satisfies the given predicate
    /// </summary>
    /// <param name="predicate">Condition</param>
    T? GetSingle(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Asynchronously retrieves a single entity from data store that satisfies the given condition
    /// </summary>
    /// <param name="predicate">Condition</param>
    /// <returns>Entity</returns>
    Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate);
}