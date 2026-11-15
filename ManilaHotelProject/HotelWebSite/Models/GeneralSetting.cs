
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

namespace HotelMVC.Models
{
    [Table("GeneralSetting")]
    public partial class GeneralSetting
    {
        public int GeneralSettingID { get; set; }

        [StringLength(150)]
        public string CompanyName { get; set; }

        [StringLength(100)]
        public string CompanyLogo { get; set; }

        [StringLength(50)]
        public string AdminEmail { get; set; }

        [StringLength(50)]
        public string SupportEmail { get; set; }

        [StringLength(100)]
        public string StreetAddress { get; set; }

        [StringLength(50)]
        public string CityName { get; set; }

        [StringLength(50)]
        public string Province { get; set; }

        [StringLength(20)]
        public string PostalCode { get; set; }

        [StringLength(50)]
        public string CountryName { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        public string OperationTiming { get; set; }

        [StringLength(20)]
        public string ShortDateFormat { get; set; }

        [StringLength(20)]
        public string LongDateFormat { get; set; }

        [StringLength(10)]
        public string TimeFormat { get; set; }

        public bool? FlightAccess { get; set; }

        public bool? HotelAccess { get; set; }

        public bool? OperationStatus { get; set; }

        [StringLength(10)]
        public string DefaultCurrency { get; set; }
    }
}
