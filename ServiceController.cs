//# ServiceController
//Get and monitor services

//For each service the following first checks to see if the service is running. If so, it gets the service's processId and uses //ManagementObjectSearch to retrieve the corresponding process object. From there it calls GetOwner(out string user, out string domain) // from the underlying Win32_Process object, and outputs the result if the call was successful.

// The code below worked locally, however I don't have the access to test this against a remote computer. Even locally I had to run the // application as an administrator. for GetOwner to not return an error result of 2 (Access Denied).

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;

var services = ServiceController.GetServices("MyRemotePC");
var getOptions = new ObjectGetOptions(null, TimeSpan.MaxValue, true);
var scope = new ManagementScope(@"\\MyRemotePC\root\cimv2");

foreach (ServiceController service in services)
{
    Console.WriteLine($"The {service.DisplayName} service is currently {service.Status}.");

    if (service.Status != ServiceControllerStatus.Stopped)
    {
        var svcObj = new ManagementObject(scope, new ManagementPath($"Win32_Service.Name='{service.ServiceName}'"), getOptions);
        var processId = (uint)svcObj["ProcessID"];
        var searcher = new ManagementObjectSearcher(scope, new SelectQuery($"SELECT * FROM Win32_Process WHERE ProcessID = '{processId}'"));
        var processObj = searcher.Get().Cast<ManagementObject>().First();
        var props = processObj.Properties.Cast<PropertyData>().ToDictionary(x => x.Name, x => x.Value);
        string[] outArgs = new string[] { string.Empty, string.Empty };
        var returnVal = (UInt32)processObj.InvokeMethod("GetOwner", outArgs);
        if (returnVal == 0)
        {
            var userName = outArgs[1] + "\\" + outArgs[0];
            Console.WriteLine(userName);
        }
    }
}
