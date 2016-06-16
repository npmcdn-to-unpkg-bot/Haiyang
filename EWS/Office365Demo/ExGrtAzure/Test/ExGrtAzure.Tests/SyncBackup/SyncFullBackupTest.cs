﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup;
using Arcserve.Office365.Exchange.DataProtect.Impl.Backup;
using Arcserve.Office365.Exchange.StorageAccess.MountSession;
using Arcserve.Office365.Exchange.EwsApi.Impl.Increment;
using Arcserve.Office365.Exchange.StorageAccess.MountSession.EF.Data;
using Arcserve.Office365.Exchange.Manager;
using System.IO;
using Arcserve.Office365.Exchange.Thread;
using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Fakes;
using Arcserve.Office365.Exchange.Data.Increment;
using System.Collections.Generic;
using Arcserve.Office365.Exchange.Data;
using Arcserve.Office365.Exchange.Data.Mail;
using System.Diagnostics;
using Arcserve.Office365.Exchange.Data.Account;
using Arcserve.Office365.Exchange.Util;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.Util.Setting;
using Arcserve.Office365.Exchange.DataProtect.Impl;
using Arcserve.Office365.Exchange.EwsApi.Interface;
using System.Text.RegularExpressions;

namespace ExGrtAzure.Tests
{
    [TestClass]
    public class SyncBackupTest
    {
        private void ClearEnv(CatalogInfo newCatalog)
        {
            var dir = newCatalog.WorkFolder;
            DeleteDirectory(dir);
        }

        public static void DeleteDirectory(string folder)
        {
            if (Directory.Exists(folder))
            {
                try
                {
                    Directory.Delete(folder, true);
                }
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(2000);
                    Directory.Delete(folder, true);
                }
                Directory.CreateDirectory(folder);
            }
        }

        [TestMethod]
        public void TestCallBackupProcess()
        {
            //var workFolder = String.Format(@"""{0}""", "E:\\10 Test\\01O365Test\\Catalog10");

            string folder = AppDomain.CurrentDomain.BaseDirectory;
            folder = Path.Combine(folder, "Catalog1");
            var workFolder = "\"" + folder + "\"";
            DeleteDirectory(folder);
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2} -Mailboxes:{3} -WorkFolder:{4} -ItemCount:{5}",
                "ExchangeBackup",
                "devO365admin@arcservemail.onmicrosoft.com",
                "JackyMao1!",
                "haiyang.ling@arcserve.com",
                workFolder,
                "4");
            ProcessStartInfo startInfo = new ProcessStartInfo("Arcserve.Office365.Exchange.DataProtect.Tool.exe", arg);
            var p = Process.Start(startInfo);
            p.WaitForExit();
        }

        
        [TestMethod]
        public void TestFullBackup()
        {
            try
            {
                var workFolder = "E:\\10Test\\01O365Test\\Catalog10";
                DeleteDirectory(workFolder);
                using (var catalogAccess = new CatalogAccess("", "", workFolder, "arcserve"))
                {
                    var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();
                    var dataClient = new DataFromClient(null, 4); 
                    dataClient.InitTaskSyncContext(taskSyncContextBase);
                    catalogAccess.InitTaskSyncContext(taskSyncContextBase);

                    var ewsAdapter = EwsFactory.Instance.NewEwsAdapter();
                    ewsAdapter.InitTaskSyncContext(taskSyncContextBase);
                    var dataConvert = new DataConvert(); 
                    var adminInfo = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
                    {
                        OrganizationName = "arcserve",
                        UserName = "devO365admin@arcservemail.onmicrosoft.com",
                        UserPassword = "JackyMao1!"
                    };
                    using (var backupFlow = DataProtectFactory.Instance.NewBackupInstance(catalogAccess, ewsAdapter, dataClient, dataConvert, adminInfo))
                    {
                        backupFlow.InitTaskSyncContext(taskSyncContextBase);
                        backupFlow.BackupSync();
                    }
                }
            }
            finally
            {
                DisposeManager.DisposeInstance();
            }
        }

        //[TestMethod]
        //public void TestOrganization()
        //{
        //    var org = "devO365admin@arcservemail.onmicrosoft.com".GetOrganization();
        //}

        [TestMethod]
        public void TestSyncBackup()
        {
            try
            {
                var newCatalogInfo = GetNewCatalogFile();
                var oldCatalogInfo = GetOldCatalogFile();
                ClearEnv(newCatalogInfo);
                using (var catalogAccess = new CatalogAccess(newCatalogInfo.WorkFolder, oldCatalogInfo.CatalogFilePath, newCatalogInfo.DataFolder, "arcserve"))
                {
                    var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();
                    var dataClient = new DataFromClientFilterFolderByDisplayName();
                    dataClient.InitTaskSyncContext(taskSyncContextBase);
                    catalogAccess.InitTaskSyncContext(taskSyncContextBase);

                    var ewsAdapter = EwsFactory.Instance.NewEwsAdapter();
                    ewsAdapter.InitTaskSyncContext(taskSyncContextBase);
                    var dataConvert = new DataConvert();
                    var adminInfo = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
                    {
                        OrganizationName = "arcserve",
                        UserName = "devO365admin@arcservemail.onmicrosoft.com",
                        UserPassword = "JackyMao1!"
                    };
                    using (var backupFlow = DataProtectFactory.Instance.NewBackupInstance(catalogAccess, ewsAdapter, dataClient, dataConvert, adminInfo))
                    {
                        backupFlow.InitTaskSyncContext(taskSyncContextBase);
                        backupFlow.BackupSync();
                    }
                }
            }
            finally
            {
                DisposeManager.DisposeInstance();
            }
        }

        [TestMethod]
        public void TestSyncBackupJacky()
        {
            try
            {
                var newCatalogInfo = GetNewCatalogFile();
                var oldCatalogInfo = GetOldCatalogFile();
                ClearEnv(newCatalogInfo);
                using (var catalogAccess = new CatalogAccess(newCatalogInfo.WorkFolder, oldCatalogInfo.CatalogFilePath, newCatalogInfo.DataFolder, "arcserve"))
                {
                    var taskSyncContextBase = DataProtectFactory.Instance.NewDefaultTaskSyncContext();
                    var dataClient = new DataFromClientWithJacky();
                    dataClient.InitTaskSyncContext(taskSyncContextBase);
                    catalogAccess.InitTaskSyncContext(taskSyncContextBase);

                    var ewsAdapter = EwsFactory.Instance.NewEwsAdapter();
                    ewsAdapter.InitTaskSyncContext(taskSyncContextBase);
                    var dataConvert = new DataConvert();
                    var adminInfo = new Arcserve.Office365.Exchange.Data.Account.OrganizationAdminInfo()
                    {
                        OrganizationName = "ArcserveJacky",
                        UserName = "ArcserveJacky@ArcserveJacky.onmicrosoft.com",
                        UserPassword = "Arcserve1!"
                    };
                    using (var backupFlow = DataProtectFactory.Instance.NewBackupInstance(catalogAccess, ewsAdapter, dataClient, dataConvert, adminInfo))
                    {
                        backupFlow.InitTaskSyncContext(taskSyncContextBase);
                        backupFlow.BackupSync();
                    }
                }
            }
            finally
            {
                DisposeManager.DisposeInstance();
            }
        }

        [TestMethod]
        public void TestCallBackupProcessWithJacky()
        {
            //var workFolder = String.Format(@"""{0}""", "E:\\10 Test\\01O365Test\\Catalog10");

            string folder = AppDomain.CurrentDomain.BaseDirectory;
            folder = Path.Combine(folder, "Catalog1");
            var workFolder = "\"" + folder + "\"";
            DeleteDirectory(folder);
            var arg = string.Format("-JobType:{0} -AdminUserName:{1} -AdminPassword:{2} -Mailboxes:{3} -WorkFolder:{4}",
                "ExchangeBackup",
                "ArcserveJacky@ArcserveJacky.onmicrosoft.com",
                "Arcserve1!",
                "ArcserveJacky@ArcserveJacky.onmicrosoft.com;Jacky.Mao@ArcserveJacky.onmicrosoft.com",
                workFolder);
            ProcessStartInfo startInfo = new ProcessStartInfo("Arcserve.Office365.Exchange.DataProtect.Tool.exe", arg);
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            using (var p = Process.Start(startInfo))
            {
                using (StreamReader reader = p.StandardOutput)
                {
                    var str = reader.ReadToEnd();
                    Console.WriteLine(str);
                    Debug.WriteLine(str);
                }
            }
        }



        public IDataFromClient<IJobProgress> GetDataFromClient()
        {
            var result = new StubIDataFromClient<IJobProgress>();
            result.GetAllMailboxes = () =>
            {
                return new List<IMailboxDataSync>(1)
                {
                    new MailboxDataSyncBase("Haiyang.Ling", "haiyang.ling@arcserve.com")
                    {
                        Id = "ce7b5ec2-8732-4b85-a1bd-3196a2284bf2",
                        Name = "Haiyang.Ling"
                    }
                };
            };

            result.IsFolderInPlanString = (folderId) =>
            {
                if (folderId == "AAMkAGYxYzc0MTAyLTI3MjAtNDA5Zi04ZDY2LTlmODU5NmJkZDlhZgAuAAAAAADKmQFKsxwfSKwEXH3khGtpAQA1BzsargwHRq9aRKbs1Mp0AAAx45vvAAA=")
                    return true;
                return false;
            };

            result.GetLatestCatalogJob = () =>
            {
                return null;
            };

            result.IsFolderClassValidString = (folderClass) =>
            {
                return FolderClassUtil.IsFolderValid(folderClass);
            };

            return result;
        }

        public CatalogInfo GetNewCatalogFile()
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            folder = Path.Combine(folder, "Catalog1");
            var catalogFolder = folder;
            if (Directory.Exists(catalogFolder))
            {
                DeleteDirectory(catalogFolder);
            }

            Directory.CreateDirectory(catalogFolder);

            var fileName = Path.Combine(catalogFolder, string.Format("Catalog{0}.mdf", DateTime.Now.Ticks));
            if (File.Exists(fileName))
                File.Delete(fileName);
            var workFolder = Path.Combine(catalogFolder, "Data");
            return new CatalogInfo()
            {
                WorkFolder = catalogFolder,
                DataFolder = workFolder,
                CatalogFilePath = fileName
            };
            
        }

        public CatalogInfo GetOldCatalogFile()
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            folder = Path.Combine(folder, "Catalog0");
            var catalogFolder = folder;
            if (!Directory.Exists(catalogFolder))
            {
                return new CatalogInfo();
            }

            var catalogFile = Directory.GetFiles(catalogFolder, "*.mdf");
            if (catalogFile.Length <= 0)
                return new CatalogInfo();

            var fileName = catalogFile[0];
            var workFolder = Path.Combine(catalogFolder, "Data");
            return new CatalogInfo()
            {
                WorkFolder = catalogFolder,
                DataFolder = workFolder,
                CatalogFilePath = fileName
            };
        }
    }

    public class CatalogInfo
    {
        public string WorkFolder;
        public string DataFolder;
        public string CatalogFilePath;
    }

    public class DataFromClient : IDataFromClient<IJobProgress>
    {
        protected DataFromClient() { }
        public DataFromClient(int? folderCount, int? itemCount)
        {
            FolderCount = folderCount;
            ItemCount = itemCount;

        }
        private int? ItemCount = null;
        public CancellationToken CancelToken
        {
            get; set;
        }

        public IJobProgress Progress
        {
            get; set;
        }

        public TaskScheduler Scheduler
        {
            get; set;
        }

        public virtual ICollection<IMailboxDataSync> GetAllMailboxes()
        {
            return new List<IMailboxDataSync>(1)
            {
                new MailboxDataSyncBase("Haiyang.Ling", "haiyang.ling@arcserve.com")
                {
                    Id = "ce7b5ec2-8732-4b85-a1bd-3196a2284bf2",
                    Name = "Haiyang.Ling"
                }
            };
        }

        public System.Threading.Tasks.Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync()
        {
            throw new NotImplementedException();
        }


        public ICatalogJob GetLatestCatalogJob()
        {
            return null;
        }

        public System.Threading.Tasks.Task<ICatalogJob> GetLatestCatalogJobAsync()
        {
            throw new NotImplementedException();
        }

        public void InitTaskSyncContext(ITaskSyncContext<IJobProgress> mainContext)
        {
            this.CloneSyncContext(mainContext);
        }

        public bool IsFolderClassValid(string folderClass)
        {
            return FolderClassUtil.IsFolderValid(folderClass);
        }

        private int? FolderCount = null;
        private int _dealtFolderCount = 0;
        public virtual bool IsFolderInPlan(string uniqueFolderId)
        {
            if (FolderCount.HasValue)
            {
                _dealtFolderCount++;
                if (_dealtFolderCount <= FolderCount)
                    return true;
                return false;
            }
            else
            {
                return true;
            }
        }

        public System.Threading.Tasks.Task<bool> IsFolderInPlanAsync(string uniqueFolderId)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsItemValid(IItemDataSync item, IFolderDataSync parentFolder)
        {
            return IsItemValid(item.ItemId, parentFolder.FolderId);
        }

        Dictionary<string, HashSet<string>> _folderItemCount = new Dictionary<string, HashSet<string>>();
        private bool IsItemValid(string itemId, string parentId)
        {
            bool returnResult = false;
            if (ItemCount.HasValue)
            {
                using (_folderItemCount.LockWhile(() =>
                {
                    HashSet<string> result = null;
                    if (!_folderItemCount.TryGetValue(parentId, out result))
                    {
                        result = new HashSet<string>();
                        _folderItemCount.Add(parentId, result);
                    }

                    if (result.Contains(itemId))
                    {
                        returnResult = true;
                        return;
                    }

                    if (result.Count > ItemCount)
                    {
                        returnResult = false;
                        return;
                    }
                    else
                    {
                        returnResult = true;
                        result.Add(itemId);
                    }
                }))
                { }
                return returnResult;
            }
            else
            {
                return true;
            }
        }

        public virtual bool IsItemValid(string itemChangeId, IFolderDataSync parentFolder)
        {
            return IsItemValid(itemChangeId, parentFolder.FolderId);
        }

        public virtual bool IsFolderInPlan(IFolderDataSync folderData)
        {
            return true;
        }
    }

    public class DataFromClientFilterFolderByDisplayName : DataFromClient
    {
        public override bool IsFolderInPlan(string uniqueFolderId)
        {
            return true;
        }

        public override bool IsItemValid(IItemDataSync item, IFolderDataSync parentFolder)
        {
            return true;
        }

        public override bool IsItemValid(string itemChangeId, IFolderDataSync parentFolder)
        {
            return true;
        }

        public override bool IsFolderInPlan(IFolderDataSync folderData)
        {
            if (((IFolderDataBase)folderData).DisplayName == "04Knowledge")
            {
                return true;
            }
            return false;
        }
    }

    public class DataFromClientWithJacky : DataFromClient
    {
        public override ICollection<IMailboxDataSync> GetAllMailboxes()
        {
            var mailboxes = new List<string>(2)
            {
                "ArcserveJacky@ArcserveJacky.onmicrosoft.com",
                "Jacky.Mao@ArcserveJacky.onmicrosoft.com"
            };
            var result = new List<IMailboxDataSync>(mailboxes.Count);
            foreach (var mailbox in mailboxes)
            {
                result.Add(new MailboxDataSyncBase() { MailAddress = mailbox });
            }
            return result;
        }
    }
}