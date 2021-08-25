﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Energy.Exceptions
{
    public class InvalidLoadException : Exception
    {
        public InvalidLoadException(string message) : base(message)
        {
        }
    }
}
