using System;

namespace PowerplantCodingChallenge.Models.Exceptions
{
    public class InvalidLoadException : Exception
    {
        public InvalidLoadException(string message) : base(message)
        {
        }
    }
}
