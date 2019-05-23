using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 统一管理控件逻辑
/// </summary>
public class ControlManager : MonoBehaviour {

    /// <summary>
    /// 交互页面列表
    /// 类似WPF中的page
    /// </summary>
    private static Dictionary<string, Canvas> _pagemap;
    public static Dictionary<string, Canvas> Pages {
        get { return _pagemap; }
    }

    /// <summary>
    /// 对应page中的按钮集合
    /// </summary>
    private static Dictionary<string, List<Button>> _buttonmap;
    public static Dictionary<string, List<Button>> Buttons {
        get { return _buttonmap; }
    }

    /// <summary>
    /// 输入集合
    /// </summary>
    public static Dictionary<string, InputField> _inputmap;
    public static Dictionary<string, InputField> Inputs {
        get { return _inputmap; }
    }

    /// <summary>
    /// 按钮贴图列表
    /// </summary>
    private static Dictionary<Button, List<Sprite>> _buttonTextures;
    public static Dictionary<Button, List<Sprite>> ButtonTextures {
        get { return _buttonTextures; }
    }

    /// <summary>
    /// 其它脚本中通过订阅事件处理按钮消息
    /// </summary>
    public static event ButtonActionCallback ButtonEvent;

    void Start() {
        _pagemap = new Dictionary<string, Canvas>();
        _inputmap = new Dictionary<string, InputField>();
        _buttonmap = new Dictionary<string, List<Button>>();
        _buttonTextures = new Dictionary<Button, List<Sprite>>();

        foreach (var page in FindObjectsOfType<Canvas>()) {
            _pagemap.Add(page.name, page);
            var buttons = page.GetComponentsInChildren<Button>().ToList();
            _buttonmap.Add(page.name, buttons);
            foreach (Button b in buttons) {
                b.onClick.AddListener(() => {
                    ButtonEvent?.Invoke(b, page);
                });
                switch (b.name) {
                    case "ChangeView":
                        ButtonTextures.Add(b, ResourceManager.GetResourceList<Sprite>("/texture", new string[] { "ViewMode_F", "ViewMode_T" }));
                        break;
                }
            }
            var inputs = page.GetComponentsInChildren<InputField>().ToList();
            foreach (InputField ipt in inputs) {
                _inputmap.Add(ipt.name, ipt);
            }
        }
    }

    

}
