module main
    open AsyncPrimitives
    open System
    open System.Threading
    Console.Title <- "asyncManualResetEvent"
    let amre = AsyncManualResetEvent()
    let x = async{let! x = amre.WaitAsync()
                  Console.WriteLine("First signalled")}

    let y = async{let! x = amre.WaitAsync()
                 Console.WriteLine("Second signalled")}

    let z = async{let! x = amre.WaitAsync()
                  Console.WriteLine("Third signalled")}
    //start async x and y
    Async.Start x
    Async.Start y
    //reset the asyncManualResetEvent, this will test whether the
    //async workflows x and y are orphaned due to the AsyncResultCell being recycled
    amre.Reset()

    //now start the async z
    Async.Start z

    //we set a single time, this should result in the three async workflows completing
    amre.Set()

    Console.ReadLine() |> ignore

    //******************************* asyncAutoResetEvent *******************************
    Console.Clear()
    
    Console.Title <- "asyncAutoResetEvent"
    printfn "** AsyncAutoResetEvent **"
    let are = AsyncAutoResetEvent(false)
 
    let wait1 = async{ let! w = are.WaitAsync()
                       printfn "wait1 signalled, thread: %i" Thread.CurrentThread.ManagedThreadId}
 
    let wait2 = async{ let! w = are.WaitAsync()
                       printfn "wait2 signalled, thread: %i" Thread.CurrentThread.ManagedThreadId}
 
    printfn "Current threadId: %i" Thread.CurrentThread.ManagedThreadId
    printfn "starting wait1"
    Async.Start wait1
 
    printfn "starting wait2"
    Async.Start wait2
 
    printfn "Press Any key to call set, wait1 should be signalled"
    Console.ReadKey() |> ignore
    are.Set()
 
    printfn "Press Any key to call set, wait2 should be signalled"
    Console.ReadKey() |> ignore
    are.Set()
 
    printfn "Press Any key to exit"
    Console.ReadKey() |> ignore
