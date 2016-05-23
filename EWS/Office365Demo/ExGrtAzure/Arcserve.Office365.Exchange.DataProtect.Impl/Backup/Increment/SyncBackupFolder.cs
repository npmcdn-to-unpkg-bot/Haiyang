﻿using Arcserve.Office365.Exchange.DataProtect.Interface.Backup.Increment;
using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Arcserve.Office365.Exchange.Data.Increment;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.EwsApi.Increment;

namespace Arcserve.Office365.Exchange.DataProtect.Impl.Backup.Increment
{
    public class SyncBackupFolder : BackupFolderFlowTemplate, ITaskSyncContext<IJobProgress>, IExchangeAccess<IJobProgress>
    {
        public ICatalogAccess<IJobProgress> CatalogAccess { get; set; }
        public IEwsServiceAdapter<IJobProgress> EwsServiceAdapter { get; set; }
        public IDataFromClient<IJobProgress> DataFromClient { get; set; }
        

        public override Func<FolderId, string, ChangeCollection<ItemChange>> FuncGetChangedItems
        {
            get
            {
                return (folderId, lastSyncStatus) =>
                {
                    return EwsServiceAdapter.SyncItems(folderId, lastSyncStatus);
                };
            }
        }

        public override Func<BackupItemFlow> FuncNewBackupItem
        {
            get
            {
                return () =>
                {
                    var result = new SyncBackupItem();
                    result.CloneSyncContext(this);
                    result.CloneExchangeAccess(this);
                    return result;
                };
            }
        }

        public override Action<IFolderDataSync> ActionAddFolderToCatalog
        {
            get
            {
                return (folder) =>
                {
                    CatalogAccess.AddFolderToCatalog(folder);
                };
            }
        }

        public IDataConvert DataConvert
        {
            get; set;
        }
    }
}
