using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using PowerplantCodingChallenge.Models;
using PowerplantCodingChallenge.Models.Exceptions;

namespace PowerplantCodingChallenge.Test.Models
{
    public class PowerPlantTest
    {
        private EnergyMetrics _energyMetrics;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _energyMetrics = new EnergyMetrics()
            {
                Co2 = 20,
                GasCost = 15,
                KersosineCost = 50,
                WindEfficiency = 50
            };
        }

        [Test]
        public void EnergyTypeInitialization_Gas()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "gasfired";
            Assert.AreEqual(powerPlant.EnergySource, EnergySource.Gas);
        }

        [Test]
        public void EnergyTypeInitialization_Kerosine()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "turbojet";
            Assert.AreEqual(powerPlant.EnergySource, EnergySource.Kerosine);
        }

        [Test]
        public void EnergyTypeInitialization_Wind()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "windturbine";
            Assert.AreEqual(powerPlant.EnergySource, EnergySource.Wind);
        }

        [Test]
        public void EnergyTypeInitialization_Unknown()
        {
            PowerPlant powerPlant = new PowerPlant();

            Assert.Throws(typeof(InvalidEnergyTypeException), () => powerPlant.Type = "qslmgj");
        }

        [Test]
        public void Init_Gas()
        {
            PowerPlant powerPlant = new PowerPlant() { Efficiency = 0.5d, EnergySource = EnergySource.Gas };

            powerPlant.Init(_energyMetrics, false);
            Assert.AreEqual(30, powerPlant.CostPerMW);
        }

        [Test]
        public void Init_Kersosine()
        {
            PowerPlant powerPlant = new PowerPlant() { Efficiency = 0.5d, EnergySource = EnergySource.Kerosine };

            powerPlant.Init(_energyMetrics, false);
            Assert.AreEqual(100, powerPlant.CostPerMW);
        }

        [Test]
        public void Init_Wind()
        {
            PowerPlant powerPlant = new PowerPlant() { Efficiency = 1, EnergySource = EnergySource.Wind, PMax = 100 };

            powerPlant.Init(_energyMetrics, false);
            Assert.AreEqual(0, powerPlant.CostPerMW);
            Assert.AreEqual(50, powerPlant.PMax);
        }
    }
}
