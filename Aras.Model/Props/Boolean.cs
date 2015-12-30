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

namespace Aras.Model.Properties
{
    public class Boolean : Property
    {
        public override Object Value
        {
            get
            {
                return base.Value;
            }
            set
            {
                if ((value == null) || (value is System.Boolean))
                {
                    base.Value = value;
                }
                else
                {
                    throw new Exceptions.ArgumentException("Value must be a System.Boolean");
                }
            }
        }

        internal override string DBValue
        {
            get
            {
                if (this.Value == null)
                {
                    return null;
                }
                else
                {
                    if ((System.Boolean)this.Value)
                    {
                        return "1";
                    }
                    else
                    {
                        return "0";
                    }
                }
            }
            set
            {
                if (value == null)
                {
                    this.SetValue(null);
                }
                else
                {
                    this.SetValue("1".Equals(value));
                }
            }
        }

        internal Boolean(Model.Item Item, PropertyTypes.Boolean Type)
            :base(Item, Type)
        {

        }
    }
}