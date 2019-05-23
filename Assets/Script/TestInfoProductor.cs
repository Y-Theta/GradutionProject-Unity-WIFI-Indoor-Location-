///------------------------------------------------------------------------------
/// @ Y_Theta
///------------------------------------------------------------------------------
using UnityEngine;
using Random = System.Random;

class TestInfoProductor : IInfoProductor {
    #region Properties
    private string[] _dataset;

    public event ServerConnected DataSended;
    #endregion

    #region Methods
    public void UpLoad() {
        InterProcSocket.Singleton.Value.PostData(_dataset[new Random().Next(0, 10000)].Substring(1));
        DataSended?.Invoke();
    }

    private void LoadFile() {
        string data = ResourceManager.GetResource<TextAsset>("/text", "testData").text;
        _dataset = data.Split('\n');
    }
    #endregion

    public TestInfoProductor() {
        LoadFile();
    }
}

