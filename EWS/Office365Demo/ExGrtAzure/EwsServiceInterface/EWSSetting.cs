﻿using EwsServiceInterface.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace EwsServiceInterface
{
    public class EWSSetting
    {
        private static object _lock = new object();
        private static EWSSetting _instance = null;
        public static EWSSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    using (_lock.LockWhile(() =>
                      {
                          if (_instance == null)
                          {
                              _instance = new EWSSetting();
                          }
                      }))
                    { }
                }
                return _instance;
            }
        }

        protected EWSSetting()
        {
            OverrideCertValidation = true;
            EnableSslDetailLogging = false;
            AllowAutodiscoverRedirect = true;
            OverrideTimezone = false;
            SelectedTimeZoneId = string.Empty;
            EnableScpLookups = true;
            PreAuthenticate = false;
            OverrideTimeout = false;
            Timeout = 100000;
            UserAgent = "UDP ExGrt";
            SetDefaultProxy = false;
            BypassProxyForLocalAddress = false;
            SpecifyProxySettings = false;
            ProxyServerName = "127.0.0.1";
            ProxyServerPort = 8888;
            OverrideProxyCredentials = false;
            ProxyServerUser = string.Empty;
            ProxyServerPassword = string.Empty;
            ProxyServerDomain = string.Empty;
            XAnchorMailbox = string.Empty;
            SetXAnchorMailbox = false;
        }

        public bool OverrideCertValidation { get; protected set; }
        public bool EnableSslDetailLogging { get; protected set; }
        public bool AllowAutodiscoverRedirect { get; protected set; }
        public bool OverrideTimezone { get; private set; }
        public string SelectedTimeZoneId { get; protected set; }
        public bool EnableScpLookups { get; protected set; }
        public bool PreAuthenticate { get; private set; }
        public bool OverrideTimeout { get; protected set; }
        public int Timeout { get; protected set; }
        public string UserAgent { get; protected set; }
        public bool SetDefaultProxy { get; protected set; }
        public bool BypassProxyForLocalAddress { get; protected set; }
        public bool SpecifyProxySettings { get; protected set; }
        public string ProxyServerName { get; protected set; }
        public int ProxyServerPort { get; protected set; }
        public bool OverrideProxyCredentials { get; protected set; }
        public string ProxyServerUser { get; protected set; }
        public string ProxyServerPassword { get; protected set; }
        public string ProxyServerDomain { get; protected set; }

        public bool SetXAnchorMailbox { get; protected set; }
        public string XAnchorMailbox { get; protected set; }
    }
}