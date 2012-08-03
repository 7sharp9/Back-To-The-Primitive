namespace AsyncPrimitives

open System.Collections.Generic
open System.Threading

    type AsyncManualResetEvent() =
        let aResCell = ref <| AsyncResultCell<_>()

        member x.WaitAsync() = (!aResCell).AsyncResult
        member x.Set() = (!aResCell).RegisterResult(AsyncOk(true))
        member x.Reset() =
            let rec swap newVal = 
                let currentValue = !aResCell
                let result = Interlocked.CompareExchange<_>(aResCell, newVal, currentValue)
                if obj.ReferenceEquals(result, currentValue) then ()
                else Thread.SpinWait 20
                     swap newVal
            swap <| AsyncResultCell<_>()
                
    type asyncAutoResetEvent() =
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
            lock awaits (fun () ->
                if awaits.Count > 0 then
                    awaits.Dequeue().RegisterResult(AsyncOk(true), reuseThread = true)
                elif (not signalled) then
                    signalled <- true)