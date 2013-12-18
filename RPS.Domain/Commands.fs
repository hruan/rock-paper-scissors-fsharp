﻿module Commands

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

let inline (|IsState|_|) expectedState state =
    if expectedState = state.gameState then Some (state) else None

let inline (|IsValidMove|_|) value eq valueOf state =
    if eq value (valueOf state) then Some (state) else None

let createGame (command: CreateGameCommand) state =
   match state with
    | IsState GameState.NotStarted _ ->
        [ GameCreatedEvent { name = command.name; playerName = command.playerName};
          MoveMadeEvent { move = command.firstMove; playerName = command.playerName } ]
    | _ -> List.empty

let makeMove (command: MakeMoveCommand) state =
    let creatorOf state = state.creatorName
    match state with
    | IsState GameState.Started validState
      & IsValidMove command.playerName (<>) creatorOf _ ->
        let result = wins state.creatorMove command.move
        [ MoveMadeEvent { playerName = command.playerName; move = command.move };
          GameEndedEvent { result = result; players = (state.creatorName, command.playerName) } ]
    | _ -> List.empty