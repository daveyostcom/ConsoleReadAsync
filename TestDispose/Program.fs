
open System
open System.Threading
open System.Threading.Tasks

let hang() = task {
  use readLine = new ConsoleReadAsync.ConsoleReadAsync()
  readLine.readLineAsync "[prompt]> " |> ignore
  Task.Delay 2000 |> Task.WaitAll 
  Console.WriteLine "\nreadAsync() returned." }
  
do
  use cts = new CancellationTokenSource()
  let t = hang()
  t |> Task.WaitAll
  Console.WriteLine "Type a key to exit."
  Console.ReadKey() |> ignore
  Task.Delay 2000 |> Task.WaitAll 
  Console.WriteLine "Test Done"
