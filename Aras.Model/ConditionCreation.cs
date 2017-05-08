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

namespace Aras
{
    public class Conditions
    {

        public static Model.Conditions.None None()
        {
            return new Model.Conditions.None();
        }

        public static Model.Conditions.All All()
        {
            return new Model.Conditions.All();
        }

        public static Model.Conditions.Property Eq(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.eq, Value);
        }

        public static Model.Conditions.Property Ge(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.ge, Value);
        }

        public static Model.Conditions.Property Gt(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.gt, Value);
        }

        public static Model.Conditions.Property Le(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.le, Value);
        }

        public static Model.Conditions.Property Lt(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.lt, Value);
        }

        public static Model.Conditions.Property Ne(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.ne, Value);
        }

        public static Model.Conditions.Property Like(String Name, Object Value)
        {
            return new Model.Conditions.Property(Name, Model.Conditions.Operators.like, Value);
        }

        public static Model.Conditions.And And(Model.Condition Left, Model.Condition Right)
        {
            return new Model.Conditions.And(Left, Right);
        }

        public static Model.Conditions.Or Or(Model.Condition Left, Model.Condition Right)
        {
            return new Model.Conditions.Or(Left, Right);
        }
    }
}
