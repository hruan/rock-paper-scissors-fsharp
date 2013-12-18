#load "Game.fs"
#load "Events.fs"
#load "Commands.fs"
#load "State.fs"

open System

// Valid command
Commands.makeMove
    { move = Game.Move.Rock; playerName = "Batman"; id = Guid.Empty }
    { gameState = Game.GameState.Started; creatorName = "Robin"; creatorMove = Game.Move.Rock }

// Invalid commands
Commands.makeMove
    { move = Game.Move.Rock; playerName = "Batman"; id = Guid.Empty }
    { gameState = Game.GameState.Started; creatorName = "Batman"; creatorMove = Game.Move.Rock }

Commands.makeMove
    { move = Game.Move.Rock; playerName = "Batman"; id = Guid.Empty }
    { gameState = Game.GameState.NotStarted; creatorName = "Robin"; creatorMove = Game.Move.Rock }