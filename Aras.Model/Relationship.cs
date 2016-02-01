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
    public class Relationship : Item
    {
        public RelationshipType RelationshipType
        {
            get
            {
                return (RelationshipType)this.ItemType;
            }
        }

        public override void Refresh()
        {
            List<String> propertynames = new List<String>();
            propertynames.Add("id");
            propertynames.Add("related_id");

            foreach (Property property in this.Properties)
            {
                propertynames.Add(property.Type.Name);
            }

            IO.Item dbitem = new IO.Item(this.ItemType.Name, "get");
            dbitem.Select = String.Join(",", propertynames);
            dbitem.ID = this.ID;
            IO.SOAPRequest request = new IO.SOAPRequest(IO.SOAPOperation.ApplyItem, this.ItemType.Session, dbitem);
            IO.SOAPResponse response = request.Execute();

            if (!response.IsError)
            {
                this.UpdateProperties(response.Items.First());

                this.OnRefresh();
            }
            else
            {
                throw new Exceptions.ServerException(response);
            }
        }

        public Item Source { get; private set; }

        private Item _related;
        public Item Related
        {
            get
            {
                return this._related;
            }
            set
            {
                this._related = value;
            }
        }

        internal override void UpdateProperties(IO.Item DBItem)
        {
            base.UpdateProperties(DBItem);

            IO.Item dbrelated = DBItem.GetPropertyItem("related_id");

            if (dbrelated != null)
            {
                this._related = this.ItemType.Session.Get(this.RelationshipType.Related, dbrelated.ID);
            }
        }

        public Relationship(RelationshipType RelationshipType, Item Source, Item Related)
            : base(RelationshipType)
        {
            this.Source = Source;
            this._related = Related;
        }

        public Relationship(RelationshipType RelationshipType, Item Source, IO.Item DBItem)
            : base(RelationshipType, DBItem)
        {
            this.Source = Source;
        }
    }
}
