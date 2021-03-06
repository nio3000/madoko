/*---------------------------------------------------------------------------
  Copyright 2013-2015 Microsoft Corporation.
 
  This is free software; you can redistribute it and/or modify it under the
  terms of the Apache License, Version 2.0. A copy of the License can be
  found in the file "license.txt" at the root of this distribution.
---------------------------------------------------------------------------*/

using System.Diagnostics;
using System.Threading;

public static class ShellCommand
{
  // cmd : string, callback : (int,string,string) -> <io|e> (), timeout : int = 0, cwd : string = "") : <io|e> () {
  public static object system( string cmd, Fun3<int,string,string,Unit> callback, int timeout, string cwd ) {
    ProcessStartInfo pinfo = new ProcessStartInfo();
    pinfo.UseShellExecute = true;
    if (cwd != null && cwd.Length > 0) pinfo.WorkingDirectory = cwd;
    pinfo.FileName = "cmd.exe";
    pinfo.Verb = "runas";
    pinfo.Arguments = "/c " + cmd;
    pinfo.WindowStyle = ProcessWindowStyle.Hidden;
    pinfo.RedirectStandardError = true;
    pinfo.RedirectStandardOutput = true;
    Process process = new Process();
    process.StartInfo = pinfo;
    process.EnableRaisingEvents = true;
    process.Exited += new EventHandler( delegate(object sender, System.EventArgs args ) {
      string stdout = process.StandardOutput.ReadToEnd();
      string stderr = process.StandardError.ReadToEnd();
      callback.Apply( process.ExitCode, stdout, stderr );
      return;
    });
    process.Start();
    if (timeout > 0) {
      new Timer( new TimerCallback( delegate( object state ) { 
                    if (!process.HasExited) {
                      try {
                        process.Kill(); 
                      } 
                      catch(Exception) {}
                    }
                 }),
                 null, timeout, Timeout.Infinite );
    }
    return Unit.unit;
  }
}
