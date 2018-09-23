using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Bookstore
{
    public class PersistenceActor<TEntity> : ReceiveActor where TEntity : class
    {
        public PersistenceActor(IServiceScopeFactory serviceScopeFactory)
        {
            Receive<Recover>(find =>
            {
                Recover(find.KeyValues).PipeTo(Context.Parent, Self, entity => new RecoverySuccess<TEntity>(entity),
                    exception => new RecoveryFailure(exception));

                async Task<TEntity> Recover(object[] keyValues)
                {
                    using (var serviceScope = serviceScopeFactory.CreateScope())
                    {
                        var repository = serviceScope.ServiceProvider.GetService<IRepository<TEntity>>();
                        return await repository.FindAsync(keyValues);
                    }
                }
            });

            Receive<Create<TEntity>>(create =>
            {
                Create(create.Entity).PipeTo(Context.Parent, Self, entity => new CreateSuccess<TEntity>(entity),
                    exception => new CreateFailure(exception));

                async Task<TEntity> Create(TEntity entity)
                {
                    using (var serviceScope = serviceScopeFactory.CreateScope())
                    {
                        var repository = serviceScope.ServiceProvider.GetService<IRepository<TEntity>>();
                        var newEntity = repository.Add(entity);
                        await repository.SaveAsync();
                        return newEntity;
                    }
                }
            });

            Receive<Update<TEntity>>(update =>
            {
                Update(update.Entity).PipeTo(Context.Parent, Self, entity => new UpdateSuccess<TEntity>(entity),
                    exception => new UpdateFailure(exception));

                async Task<TEntity> Update(TEntity entity)
                {
                    using (var serviceScope = serviceScopeFactory.CreateScope())
                    {
                        var repository = serviceScope.ServiceProvider.GetService<IRepository<TEntity>>();
                        var updatedEntity = repository.Update(entity);
                        await repository.SaveAsync();
                        return updatedEntity;
                    }
                }
            });
        }
    }

    public class Create<T> where T : class
    {
        public T Entity { get; }
        public Create(T entity) => Entity = entity;
    }

    public class CreateSuccess<T> where T : class
    {
        public T Entity { get; }
        public CreateSuccess(T entity) => Entity = entity;
    }

    public class CreateFailure
    {
        public Exception Exception { get; }
        public CreateFailure(Exception exception) => Exception = exception;
    }

    public class Recover
    {
        public object[] KeyValues { get; }

        public Recover(params object[] keyValues) => KeyValues = keyValues;
    }

    public class RecoverySuccess<T> where T : class
    {
        public T Entity { get; }
        public RecoverySuccess(T entity) => Entity = entity;
    }

    public class RecoveryFailure
    {
        public Exception Exception { get; }
        public RecoveryFailure(Exception exception) => Exception = exception;
    }

    public class Update<T> where T : class
    {
        public T Entity { get; }
        public Update(T entity) => Entity = entity;
    }

    public class UpdateSuccess<T> where T : class
    {
        public T Entity { get; }
        public UpdateSuccess(T entity) => Entity = entity;
    }

    public class UpdateFailure
    {
        public Exception Exception { get; }
        public UpdateFailure(Exception exception) => Exception = exception;
    }
}