using Akka.Actor;
using Bookstore.Messages;
using System;

namespace Bookstore.Domain
{
    public class BooksManagerActor : ReceiveActor
    {
        public BooksManagerActor()
        {
            Receive<CreateBook>(command => GetOrCreate(Guid.NewGuid()).Forward(command));
            Receive<UpdateBook>(command => GetOrCreate(command.Id).Forward(command));
            Receive<GetBookById>(query => GetOrCreate(query.Id).Forward(query));
        }

        private IActorRef GetOrCreate(Guid id)
        {
            var childName = id.ToString();
            var child = Context.Child(childName);

            if (child.IsNobody())
                return Context.ActorOf(Props.Create(() => new BookActor()), childName);
            else
                return child;
        }
    }
}