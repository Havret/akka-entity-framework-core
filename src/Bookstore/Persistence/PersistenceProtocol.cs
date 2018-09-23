using System;

namespace Bookstore.Persistence
{
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

    public class Remove<T> where T : class
    {
        public T Entity { get; }
        public Remove(T entity) => Entity = entity;
    }
    
    public class RemoveSuccess<T> where T : class
    {
        public T Entity { get; }
        public RemoveSuccess(T entity) => Entity = entity;
    }

    public class RemoveFailure
    {
        public Exception Exception { get; }
        public RemoveFailure(Exception exception) => Exception = exception;
    }
}