using System;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;
using static Assets.Scripts.RenderMode;

public class MainControl : MonoBehaviour {

    #region Properties
    /// <summary>
    /// 主视角相机
    /// </summary>
    public Camera MainCamera;
    public GameObject LOC_ID;

    /// <summary>
    /// 俯视图相机
    /// </summary>
    public Camera TopCamera;
    private Vector3 _targetpos = new Vector3(0, 1.6f, 0);
    private Quaternion _targetangle = Quaternion.Euler(0, 0, 0);

    /// <summary>
    /// 客户端控制台
    /// </summary>
    public Text Log;
    public GameObject SettingPanel;

    /// <summary>
    /// 用于线程间通信的委托
    /// </summary>
    private Action _uiActions;

    /// <summary>
    /// 滑动旋转因子
    /// </summary>
    private float _movefactor = 0.48f;
    private float _movefactor_1 = 0.12f;

    /// <summary>
    /// 滑动缩放因子
    /// </summary>
    private float _scalefactor = 0.05f;
    private float _scalefactor_1 = 0.04f;

    /// <summary>
    /// 相机移动因子
    /// </summary>
    private float _cameramovefactor = 2.4f;
    private float _camerarotatefactor = 1.2f;

    /// <summary>
    /// 上一次的手指距离
    /// </summary>
    private float _lastdis = 0;

    /// <summary>
    /// 判断开始双指触控
    /// </summary>
    private bool _scalestart = false;

    /// <summary>
    /// 当前加载的楼层模型
    /// </summary>
    private GameObject _tempmodel = null;
    private string _tempmodelname = "L1";
    private int _templayer = 0;

    /// <summary>
    /// 上传信息给服务器
    /// </summary>
    private IInfoProductor _Uploader;

    /// <summary>
    /// 当前视角
    /// </summary>
    private int _tempviewmode = 0;
    private bool _viewbtnvis = true;
    private Button _viewbtn;

    /// <summary>
    /// 用于反馈连接情况的计时器
    /// </summary>
    private Timer _buffertimer;
    private string _buffertitle;
    private int _calcu;

    #endregion 

    void Start() {
        Input.multiTouchEnabled = true;
        _Uploader = new TestInfoProductor();
        _Uploader.DataSended += _Uploader_DataSended;
        LoadModel("L1");
        ControlManager.ButtonEvent += ControlManager_ButtonEvent;
        _viewbtn = ControlManager.Buttons["ModelPage"].Find((btnn) => { return btnn.name.Equals("ChangeView"); });
        SwitchCamera(0);
        SettingPanel.SetActive(false);
        _buffertitle = "连接服务器";
        //异步连接服务器
        Task.Run(() => {
            LogBuffer(true);
            try {
                if (InterProcSocket.Singleton.Value.Connect())
                    InterProcSocket.Singleton.Value.MessageReceived += Value_MessageReceived;
            }
            catch {
                LogBuffer(false);
                _uiActions += new Action(() => {
                    Log.text = "服务器连接失败";
                });
            }
            finally {
                LogBuffer(false);
            }
        });
    }

    /// <summary>
    /// 在此处控制所有按钮事件
    /// </summary>
    /// <param name="btn">被激活的按钮实例</param>
    /// <param name="args">附带参数</param>
    private void ControlManager_ButtonEvent(Button btn, Canvas holder, object args) {
        switch (btn.name) {
            //TODO::接收异步通信消息
            case "StartLocation":
                _buffertitle = "定位中";
                Task.Run(() => {
                    LogBuffer(true);
                    try {
                        if (!InterProcSocket.Singleton.Value.Connected) {
                            InterProcSocket.Singleton.Value.Connect();
                            InterProcSocket.Singleton.Value.MessageReceived -= Value_MessageReceived;
                            InterProcSocket.Singleton.Value.MessageReceived += Value_MessageReceived;
                        }
                        _Uploader.UpLoad();
                    }
                    catch {
                        LogBuffer(false);
                        _uiActions += new Action(() => {
                            Log.text = "定位失败，请检查网络连接";
                        });
                    }
                });
                break;
            case "ChangeView":
                _tempviewmode = _tempviewmode == 0 ? 1 : 0;
                btn.image.sprite = ControlManager.ButtonTextures[btn][_tempviewmode == 0 ? 0 : 1];
                SwitchCamera(_tempviewmode);
                break;
            case "Shell":
                _viewbtnvis = !_viewbtnvis;
                _viewbtn.gameObject.SetActive(_viewbtnvis);
                break;
            case "Setting":
                SettingPanel.SetActive(!SettingPanel.activeSelf);
                break;
            case "Submit":
                _buffertitle = "重新连接服务器";
                Task.Run(() => {
                    try {
                        InterProcSocket.Singleton.Value.Connect(ControlManager.Inputs["Input_IP_Test"].text);
                        InterProcSocket.Singleton.Value.MessageReceived -= Value_MessageReceived;
                        InterProcSocket.Singleton.Value.MessageReceived += Value_MessageReceived;
                    }
                    catch {
                        _uiActions += new Action(() => {
                            Log.text = "连接失败，请检查网络连接";
                        });
                    }
                });
                break;
            default: break;
        }
    }

    /// <summary>
    /// 切换视角相机
    /// </summary>
    private void SwitchCamera(int cameraid) {
        switch (cameraid) {
            case 0://Main
                MainCamera.enabled = true;
                TopCamera.enabled = false;
                LOC_ID.SetActive(false);
                UnloadModel();
                LoadModel(_tempmodelname);
                break;
            case 1://Top
                TopCamera.enabled = true;
                MainCamera.enabled = false;
                LOC_ID.SetActive(true);
                UnloadModel();
                LoadModel("Xinxi", 56);
                break;
            case 2://Pers

                LOC_ID.SetActive(false);
                break;
        }
    }

    /// <summary>
    /// 加载StreamingAssets中的模型
    /// </summary>
    /// <param name="name">模型名称</param>
    private void LoadModel(string name, int transparent = 100) {
        //资源加载 加载的是打包到资源包里面，每个资源的名字
        _tempmodel = Instantiate(ResourceManager.GetResource<GameObject>("/model", name)) as GameObject;
        _tempmodel.transform.position = Vector3.zero;
        _tempmodel.transform.localScale = new Vector3(400, 400, 400);
        //取消阴影显示
        foreach (MeshRenderer render in _tempmodel.GetComponentsInChildren<MeshRenderer>()) {
            render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        //if(transparent != 100) {
        //    foreach (MeshRenderer render in _tempmodel.GetComponentsInChildren<MeshRenderer>()) {
        //        SetMaterialsColor(render, transparent / 100);
        //    }
        //}
    }

    /// <summary>
    /// 从场景中移除当前模型
    /// </summary>
    /// <param name="name">模型名称</param>
    private void UnloadModel() {
        Destroy(_tempmodel);
    }

    /// <summary>
    /// 用户通过单指滑动旋转视角
    /// 双指滑动放缩视图
    /// </summary>
    private void TouchMove() {
        Vector2 _spoint = Vector2.zero;
        if (Input.touchCount <= 0) return;
        else if (Input.touchCount == 1) {
            if (Input.touches[0].phase == TouchPhase.Began) {
                _spoint = Input.touches[0].position;
            }
            else if (Input.touches[0].phase == TouchPhase.Moved) {
                if (_tempviewmode == 0) {
                    //第一人称时触屏滑动操作
                    MainCamera.transform.Rotate(new Vector3(0, Input.touches[0].deltaPosition.x * _movefactor, 0));
                    _targetangle = MainCamera.transform.rotation;
                }
                else if (_tempviewmode == 1) {
                    //第三人称时触屏滑动操作
                    TopCamera.transform.Translate(new Vector3(-Input.touches[0].deltaPosition.x * _movefactor_1, -Input.touches[0].deltaPosition.y * _movefactor_1, 0));
                }
                else {

                }
                _scalestart = true;
            }
        }
        else if (Input.touchCount == 2) {
            if (_scalestart)
                _lastdis = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);

            if (Input.touches[0].phase == TouchPhase.Moved || Input.touches[1].phase == TouchPhase.Moved) {
                float newdis = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                float _scale = 1;
                if (_tempviewmode == 0) {
                    //第一人称时触屏放缩操作
                    _scale = _lastdis - newdis > 0 ? 1 + _scalefactor : 1 - _scalefactor;
                    MainCamera.fieldOfView = MainCamera.fieldOfView * _scale < 24 ? 24 : MainCamera.fieldOfView * _scale > 72 ? 72 : MainCamera.fieldOfView * _scale;
                }
                else if (_tempviewmode == 1) {
                    //第三人称时触屏放缩操作
                    _scale = _lastdis - newdis > 0 ? 1 + _scalefactor_1 : 1 - _scalefactor_1;
                    TopCamera.orthographicSize = TopCamera.orthographicSize * _scale > 160 ? 160 : TopCamera.orthographicSize * _scale < 20 ? 20 : TopCamera.orthographicSize * _scale;
                }
                else {

                }
                _lastdis = newdis;
            }
            _scalestart = false;
        }
    }

    /// <summary>
    /// 反馈窗口反馈缓冲情况
    /// </summary>
    private void LogBuffer(bool on) {
        if (_buffertimer == null)
            _buffertimer = new Timer(400);
        lock (_buffertimer) {
            if (on) {
                if (_buffertimer.Enabled)
                    return;
                _buffertimer.Elapsed -= DrawBuffer;
                _buffertimer.Elapsed += DrawBuffer;
                _buffertimer.Enabled = true;
            }
            else {
                if (_buffertimer == null)
                    return;
                _buffertimer.Enabled = false;
            }
        }
    }

    /// <summary>
    /// 异步绘制
    /// </summary>
    private void DrawBuffer(object sender, ElapsedEventArgs e) {
        string dot = "";
        for (int i = 0; i < _calcu; i++)
            dot += "·";
        _calcu = _calcu + 1 > 5 ? 0 : _calcu + 1;
        _uiActions += new Action(() => {
            Log.text = _buffertitle + dot;
        });
    }

    /// <summary>
    /// 接收服务端消息
    /// </summary>
    /// <param name="msg">消息文本</param>
    private void Value_MessageReceived(string msg) {
        _uiActions += new Action(() => {
            string[] vector = msg.Split(',');
            if (vector.Length != 3) {
                if (vector[0].Equals(InterProcSocket.DISCONNECT)) {
                    Log.text = "Server Offline !";
                }
                return;
            }
            if (int.Parse(vector[2]) != _templayer && int.Parse(vector[2]) <= 2) {
                _templayer = int.Parse(vector[2]);
                UnloadModel();
                LoadModel("L" + (_templayer + 1).ToString());
                _tempmodelname = "L" + (_templayer + 1).ToString();
            }
            int x, y, z;
            x = int.Parse(vector[0]);
            y = int.Parse(vector[1]);
            z = int.Parse(vector[2]);
            Vector3 point = PositionManager.Singleton.Value.GetPosition(x, y, z);
            _targetpos = point;
            int angle = PositionManager.Singleton.Value.GetFirendlyCameraDirection(x, y, z) == 1 ? 270 : 180;
            if (y > 15)
                angle -= 180;
            _targetangle = Quaternion.Euler(0, angle, 0);
            Log.text = $"服务器 : 楼层 => {z + 1}  坐标 => ( {x} - {y} )";
        });
    }

    /// <summary>
    /// 消息发送成功回调
    /// </summary>
    private void _Uploader_DataSended() {
        LogBuffer(false);
        _uiActions += new Action(() => {
            Log.text = "服务器已连接!";
        });
    }

    /// <summary>
    /// 重定位摄像机
    /// </summary>
    private void CameraRelocate() {
        if (MainCamera.transform.position != _targetpos) {
            MainCamera.transform.position = Vector3.Lerp(MainCamera.transform.position, _targetpos, Time.deltaTime * _cameramovefactor);
        }
        if (MainCamera.transform.rotation != _targetangle) {
            MainCamera.transform.rotation = Quaternion.Lerp(MainCamera.transform.rotation, _targetangle, Time.deltaTime * _camerarotatefactor);
        }
    }

    /// <summary>
    /// 帧事件
    /// </summary>
    private void Update() {
        //执行通信回调事件：定位
        if (_uiActions != null) {
            _uiActions.Invoke();
            _uiActions = null;
        }
        CameraRelocate();
        TouchMove();
    }

    /// <summary>
    /// 修改遮挡物体所有材质
    /// </summary>
    /// <param name="_renderer">材质</param>
    /// <param name="Transpa">透明度</param>
    private void SetMaterialsColor(Renderer _renderer, float Transpa) {
        //换shader或者修改材质

        //获取当前物体材质球数量
        int materialsNumber = _renderer.sharedMaterials.Length;
        for (int i = 0; i < materialsNumber; i++) {
            SetMaterialRenderingMode(_renderer.materials[i], RenderingMode.Transparent);
            //获取当前材质球颜色
            Color color = _renderer.materials[i].color;
            //设置透明度  0-1;  0 = 完全透明
            color.a = Transpa;
            //置当前材质球颜色
            _renderer.materials[i].SetColor("_Color", color);
        }

    }

}
