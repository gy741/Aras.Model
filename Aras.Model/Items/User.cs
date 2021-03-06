﻿/*  
  Copyright 2017 Processwall Limited

  Licensed under the Apache License, Version 2.0 (the "License");
  you may not use this file except in compliance with the License.
  You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

  Unless required by applicable law or agreed to in writing, software
  distributed under the License is distributed on an "AS IS" BASIS,
  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
  See the License for the specific language governing permissions and
  limitations under the License.
 
  Company: Processwall Limited
  Address: The Winnowing House, Mill Lane, Askham Richard, York, YO23 3NW, United Kingdom
  Tel:     +44 113 815 3440
  Web:     http://www.processwall.com
  Email:   support@processwall.com
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aras.Model.Items
{
    [Attributes.ItemType("User")]
    public class User : Item
    {
        public String LoginName
        {
            get
            {
                return (String)this.Property("login_name").Value;
            }
            set
            {
                this.Property("login_name").Value = value;
            }
        }

        public String FirstName
        {
            get
            {
                return (String)this.Property("first_name").Value;
            }
            set
            {
                this.Property("first_name").Value = value;
            }
        }

        public String LastName
        {
            get
            {
                return (String)this.Property("last_name").Value;
            }
            set
            {
                this.Property("last_name").Value = value;
            }
        }

        public String FullName
        {
            get
            {
                return this.FirstName + " " + this.LastName;
            }
        }

        public Vault Vault
        {
            get
            {
                return (Vault)this.Property("default_vault").Value;
            }
            set
            {
                this.Property("default_vault").Value = value;
            }
        }

        public User(Store Store)
            : base(Store)
        {
          
        }

        public User(Store Store, IO.Item DBItem)
            : base(Store, DBItem)
        {
          
        }
    }
}
