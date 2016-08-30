using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LuaInterface;
using System.Reflection;
using System.Collections;


namespace ECSTOOL
{
    #region Test Lua&C#
    //-- 使 _G.print 函数重定向到 C# 里面的自定义函数  
    //local print = MyCSPrint   
    //-- 因为这个函数后面要给 luaDemo_2.lua 使用  
    //-- 所以这里不能用 local 修饰符  
    //function Print(...)  
    //    -- 将所有参数化为一个 table 传给 C# 的函数  
    //    -- arg["n"] 为参数个数  
    //    print(arg)  
    //end 
    //local t = { "123a", "cba223" }  
    //Print(t)  
    //Print("测试test~~~@$#!%@#$%&^(^&*)")  
    //Print("测试test~~~@$#!%@#$%&^(^&*)", t)  

    //-- 这里不能用 local 修饰符  
    //function MyLuaPrint(...)  
    //    local t = { "haha", 123, szContent }  
    //    --require("luaDemo_1")  
    //    --dofile("luaDemo_1.lua")  
    //    loadfile("luaDemo_1.lua")  
    //    Print(...) -- 调用 luaDemo_1.lua 里面的打印函数   
    //    return 2012, t, "abc"  
    //end  

    //Lua luaVM = new Lua();  
    //MyLuaEngine myLuaEngine = new MyLuaEngine();  
    //luaVM.RegisterFunction("MyCSPrint", myLuaEngine, myLuaEngine.GetType().GetMethod("MyCSPrint"));  
    //// Lua 调用 C# 函数  
    //luaVM.DoFile("luaDemo_1.lua");  
    //luaVM.DoString("Print(nil, \"测试啊啊testing~~~\");");    
    //// C# 调用 Lua 函数  
    //luaVM.DoFile("luaDemo_2.lua");  
    //LuaFunction luaFunc = luaVM.GetFunction("MyLuaPrint");  
    //if (luaFunc != null)  
    //{  
    //    object[] objRet = luaFunc.Call("abc", 123);  
    //    Console.WriteLine("函数返回 {0} 个参数！", objRet.Length);  
    //}  
    //else  
    //{  
    //    Console.WriteLine("获取 lua 函数失败！");  
    //} 

    //class MyLuaEngine
    //{
    //    public static object[] MyCSPrint(LuaTable luaTbl)
    //    {
    //        object[] _array;
    //        // pairs  
    //        //foreach (object oKey in luaTbl.Keys)  
    //        //{  
    //        //    if (oKey.ToString() == "n") // “参数个数”索引  
    //        //        continue;  
    //        //    Console.Write("{0}\t", luaTbl[oKey]);  
    //        //}  
    //        // ipairs  
    //        int length = int.Parse(luaTbl["n"].ToString());
    //        _array = new object[length];
    //        for (int i = 1; i <= length; i++)
    //        {
    //            //Console.Write("{0}\t", luaTbl[i]); 
    //            _array[i - 1] = luaTbl[i];
    //        }

    //        return _array;
    //    }
    //}
    #endregion

    /// <summary>  
    /// Lua函数描述特性类  
    /// </summary>  
    public class LuaFunc : Attribute
    {
        private String FunctionName;

        public LuaFunc(String strFuncName)
        {
            FunctionName = strFuncName;
        }

        public String getFuncName()
        {
            return FunctionName;
        }
    }
    public class AttrLuaFunc : Attribute
    {
        private String FunctionName;
        private String FunctionDoc;
        private String[] FunctionParameters = null;

        public AttrLuaFunc(String strFuncName, String strFuncDoc, params String[] strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParamDocs;
        }

        public AttrLuaFunc(String strFuncName, String strFuncDoc)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
        }

        public String getFuncName()
        {
            return FunctionName;
        }

        public String getFuncDoc()
        {
            return FunctionDoc;
        }

        public String[] getFuncParams()
        {
            return FunctionParameters;
        }
    }

    /// <summary>  
    /// Lua引擎  
    /// </summary>  
    public class LuaFramework
    {
        public Lua pLuaVM = null;//lua虚拟机  
        public string message = "";
        public object[] results;
        public Hashtable pLuaFuncs = null;

        public LuaFramework()
        {
            pLuaVM = new Lua();
            pLuaFuncs = new Hashtable();
            RegisterLuaFunctions(this);
        }
        /// <summary>  
        /// 注册lua函数  
        /// </summary>  
        /// <param name="pLuaAPIClass">lua函数类</param>  
        public void BindLuaApiClass(Object pLuaAPIClass)
        {
            foreach (MethodInfo mInfo in pLuaAPIClass.GetType().GetMethods())
            {
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    LuaFunc lf=attr as LuaFunc;
                    if (lf != null)
                    {
                        string LuaFunctionName = lf.getFuncName();
                        pLuaVM.RegisterFunction(LuaFunctionName, pLuaAPIClass, mInfo);
                    }
                }
            }
        }

        /// <summary>  
        /// 执行lua脚本文件  
        /// </summary>  
        /// <param name="luaFileName">脚本文件名</param>  
        public void ExecuteFile(string luaFileName)
        {
            try
            {
                this.results = pLuaVM.DoFile(luaFileName);
            }
            catch (Exception e)
            {
                this.message = "Error : \n" + e.ToString();
            }
        }

        /// <summary>  
        /// 执行lua脚本内容  
        /// </summary>  
        /// <param name="luaCommand">lua指令</param>  
        public void ExecuteString(string luaCommand)
        {
            try
            {
                this.results = pLuaVM.DoString(luaCommand);
            }
            catch (Exception e)
            {
                this.message = "Error : \n" + e.ToString();
            }
        }
        public void RegisterLuaFunctions(Object pTarget)
        {
            // Sanity checks
            if (pLuaVM == null || pLuaFuncs == null)
                return;

            // Get the target type
            Type pTrgType = pTarget.GetType();

            // ... and simply iterate through all it's methods
            foreach (MethodInfo mInfo in pTrgType.GetMethods())
            {
                // ... then through all this method's attributes
                foreach (Attribute attr in Attribute.GetCustomAttributes(mInfo))
                {
                    // and if they happen to be one of our AttrLuaFunc attributes
                    if (attr.GetType() == typeof(AttrLuaFunc))
                    {
                        AttrLuaFunc pAttr = (AttrLuaFunc)attr;
                        Hashtable pParams = new Hashtable();

                        // Get the desired function name and doc string, along with parameter info
                        String strFName = pAttr.getFuncName();
                        String strFDoc = pAttr.getFuncDoc();
                        String[] pPrmDocs = pAttr.getFuncParams();


                        // Now get the expected parameters from the MethodInfo object
                        ParameterInfo[] pPrmInfo = mInfo.GetParameters();

                        // If they don't match, someone forgot to add some documentation to the
                        // attribute, complain and go to the next method
                        if (pPrmDocs != null && (pPrmInfo.Length != pPrmDocs.Length))
                        {
                            //Console.WriteLine("Function " + mInfo.Name + " (exported as " +
                            //                                  strFName + ") argument number mismatch. Declared " +
                            //                                  pPrmDocs.Length + " but requires " +
                            //                                  pPrmInfo.Length + ".");
                            break;
                        }
                        String[] pPrms = new String[pPrmInfo.Length];
                        // Build a parameter <-> parameter doc hashtable
                        for (int i = 0; i < pPrmInfo.Length; i++)
                        {
                            pParams.Add(pPrmInfo[i].Name, pPrmDocs[i]);
                            pPrms[i] = pPrmInfo[i].Name;
                        }

                        // Get a new function descriptor from this information
                        LuaFuncDescriptor pDesc = new LuaFuncDescriptor(strFName, strFDoc, pPrms, pPrmDocs);

                        // Add it to the global hashtable
                        pLuaFuncs.Add(strFName, pDesc);

                        // And tell the VM to register it.
                        pLuaVM.RegisterFunction(strFName, pTarget, mInfo);
                    }
                }
            }
        }

        #region 测试注释
        //private bool bRunning = true;
        //public void Run()
        //{
        //    String strInput;

        //    while (bRunning)
        //    {
        //        Console.Write("> ");

        //        strInput = Console.ReadLine();
        //        if (strInput == "quit")
        //            bRunning = false;
        //        else
        //        {
        //            Console.WriteLine();
        //            try
        //            {
        //                pLuaVM.DoString(strInput);
        //            }

        //            catch (Exception ex)
        //            {
        //                Console.WriteLine(ex.Message);
        //            }

        //            finally
        //            {
        //                Console.WriteLine();
        //            }
        //        }
        //    }
        //}
        //[AttrLuaFunc("quit", "Exit the program.")]
        //public void quit() 
        //{ 
        //    bRunning = false;
        //}
        //[AttrLuaFunc("help", "List available commands.")]
        //public void help()
        //{
        //    Console.WriteLine("Available commands: ");
        //    Console.WriteLine();

        //    IDictionaryEnumerator Funcs = pLuaFuncs.GetEnumerator();
        //    while (Funcs.MoveNext())
        //    {
        //        Console.WriteLine(((LuaFuncDescriptor)Funcs.Value).getFuncHeader());
        //    }
        //}
        //[AttrLuaFunc("helpcmd", "Show help for a given command", "Command to get help of.")]
        //public void help(String strCmd)
        //{
        //    if (!pLuaFuncs.ContainsKey(strCmd))
        //    {
        //        Console.WriteLine("No such function or package: " + strCmd);
        //        return;
        //    }

        //    LuaFuncDescriptor pDesc = (LuaFuncDescriptor)pLuaFuncs[strCmd];
        //    Console.WriteLine(pDesc.getFuncFullDoc());
        //}
        #endregion
    }
    public class LuaFuncDescriptor
    {
        private String FunctionName;
        private String FunctionDoc;
        private String[] FunctionParameters;
        private String[] FunctionParamDocs;
        private String FunctionDocString;

        public LuaFuncDescriptor(String strFuncName, String strFuncDoc, String[] strParams, String[] strParamDocs)
        {
            FunctionName = strFuncName;
            FunctionDoc = strFuncDoc;
            FunctionParameters = strParams;
            FunctionParamDocs = strParamDocs;

            String strFuncHeader = strFuncName + "(%params%) - " + strFuncDoc;
            String strFuncBody = "\n\n";
            String strFuncParams = "";

            Boolean bFirst = true;

            for (int i = 0; i < strParams.Length; i++)
            {
                if (!bFirst)
                    strFuncParams += ", ";

                strFuncParams += strParams[i];
                strFuncBody += "\t" + strParams[i] + "\t\t" + strParamDocs[i] + "\n";

                bFirst = false;
            }

            strFuncBody = strFuncBody.Substring(0, strFuncBody.Length - 1);
            if (bFirst)
                strFuncBody = strFuncBody.Substring(0, strFuncBody.Length - 1);

            FunctionDocString = strFuncHeader.Replace("%params%", strFuncParams) + strFuncBody;
        }

        public String getFuncName()
        {
            return FunctionName;
        }

        public String getFuncDoc()
        {
            return FunctionDoc;
        }

        public String[] getFuncParams()
        {
            return FunctionParameters;
        }

        public String[] getFuncParamDocs()
        {
            return FunctionParamDocs;
        }

        public String getFuncHeader()
        {
            if (FunctionDocString.IndexOf("\n") == -1)
                return FunctionDocString;

            return FunctionDocString.Substring(0, FunctionDocString.IndexOf("\n"));
        }

        public String getFuncFullDoc()
        {
            return FunctionDocString;
        }
    }
}
