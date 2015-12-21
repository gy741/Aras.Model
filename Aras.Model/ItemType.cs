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
    public class ItemType
    {
        private readonly String[] SystemProperties = new String[] { "id", "config_id" };

        public Session Session { get; private set; }

        public string ID { get; private set; }

        public String Name { get; private set; }

        private Dictionary<String, PropertyType> _propertyTypeCache;
        private Dictionary<String, PropertyType> PropertyTypeCache
        {
            get
            {
                if (this._propertyTypeCache == null)
                {
                    this._propertyTypeCache = new Dictionary<String, PropertyType>();

                    IO.Item props = new IO.Item("Property", "get");
                    props.Select = "name,data_type,stored_length";
                    props.SetProperty("source_id", this.ID);
                    IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.Session, props);
                    IO.SOAPResponse response = request.Execute();

                    if (!response.IsError)
                    {
                        foreach(IO.Item thisprop in response.Items)
                        {
                            String name = thisprop.GetProperty("name");

                            if (!SystemProperties.Contains(name))
                            {
                                switch (thisprop.GetProperty("data_type"))
                                {
                                    case "string":
                                        Int32 length = 32;
                                        Int32.TryParse(thisprop.GetProperty("stored_length"), out length);
                                        this._propertyTypeCache[name] = new PropertyTypes.String(this, name, length);
                                        break;
                                    case "integer":
                                        this._propertyTypeCache[name] = new PropertyTypes.Integer(this, name);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new Exceptions.ServerException(response.ErrorMessage);
                    }
                }

                return this._propertyTypeCache;
            }
        }

        public PropertyType PropertyType(String Name)
        {
            return this.PropertyTypeCache[Name];
        }

        public IEnumerable<PropertyType> PropertyTypes
        {
            get
            {
                return this.PropertyTypeCache.Values;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        internal ItemType(Session Session, String ID, String Name)
        {
            this.Session = Session;
            this.ID = ID;
            this.Name = Name;
        }
    }
}
