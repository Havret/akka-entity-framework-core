using System;
using Akka.Actor;
using Bookstore.Dto;
using Bookstore.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace Bookstore.Domain
{
    public class BookActor : EntityActor<Book, BookstoreContext>
    {
        public BookActor(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
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

            Receive<GetBookById>(query =>
            {
                if (Entity != null)
                    Sender.Tell(GetBookDto(Entity));
                else
                {
                    Sender.Tell(BookNotFound.Instance);
                    Context.Stop(Self);
                }

                BookDto GetBookDto(Book book) => new BookDto
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    Cost = book.Cost,
                    InventoryAmount = book.InventoryAmount
                };
            });
        }

        protected override void OnPersistFailure(Exception cause)
        {
            Log.Error(cause, "Could not persist Book");
        }

        protected override Guid Id { get; }
    }
}