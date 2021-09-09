using System;

namespace PowerPlantCodingChallenge.API.Models.Exceptions
{
    public class InvalidLoadException : Exception
    {
        public InvalidLoadException(string message) : base(message)
        {
        }
    }
}
