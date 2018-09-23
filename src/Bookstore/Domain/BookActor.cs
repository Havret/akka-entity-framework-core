using Akka.Actor;
using Bookstore.Messages;
using Bookstore.Persistence;
using System;

namespace Bookstore.Domain
{
    public class BookActor : EntityActor<Book>
    {
        public BookActor()
        {
            Id = Guid.Parse(Context.Self.Path.Name);

            Receive<CreateBook>(command =>
            {
                var newBook = new Book
                {
                    Id = Id,
                    Title = command.Title,
                    Author = command.Author,
                    Cost = command.Cost,
                    InventoryAmount = command.InventoryAmount,
                };

                Persist(newBook, book =>
                {
                    Log.Info("Book created");
                });
            });

            Receive<UpdateBook>(command =>
            {
                command.Patch.ApplyTo(Entity);
                Persist(Entity, book =>
                {
                    Log.Info("Book updated");
                });
            });

            Receive<GetBookById>(query =>
            {
                if (Entity != null)
                    Sender.Tell(Mappings.Map(Entity));
                else
                {
                    Sender.Tell(BookNotFound.Instance);
                    Context.Stop(Self);
                }
            });
        }

        protected override void OnPersistFailure(Exception cause)
        {
            Log.Error(cause, "Could not persist Book");
        }

        protected override Guid Id { get; }
    }
}