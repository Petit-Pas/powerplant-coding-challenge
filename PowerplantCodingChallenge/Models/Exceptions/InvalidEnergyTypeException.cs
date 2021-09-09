using System;

namespace PowerPlantCodingChallenge.API.Models.Exceptions
{
    public class InvalidEnergyTypeException : Exception
    {
        public InvalidEnergyTypeException(string message)  : base(message)
        {
        }
    }
}
