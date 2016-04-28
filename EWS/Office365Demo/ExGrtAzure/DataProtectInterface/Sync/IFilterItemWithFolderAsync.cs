﻿using EwsDataInterface.Sync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataProtectInterface.Sync
{
    public interface IFilterItemWithFolderAsync<ProgressType> : IFilterItemWithMailboxAsync<ProgressType>
    {
        List<IFolderDataSync> GetAllFolders(IMailboxDataSync mailboxData);
        Task<List<IFolderDataSync>> GetAllFoldersAsync(IMailboxDataSync mailboxData);
    }

    public interface IFilterItemSync<ProgressType> : ITaskSyncContext<ProgressType>
    {
        bool IsFilterMailbox(IMailboxDataSync mailbox);
        bool IsFilterFolder(IFolderDataSync currentFolder, IMailboxDataSync mailbox, Stack<IFolderDataSync> folders);
        bool IsFilterItem(IItemDataSync item, IMailboxDataSync mailbox, Stack<IFolderDataSync> folders);
    }
    public interface IFilterItemWithMailboxAsync<ProgressType> : IFilterItemSync<ProgressType>
    {
        List<IMailboxDataSync> GetAllMailbox();
        Task<List<IMailboxDataSync>> GetAllMailboxAsync();
    }

    
}
