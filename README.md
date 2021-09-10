# Power Plant Coding Challenge

This is a coding challenge made by GEM from ENGIE. 

You may find the actual instructions in this same repository in the file named CHALLENGE_README.md or you may access the original repository at https://github.com/gem-spaas/powerplant-coding-challenge. 

## Requirements

The project has been built using .Net 5.0, so make sure it is properly installed on your machine.

## Build the project

In the main folder you may use those commands:
* `dotnet restore`
* `dotnet build`

## Launch the API

You may start the API by using `dotnet run` in the `PowerplantCodingChallenge` folder.

## Using Docker

You may start the API using a Docker container instead.
Please make sure that docker is installed on your machine and is correctly running.

Docker should be configured to use **Linux** images

#### Building the image 

From the root folder, execute the following command:

`docker build -f .\PowerplantCodingChallenge\Dockerfile -t imagename .`

#### Creating and starting a container

Execute the following command:
`docker container run -d --name containername -p 8888:80 imagename`

## Documentation

You may find a swagger API documentation on http://localhost:8888/swagger/index.html

### Tests

A set of unit tests has been provided with the solution, you can execute them by using `dotnet test` in the `PowerplantCodingChallenge.Test` folder

### CO2

CO2 emission allowances can be taken into account, this comportment is taken into account following the value of `PowerPlantCodingChallenge:CO2Enabled` in appsettings.json.

### Websocket

One or more websocket clients can be used to listen to all requests along with their responses.

To connect, the client needs to use the route `/ws`

## Algorithm

#### Problem
The 1st idea I had while trying to find a solution for this problem was to sort all power plants by cost/MWh and then turning them ON one by one until reaching the asked load, backtracking to reduce the load if it got exceeded.

I soon realized that the required backtracking could easily get very complicated if a more costly power plant had to be turned ON instead of a less costly to reach a cheaper production plan in the end.

Here is a very basic example of the problem I am talking about:
```json
{
  "load": 60,
  "fuels":
  {
    "gas(euro/MWh)": 20,
    "kerosine(euro/MWh)": 50.8,
    "co2(euro/ton)": 0,
    "wind(%)": 100
  },
  "powerplants": [
    {
      "name": "wind",
      "type": "windturbine",
      "efficiency": 1,
      "pmin": 0,
      "pmax": 20
    },
    {
      "name": "gasfiredbig1",
      "type": "gasfired",
      "efficiency": 0.9,
      "pmin": 50,
      "pmax": 100
    },
    {
      "name": "inefficient",
      "type": "gasfired",
      "efficiency": 0.1,
      "pmin": 0,
      "pmax": 100
    }
  ]
}
```
We have 1 wind turbine, which we would love to turn ON since it does not cost anything to run, a gas turbine that is very efficient, and another gas turbine that is not efficient at all.

An algorithm that could only backtrack to reduce the power delivered by a power plant we already decided to turn ON instead of turning it off completely would always give the same solution:
- Turn ON the wind turbine
  - 20 MWh at 0 cost
- Turn the 1st gas turbine ON, realize you cannot reduce the amount of energy produced, and turn it back OFF.
  - 0 MWh at 0 cost
- Turn the last inefficient gas turbine ON and set its power level to 40 MWh.
  - 40 MWh at 40/0.1*20 => 8000 cost

But here is the optimal solution:
- Keep the wind turbine OFF
  - 0 MWh at 0 cost
- Turn the efficient gas turbine ON and set its power level to 60.
  - 60 MWh at 60/0.9*20 => 1333.33 cost
- Keep the inefficient gas turbine OFF.

Seeing how easy it is to have a much less efficient solution on a very easy scenario, it's more than probable than we'd be missing on a lot of smaller opportunities on big scenarios if we did not take this into account.

Hence, I have made the decision to go with an algorithm that would generate different possible scenarios and then compare them.

#### Scenarios

In order to be able to create efficient scenarios, it was required to have a correct respresentation of what power plant was in relation to the scenario.

Every power plant production, no matter what type, could be divided into 2 parts: 
- The value that the plant produces as soon as it is turned ON
- The additional value that the plant *can* produces if it is turned ON

A scenario would then only need to be a set of ON/OFF for each power plant. If we were to write all possible scenarios for 2 power plants (one with PMin 50 and PMax 100, the other with 20 and 60), it would give:
- OFF OFF 
  - PMin = 0
  - PMax = 0
- ON OFF
  - PMin = 50
  - PMax = 100
- OFF ON
  - PMin = 20
  - PMax = 60
- ON ON
  - PMin = 70
  - PMax = 160

What those information mean, is that 2nd scenario for instance, can fulfill **any** scenario that requires between 50 and 100 MWh.

If the required load was to be 80, 2nd and 4th scenarios are able to fulfill the demand, and we still need to compare them in order to find the cheapest one.

#### Scenario generation optimisation

Now of course, if we were to generate and calculate the cost of every possible scenario, that would be a waste of time and ressources, as every scenario will not be usable for a given load.

The way I optimized that is by generating the scenarios in a recursive tree, stopping branches when needed.

The basic idea is that for each power plant (aka node), we have 2 possibilities, it is either ON or OFF, I then can create a separation, creating 2 new branches from every node.

I can limit the amount of separations I do:
- Some nodes will have PMin = 0, which means that it does not change anything wether they are turned ON or OFF, it is then not required to split the branch into 2 new nodes, I can just mark it as ON and go on.

I can completely stop a branch by keeping track of its the current PMin and PMax (note that these only work if the power plant are ordered by cost/MWh):
- If PMax ever goes lower than the required load, the branch is abandoned
- If PMin ever goes higher than the required load, the banch is abandoned
- If the required load is between the PMin and the PMax, we know that turning ON any later power plant in the list will not help since we can already provide the asked load, and they are just gonna be more expensive. So the branch is evaluated as is, and then abandoned.

Doing all these allow to drastically decrease the amount of scenarios that get computed.