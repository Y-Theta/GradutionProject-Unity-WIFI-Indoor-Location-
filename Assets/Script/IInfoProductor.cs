///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 获取wifi信息的接口
/// 因为测试时要使用模拟数据，而实际应用中要获取当前传感器数据，故通过接口抽离逻辑
/// </summary>
public interface IInfoProductor {
    /// <summary>
    /// 向服务端发送指纹信息
    /// </summary>
    void UpLoad();

    /// <summary>
    /// 信息发送成功
    /// </summary>
    event ServerConnected DataSended;
}
