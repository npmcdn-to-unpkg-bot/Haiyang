﻿using Arcserve.Office365.Exchange.Thread;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Arcserve.Office365.Exchange.Data.Increment;
using Arcserve.Office365.Exchange.Data;

namespace Arcserve.Office365.Exchange.DataProtect.Interface.Backup
{
    public interface IDataFromClient<ProgressType> : ITaskSyncContext<ProgressType>
    {
        ICollection<IMailboxDataSync> GetAllMailboxes();
        Task<ICollection<IMailboxDataSync>> GetAllMailboxesAsync();
        ICatalogJob GetLatestCatalogJob();
        Task<ICatalogJob> GetLatestCatalogJobAsync();
        bool IsFolderInPlan(string uniqueFolderId);
        Task<bool> IsFolderInPlanAsync(string uniqueFolderId);
        bool IsFolderClassValid(string folderClass);
        bool IsItemValid(IItemDataSync item, IFolderDataSync parentFolder);
        bool IsItemValid(string itemChangeId, IFolderDataSync parentFolder);
        //IEnumerable<IFolderDataSync> GetFolders(IMailboxDataSync mailboxData);
        //Task<IEnumerable<IFolderDataSync>> GetFoldersAsync(IMailboxDataSync mailboxData);
    }
}