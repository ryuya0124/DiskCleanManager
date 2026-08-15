using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DiskCleanManager.Services;

public record CommandResult(bool Success, string Output, string Error, int ExitCode);

public static class CommandRunnerService
{
    /// <summary>
    /// PowerShell 経由でコマンドを実行し、stdout / stderr をキャプチャして返す。
    /// </summary>
    public static async Task<CommandResult> RunAsync(
        string command,
        CancellationToken ct = default)
    {
        var psCommand = $"[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; $OutputEncoding = [System.Text.Encoding]::UTF8; {command}";
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -Command \"{EscapeForPowerShell(psCommand)}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8,
        };

        using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
        var stdout = new StringBuilder();
        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data != null) stdout.AppendLine(e.Data); };
        process.ErrorDataReceived  += (_, e) => { if (e.Data != null) stderr.AppendLine(e.Data); };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { }
            throw;
        }

        return new CommandResult(
            Success: process.ExitCode == 0,
            Output: stdout.ToString().Trim(),
            Error: stderr.ToString().Trim(),
            ExitCode: process.ExitCode);
    }

    private static string EscapeForPowerShell(string cmd)
        => cmd.Replace("\"", "\\\"");
}
