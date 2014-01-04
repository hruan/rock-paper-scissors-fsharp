#load "Game.fs"
#load "Events.fs"
#load "Commands.fs"
#load "State.fs"
#load "ApplicationService.fs"
#load "CommandRegistrations.fs"

open System
open Game

let gameId = Guid.NewGuid().ToString()
let defaultState = { gameId = gameId; gameState = GameState.Started; creatorName = "Robin"; creatorMove = Game.Move.Rock }

// Valid command
Commands.makeMove
    (Commands.MakeMoveCommand { move = Game.Move.Rock; playerName = "Batman"; gameId = gameId })
    defaultState

// Invalid commands
Commands.makeMove
    (Commands.MakeMoveCommand { move = Game.Move.Rock; playerName = "Batman"; gameId = gameId })
    { defaultState with creatorName = "Batman" }

Commands.makeMove
    (Commands.MakeMoveCommand { move = Game.Move.Rock; playerName = "Batman"; gameId = gameId })
    { defaultState with gameState = Game.GameState.NotStarted }

open ApplicationService

let createCmd = Commands.CreateGameCommand { playerName = "Batman"; firstMove = Move.Paper }
handlers.Post (RegisterCommandHandler (createCmd.GetType(), Commands.createGame))
applicationService.Post createCmd

let moveCmd = Commands.MakeMoveCommand { gameId = String.Empty; move = Move.Rock; playerName = "Robin" }
handlers.Post (RegisterCommandHandler (moveCmd.GetType(), Commands.makeMove))