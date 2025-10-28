using UnityEngine;
using UnityEngine.SceneManagement;

public class Generic : MonoBehaviour
{
    /// <summary>
    /// 查找场景中的节点
    /// </summary>
    /// <param name="sNodeName"></param>
    /// <returns></returns>
    static public Transform GetNodeInScene(string sNodeName)
    {
        if (sNodeName == null)//避免空值报错
        {
            return null;
        }

        GameObject kGameObject = GameObject.Find(sNodeName);//快速查找活跃对象
        if (kGameObject != null)
        {
            return kGameObject.transform;//如果能找到，直接返回
        }
        
        //获取当前场景的所有顶层对象，并存入数组
        GameObject[] aSceneNode = SceneManager.GetActiveScene().GetRootGameObjects();
        
        foreach (GameObject KSceneNode in aSceneNode)//将aSceneNode的每个对象依次赋值给KSceneNode
        {
            Transform kNode = GetNode(KSceneNode.transform, sNodeName);//调用递归查找子节点
            if (kNode != null)
            {
                return kNode;
            }
        }
        return null;
    }
    /// <summary>
    /// 查找某一个物体下的节点
    /// </summary>
    /// <param name="kNode">父节点</param>
    /// <param name="sNodeName">子节点</param>
    /// <returns></returns>
    static public Transform GetNode(Transform kNode, string sNodeName)//使用该方法的好处在于，可以查找到隐藏的物体
    {
        Transform kNode2 = kNode.Find(sNodeName);//在KNode的子节点中查找
        if (kNode2 != null)
        {
            return kNode2;//如果找到了，直接返回
        }


        for (int i = 0; i < kNode.childCount; i++)//遍历KNode的子节点
        {
            kNode2 = kNode.GetChild(i);//获取并将KNode2指向第i个子节点
            kNode2 = GetNode(kNode2, sNodeName);//在新的KNode2的子节点中查找
            if (kNode2 != null)
            {
                return kNode2;
            }
        }
        return null;//如果没有找到，返回null
    }
}
