﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Charity_Management_System
{
    public class ProjectResource
    {
        public int resourceID { get; set; }
        public string resourceName { get; set; }
        public int projectID { get; set; }
        public int quantity { get; set; }
        public int expectedQuantity { get; set; }
        public string unit { get; set; }
        public virtual Project project { get; set; }
    }
}
