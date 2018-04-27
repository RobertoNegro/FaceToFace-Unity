using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppProtocol : MonoBehaviour
{
    #region Unity-side constants
    public const string JAVA_CLASS = "com.negroroberto.uhealth.unity.AppSocket";
    public const string JAVA_METHOD = "ReceiveRawValue";
    #endregion

    #region Unity -> App
    public static class UnityToApp
    {
        public const string UNITY_LOADED = "UNITY_LOADED";

        public const string ERROR_COMMAND = "ERROR_COMMAND";
        public const string ERROR_PARAMS = "ERROR_PARAMS";
        
        public const string AVATAR_LOADING = "AVATAR_LOADING";
        public const string AVATAR_FINISH = "AVATAR_FINISH";

        public const string SPEECH_START = "SPEECH_START";
        public const string SPEECH_FINISH = "SPEECH_FINISH";
    }
    #endregion

    #region App -> Unity
    public static class AppToUnity
    {
        public const string PARAM_TRUE = "TRUE";
        public const string PARAM_FALSE = "FALSE";

        public const string PARAM_CAMERA_TARGET_HEAD = "HEAD";
        public const string PARAM_CAMERA_TARGET_UPPER_BODY = "UPPER_BODY";
        public const string PARAM_CAMERA_TARGET_HALF_BODY = "HALF_BODY";

        public const string ERROR_COMMAND = "ERROR_COMMAND";
        public const string ERROR_PARAMS = "ERROR_PARAMS";

        public const string BG_COLOR = "BG_COLOR";
        // 1 string parameter: color (hex)

        public const string SPEECH = "SPEECH";
        // 1 string parameter: filename

        public const string ANIM_RUN = "ANIM_RUN";
        // 1 string parameter: param_true or param_false

        public const string ANIM_GREETINGS = "ANIM_GREETINGS";

        public const string CAMERA_TARGET = "CAMERA_TARGET";
        // 1 string parameter: PARAM_CAMERA_TARGET_...
        public const string CAMERA_ANGLE = "CAMERA_ANGLE";
        // 1 string parameter: angle
        public const string CAMERA_DISTANCE = "CAMERA_DISTANCE";
        // 1 string parameter: distance
        public const string CAMERA_HEIGHT = "CAMERA_HEIGHT";
        // 1 string parameter: height

        public const string AVATAR_REMOVE = "AVATAR_REMOVE";
        public const string AVATAR_CREATE = "AVATAR_CREATE";
        // 5 string parameters: player id, body id, avatar id, hair id, hair color (hex)

        public const string TEST = "TEST";
    }
    #endregion

    #region Methods
    public static string CreateCommand(string cmd)
    {
        if (cmd == null)
            return null;

        string res = "#{" + cmd + "}";
        return res;
    }
    public static string CreateCommand(string cmd, string param)
    {
        if (cmd == null)
            return null;

        if (param == null)
            CreateCommand(cmd);

        string res = "#{" + cmd + "=" + param + "}";        
        return res;
    }
    public static string CreateCommand(string cmd, string[] param)
    {
        if (cmd == null)
            return null;

        if (param == null)
            return CreateCommand(cmd);

        string res = "#{" + cmd + "=";
        for (int i = 0; i < param.Length; i++)
        {
            res += param[i];
            if (i < param.Length - 1)
                res += ",";
        }
        
        res += "}";
        return res;
    }

    public static bool IsCommand(string s)
    {
        if (s == null)
            return false;

        return s.StartsWith("#{") && s.EndsWith("}");
    }

    public static string GetCommand(string s)
    {
        if (s == null)
            return null;

        string res = s.Substring(2);
        if (res.Contains("="))
            res = res.Substring(0, res.IndexOf('='));
        else
            res = res.Substring(0, res.Length - 1);
        return res;
    }

    public static string GetParam(string s)
    {
        if (s == null)
            return null;

        string res = "";

        if (s.Contains("=") && s.IndexOf("=") < s.Length - 1)
        {
            res = s.Substring(s.IndexOf('=') + 1);
            res = res.Substring(0, res.Length - 1);
        }

        return res;
    }

    public static string[] GetParams(string s)
    {
        if (s == null)
            return null;

        string param = GetParam(s);

        if (param.Contains(","))
            return GetParam(s).Split(',');
        else
            return new string[] { param };
    }
    #endregion
}
