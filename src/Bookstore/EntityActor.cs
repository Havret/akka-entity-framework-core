using Akka.Actor;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Akka.Event;

namespace Bookstore
{
    public abstract class EntityActor<TEntity> : ReceiveActor, IWithUnboundedStash
        where TEntity : class
    {
        private readonly IActorRef _persistenceActor;
        private readonly Queue<Action<TEntity>> _pendingInvocations = new Queue<Action<TEntity>>();
        protected TEntity Entity { get; private set; }
        protected abstract Guid Id { get; }
        public IStash Stash { get; set; }
        protected virtual ILoggingAdapter Log { get; }

        protected EntityActor(IServiceScopeFactory serviceScopeFactory)
        {
            _persistenceActor = Context.ActorOf(Props.Create(() => new PersistenceActor<TEntity>(serviceScopeFactory)));
            Log = Context.GetLogger();
        }

        private void Recovering()
        {
            Receive<RecoverySuccess<TEntity>>(success =>
            {
                Entity = success.Entity;
                Stash.UnstashAll();
                UnbecomeStacked();
            });

            ReceiveAny(message => Stash.Stash());
        }

        private void Creating()
        {
            Receive<CreateSuccess<TEntity>>(success =>
            {
                Entity = success.Entity;
                Stash.UnstashAll();
                UnbecomeStacked();
                _pendingInvocations.Dequeue()(Entity);
            });
            Receive<CreateFailure>(failure =>
            {
                Entity = default(TEntity);
                Stash.UnstashAll();
                UnbecomeStacked();
                OnPersistFailure(failure.Exception);
                _pendingInvocations.Dequeue();
            });

            ReceiveAny(message => Stash.Stash());
        }

        private void Updating()
        {
            Receive<UpdateSuccess<TEntity>>(success =>
            {
                Entity = success.Entity;
                Stash.UnstashAll();
                UnbecomeStacked();
                _pendingInvocations.Dequeue()(Entity);
            });
            Receive<UpdateFailure>(failure =>
            {
                _persistenceActor.Tell(new Recover(Id), Sender);
                OnPersistFailure(failure.Exception);
                UnbecomeStacked();
                BecomeStacked(Recovering);
            });
            ReceiveAny(message => Stash.Stash());
        }

        protected override void PreStart()
        {
            _persistenceActor.Tell(new Recover(Id), Sender);
            BecomeStacked(Recovering);
        }

        protected void Persist(TEntity entity, Action<TEntity> handler)
        {
            _pendingInvocations.Enqueue(handler);

            if (Equals(Entity, default(TEntity)))
            {
                _persistenceActor.Tell(new Create<TEntity>(entity), Sender);
                BecomeStacked(Creating);
            }
            else
            {
                _persistenceActor.Tell(new Update<TEntity>(entity), Sender);
                BecomeStacked(Updating);
            }
        }

        protected virtual void OnPersistFailure(Exception cause)
        {
            Log.Error(cause, "Failed to persist entity type [{0}] for Id [{1}]", typeof(TEntity), Id);
        }
    }
}