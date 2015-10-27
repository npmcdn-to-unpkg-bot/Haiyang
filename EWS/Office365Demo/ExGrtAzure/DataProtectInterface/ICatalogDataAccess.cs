﻿using EwsDataInterface;
using System;
using System.Collections.Generic;

namespace DataProtectInterface
{
    /// <summary>
    /// An interface use to get/save/update 
    /// </summary>
    public interface ICatalogDataAccess : IDataAccess
    {

        void SaveCatalogJob(ICatalogJob catalogJob);

        void SaveMailbox(IMailboxData mailboxAddress);

        void SaveFolder(IFolderData folder, IMailboxData mailboxData, IFolderData parentFolderData);

        void SaveItem(IItemData item, IMailboxData mailboxData, IFolderData parentFolderData);

        void SaveItemContent(IItemData item, DateTime startTime, bool isCheckExist = false, bool isExist = false);

        ICatalogJob GetLastCatalogJob(DateTime thisJobStartTime);

        bool IsItemContentExist(string itemId);
    }
}