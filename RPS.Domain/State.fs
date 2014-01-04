module State

open Game
open Events

let restoreState state events =
    let step event state =
        match event with
        | GameCreatedEvent e ->
            { state with gameId = e.gameId; gameState = GameState.Started; creatorName = e.playerName }
        | MoveMadeEvent e when e.playerName = state.creatorName ->
            { state with creatorMove = e.move }
        | GameEndedEvent e ->
            { state with gameState = GameState.Ended }
        | _ -> state

    List.foldBack step events state