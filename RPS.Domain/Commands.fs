module Commands

open System
open Game
open Events

type Command =
    | CreateGameCommand of CreateGameCommand
    | MakeMoveCommand   of MakeMoveCommand
and CreateGameCommand =
    { playerName: string
      firstMove: Move }
and MakeMoveCommand =
    { gameId: string
      move: Move
      playerName: string }

let inline (|IsState|_|) expectedState state =
    if expectedState = state.gameState then Some (state) else None

let inline (|IsValidWhen|_|) value eq valueOf state =
    if eq value (valueOf state) then Some (state) else None

let inline (|NonAggregateCommand|_|) command =
    match command with
    | CreateGameCommand cmd -> Some cmd
    | _ -> None

let aggregateId command =
    match command with
    | NonAggregateCommand _ -> None
    | _ -> match command with
           | MakeMoveCommand cmd -> Some cmd.gameId
           | _ -> None

let createGame command state =
    match command with
    | CreateGameCommand command ->
        match state with
        | IsState GameState.NotStarted _ ->
            let gameId = Guid.NewGuid.ToString ()
            [ GameCreatedEvent { gameId = gameId; playerName = command.playerName};
              MoveMadeEvent { gameId = gameId; move = command.firstMove; playerName = command.playerName } ]
        | _ -> List.empty
    | _ -> List.empty

let makeMove command state =
    match command with
    | MakeMoveCommand command ->
        let creatorOf state = state.creatorName
        match state with
        | IsState GameState.Started _
          & IsValidWhen command.playerName (<>) creatorOf s ->
            let result = wins state.creatorMove command.move
            [ MoveMadeEvent { gameId = s.gameId; playerName = command.playerName; move = command.move };
              GameEndedEvent { gameId = s.gameId; result = result; players = (state.creatorName, command.playerName) } ]
        | _ -> List.empty
    | _ -> List.empty