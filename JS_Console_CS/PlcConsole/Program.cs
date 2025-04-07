using PlcConsole;

ManualResetEvent quitEvent = new ManualResetEvent(false);

Console.WriteLine("프로그램 시작...");

PlcComm plcComm = new PlcComm();
Modbus modbus = new Modbus();
WebSock webSock = new WebSock();

Console.WriteLine("접속 URL ws://127.0.0.1:8080");
Console.WriteLine("프로그램 종료 F4");

_ = Task.Run(() =>
{
    while (!(Console.ReadKey(true).Key == ConsoleKey.F4)) ;
    quitEvent.Set();
});

quitEvent.WaitOne();
Console.WriteLine("프로그램이 종료 됩니다.");
