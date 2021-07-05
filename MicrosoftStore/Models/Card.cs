﻿using MicrosoftStore.Enums;
using MicrosoftStore.Responses;
using System.Collections.Generic;

namespace MicrosoftStore.Models
{
    public class Card : Payload
    {
        public string ProductId { get; set; }
        public TileLayout TileLayout { get; set; }
        public string Title { get; set; }
        public List<ImageItem> Images { get; set; }
        public string DisplayPrice { get; set; }
        public double Price { get; set; }
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public List<string> PackageFamilyNames { get; set; }
        public List<string> ContentIds { get; set; }
        public bool GamingOptionsXboxLive { get; set; }
        public string AvailableDevicesDisplayText { get; set; }
        public string AvailableDevicesNarratorText { get; set; }
        public string TypeTag { get; set; }
        public string RecommendationReason { get; set; }
        public string LongDescription { get; set; }
        public string ProductFamilyName { get; set; }
        public string Schema { get; set; }
    }
}
