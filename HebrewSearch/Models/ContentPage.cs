﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HebrewSearch.Models
{
    public class ContentPage
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string[] Categories { get; set; }
    }
}