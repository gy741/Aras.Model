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
    [Model.Attributes.ItemType("v_Order")]
    public class Order : Model.Item
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

        public Part TopLevelPart
        {
            get
            {
                return (Part)this.Property("part").Value;
            }
            set
            {
                this.Property("part").Value = value;
            }
        }

        public Part ConfiguredPart
        {
            get
            {
                return (Part)this.Property("configured_part").Value;
            }
            set
            {
                this.Property("configured_part").Value = value;
            }
        }

        private Dictionary<Model.Design.Part, Double> PartCache;

        private void RefeshPartCache(Transaction Transaction)
        {
            this.PartCache.Clear();
            this.RefeshPart(this.TopLevelPart, 1.0, Transaction);
        }

        private Model.Design.OrderContext OrderContext(Model.Design.VariantContext VariantContext, Transaction Transaction)
        {
            Model.Design.OrderContext ret = null;

            foreach (Model.Design.OrderContext ordercontext in this.Store("v_Order Context"))
            {
                if (ordercontext.VariantContext.Equals(VariantContext))
                {
                    ret = ordercontext;
                    break;
                }
            }

            if (ret == null)
            {
                ret = (Model.Design.OrderContext)this.Store("v_Order Context").Create(VariantContext, Transaction);
            }

            return ret;
        }

        private void RefeshPart(Model.Design.Part Part, Double Quantity, Transaction Transaction)
        {
            // Process Part Variants
            if (Part.IsVariant)
            {
                Part.Store("Part Variants").Refesh();

                foreach (Model.Design.PartVariant partvariant in Part.Store("Part Variants"))
                {
                    Boolean selected = true;
                    Double variant_quantity = 0.0;

                    partvariant.Store("Part Variant Rule").Refesh();

                    foreach (Model.Design.PartVariantRule partvariantrule in partvariant.Store("Part Variant Rule"))
                    {
                        if (partvariantrule.Related != null)
                        {
                            // Get Order Context
                            Model.Design.OrderContext ordercontext = this.OrderContext(partvariantrule.VariantContext, Transaction);
                            String order_context_value = null;
                            Double order_context_quantity = 0.0;

                            switch (partvariantrule.VariantContext.ContextType.Value)
                            {
                                case "Boolean":
                                case "List":

                                    order_context_value = ordercontext.Value;
                                    order_context_quantity = ordercontext.Quantity;

                                    break;

                                case "Method":

                                    IO.Item dborder = new IO.Item(this.ItemType.Name, partvariantrule.VariantContext.Method);
                                    dborder.ID = this.ID;

                                    // Add this Order Context
                                    IO.Item dbordercontext = ordercontext.GetIOItem();
                                    dborder.AddRelationship(dbordercontext);

                                    // Add all other order Context
                                    foreach (Model.Design.OrderContext otherordercontext in this.Store("v_Order Context"))
                                    {
                                        if (!otherordercontext.Equals(ordercontext))
                                        {
                                            IO.Item dbotherordercontext = otherordercontext.GetIOItem();
                                            dborder.AddRelationship(dbotherordercontext);
                                        }
                                    }

                                    IO.SOAPRequest request = this.Session.IO.Request(IO.SOAPOperation.ApplyItem, dborder);
                                    IO.SOAPResponse response = request.Execute();

                                    if (!response.IsError)
                                    {
                                        if (response.Items.Count() == 1)
                                        {
                                            order_context_value = response.Items.First().GetProperty("value");
                                            order_context_quantity = Double.Parse(response.Items.First().GetProperty("quantity"));
                                        }
                                        else
                                        {
                                            throw new Model.Exceptions.ServerException("Variant Method failed to return a result: " + partvariantrule.VariantContext.Method);
                                        }
                                    }
                                    else
                                    {
                                        throw new Model.Exceptions.ServerException(response);
                                    }

                                    break;

                                default:
                                    throw new Model.Exceptions.ArgumentException("Unsupported Variant Context Type: " + partvariantrule.VariantContext.ContextType.Value);
                            }

                            if (String.Compare(partvariantrule.Value, order_context_value) == 0)
                            {
                                Double calc_quantity = Quantity * partvariant.Quantity * order_context_quantity;

                                if (calc_quantity > variant_quantity)
                                {
                                    variant_quantity = calc_quantity;
                                }
                            }
                            else
                            {
                                selected = false;
                            }
                        }
                        else
                        {
                            selected = false;
                        }

                        if (!selected)
                        {
                            break;
                        }
                    }

                    if (selected)
                    {
                        this.RefeshPart((Model.Design.Part)partvariant.Related, variant_quantity, Transaction);
                    }
                }
            }
            else
            {
                // Add this Part to Cache
                if (!this.PartCache.ContainsKey(Part))
                {
                    this.PartCache[Part] = Quantity;
                }
                else
                {
                    this.PartCache[Part] += Quantity;
                }
            }

            // Process Part BOM
            Part.Store("Part BOM").Refesh();

            foreach (Model.Design.PartBOM partbom in Part.Store("Part BOM"))
            {
                if (partbom.Related != null)
                {
                    Double part_bom_quantity = Quantity * partbom.Quantity;
                    this.RefeshPart((Model.Design.Part)partbom.Related, part_bom_quantity, Transaction);
                }
            }
        }

        private void LockItems(Transaction Transaction)
        {
            // Lock all Variant Orders
            if (this.TopLevelPart != null)
            {
                foreach (Model.Design.OrderContext ordercontext in this.Store("v_Order Context"))
                {
                    ordercontext.Update(Transaction);
                }
            }

            // Lock Configured Part
            if (this.ConfiguredPart != null)
            {
                this.ConfiguredPart.Update(Transaction);
            }
        }

        protected override void OnUpdate(Transaction Transaction)
        {
            base.OnUpdate(Transaction);

            this.LockItems(Transaction);
        }

        [Attributes.Action("BuildFlatBOM")]
        public void BuildFlatBOM(Transaction Transaction)
        {
            // Lock Items
            this.LockItems(Transaction);

            if (this.TopLevelPart != null)
            {
                // Ensure Configured Part exists
                if (this.ConfiguredPart == null)
                {
                    Model.Queries.Item partquery = this.Session.Store("Part").Query(Aras.Conditions.Eq("item_number", this.ItemNumber));

                    if (partquery.Count() == 0)
                    {
                        // Create ConfiguredPart
                        this.ConfiguredPart = (Model.Design.Part)this.Session.Store("Part").Create(Transaction);
                        this.ConfiguredPart.ItemNumber = this.ItemNumber;
                    }
                    else
                    {
                        // Use Existing Configured Part
                        this.ConfiguredPart = (Model.Design.Part)partquery.First();
                        this.ConfiguredPart.Update(Transaction);
                    }
                }

                // Update Configured Part Properties
                this.ConfiguredPart.Class = this.ConfiguredPart.ItemType.GetClassName("TopLevel");
                this.ConfiguredPart.Property("name").Value = this.Property("name").Value;
                this.ConfiguredPart.Property("cmb_name").Value = this.Property("name").Value;
                this.ConfiguredPart.Property("description").Value = this.Property("description").Value;

                // Refesh Part Cache
                this.RefeshPartCache(Transaction);

                // Build List of Parts
                List<Model.Design.Part> requiredparts = new List<Model.Design.Part>();

                foreach (Model.Design.Part part in this.PartCache.Keys)
                {
                    if (part.Class.Name == "BOM")
                    {
                        requiredparts.Add(part);
                    }
                }

                // Build list of current Parts and remove Part not need anymore
                List<Model.Design.Part> currentparts = new List<Model.Design.Part>();

                foreach (Model.Design.PartBOM currentpartbom in this.ConfiguredPart.Store("Part BOM"))
                {
                    Model.Design.Part currentpart = (Model.Design.Part)currentpartbom.Related;

                    if (requiredparts.Contains(currentpart))
                    {
                        currentparts.Add(currentpart);

                        // Update Quanity
                        currentpartbom.Update(Transaction);
                        currentpartbom.Quantity = this.PartCache[currentpart];
                    }
                    else
                    {
                        // Remove PartBOM
                        currentpartbom.UnlockDelete(Transaction);
                    }
                }

                // Add any new Parts
                foreach (Model.Design.Part requiredpart in requiredparts)
                {
                    if (!currentparts.Contains(requiredpart))
                    {
                        Model.Design.PartBOM newpartbom = (Model.Design.PartBOM)this.ConfiguredPart.Store("Part BOM").Create(requiredpart, Transaction);
                        newpartbom.Quantity = this.PartCache[requiredpart];
                    }
                }

                // Set Sequence Number
                int cnt = 10;

                foreach (Model.Design.PartBOM partbom in this.ConfiguredPart.Store("Part BOM").CurrentItems())
                {
                    if (partbom.SortOrder != cnt)
                    {
                        partbom.Update(Transaction);
                        partbom.SortOrder = cnt;
                    }

                    cnt += 10;
                }
            }
        }

        protected override void OnRefresh()
        {
            base.OnRefresh();
        
            // Refresh Order Contexts
            this.Store("v_Order Context").Refesh();

            // Refresh Variant Contexts
            foreach (Model.Design.OrderContext ordercontext in this.Store("v_Order Context"))
            {
                if (ordercontext.VariantContext != null)
                {
                    ordercontext.VariantContext.Refresh();

                    if (ordercontext.VariantContext.List != null)
                    {
                        ordercontext.VariantContext.List.Refresh();
                    }
                }
            }

            // Refresh Configured Part
            if (this.ConfiguredPart != null)
            {
                this.ConfiguredPart.Refresh();

                this.ConfiguredPart.Store("Part BOM").Refesh();
            }
        }

        private void Initialise()
        {
            this.PartCache = new Dictionary<Part, Double>();
        }

        public Order(Aras.Model.ItemType ItemType, Aras.Model.Transaction Transaction)
            : base(ItemType, Transaction)
        {
            this.Initialise();
        }

        public Order(Aras.Model.ItemType ItemType, Aras.IO.Item DBItem)
            : base(ItemType, DBItem)
        {
            this.Initialise();
        }

    }
}
