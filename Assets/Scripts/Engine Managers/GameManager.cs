/*
    Copyright 2022 Krispy

    Use of this source code is governed by an MIT-style
    license that can be found in the LICENSE file or at
    https://opensource.org/licenses/MIT.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

using UnityEngine.SceneManagement;

public class GameManager : ISingletonInterface<GameManager>
{
   /* Initialize the LuaVM */
    LuaEnv luaenv = null;
    [Tooltip("Toggles LuaVM.")]
    public bool luaEnable = true;

    /* Toggle debug mode */
    [Tooltip("Toggles debug mode.")]
    public bool debugMode;

    /* Lua file search paths https://github.com/lua/lua/blob/fc6c74f1004b5f67d0503633ddb74174a3c8d6ad/luaconf.h#L208 */
    private string[] searchPaths = new string[] {"?.lua", "?/init.lua"};
    private string initCode = "require 'main'";
    private string dataPath = "";

    // Start is called before the first frame update
    void Start()
    {
        dataPath = GetDataPath();
        /* Execute our main.lua file */
        if ( luaEnable )
        {
            /* Creates our LuaVM and setup the custom path */
            luaenv = new LuaEnv();
            luaenv.AddLoader( LuaScripts );

            Logger.Log( Channel.LuaNative, Priority.Info, "[xFM] Lua has been initalized" );

            luaenv.DoString( initCode );
        }
        else { Logger.Log( Channel.LuaNative, Priority.Info, "[xFM] Lua is disabled" ); }
    }

    private string GetDataPath()
    {
        return (Application.platform == RuntimePlatform.Android ? Application.persistentDataPath : Application.dataPath);
    }

    // The LUA folder is on the asset path + subfolders
    // The loader works for both Editor + Runner
    private List<string> folders;
    public byte[] LuaScripts( ref string fileName )
    {
        /* Search for subfolders if we haven't */
        if ( folders == null )
        {
            folders = new List<string>();
            string luaPath = Path.Combine(dataPath, "Lua");
            if (!Directory.Exists(luaPath))
                throw new DirectoryNotFoundException("Main Lua folder does not exist: " + luaPath);
            folders.Add( luaPath );

            /* Add all of our subfolders */
            folders.AddRange(Directory.GetDirectories(luaPath));
        }

        /* Get module name from file name */
        string moduleName = fileName.Replace('.', '/');
        // VERIFY: This piece of code may be completely unneeded
        if (moduleName.EndsWith("/") || moduleName.EndsWith("\\")) // Very likely a folder
            moduleName = Path.GetDirectoryName(moduleName);
        else if (moduleName.EndsWith(".lua")) // I think this is okay, nobody names a folder a.lua
            moduleName = Path.GetFileNameWithoutExtension(moduleName);
        if (string.IsNullOrEmpty(moduleName))
            throw new Exception("The moduleName I got from the filename is null or empty, that doesn't seem right. Is a vaild filename parameter passed? (" + fileName + ")");

        /* Search for the script in all subfolders (but why???) */
        foreach (string subfolder in folders)
        {
            foreach (string searchPath in searchPaths)
            {
                string filepath = Path.Combine(subfolder, searchPath.Replace("?", moduleName));
                if (File.Exists(filepath))
                {
                    try {
                        byte[] moduledata = File.ReadAllBytes(filepath);
                        // For debugging
                        fileName = filepath;
                        return moduledata;
                    } catch (PathTooLongException e) {
                        throw;
                    } catch (IOException e) {
                        // File can't be read
                        continue;
                    }
                }
            }
        }

        return null;
    }

    public void ExecuteLuaArgument( string luaArg )
    {
        if ( luaEnable )
        {
            Logger.Log( Channel.LuaNative, Priority.Info, "[xFM] Executing: " + luaArg );
            luaenv.DoString( luaArg );
        }
    }

    // Update is called once per frame
    public virtual void  Update()
    {
        if ( luaenv != null )
            luaenv.Tick();

        // Workaround to #1
        if (Input.GetKeyDown(KeyCode.X))
            SceneManager.LoadScene("MainWorld");
    }

    // OnDestroy occurs when a Scene or game ends.
    public override void OnDestroy()
    {
        if ( luaenv != null )
            luaenv.Dispose();

        base.OnDestroy();
    }
}
