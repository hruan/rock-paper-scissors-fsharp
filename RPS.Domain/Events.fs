module Events

open Game

type Event =
    | MoveMadeEvent    of MoveMadeEvent
    | GameCreatedEvent of GameCreatedEvent
    | GameEndedEvent   of GameEndedEvent
and MoveMadeEvent =
    { gameId: string
      playerName: string
      move: Move }
and GameCreatedEvent =
    { gameId: string
      playerName: string }
and GameEndedEvent =
    { gameId: string
      result: GameResult
      players: string * string }

let aggregateId event =
    match event with
    | GameCreatedEvent evt -> evt.gameId
    | MoveMadeEvent evt    -> evt.gameId
    | GameEndedEvent evt   -> evt.gameId