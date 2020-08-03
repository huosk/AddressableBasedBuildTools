using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IPipePhase
{
    Task<bool> Process(PipeContext context);
}

/// <summary>
/// 打包管线处理完毕之后的回调
/// </summary>
public interface IPipeCompleteCallback
{
    void OnPipeComplete(bool succes);
}

public abstract class APipePhase : IPipePhase
{
    public abstract Task<bool> Process(PipeContext context);
}