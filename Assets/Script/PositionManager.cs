///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System;
using System.Xml;
using UnityEngine;

/// <summary>
/// 用于将定位坐标转化为模型坐标的类
/// </summary>
public class PositionManager {
    #region Propertiesd
    /// <summary>
    /// 单例
    /// </summary>
    public static Lazy<PositionManager> Singleton = new Lazy<PositionManager>();

    /// <summary>
    /// 相机高度
    /// </summary>
    private float _cameraheight = 1.6f;

    /// <summary>
    /// 楼层信息
    /// </summary>
    private LayerInfo[] _layerinfos;
    #endregion

    #region Methods
    /// <summary>
    /// 通过预定义序号获得模型坐标
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public Vector3 GetPosition(int x, int y, int z) {
        try {
            LayerInfo now = _layerinfos[z];
            Vector2Int key = new Vector2Int(x, y);
            return new Vector3(now[key].x, _cameraheight, now[key].y);
        }
        catch {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// 获得相机的友好视角轴
    /// </summary>
    /// <returns></returns>
    public int GetFirendlyCameraDirection(int x, int y, int z) {
        try {
            LayerInfo now = _layerinfos[z];
            return now.DirectionMap[x];
        }
        catch {
            return 0;
        }
    }

    /// <summary>
    /// 装填信息字典
    /// </summary>
    private void LoadLayerInfo() {
        _layerinfos = new LayerInfo[3];
        XmlDocument pointsmap = new XmlDocument();
        pointsmap.LoadXml(ResourceManager.GetResource<TextAsset>("/xml", "PointsMap").text);

        XmlElement root = pointsmap.DocumentElement;

        if (root.HasChildNodes) {
            XmlNodeList floor = root.ChildNodes;
            for (int i = 0; i < floor.Count; i++) {
                _layerinfos[i] = new LayerInfo();
                XmlNodeList pointxs = floor[i].ChildNodes;
                _layerinfos[i].DirectionMap = new int[pointxs.Count];
                for (int j = 0; j < pointxs.Count; j++) {
                    XmlNode pointx = pointxs[j];
                    string constaxis =  pointx.Attributes["constaxis"].Value.ToString();
                    float constvalue = float.Parse(pointx.Attributes["value"].Value.ToString());
                    string variableaxis = constaxis == "z" ? "x" : "z";
                    _layerinfos[i].DirectionMap[j] = constaxis == "z" ? 1 : 0;
                    XmlNodeList pointys = pointxs[j].ChildNodes;
                    for (int k = 0; k < pointys.Count; k++) {
                        if(variableaxis == "x") {
                            _layerinfos[i][new Vector2Int(j, k)] = new Vector2(float.Parse(pointys[k].Attributes["x"].Value), constvalue);
                        }
                        else {
                            _layerinfos[i][new Vector2Int(j, k)] = new Vector2(constvalue, float.Parse(pointys[k].Attributes["z"].Value));
                        }
                    }
                }
            }
        }

        pointsmap = null;
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void Init() {
        LoadLayerInfo();
    }
    #endregion

    #region Constructors
    public PositionManager() { Init(); }
    #endregion
}

