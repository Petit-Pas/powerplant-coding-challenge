using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using PowerplantCodingChallenge.Energy.Exceptions;
using PowerplantCodingChallenge.Energy.Types;

namespace PowerplantCodingChallenge.Test.Energy.Types
{
    public class PowerPlantTest
    {
        private EnergyMetrics _energyMetrics;

        [SetUp]
        public void Setup()
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
            Assert.AreEqual(powerPlant.EnergySource, EnergySources.Gas);
        }

        [Test]
        public void EnergyTypeInitialization_Kerosine()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "turbojet";
            Assert.AreEqual(powerPlant.EnergySource, EnergySources.Kerosine);
        }

        [Test]
        public void EnergyTypeInitialization_Wind()
        {
            PowerPlant powerPlant = new PowerPlant();

            powerPlant.Type = "windturbine";
            Assert.AreEqual(powerPlant.EnergySource, EnergySources.Wind);
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
            PowerPlant powerPlant = new PowerPlant() { Efficiency = 0.5d, EnergySource = EnergySources.Gas };

            powerPlant.Init(_energyMetrics);
            Assert.AreEqual(30, powerPlant.CostPerMW);
        }

        [Test]
        public void Init_Kersosine()
        {
            PowerPlant powerPlant = new PowerPlant() { Efficiency = 0.5d, EnergySource = EnergySources.Kerosine };

            powerPlant.Init(_energyMetrics);
            Assert.AreEqual(100, powerPlant.CostPerMW);
        }

        [Test]
        public void Init_Wind()
        {
            PowerPlant powerPlant = new PowerPlant() { Efficiency = 1, EnergySource = EnergySources.Wind, PMax = 100 };

            powerPlant.Init(_energyMetrics);
            Assert.AreEqual(0, powerPlant.CostPerMW);
            Assert.AreEqual(50, powerPlant.PMax);
        }
    }
}
