module RPS.FSharp
open System

type Move =
  | Rock
  | Paper
  | Scissors

type GameResult =
  | PlayerOneWin
  | PlayerTwoWin
  | Tie

let wins playerOneMove playerTwoMove =
    match playerOneMove,playerTwoMove with 
    | Move.Rock,Move.Paper     -> GameResult.PlayerTwoWin
    | Move.Scissors,Move.Rock  -> GameResult.PlayerTwoWin
    | Move.Paper,Move.Scissors -> GameResult.PlayerTwoWin
    | x, y when x = y          -> GameResult.Tie
    | _                        -> GameResult.PlayerOneWin
    
type CreateGameCommand =
    { playerName: string
      firstMove: Move
      name:string
      id:Guid }

type GameState =
    | NotStarted
    | Created 
    | Started
    | Ended

type State =
    { gameState: GameState
      creatorName: string
      creatorMove: Move }

type MoveMadeEvent =
    { playerName: string
      move: Move }

type GameCreatedEvent =
    { name: string
      playerName: string }

type GameEndedEvent =
    { result: GameResult
      players: string * string }

type Event =
    | MoveMadeEvent    of MoveMadeEvent
    | GameCreatedEvent of GameCreatedEvent
    | GameEndedEvent   of GameEndedEvent

let createGame (command:CreateGameCommand) state : list<Event> =
   match state.gameState with
    | GameState.NotStarted ->
        [ GameCreatedEvent { name = command.name; playerName = command.playerName};
          MoveMadeEvent { move = command.firstMove; playerName = command.playerName } ]
    | _ -> List.empty

type MakeMoveCommand =
    { move: Move
      playerName: string
      id: Guid }

let isValidPlayer playerName state =
    state.creatorName <> playerName

let makeMove (command:MakeMoveCommand) state : list<Event> =
    match state.gameState with
    | GameState.Started when isValidPlayer command.playerName state ->
        let result = wins state.creatorMove command.move
        [ MoveMadeEvent { playerName = command.playerName; move = command.move };
          GameEndedEvent { result = result; players = (state.creatorName, command.playerName) } ]
    | _ -> List.empty

let restoreState state events =
    let step event state =
        match event with
        | GameCreatedEvent e ->
            { gameState = GameState.Started; creatorName = e.playerName; creatorMove = state.creatorMove }
        | MoveMadeEvent e when e.playerName = state.creatorName ->
            { gameState = state.gameState; creatorName = state.creatorName; creatorMove = e.move }
        | GameEndedEvent e ->
            { state with gameState = GameState.Ended }
        | _ -> state

    List.foldBack step events state
