using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using System;
using System.IO;
using System.Threading;

public static class FileStaticAPI
{
    public static void FindFileByExt(string dirPath, string extName, ref List<string> dirs)
    {
        string[] list=Directory.GetFiles(dirPath, extName, SearchOption.AllDirectories);
        foreach (string path in list)
        {
            dirs.Add(path);
        }
    }

    /// ����ļ��Ƿ����Application.dataPathĿ¼
    public static bool IsFileExists (string fileName)
    {
        if (fileName.Equals (string.Empty)) {
            return false;
        }
 
        return File.Exists (fileName);
    }
 
    /// ��Application.dataPathĿ¼�´����ļ�
    public static void CreateFile (string fileName)
    {
        if (!IsFileExists (fileName)) {
            CreateFolder (fileName.Substring (0, fileName.LastIndexOf ('/')));
 
#if UNITY_4 || UNITY_5
            FileStream stream = File.Create (fileName);
            stream.Close ();
#else
            File.Create (fileName);
#endif
        }
 
    }
 
    /// д�����ݵ���Ӧ�ļ�
    public static void Write (string fileName, string contents)
    {
        CreateFolder (fileName.Substring (0, fileName.LastIndexOf ('/')));
 
        TextWriter tw = new StreamWriter (fileName, false);
        tw.Write (contents);
        tw.Close (); 
 
        AssetDatabase.Refresh ();
    }
 
    /// �Ӷ�Ӧ�ļ���ȡ����
    public static string Read (string fileName)
    {
#if !UNITY_WEBPLAYER
        if (IsFileExists (fileName)) {
            return File.ReadAllText (fileName);
        } else {
            Debug.LogWarning(fileName+" file not find!");
            return "";
        }
#endif
 
#if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::CopyFolder is innored under wep player platfrom");
#endif
    }
 
    /// �����ļ�
    public static void CopyFile (string srcFileName, string destFileName)
    {
        if (IsFileExists (srcFileName) && !srcFileName.Equals (destFileName)) {
            int index = destFileName.LastIndexOf ("/");
            string filePath = string.Empty;
 
            if (index != -1) {
                filePath = destFileName.Substring (0, index);
            }
 
            if (!Directory.Exists (GetFullPath (filePath))) {
                Directory.CreateDirectory (GetFullPath (filePath));
            }
 
            File.Copy (GetFullPath (srcFileName), GetFullPath (destFileName), true);
 
            AssetDatabase.Refresh ();
        }
    }
 
    /// ɾ���ļ�
    public static void DeleteFile (string fileName)
    {
        if (IsFileExists (fileName)) {
            File.Delete (GetFullPath (fileName));
 
            AssetDatabase.Refresh ();
        }
    }
 
    /// ����Ƿ�����ļ���
    public static bool IsFolderExists (string folderPath)
    {
        if (folderPath.Equals (string.Empty)) {
            return false;
        }
 
        return Directory.Exists (folderPath);
    }
 
    /// �����ļ���
    public static void CreateFolder (string folderPath)
    {
        if (!IsFolderExists (folderPath)) {
            Directory.CreateDirectory (folderPath);
 
            AssetDatabase.Refresh ();
        }
    }
 
    /// �����ļ���
    public static void CopyFolder (string srcFolderPath, string destFolderPath)
    {
 
#if !UNITY_WEBPLAYER
        if (!IsFolderExists (srcFolderPath)) {
            return;
        }
 
        CreateFolder (destFolderPath);
 
 
        srcFolderPath = GetFullPath (srcFolderPath);
        destFolderPath = GetFullPath (destFolderPath);
 
        // �������еĶ�ӦĿ¼
        foreach (string dirPath in Directory.GetDirectories(srcFolderPath, "*", SearchOption.AllDirectories)) {
            Directory.CreateDirectory (dirPath.Replace (srcFolderPath, destFolderPath));
        }
 
        // ����ԭ�ļ������������ݵ�Ŀ���ļ��У�ֱ�Ӹ���
        foreach (string newPath in Directory.GetFiles(srcFolderPath, "*.*", SearchOption.AllDirectories)) {
 
            File.Copy (newPath, newPath.Replace (srcFolderPath, destFolderPath), true);
        }
 
        AssetDatabase.Refresh ();
#endif
 
#if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::CopyFolder is innored under wep player platfrom");
#endif
    }
 
    /// ɾ���ļ���
    public static void DeleteFolder (string folderPath)
    {
        #if !UNITY_WEBPLAYER
        if (IsFolderExists (folderPath)) {
 
            Directory.Delete (GetFullPath (folderPath), true);
 
            AssetDatabase.Refresh ();
        }
        #endif
 
        #if UNITY_WEBPLAYER
        Debug.LogWarning("FileStaticAPI::DeleteFolder is innored under wep player platfrom");
        #endif
    }
 
    /// ����Application.dataPath������Ŀ¼
    private static string GetFullPath (string srcName)
    {
        if (srcName.Equals (string.Empty)) {
            return Application.dataPath;
        }
 
        if (srcName [0].Equals ('/')) {
            srcName.Remove (0, 1);
        }
 
        return Application.dataPath + "/" + srcName;
    }
 
    /// ��Assets�´���Ŀ¼
    public static void CreateAssetFolder (string assetFolderPath)
    {
        if (!IsFolderExists (assetFolderPath)) {
            int index = assetFolderPath.IndexOf ("/");
            int offset = 0;
            string parentFolder = "Assets";
            while (index != -1) {
                if (!Directory.Exists (GetFullPath (assetFolderPath.Substring (0, index)))) {
                    string guid = AssetDatabase.CreateFolder (parentFolder, assetFolderPath.Substring (offset, index - offset));
                    // ��GUID(ȫ��Ψһ��ʶ��)ת��Ϊ��Ӧ����Դ·����
                    AssetDatabase.GUIDToAssetPath (guid);
                }
                offset = index + 1;
                parentFolder = "Assets/" + assetFolderPath.Substring (0, offset - 1);
                index = assetFolderPath.IndexOf ("/", index + 1);
            }
 
            AssetDatabase.Refresh ();
        }
    }
 
    /// ����Assets������
    public static void CopyAsset (string srcAssetName, string destAssetName)
    {
        if (IsFileExists (srcAssetName) && !srcAssetName.Equals (destAssetName)) {
            int index = destAssetName.LastIndexOf ("/");
            string filePath = string.Empty;
 
            if (index != -1) {
                filePath = destAssetName.Substring (0, index + 1);
                //Create asset folder if needed
                CreateAssetFolder (filePath);
            }
 
 
            AssetDatabase.CopyAsset (GetFullAssetPath (srcAssetName), GetFullAssetPath (destAssetName));
            AssetDatabase.Refresh ();
        }
    }
 
    /// ɾ��Assets������
    public static void DeleteAsset (string assetName)
    {
        if (IsFileExists (assetName)) {
            AssetDatabase.DeleteAsset (GetFullAssetPath (assetName));           
            AssetDatabase.Refresh ();
        }
    }
 
    /// ��ȡAssets������·��
    private static string GetFullAssetPath (string assetName)
    {
        if (assetName.Equals (string.Empty)) {
            return "Assets/";
        }
 
        if (assetName [0].Equals ('/')) {
            assetName.Remove (0, 1);
        }
 
        return "Assets/" + assetName;
    }
}
 
#endif