using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeNode
{
    public int Val { get; set; }
    public TreeNode Left { get; set; }
    public TreeNode Right { get; set; }

    public TreeNode(int x)
    {
        Val = x;
    }
}
public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TreeNode root = new TreeNode(1);
        root.Left = new TreeNode(2);
        root.Right = new TreeNode(3);
        root.Left.Left = new TreeNode(4);
        root.Left.Right = new TreeNode(5);

        //Debug.Log("PreOrderTraversal:");
        //PreOrderTraversal(root); // 输出: 1 2 4 5 3   


        Debug.Log("InOrderTraversal:");
        InOrderTraversal(root); // 输出: 4 2 5 1 3   


        //Debug.Log("PostOrderTraversal:");
        //PostOrderTraversal(root); // 输出: 4 5 2 3 1   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // 前序遍历：根-左-右  
    public void PreOrderTraversal(TreeNode root)
    {
        if (root == null) return;
        Debug.Log(root.Val + " "); // 访问根节点  
        PreOrderTraversal(root.Left); // 遍历左子树  
        PreOrderTraversal(root.Right); // 遍历右子树  
    }

    // 中序遍历：左-根-右  
    public void InOrderTraversal(TreeNode root)
    {
        if (root == null) return;
        InOrderTraversal(root.Left); // 遍历左子树  
        Debug.Log(root.Val + " "); // 访问根节点  
        InOrderTraversal(root.Right); // 遍历右子树  
    }

    // 后序遍历：左-右-根  
    public void PostOrderTraversal(TreeNode root)
    {
        if (root == null) return;
        PostOrderTraversal(root.Left); // 遍历左子树  
        PostOrderTraversal(root.Right); // 遍历右子树  
        Debug.Log(root.Val + " "); // 访问根节点  
    }
}
