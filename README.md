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

### Tests
A set of unit tests has been provided with the solution, you can execute them by using `dotnet test` in the `PowerplantCodingChallenge.Test` folder

### CO2

CO2 emission allowances can be taken into account, this comportment is taken into account following the value of `PowerPlantCodingChallenge:CO2Enabled` in appsettings.json.

### Websocket

One or more websocket clients can be used to listen to all requests along with their responses.

To connect, the client needs to use the route `/ws`