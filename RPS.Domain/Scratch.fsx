#load "Game.fs"
#load "Events.fs"
#load "Commands.fs"
#load "State.fs"
#load "ApplicationService.fs"
#load "CommandRegistrations.fs"

open System
open Game

let defaultState = { gameState = GameState.Started; creatorName = "Robin"; creatorMove = Game.Move.Rock }

// Valid command
Commands.makeMove
    { move = Game.Move.Rock; playerName = "Batman"; id = Guid.Empty }
    defaultState

// Invalid commands
Commands.makeMove
    { move = Game.Move.Rock; playerName = "Batman"; id = Guid.Empty }
    { defaultState with creatorName = "Batman" }

Commands.makeMove
    { move = Game.Move.Rock; playerName = "Batman"; id = Guid.Empty }
    { defaultState with gameState = Game.GameState.NotStarted }