///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 按钮事件委托,参数为触发的按钮实例以及需要的附带信息
/// </summary>
/// <param name="btn">被点击的按钮实例</param>
/// <param name="args">额外信息</param>
/// <param name="holder">按钮所在的页面</param>
public delegate void ButtonActionCallback(Button btn,Canvas holder,object args = null);

/// <summary>
/// 连接上服务器
/// </summary>
public delegate void ServerConnected();
