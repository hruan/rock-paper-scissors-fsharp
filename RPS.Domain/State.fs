module State

open Game
open Events

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