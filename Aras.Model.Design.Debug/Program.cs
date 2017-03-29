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
using Aras.Model;
using System.IO;

namespace Aras.Model.Design.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connect to Server
            Model.Server server = new Model.Server("http://localhost/11SP9");

            // Load Solution
            server.LoadAssembly("Aras.Model.Design");
            
            // Get Database
            Model.Database database = server.Database("Development");
            
            // Start Session
            Model.Session session = database.Login("admin", IO.Server.PasswordHash("innovator"));

            // Find Document
            Model.Design.Document document = (Model.Design.Document)session.Store("Document").Get("994DE0C5BCEA480B807B752C6103D7FE");

            using (Transaction trans = session.BeginTransaction())
            {
                document.Update(trans);
                document.Name = "Test";
                trans.Commit(true);
            }
            
        }
    }
}
