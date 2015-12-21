﻿/*  
  Aras.Model provides a .NET cient library for Aras Innovator

  Copyright (C) 2015 Processwall Limited.

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU Affero General Public License as published
  by the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU Affero General Public License for more details.

  You should have received a copy of the GNU Affero General Public License
  along with this program.  If not, see http://opensource.org/licenses/AGPL-3.0.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.Model
{
    public class Session
    {
        public String ID { get; private set; }

        public Database Database { get; private set; }

        public String UserID { get; private set; }

        public String Username { get; private set; }

        public String Password { get; private set; }

        private Dictionary<String, ItemType> ItemTypeCache;

        public ItemType ItemType(String Name)
        {
            if (!this.ItemTypeCache.ContainsKey(Name))
            {
                IO.Item itemtype = new IO.Item("ItemType", "get");
                itemtype.Select = "id,name";
                itemtype.SetProperty("name", Name);
                IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this, itemtype);
                IO.SOAPResponse response = request.Execute();

                if (!response.IsError)
                {
                    this.ItemTypeCache[Name] = new ItemType(this, response.Items.First().GetProperty("id"), response.Items.First().GetProperty("name"));
                }
                else
                {
                    throw new Exceptions.ServerException(response.ErrorMessage);
                }
            }

            return this.ItemTypeCache[Name];
        }

        private Dictionary<String, Item> ItemCache;

        internal Item ItemFromCache(String ID, String ConfigID, ItemType Type)
        {
            if (!this.ItemCache.ContainsKey(ID))
            {
                this.ItemCache[ID] = new Item(ID, ConfigID, Type);
            }

            return this.ItemCache[ID];
        }

        public Queries.Item Query(ItemType Type)
        {
            return new Queries.Item(Type);
        }

        public Queries.Item Query(String ItemTypeName)
        {
            return new Queries.Item(this.ItemType(ItemTypeName));
        }

        public Transaction BeginTransaction()
        {
            return new Transaction(this);
        }

        public Item Create(String Type, String Select, Transaction Transaction)
        {
            return this.Create(this.ItemType(Type), Select, Transaction);
        }

        public Item Create(ItemType Type, String Select, Transaction Transaction)
        {
            Item item = new Item(Type);
            item.Select = Select;
            this.ItemCache[item.ID] = item;
            Transaction.Add("add", item);
            return item;
        }

        internal Session(Database Database, String UserID, String Username, String Password)
        {
            this.ID = Server.NewID();
            this.Database = Database;
            this.UserID = UserID;
            this.Username = Username;
            this.Password = Password;
            this.ItemTypeCache = new Dictionary<String, ItemType>();
            this.ItemCache = new Dictionary<String, Item>();
        }
    }
}
