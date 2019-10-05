using System;

namespace Publitio
{
    public class InvalidJsonException : Exception
    {
        public static InvalidJsonException ForJson(string json, string uri) =>
            new InvalidJsonException(
                $"Invalid JSON:\n{json}\nThis might be due to an internal server error, " +
                $"or \"{uri}\" might be an invalid URI, or you may be using the wrong HTTP method.");

        public InvalidJsonException()
            : base() { }

        public InvalidJsonException(string message)
            : base(message) { }

        public InvalidJsonException(string message, Exception inner)
            : base(message, inner) { }
    }
}