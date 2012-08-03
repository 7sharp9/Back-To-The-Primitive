namespace AsyncPrimitives 
open System
open System.Threading
open System.Collections.Generic
 
    type AsyncAutoResetEvent(?reusethread) =
        let reuseThread = defaultArg reusethread false
        let completed = async.Return true
        let mutable awaits = Queue<_>()
        let mutable signalled = false
 
        member x.WaitAsync() =
            lock awaits (fun () ->
                if signalled then
                    signalled <- false
                    completed
                else
                    let are = AsyncResultCell<_>()
                    awaits.Enqueue are
                    are.AsyncResult)
 
        member x.Set() =
            let getWaiter()=
                lock awaits (fun () ->
                    match (awaits.Count, signalled) with
                    | (x,_) when x > 0 -> Some <| awaits.Dequeue()
                    | (_,y) when not y -> signalled <- true;None
                    | (_,_) -> None)
            match getWaiter() with
            | Some a -> a.RegisterResult(AsyncOk(true), reuseThread)
            | None _ -> ()