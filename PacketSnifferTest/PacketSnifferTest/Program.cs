using PacketSnifferTest;
using SharpPcap;
using SharpPcap.LibPcap;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

Process CurrentProcess = null;
Process ipProcess;
List<NetStatModel> list = new List<NetStatModel>();



void GetProcess(){
    var name = Encoding.UTF8.GetString(new byte[] { 68, 50, 82 });
    Process[] processList = Process.GetProcesses();

    foreach (Process theProcess in processList)
    {
        string processName = theProcess.ProcessName;

        if (processName.Equals(name, StringComparison.OrdinalIgnoreCase))
        {
            CurrentProcess = theProcess;
        }
    }
}

void GetIp(){
    string command = "/C netstat -a -n -o";
    ipProcess = new Process();
    ipProcess.StartInfo.FileName = "cmd.exe";
    ipProcess.StartInfo.RedirectStandardOutput = true;
    //ipProcess.StartInfo.Verb = "runas";
    ipProcess.StartInfo.Arguments = command;
    ipProcess.StartInfo.CreateNoWindow = true;
    ipProcess.StartInfo.UseShellExecute = false;
    ipProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
    ipProcess.OutputDataReceived += NetstatOutput;
    ipProcess.Start();
    ipProcess.BeginOutputReadLine();
    ipProcess.WaitForExit();

    ipProcess.Close();
    list = list.Where(w => w.DestIp.EndsWith(":123")).ToList();
    //list = list.Where(w => w.DestIp.EndsWith(":443") && w.Pip == CurrentProcess.Id && !w.DestIp.StartsWith("127") && w.State.Equals("ESTABLISHED")).ToList();

}
void NetstatOutput(object sender, DataReceivedEventArgs e)
{
    var modelOut = TryParse(e.Data);  
    if (modelOut != null) {
        list.Add(modelOut);
    }
}

NetStatModel TryParse(string data)
{
    NetStatModel modelOut = null;
    if (!string.IsNullOrEmpty(data))
    {
        data = data.Trim();
        if(data[0] == 'T' || data[0] == 'U')
        {
            modelOut = new NetStatModel();
            var columns = data.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            modelOut.Type = columns[0];
            modelOut.SrcIP = columns[1];
            modelOut.DestIp = columns[2];

            if ( columns.Length == 4)
            {
                int v = Convert.ToInt32(columns[3].Trim());
                modelOut.Pip = v;
            }
            else
            {
                int v = Convert.ToInt32(columns[4].Trim());
                modelOut.Pip = v;
                modelOut.State = columns[3];
            }
        }
    }
    return modelOut;
}
List<string> ignoreIps = new List<string>
{
    "37.244.28.180:443",
    "24.105.29.76:443",
    "34.117.122.6:443"
};

//var listener = new PcapListener("0.0.0.0", 9443);
GetProcess();
GetIp();
List<PcapListener> listeners = new List<PcapListener>();
foreach (var item in list)
{
    var ip = item.DestIp.Split(":", StringSplitOptions.RemoveEmptyEntries);
    if (NotIgnored(item.DestIp))
    {
        listeners.Add(new PcapListener(GetDevice(item.SrcIP, item.DestIp),ip[0], Int32.Parse(ip[1])));
    }
}

LibPcapLiveDevice GetDevice(string srcIp,string dstIP)
{
    var ip = srcIp.Split(":", StringSplitOptions.RemoveEmptyEntries)[0];
    var dip = dstIP.Split(":", StringSplitOptions.RemoveEmptyEntries)[0];

    var devices = LibPcapLiveDeviceList.Instance.FirstOrDefault(w => w.Addresses.FirstOrDefault(w =>
    {
        if (w.Addr != null && w.Addr.ipAddress!=null)
        {
            return w.Addr.ipAddress.ToString().Equals(ip) || w.Addr.ipAddress.ToString().Equals(dip);
        }
        return false;
    }) != null) ;
    
    return devices;
}

bool NotIgnored(string ip)
{
    return !ignoreIps.Contains(ip);
}

await Task.Run(async () =>
{
    do
    {
        await Task.Delay(TimeSpan.FromHours(1));
    } while (true);
});

