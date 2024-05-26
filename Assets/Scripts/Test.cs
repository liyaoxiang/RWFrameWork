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
        //PreOrderTraversal(root); // ���: 1 2 4 5 3   


        Debug.Log("InOrderTraversal:");
        InOrderTraversal(root); // ���: 4 2 5 1 3   


        //Debug.Log("PostOrderTraversal:");
        //PostOrderTraversal(root); // ���: 4 5 2 3 1   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    // ǰ���������-��-��  
    public void PreOrderTraversal(TreeNode root)
    {
        if (root == null) return;
        Debug.Log(root.Val + " "); // ���ʸ��ڵ�  
        PreOrderTraversal(root.Left); // ����������  
        PreOrderTraversal(root.Right); // ����������  
    }

    // �����������-��-��  
    public void InOrderTraversal(TreeNode root)
    {
        if (root == null) return;
        InOrderTraversal(root.Left); // ����������  
        Debug.Log(root.Val + " "); // ���ʸ��ڵ�  
        InOrderTraversal(root.Right); // ����������  
    }

    // �����������-��-��  
    public void PostOrderTraversal(TreeNode root)
    {
        if (root == null) return;
        PostOrderTraversal(root.Left); // ����������  
        PostOrderTraversal(root.Right); // ����������  
        Debug.Log(root.Val + " "); // ���ʸ��ڵ�  
    }
}
