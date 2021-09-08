using PowerplantCodingChallenge.Models.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PowerplantCodingChallenge.Models
{
    public enum EnergySource
    {
        Gas,
        Kerosine,
        Wind
    }

    public static class EnergySourceConverter
    {
        public static EnergySource ConvertToEnergySource(this string energy)
        {
            return energy switch
            {
                "gasfired" => EnergySource.Gas,
                "turbojet" => EnergySource.Kerosine,
                "windturbine" => EnergySource.Wind,
                _ => throw new InvalidEnergyTypeException($"Unrecognized energy type '{energy}'"),
            };
        }

        public static string ConvertToString(this EnergySource energy)
        {
            return energy switch
            {
                EnergySource.Gas => "gasfired",
                EnergySource.Kerosine => "turbojet",
                EnergySource.Wind => "windturbine",
                _ => throw new InvalidEnergyTypeException("Invalid enum value"),
            };
        }
    }
}
