using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRestartManager : UnitySingleton<GameRestartManager>
{
    // 静态实例用于全局访问
    public override void Awake()
    {
        base.Awake();
    }

    // 重新开始游戏的方法
    public void RestartGame()
    {
        // 1. 停止所有协程（关键步骤！）
        StopAllCoroutines();
        
        // 2. 重置时间缩放（避免从暂停状态恢复）
        Time.timeScale = 1f;
        
        // 3. 关闭所有UI界面（避免残留UI）
        if (UIManager.instance != null)
        {
            //UIManager.instance.CloseAllUI();
        }
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 完全退出游戏的方法（可选）
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
