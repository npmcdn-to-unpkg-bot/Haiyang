﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Exchange.WebServices.Data;
using Arcserve.Office365.Exchange.EwsApi.Impl.Common;

namespace Arcserve.Office365.Exchange.EwsApi.Impl.Impl
{
    public class MailboxOperatorImpl : IMailbox
    {

        public MailboxOperatorImpl()
        {
        }

        public ExchangeService CurrentExchangeService
        {
            get; private set;
        }

        public string MailboxPrincipalAddress
        {
            get; private set;
        }

        public void ConnectMailbox(EwsServiceArgument argument, string connectMailAddress)
        {
            argument.SetConnectMailbox(connectMailAddress);
            MailboxPrincipalAddress = connectMailAddress;
            CurrentExchangeService = EwsProxyFactory.CreateExchangeService(argument, MailboxPrincipalAddress);
        }

        public IFolder NewFolderOperatorInstance()
        {
            return new FolderOperatorImpl(CurrentExchangeService);
        }
    }
}