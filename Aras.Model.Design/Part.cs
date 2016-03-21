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

namespace Aras.Model.Design
{
    [Model.Attributes.ItemType("Part")]
    public class Part : Model.Item
    {
        public String ItemNumber
        {
            get
            {
                return (String)this.Property("item_number").Value;
            }
            set
            {
                this.Property("item_number").Value = value;
            }
        }

        public Boolean IsVariant
        {
            get
            {
                if ((this.Class != null) && (this.Class.Name == "Variant"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        private List<PartBOM> _partBOMS;
        public IEnumerable<PartBOM> PartBOMS
        {
            get
            {
                if (this._partBOMS == null)
                {
                    this._partBOMS = new List<PartBOM>();

                    Queries.Relationship pbquery = (Queries.Relationship)this.Store("Part BOM").Query();
                    pbquery.Refresh();

                    foreach (PartBOM pb in pbquery)
                    {
                        this._partBOMS.Add(pb);
                    }
                }

                return this._partBOMS;
            }
        }

        private List<PartVariant> _partVariants;
        public IEnumerable<PartVariant> PartVariants
        {
            get
            {
                if (this._partVariants == null)
                {
                    this._partVariants = new List<PartVariant>();

                    Queries.Relationship pvquery = (Queries.Relationship)this.Store("Part Variants").Query();
                    pvquery.Refresh();

                    foreach(PartVariant pv in pvquery)
                    {
                        this._partVariants.Add(pv);
                    }
                }

                return this._partVariants;
            }
        }

        private IEnumerable<PartBOM> ConfiguredPartVariant(Order Order, Part Variant)
        {
            List<PartBOM> ret = new List<PartBOM>();

            // Add Configured Variants
            foreach (PartVariant partvariant in Variant.PartVariants)
            {
                PartBOM configurepartbom = partvariant.ConfiguredPartBOM(Order, this);

                if (configurepartbom != null)
                {
                    if (configurepartbom.Related != null)
                    {
                        if (((Part)configurepartbom.Related).IsVariant)
                        {
                            foreach (PartBOM childpartbom in this.ConfiguredPartVariant(Order, (Part)configurepartbom.Related))
                            {
                                ret.Add(childpartbom);
                            }
                        }
                        else
                        {
                            ret.Add(configurepartbom);
                        }
                    }
                }
            }

            return ret;
        }

        public IEnumerable<PartBOM> ConfiguredPartBOM(Order Order)
        {
            List<PartBOM> ret = new List<PartBOM>();

            // Add PartBOM
            foreach (PartBOM partbom in this.PartBOMS)
            {

                Part related = (Part)partbom.Related;

                if (related != null)
                {
                    if (related.IsVariant)
                    {
                        // Add Configured Variants
                        foreach (PartBOM configurepartbom in this.ConfiguredPartVariant(Order, related))
                        {
                            ret.Add(configurepartbom);
                        }
                    }
                    else
                    {
                        ret.Add(partbom);
                    }
                }

            }

            return ret;
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();

            // Reset PartBOMS
            this._partBOMS = null;

            // Reset PartVariants
            this._partVariants = null;
        }

        public Part(Model.ItemType ItemType)
            : base(ItemType)
        {
 
        }

        public Part(Model.ItemType ItemType, IO.Item DBItem)
            : base(ItemType, DBItem)
        {

        }
    }
}
