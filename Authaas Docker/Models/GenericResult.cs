using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authaas_Docker.Models
{
    public class GenericResult
    {
        protected GenericResult(bool success, string error)
        {
            if (success && error != string.Empty)
                throw new InvalidOperationException();
            if (!success && error == string.Empty)
                throw new InvalidOperationException();
            Success = success;
            Error = error;
        }

        public bool Success { get; }
        public string Error { get; }
        public bool IsFailure => !Success;

        public static GenericResult Fail(string message)
        {
            return new GenericResult(false, message);
        }

        public static GenericResult<T> Fail<T>(string message)
        {
            return new GenericResult<T>(default, false, message);
        }

        public static GenericResult Ok()
        {
            return new GenericResult(true, string.Empty);
        }

        public static GenericResult<T> Ok<T>(T value)
        {
            return new GenericResult<T>(value, true, string.Empty);
        }
    }

    public class GenericResult<T> : GenericResult
    {
        protected internal GenericResult(T value, bool success, string error)
            : base(success, error)
        {
            Value = value;
        }

        public T Value { get; set; }
    }
}
