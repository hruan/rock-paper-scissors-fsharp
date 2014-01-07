module ApplicationService

open System
open Game
open Commands
open State
open Events

type CommandHandler   = Command -> State -> Event list
type AggregateHandler = Command * CommandHandler * AsyncReplyChannel<Event list>

type EventMutator = Event list * AsyncReplyChannel<Command list>
type MutatorType =
    | Sink       of (Event -> unit)
    | Receptor   of (Event -> Command list)

type ApplicationServiceCommand =
    | RegisterCommandHandler of Type * CommandHandler
    | LocateCommandHandler   of Command * AsyncReplyChannel<Event list>

[<CustomEquality; CustomComparison>]
type CommandType =
    { ``type``: Type }

    override self.Equals other =
        match other with
        | :? CommandType as info -> self.``type``.FullName = info.``type``.FullName
        | _ -> false

    override self.GetHashCode() =
        self.``type``.FullName.GetHashCode()

    interface IComparable with
        member self.CompareTo other =
            match other with
            | :? CommandType as info -> compare self.``type``.FullName info.``type``.FullName
            | _ -> invalidArg "other" "can't compare objects of different types"

let handlers = MailboxProcessor<ApplicationServiceCommand>.Start (fun inbox ->
    let locateAggregate = MailboxProcessor<AggregateHandler>.Start (fun inbox ->
        let rec loop aggregates = async {
            let! command, handler, ch = inbox.Receive()
            match command with
            | NonAggregateCommand _ ->
                let events = handler command EmptyState
                match events with
                | h :: _ ->
                    let aggregate = MailboxProcessor<AggregateHandler>.Start (fun inbox ->
                        let rec aggrLoop state handler = async {
                            let! cmd, handler, ch = inbox.Receive()
                            let evts = handler cmd state
                            ch.Reply evts
                            return! aggrLoop (State.restoreState state evts) handler
                        }

                        aggrLoop (State.restoreState EmptyState events) handler)
                    printfn "new actor for aggregate %A" (aggregateId h)
                    ch.Reply events
                    return! loop (aggregates |> Map.add (aggregateId h) aggregate)
                | _ -> ch.Reply events; return! loop aggregates
            | _ ->
                match Commands.aggregateId command with
                | Some id ->
                    printfn "processing command for aggregate %A" id
                    if aggregates |> Map.containsKey id then
                        let evts = aggregates.[id].PostAndReply (fun ch -> (command, handler, ch))
                        printfn "events for command %A: %A" command evts
                        ch.Reply evts
                    else printfn "%A does not exist!" id; ch.Reply List.empty
                    return! loop aggregates
                | _ -> ch.Reply List.empty; return! loop aggregates
        }

        loop Map.empty
    )
        
    let rec loop commandHandlers = async {
        let! command = inbox.Receive()
        match command with
        | RegisterCommandHandler (cmdType, handler) ->
            let t = { ``type`` = cmdType }
            printfn "registering handler %A for command type %A" handler t
            if commandHandlers |> Map.containsKey t then printfn "handler already exists!"; return! loop commandHandlers
            else return! loop (commandHandlers |> Map.add t handler)
        | LocateCommandHandler (cmd, ch) ->
            let t = { ``type`` = cmd.GetType() }
            printfn "processing command %A of type %A" cmd t
            let handler = commandHandlers |> Map.tryFind t
            match handler with
            | Some handler ->
                let events = locateAggregate.PostAndReply (fun ch -> cmd, handler, ch)
                ch.Reply events
                return! loop commandHandlers
            | None ->
                printfn "no handler registered for command %A" cmd
                ch.Reply List.empty
                return! loop commandHandlers
    }

    loop Map.empty
)

let eventPublisher = MailboxProcessor<EventMutator>.Start(fun inbox ->
    let handle event receiver =
        match receiver with
        | Sink       recv -> recv event; List.empty
        | Receptor   recv -> recv event

    let rec loop receivers = async {
        let! events, ch = inbox.Receive()
        if List.isEmpty receivers then ch.Reply List.empty
        else ch.Reply (receivers |> List.fold (fun evts r ->
            evts @ (events
                    |> List.map (fun evt -> handle evt r)
                    |> List.reduce (fun a b -> a @ b))) List.empty)
        return! loop receivers
    }

    loop List.empty
)

let applicationService = MailboxProcessor<Command>.Start(fun inbox ->
    let rec loop() = async {
        let! command = inbox.Receive()
        printfn "locating handler for command %A" command
        let! events = handlers.PostAndAsyncReply (fun ch -> LocateCommandHandler (command, ch))
        printfn "publishing events: %A" events
        let! newCommands = eventPublisher.PostAndAsyncReply (fun ch -> events, ch)
        printfn "posting new commands: %A" newCommands
        newCommands |> List.iter (fun c -> inbox.Post c)
        printfn "next!"
        return! loop()
    }

    loop()
)
