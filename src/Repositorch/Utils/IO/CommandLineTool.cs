using System.Diagnostics;

namespace System.IO
{
	public abstract class CommandLineTool
	{
		protected Stream GetCommandOutput(string cmd, string arguments)
		{
			var output = RunAndWaitForExit(cmd, arguments);
			if (output != null)
			{
				MemoryStream buf = new MemoryStream();
				TextWriter writer = new StreamWriter(buf);
				writer.Write(output);
				writer.Flush();
				buf.Seek(0, SeekOrigin.Begin);

				return buf;
			}

			return null;
		}

		protected string RunAndWaitForExit(string command, string arguments)
		{
			ProcessStartInfo psi = new ProcessStartInfo(command, arguments);
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;

			Process process = new Process();
			process.StartInfo = psi;
			process.Start();
			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			
			return process.ExitCode == 0 ? output : null;
		}
	}
}
