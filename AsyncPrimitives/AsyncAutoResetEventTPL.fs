    namespace AsyncPrimitives
    open System
    open System.Threading.Tasks
    open Microsoft.FSharp.Core
    open Microsoft.FSharp.Core.LanguagePrimitives.IntrinsicOperators
    open Microsoft.FSharp.Core.Operators
    open Microsoft.FSharp.Control
    open Microsoft.FSharp.Collections
    open System.Collections.Generic

    ///This is just really just to show using the TaskCompletionSource<_>
    type AsyncAutoResetEventTPL() =
        let mutable awaits = new Queue<TaskCompletionSource<bool>>()
        let mutable signalled = false

        member x.WaitAsync() =
            lock awaits (fun () ->
                let tcs = new TaskCompletionSource<bool>()
                awaits.Enqueue(tcs)
                tcs.Task)

        member x.Set() =
            let toRelease = ref Unchecked.defaultof<_>
            lock awaits (fun() ->
                if awaits.Count > 0 then
                    toRelease := awaits.Dequeue()
                elif (not signalled) then
                    signalled <- true )
            match !toRelease with
            | null -> ()
            | _ -> toRelease.Value.SetResult(true)         
