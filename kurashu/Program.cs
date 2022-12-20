using System.Diagnostics;
using System.Media;
using ThreadState = System.Diagnostics.ThreadState;

// See https://aka.ms/new-console-template for more information

List<int> trackedProcessIds = new();


while (true)
{
    Thread.Sleep(1000);
    GetHungProcesses();
}

void GetHungProcesses()
{
    var processes = Process.GetProcesses();
    foreach (var p in processes)
    {
        p.Refresh();
    }

    var hung = processes.Where(p => !p.Responding &&
                                    !p.Threads.OfType<ProcessThread>()
                                        .Any(t => t.ThreadState == ThreadState.Wait &&
                                                  t.WaitReason == ThreadWaitReason.Suspended));
    foreach (var p in hung)
    {
        Console.WriteLine($"{p.ProcessName} ({p.Id}) {p.MainModule.FileName} is hung");
        if (!trackedProcessIds.Contains(p.Id))
        {
            trackedProcessIds.Add(p.Id);
            p.EnableRaisingEvents = true;
            Console.WriteLine("print");
            p.Exited += ((sender, eventArgs) =>
            {
                trackedProcessIds.Remove(p.Id);
                Console.WriteLine("exited event handler");
                var fs = File.Open(@"z.wav", FileMode.Open);
                var sp = new SoundPlayer(fs);
                sp.Play();
                
            });
        }
    }
    foreach (var p in processes.Where(p => !trackedProcessIds.Contains(p.Id)))
    {
        p.Dispose();
    }
}