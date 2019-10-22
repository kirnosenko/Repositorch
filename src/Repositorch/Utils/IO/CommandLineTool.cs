using System.Diagnostics;

namespace System.IO
{
	public abstract class CommandLineTool
	{
		protected Stream GetCommandOutput(string cmd, string arguments)
		{
			MemoryStream buf = new MemoryStream();
			TextWriter writer = new StreamWriter(buf);

			if (RunAndWaitForExit(cmd, arguments, line => writer.WriteLine(line)))
			{
				writer.Flush();
				buf.Seek(0, SeekOrigin.Begin);
				return buf;
			}

			return null;
		}
		protected bool RunAndWaitForExit(string command, string arguments, Action<string> lineOutput)
		{
			ProcessStartInfo psi = new ProcessStartInfo(command, arguments);
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;

			Process process = new Process();
			process.StartInfo = psi;
			process.OutputDataReceived += (s, e) =>
			{
				if (e.Data != null)
				{
					lineOutput(e.Data);
				}
			};
			process.Start();
			process.BeginOutputReadLine();
			process.WaitForExit();
			return process.ExitCode == 0;
		}
	}
}
