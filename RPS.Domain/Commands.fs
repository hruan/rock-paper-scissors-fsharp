module Commands

open System
open Game
open Events

type CreateGameCommand =
    { playerName: string
      firstMove: Move
      name: string
      id: Guid }

type MakeMoveCommand =
    { move: Move
      playerName: string
      id: Guid }

let createGame (command: CreateGameCommand) state =
   match state.gameState with
    | GameState.NotStarted ->
        [ GameCreatedEvent { name = command.name; playerName = command.playerName};
          MoveMadeEvent { move = command.firstMove; playerName = command.playerName } ]
    | _ -> List.empty

let isValidPlayer playerName state =
    state.creatorName <> playerName

let makeMove (command: MakeMoveCommand) state =
    match state.gameState with
    | GameState.Started when isValidPlayer command.playerName state ->
        let result = wins state.creatorMove command.move
        [ MoveMadeEvent { playerName = command.playerName; move = command.move };
          GameEndedEvent { result = result; players = (state.creatorName, command.playerName) } ]
    | _ -> List.empty