﻿using EwsServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataProtectInterface
{
    public interface IServiceContext
    {
        OrganizationAdminInfo AdminInfo { get; }
        string CurrentMailbox { get; set; }
        IServiceContext CurrentContext { get; }
        EwsServiceArgument Argument { get; }
        Dictionary<string, object> OtherInformation { get; }

        IDataAccess DataAccessObj { get; }

        TaskType TaskType { get; }

        string GetOrganizationPrefix();
    }
}