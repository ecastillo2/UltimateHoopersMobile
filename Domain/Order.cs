using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Domain
{
    public class Order
    {
        [Key]
        //Table Fields
        public string? OrderId { get; set; }
        public string? Id { get; set; }
        public string? ProfileId { get; set; }
        public string? RunId { get; set; }
        public string? JoinedRunId { get; set; }
        public string? Name { get; set; }
        public string OrderNumber { get; set; }
        public string? ConfirmationCode { get; set; }
        public string? Status { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? CompletedOrderDate { get; set; }
        public string? TransId { get; set; }
        public string? Notes { get; set; }
        public string? Comments { get; set; }
        public decimal? Payment { get; set; }              
        public string? OrderJson { get; set; }
        public string? Type { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
        public string? TrackingNumber { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingFirstName { get; set; }
        public string? ShippingLastName { get; set; }
        public string? ShippingCity { get; set; }
        public string? ShippingState { get; set; }
        public string? ShippingZip { get; set; }
		public string? PointsUsed { get; set; }
        public string? PaymentMethod { get; set; }
        [NotMapped]
        public decimal? Total { get; set; }
        [NotMapped]
        public string? CardNumber { get; set; }
        [NotMapped]
        public string? Token { get; set; }
        [NotMapped]
        public decimal GrandTotal { get; set; }

        [NotMapped]
        public string? CVV { get; set; }
        [NotMapped]
        public string? ExpirationYear { get; set; }

        [NotMapped]
        public string? ExpirationMonth { get; set; }

        [NotMapped]
        public string? PhoneNumber { get; set; }
        [NotMapped]
        public decimal PaymentTotal { get; set; }
        [NotMapped]
        public decimal Taxes { get; set; }
        [NotMapped]
        public bool Checked { get; set; }
        [NotMapped]
        public string? OrderIdStatus { get; set; }
        [NotMapped]
        public string? Address { get; set; }
        [NotMapped]
        public string? City { get; set; }
        [NotMapped]
        public string? FirstName { get; set; }
        [NotMapped]
        public string? LastName { get; set; }
        [NotMapped]
        public Profile? Profile { get; set; }
        [NotMapped]
        public Run? Run { get; set; }

        [NotMapped]
        public List<OrderItem>? OrderItems { get; set; }

    }


}
