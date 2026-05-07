using System;
using System.ServiceProcess;
using FileMirror.Service;

namespace FileMirror.Service;

public static class Program
{
    public static void Main(string[] args)
    {
        ServiceBase.Run(new FileMirrorService());
    }
}
