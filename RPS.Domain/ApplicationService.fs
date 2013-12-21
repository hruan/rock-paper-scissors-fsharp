module ApplicationService

open System
open Game
open Commands
open State
open Events

type CommandHandler = Command -> Async<Event list>

type ApplicationServiceMessage = Command * State

type EventSource = Async<Event list> * AsyncReplyChannel<Command list>

type EventReceiver =
    | Sink       of (Event -> unit)
    | Receptor   of (Event -> Command list)
    | Propagator of (Event -> Event list)

type ApplicationServiceCommand =
    | RegisterCommandHandler of Type * CommandHandler
    | LocateCommandHandler   of Command * AsyncReplyChannel<CommandHandler option>

[<CustomEquality; CustomComparison>]
type CommandType =
    { ``type``: Type }

    override self.Equals other =
        match other with
        | :? CommandType as info -> self.``type``.FullName = info.``type``.FullName
        | _ -> false

    override self.GetHashCode () =
        self.``type``.FullName.GetHashCode ()

    interface IComparable with
        member self.CompareTo other =
            match other with
            | :? CommandType as info -> compare self.``type``.FullName info.``type``.FullName
            | _ -> invalidArg "other" "can't compare objects of differen types"

let commandHandlers = MailboxProcessor<ApplicationServiceCommand>.Start(fun inbox ->
    let rec loop commandHandlers = async {
        let! command = inbox.Receive()
        match command with
        | RegisterCommandHandler (cmd, handler) ->
            let t = { ``type`` = cmd.GetType () }
            if commandHandlers |> Map.containsKey t then return! loop commandHandlers
            else return! loop (commandHandlers |> Map.add t handler)
        | LocateCommandHandler (cmd, ch) ->
            let t = { ``type`` = cmd.GetType () }
            let handler = commandHandlers |> Map.tryFind t
            ch.Reply handler
            return! loop commandHandlers
    }

    loop Map.empty<CommandType, CommandHandler>
)

let eventPublisher = MailboxProcessor<EventSource>.Start(fun inbox ->
    let handle (ch: AsyncReplyChannel<Command list>) event receiver =
        match receiver with
        | Sink       recv -> recv event
        | Receptor   recv -> recv event |> ch.Reply
        | Propagator recv ->
            let evts = async { return recv event }
            inbox.Post (evts, ch)

    let rec loop receivers = async {
        let! es, ch = inbox.Receive()
        let! events = es
        events
        |> List.map (fun evt -> handle ch evt)
        |> List.iter (fun handle -> receivers |> List.iter handle)
    }

    loop List.empty<EventReceiver>
)

let applicationService = MailboxProcessor<ApplicationServiceMessage>.Start(fun inbox ->
    let rec loop () = async {
        let! command, state = inbox.Receive()
        let! reply = commandHandlers.PostAndAsyncReply(fun ch -> LocateCommandHandler (command, ch))
        match reply with
        | Some handler ->
            let! reply = eventPublisher.PostAndTryAsyncReply(fun ch -> (handler command, ch))
            match reply with
            | Some cmds -> cmds |> List.iter (fun c -> inbox.Post (c, state)); return! loop ()
            | None -> return! loop ()
        | None -> return! loop ()
    }

    loop ()
)
