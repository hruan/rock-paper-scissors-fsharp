module Events

open Game

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
