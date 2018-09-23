using Akka.Actor;
using Bookstore.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Bookstore.Persistence
{
    public class PersistenceActor<TEntity> : ReceiveActor where TEntity : class
    {
        public PersistenceActor()
        {
            Receive<Recover>(find =>
            {
                Recover(find.KeyValues).PipeTo(Context.Parent, Self, entity => new RecoverySuccess<TEntity>(entity),
                    exception => new RecoveryFailure(exception));

                async Task<TEntity> Recover(object[] keyValues)
                {
                    using (var serviceScope = Context.System.CreateScope())
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
                    using (var serviceScope = Context.System.CreateScope())
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
                    using (var serviceScope = Context.System.CreateScope())
                    {
                        var repository = serviceScope.ServiceProvider.GetService<IRepository<TEntity>>();
                        var updatedEntity = repository.Update(entity);
                        await repository.SaveAsync();
                        return updatedEntity;
                    }
                }
            });

            Receive<Remove<TEntity>>(remove =>
            {
                Remove(remove.Entity).PipeTo(Context.Parent, Self, entity => new RemoveSuccess<TEntity>(entity),
                    exception => new RemoveFailure(exception));

                async Task<TEntity> Remove(TEntity entity)
                {
                    using (var serviceScope = Context.System.CreateScope())
                    {
                        var repository = serviceScope.ServiceProvider.GetService<IRepository<TEntity>>();
                        var updatedEntity = repository.Remove(entity);
                        await repository.SaveAsync();
                        return updatedEntity;
                    }
                }
            });
        }
    }
}