using System.Diagnostics;
using System.Text;

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
			psi.RedirectStandardError = true;

			StringBuilder error = new StringBuilder();
			Process process = new Process();
			process.StartInfo = psi;
			process.ErrorDataReceived += (s, e) =>
			{
				if (e.Data != null)
				{
					error.AppendLine(e.Data);
				}
			};
			process.Start();
			process.BeginErrorReadLine();
			var output = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			
			if (process.ExitCode == 0) return output;

			throw new ApplicationException(
				$"{command} {arguments} exit code is {process.ExitCode}." +
				(error.Length > 0 ? Environment.NewLine + error.ToString() : string.Empty));
		}
	}
}
