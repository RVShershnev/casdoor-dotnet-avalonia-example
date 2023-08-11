using System;

namespace Casdoor.AvaloniaOidcClient.Example;

public class CodeReceivedEventArgs : EventArgs
{
    public CodeReceivedEventArgs(string code) => Code = code;

    public string Code { get; }
}
