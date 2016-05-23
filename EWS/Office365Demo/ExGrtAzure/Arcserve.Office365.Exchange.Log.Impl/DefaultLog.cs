﻿using Arcserve.Office365.Exchange.Util;
using Arcserve.Office365.Exchange.Util.Setting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Arcserve.Office365.Exchange.Log.Impl
{
    public class DefaultLog : ILog
    {
        public DefaultLog()
        {
            
        }
        

        private ReaderWriterLockSlim LogStreamsProviderLock = new ReaderWriterLockSlim();

        private Dictionary<Guid, ILogStreamProvider> LogStreamsProvider = new Dictionary<Guid, ILogStreamProvider>(2);

        private void Check()
        {
            using (LogStreamsProviderLock.Read())
            {
                if (LogStreamsProvider.Count == 0)
                {
                    throw new NotSupportedException();
                }
            }
        }

        private void Write(string logDetail)
        {
            if (!CloudConfig.Instance.IsLog)
            {
                return;
            }
            using (LogStreamsProviderLock.Read())
            {
                foreach (var stream in LogStreamsProvider.Values)
                {
                    stream.WriteLine(logDetail);
                }
            }
        }

        private void WriteLine(string logDetail)
        {
            if (!CloudConfig.Instance.IsLog)
            {
                return;
            }
            using (LogStreamsProviderLock.Read())
            {
                foreach (var stream in LogStreamsProvider.Values)
                {
                    stream.WriteLine(logDetail);
                }
            }
        }

        public void WriteException(LogLevel level, string message, Exception exception, string exMsg)
        {
            WriteLine(GetExceptionString("", level, message, exception, exMsg));
        }
        const string blank = "\t";

        public event EventHandler<string> WriteLogMsgEvent;

        public void WriteLog(LogLevel level, string message)
        {
            WriteLine(GetLogString(level, message));
        }

        internal static string GetLogString(LogLevel level, string message)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN());
        }

        public void WriteLog(LogLevel level, string message, string format, params object[] args)
        {
            WriteLine(GetLogString(level, message, format, args));
        }

        internal static string GetLogString(LogLevel level, string message, string format, params object[] args)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"),
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN(),
                args.Length > 0 ? string.Format(format, args).RemoveRN() : format.RemoveRN());
        }

        public string GetTotalLog(DateTime date)
        {
            using (LogStreamsProviderLock.Read())
            {
                foreach (var stream in LogStreamsProvider.Values)
                {
                    return stream.GetTotalLog(date);
                }
            }
            return "Log file is not exist.";
        }

        public void WriteLog(string module, LogLevel level, string message)
        {
            WriteLine(GetLogString(module, level, message));
        }

        public void WriteException(string module, LogLevel level, string message, Exception exception, string exMsg)
        {
            WriteLine(GetExceptionString(module, level, message, exception, exMsg));
        }

        public void WriteLog(string module, LogLevel level, string message, string format, params object[] args)
        {
            WriteLine(GetLogString(module, level, message, format, args));
        }

        private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";
        public static string GetExceptionString(string module ,LogLevel level, string message, Exception exception, string exMsg)
        {
            StringBuilder sb = new StringBuilder();
            var curEx = exception;
            while (curEx != null)
            {
                if (curEx is AggregateException)
                {
                    sb.AppendLine(GetAggrateException(module, level, message, curEx as AggregateException, exMsg));
                }
                else
                {
                    sb.AppendLine(string.Join(blank, DateTime.Now.ToString(TimeFormat), module, System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D4"), Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString("D4") : "0000",
                        LogLevelHelper.GetLevelString(level),
                        message.RemoveRN(),
                        curEx.Message.RemoveRN(),
                        curEx.HResult.ToString("X8"),
                        curEx.StackTrace.RemoveRN(), curEx.GetType().FullName));

                    curEx = curEx.InnerException;
                }
            }
            sb.AppendLine();
            return sb.ToString();
        }

        internal static string GetAggrateException(string module, LogLevel level, string message, AggregateException ex, string exMsg)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(blank, module, DateTime.Now.ToString(TimeFormat), System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("D4"), Task.CurrentId.HasValue ? Task.CurrentId.Value.ToString("D4") : "0000",
                    LogLevelHelper.GetLevelString(level),
                    message.RemoveRN(),
                    ex.Message.RemoveRN(),
                    ex.HResult.ToString("X8"),
                    ex.StackTrace.RemoveRN(), ex.GetType().FullName));

            foreach (var innerEx in ex.Flatten().InnerExceptions)
            {
                sb.AppendLine(GetExceptionString(module, level, message, ex, exMsg));
            }
            return sb.ToString();
        }
        internal static string GetLogString(string module, LogLevel level, string message)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"), module,
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN());
        }

        internal static string GetLogString(string module, LogLevel level, string message, string format, params object[] args)
        {
            return string.Join(blank, DateTime.Now.ToString("yyyyMMddHHmmss"), module,
                LogLevelHelper.GetLevelString(level),
                message.RemoveRN(),
                args.Length > 0 ? string.Format(format, args).RemoveRN() : format.RemoveRN());
        }

        public void RegisterLogStream(ILogStreamProvider stream)
        {
            using (LogStreamsProviderLock.Write())
            {
                LogStreamsProvider[stream.StreamId] = stream;
            }
        }

        public void RemoveLogStream(ILogStreamProvider stream)
        {
            using (LogStreamsProviderLock.Write())
            {
                if (LogStreamsProvider.ContainsKey(stream.StreamId))
                    LogStreamsProvider.Remove(stream.StreamId);
            }
        }

        public void Dispose()
        {
            LogStreamsProviderLock.Dispose();
            LogStreamsProviderLock = null;
        }
    }
    internal static class StringEx
    {
        internal static string RemoveRN(this string message)
        {
            return message.Replace('\r', ' ').Replace('\n', ' ');
        }
    }

    //public class LogThreadManager
    //{
    //    private static object instanceLockObj = new object();
    //    private static LogThreadManager _instance;
    //    public static LogThreadManager Instance
    //    {
    //        get
    //        {
    //            if (_instance == null)
    //            {
    //                lock (instanceLockObj)
    //                {
    //                    if (_instance == null)
    //                    {
    //                        _instance = new LogThreadManager();
    //                    }
    //                }
    //            }
    //            return _instance;
    //        }
    //    }
    //    private static object _lockObj = new object();
    //    private System.Threading.Thread _thread;

    //    public readonly object SyncLockObj = new object();
    //    private readonly Dictionary<Guid, DisposableObj<Stream>> _allStream = new Dictionary<Guid, DisposableObj<Stream>>();
    //    private readonly Dictionary<Guid, DisposableObj<IDisposable>> _allDisposableObj = new Dictionary<Guid, DisposableObj<IDisposable>>();

    //    private LogThreadManager()
    //    {
    //        _thread = new System.Threading.Thread(Run);
    //        _thread.Name = "ManagerLogStream";
    //        _thread.Start();
    //    }

    //    private const int ExpireSecond = 120 * 1000;
    //    private void Run()
    //    {
    //        // todo if process stop, this must be disposed. So we can use sleep.
    //        System.Threading.Thread.Sleep(120 * 1000);
    //        lock (SyncLockObj)
    //        {
    //            DateTime now = DateTime.Now;

    //            Dispose(_allStream, now, (m) => { m.Close(); m.Dispose(); });
    //            Dispose(_allDisposableObj, now, m => m.Dispose());
    //        }
    //    }

    //    private delegate void DisposeCallFunc<T>(T obj);
    //    private void Dispose<T>(Dictionary<Guid, DisposableObj<T>> objects, DateTime now, DisposeCallFunc<T> func)
    //    {
    //        List<Guid> expireStreams = new List<Guid>(4);
    //        foreach (var keyValue in objects)
    //        {
    //            if (keyValue.Value.IsExpire(now, ExpireSecond))
    //            {
    //                expireStreams.Add(keyValue.Key);
    //            }
    //        }

    //        foreach (var expireItem in expireStreams)
    //        {
    //            func.Invoke(objects[expireItem].obj);
    //            objects.Remove(expireItem);
    //        }
    //    }

    //    public void AddStream(Stream stream, Guid key)
    //    {
    //        lock (SyncLockObj)
    //        {
    //            _allStream.Add(key, new DisposableObj<Stream>() { obj = stream, addTime = DateTime.Now });
    //        }
    //    }

    //    public void AddDisposal(IDisposable disObj, Guid key)
    //    {
    //        lock (SyncLockObj)
    //        {
    //            _allDisposableObj.Add(key, new DisposableObj<IDisposable>() { obj = disObj, addTime = DateTime.Now });
    //        }
    //    }

    //    public Stream GetStream(Guid key)
    //    {
    //        lock (SyncLockObj)
    //        {
    //            DisposableObj<Stream> val;
    //            if (_allStream.TryGetValue(key, out val))
    //            {
    //                return val.obj;
    //            }
    //            return null;
    //        }
    //    }


    //    class DisposableObj<T>
    //    {
    //        public T obj;
    //        public DateTime addTime;

    //        public bool IsExpire(DateTime now, int expireSecond)
    //        {
    //            if ((now - addTime).TotalSeconds > expireSecond)
    //                return true;
    //            return false;
    //        }
    //    }
    //}

    //public class DefaultLogStream : ILogStreamProvider
    //{
    //    public DefaultLogStream() { }
    //    public DefaultLogStream(string logPath)
    //    {
    //        _logPath = LogPath;
    //    }

    //    public static string GetLogPath(string logFileName)
    //    {
    //        return Path.Combine(GetSystemLogFolder(), logFileName);
    //    }

    //    public static string GetSystemLogFolder()
    //    {
    //        string logFolder = CloudConfig.Instance.LogPath;
    //        if (string.IsNullOrEmpty(logFolder))
    //        {
    //            logFolder = AppDomain.CurrentDomain.BaseDirectory;
    //            logFolder = Path.Combine(logFolder, "Log");
    //        }
    //        if (!Directory.Exists(logFolder))
    //        {
    //            Directory.CreateDirectory(logFolder);
    //        }
    //        return logFolder;
    //    }

    //    private string _logPath;
    //    protected virtual string LogPath
    //    {
    //        get
    //        {
    //            if (string.IsNullOrEmpty(_logPath))
    //            {
    //                string logFolder = GetSystemLogFolder();
    //                var logPath = Path.Combine(logFolder, LogFileName);
    //                _logPath = logPath;

    //            }
    //            return _logPath;
    //        }
    //    }

    //    protected virtual string LogFileName
    //    {
    //        get
    //        {
    //            return string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd"));
    //        }
    //    }

    //    private readonly Guid _streamKey = Guid.NewGuid();
        

    //    public Guid StreamId
    //    {
    //        get
    //        {
    //            return _streamKey;
    //        }
    //    }

    //    public void Write(string information)
    //    {
    //        var writer = new StreamWriter(LogStream);
    //        writer.Write(information);
    //        writer.Flush();
    //    }

    //    public void WriteLine(string information)
    //    {
    //        var writer = new StreamWriter(LogStream);
    //        writer.WriteLine(information);
    //        writer.Flush();
    //    }

    //    public string GetTotalLog(DateTime date)
    //    {
    //        if (File.Exists(LogPath))
    //            using (var stream = new FileStream(LogPath, FileMode.Open, FileAccess.Read))
    //            {
    //                StreamReader reader = new StreamReader(stream);
    //                return reader.ReadToEnd();
    //            }
    //        return "Log file is not exist.";
    //    }
    //}


    public class DebugOutput : ILogStreamProvider
    {
        public Guid StreamId
        {
            get
            {
                return _id;
            }
        }

        private Guid _id = Guid.NewGuid();
        private object _syncObj = new object();
        public object SyncObj
        {
            get
            {
                return _syncObj;
            }
        }

        public string GetTotalLog(DateTime date)
        {
            return "";
        }

        public void Write(string information)
        {
            Debug.Write(information);
        }

        public void WriteLine(string information)
        {
            Debug.WriteLine(information);
        }
    }
}
