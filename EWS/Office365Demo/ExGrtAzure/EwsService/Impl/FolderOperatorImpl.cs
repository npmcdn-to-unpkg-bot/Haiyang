﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using EwsServiceInterface;
using System.Threading;

namespace EwsService.Impl
{
    public class FolderOperatorImpl : IFolder
    {
        private static HashSet<string> _validFolderType;
        private static HashSet<string> ValidFolderType
        {
            get
            {
                if(_validFolderType == null)
                {
                    _validFolderType = new HashSet<string>();
                    _validFolderType.Add("IPF.Note");
                    _validFolderType.Add("IPF.Appointment");
                    _validFolderType.Add("IPF.Contact");
                }
                return _validFolderType;
            }
        }
        
        public FolderOperatorImpl(ExchangeService service)
        {
            CurrentExchangeService = service;
        }

        public ExchangeService CurrentExchangeService
        {
            get; private set;
        }

        public List<Folder> GetChildFolder(Folder parentFolder)
        {
            const int pageSize = 100;
            int offset = 0;
            bool moreItems = true;
            List<Folder> result = new List<Folder>(parentFolder.ChildFolderCount);
            while (moreItems)
            {
                FolderView oView = new FolderView(pageSize, offset, OffsetBasePoint.Beginning);
                FindFoldersResults findResult = parentFolder.FindFolders(oView);
                result.AddRange(findResult.Folders);
                if (!findResult.MoreAvailable)
                    moreItems = false;

                if (moreItems)
                    offset += pageSize;
            }
            return result;
        }

        public string GetFolderDisplayName(Folder folder)
        {
            return folder.DisplayName;
        }

        public Folder GetRootFolder()
        {
            return Folder.Bind(CurrentExchangeService, WellKnownFolderName.MsgFolderRoot);
        }

        
        public bool IsFolderNeedGenerateCatalog(Folder folder)
        {
            return ValidFolderType.Contains(folder.FolderClass);
        }

        public IItem NewItemOperatorInstance()
        {
            return new ItemOperatorImpl(CurrentExchangeService);
        }

        public FolderId CreateChildFolder(string folderDisplayName, FolderId parentFolderId)
        {
            Folder folder = new Folder(CurrentExchangeService);
            folder.DisplayName = folderDisplayName;
            folder.FolderClass = "IPF.Note";
            folder.Save(parentFolderId);
            return FindFolder(folderDisplayName, parentFolderId);
        }

        public FolderId FindFolder(string folderDisplayName, FolderId parentFolderId, int findCount = 0)
        {
            FolderView view = new FolderView(1);
            view.PropertySet = new PropertySet(BasePropertySet.IdOnly);
            view.Traversal = FolderTraversal.Shallow;
            SearchFilter filter = new SearchFilter.IsEqualTo(FolderSchema.DisplayName, folderDisplayName);
            FindFoldersResults results = CurrentExchangeService.FindFolders(parentFolderId, filter, view);

            if (results.TotalCount > 1)
            {
                throw new InvalidOperationException("Find more than 1 folder.");
            }
            else if (results.TotalCount == 0)
            {
                if (findCount > 3)
                {
                    return null;
                }
                Thread.Sleep(500);
                return FindFolder(folderDisplayName, parentFolderId, ++findCount);
            }
            else
            {
                foreach (var result in results)
                {
                    return result.Id;
                }
                throw new InvalidOperationException("Thread sleep time is too short.");
            }
        }

        public void DeleteFolder(FolderId findFolderId, DeleteMode deleteMode)
        {
            Folder folder = Folder.Bind(CurrentExchangeService, findFolderId);
            folder.Delete(deleteMode);
        }
    }
}