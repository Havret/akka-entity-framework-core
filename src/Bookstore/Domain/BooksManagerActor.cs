using Akka.Actor;
using Bookstore.Messages;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Domain
{
    public class BooksManagerActor : ReceiveActor
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public BooksManagerActor(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            Receive<CreateBook>(command => GetOrCreate(Guid.Parse("22DE779F-84C3-4978-A609-359B3F08D239")).Forward(command));
            Receive<GetBookById>(query => GetOrCreate(query.Id).Forward(query));
        }

        private IActorRef GetOrCreate(Guid id)
        {
            var childName = id.ToString();
            var child = Context.Child(childName);
            if (child.IsNobody())
                return Context.ActorOf(Props.Create(() => new BookActor(_serviceScopeFactory)), childName);

            return child;
        }
    }
}