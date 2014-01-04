module Game

open System

type Move =
    | Rock
    | Paper
    | Scissors

type GameResult =
    | PlayerOneWin
    | PlayerTwoWin
    | Tie

type GameState =
    | NotStarted
    | Created 
    | Started
    | Ended

type State =
    { gameId: string
      gameState: GameState
      creatorName: string
      creatorMove: Move }

let EmptyState =
    { gameId = ""
      gameState = GameState.NotStarted
      creatorName = ""
      creatorMove = Move.Paper }

let wins playerOneMove playerTwoMove =
    match playerOneMove, playerTwoMove with 
    | Move.Rock, Move.Paper     -> GameResult.PlayerTwoWin
    | Move.Scissors, Move.Rock  -> GameResult.PlayerTwoWin
    | Move.Paper, Move.Scissors -> GameResult.PlayerTwoWin
    | x, y when x = y           -> GameResult.Tie
    | _                         -> GameResult.PlayerOneWin
