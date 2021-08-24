using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using PowerplantCodingChallenge.Energy.Types;

namespace PowerplantCodingChallenge.Test.Energy.Types
{
    public class PowerPlantTest
    {
        [Test]
        public void EnergyTypeInitialization_Gas()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "gasfired";
            Assert.AreEqual(powerPlant.EnergyType, EnergySources.Gas);
        }

        [Test]
        public void EnergyTypeInitialization_Kerosine()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "turbojet";
            Assert.AreEqual(powerPlant.EnergyType, EnergySources.Kerosine);
        }

        [Test]
        public void EnergyTypeInitialization_Wind()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "windturbine";
            Assert.AreEqual(powerPlant.EnergyType, EnergySources.Wind);
        }

        [Test]
        public void EnergyTypeInitialization_Unknown()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "qslmgj";
            Assert.AreEqual(powerPlant.EnergyType, EnergySources.Unknown);
        }
    }
}
