module Events

open Game

type Event =
    | MoveMadeEvent    of MoveMadeEvent
    | GameCreatedEvent of GameCreatedEvent
    | GameEndedEvent   of GameEndedEvent
and MoveMadeEvent =
    { playerName: string
      move: Move }
and GameCreatedEvent =
    { name: string
      playerName: string }
and GameEndedEvent =
    { result: GameResult
      players: string * string }