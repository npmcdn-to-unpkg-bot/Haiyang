﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EwsDataInterface
{
    public interface ICatalogJob : ICatalogInfo, IItemBase
    {
        string CatalogJobName { get; }
        string Organization { get; }
    }
}
