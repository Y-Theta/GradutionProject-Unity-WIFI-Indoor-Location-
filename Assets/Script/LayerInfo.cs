///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using System.Collections.Generic;
using UnityEngine;

internal class LayerInfo {

    #region Properties

    /// <summary>
    /// 楼层信息y
    /// </summary>
    private Dictionary<Vector2Int, Vector2> _layerinfo;

    /// <summary>
    /// 相机友好方向映射
    /// </summary>
    public int[] DirectionMap;

    #endregion

    #region Methods

    #endregion

    #region Constructors

    public LayerInfo() {
        _layerinfo = new Dictionary<Vector2Int, Vector2>();
    }

    /// <summary>
    /// 添加点映射
    /// </summary>
    public Vector2 this[Vector2Int key] {
        get {
            return _layerinfo[key];
        }
        set {
            if (_layerinfo.ContainsKey(key))
                return;
            else
                _layerinfo.Add(key, value);
        }
    }
    #endregion
}

