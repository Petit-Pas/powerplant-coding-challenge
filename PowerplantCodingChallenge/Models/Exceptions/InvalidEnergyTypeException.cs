using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Models.Exceptions
{
    public class InvalidEnergyTypeException : Exception
    {
        public InvalidEnergyTypeException(string message)  : base(message)
        {

        }
    }
}
