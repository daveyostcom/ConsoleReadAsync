
open System.Threading
open System.Threading.Tasks
open ConsoleReadAsync

type Pulse =
  | FakeCursor
  | Dots
  | NoPulse

let delaySec n =
  let delayMs = int ((float n) * 1000.0)
  Task.Delay delayMs

let pulseContinuously pulse token =
  match pulse with
  | NoPulse -> Task.CompletedTask
  | _ -> task {
    let ct = (token: CancellationToken)
    while not ct.IsCancellationRequested do
      match pulse with
      | FakeCursor -> printf "*\b" ; do! delaySec 0.5
                      printf " \b" ; do! delaySec 0.5
      | Dots       -> printf "."   ; do! delaySec 0.5
      | NoPulse    -> () }

let cancelAfterDelay cts cancelSec =
  let worker() =
    task { do!  delaySec cancelSec } |> Task.WaitAll
    (cts: CancellationTokenSource).Cancel()
  Task.Run worker

let delayThenCancel delaySec =
  let cts = new CancellationTokenSource()
  cancelAfterDelay cts 3 |> ignore
  cts

do
  let pulse = NoPulse
  match pulse with
  | FakeCursor -> printfn "Flashing * cursor enabled."
  | Dots       -> printfn "Flashing dots enabled."
  | NoPulse    -> printfn "Quiet mode enabled."

  let readNTimes n prompt =
    use readLine = new ConsoleReadAsync()
    let read n = task {
      let promptN = $"{n} {prompt}"
      printfn $"You have 2 seconds to type ahead."
      do! delaySec 2
      use cts = delayThenCancel 4
      let! line1 = readLine.readLineAsync(promptN, cts.Token)
      let timeout() = printfn "\nTIMEOUT!  You didn’t enter anything."
      match line1 with
      | None -> timeout()
      | Some line1 ->
        use cts = delayThenCancel 4
        let! line2 = readLine.readLineAsync cts.Token
        match line2 with
        | None -> timeout()
        | Some line2 ->
          printfn "You entered ‘%s’ then ‘%s’." line1 line2  }
    task { for i in 1..n do  do! read i  } |> Task.WaitAll

  use ctsBackground = new CancellationTokenSource()
  let pulseContinuouslyBackground _ = pulseContinuously pulse ctsBackground.Token 
  Task.Run(pulseContinuouslyBackground) |> ignore

  readNTimes 3 "Enter something: "

  ctsBackground.Cancel()
  match pulse with
  | FakeCursor -> printfn "No more flashing ‘*’."
  | Dots       -> printfn "No more dots."
  | NoPulse    -> ()

  printfn "Type Return to exit."
  System.Console.ReadKey() |> ignore
  printfn "Done"
