﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arcserve.Exchange.FastTransferUtil.Item.Marker
{
    public class EndAttachMarker : MarkerBase
    {
        protected override string Name
        {
            get { return "EndAttachMarker"; }
        }

        protected override uint SpecificData
        {
            get { return PropertyTag.EndAttach; }
        }
    }
}