using System;

namespace PowerplantCodingChallenge.Models.Exceptions
{
    public class InvalidEnergyTypeException : Exception
    {
        public InvalidEnergyTypeException(string message)  : base(message)
        {
        }
    }
}
