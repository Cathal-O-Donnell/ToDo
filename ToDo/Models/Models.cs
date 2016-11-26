using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Linq;
using System.Web;

namespace ToDo.Models
{
    //Enums
    public enum Category { Charity, Community, Entertainment, Family, Music, Outdoor, Sporting, Theatre, Other }
    public enum Town { Cork, Dublin, Galway, Limerick, Sligo, Waterford }
    public enum ActivityCategory { Adventure, Culture, Drink, Family, Food, Historical, Shop }
    public enum FileType { EventImage = 1, Photo }
    public enum VenueType { Hall, Hotel, Nightclub, Pitch, Pub, Restraunt, Sports_Complex, Stadium, Theater }

    public class Event
    {
        //ID
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EventID { get; set; }

        //User ID Foregin Key
        public string OwnerID { get; set; }

        //Foreign Key for Club
        public int VenueID { get; set; }
        [ForeignKey("VenueID")]
        public virtual Venue Venue { get; set; }

        //Title
        [Required(ErrorMessage = "You must enter a title")]
        [DataType(DataType.Text)]
        [Display(Name = "Title")]
        public string EventTitle { get; set; }

        //Date
        [Required(ErrorMessage = "You must enter a date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date")]
        public DateTime EventDate { get; set; }

        //Time
        [Required(ErrorMessage = "You must enter a start time")]
        [DataType(DataType.Time)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:HH:mm}")]
        [Display(Name = "Time")]
        public DateTime EventTime { get; set; }

        //Description
        [Required(ErrorMessage = "Give your event a brief description")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Details")]
        public string EventDescription { get; set; }

        //Event Category
        [Required(ErrorMessage = "You must select a category from the list")]
        [Display(Name = "Category")]
        public Category EventCategory { get; set; }

        //Youtube Link     
        [Display(Name = "YouTube")]
        public string EventYouTube { get; set; }

        //SoundCloud Link
        [Display(Name = "SoundCloud")]
        public string EventSoundCloud { get; set; }

        //Facebook Link
        [Display(Name = "Facebook")]
        public string EventFacebook { get; set; }

        //Twitter Link
        [Display(Name = "Twitter")]
        public string EventTwitter { get; set; }

        //Instagram Link
        [Display(Name = "Instagram")]
        public string EventInstagram { get; set; }

        //Official Website Link
        [Display(Name = "Website")]
        [DataType(DataType.Url, ErrorMessage = "This is not a valid Url")]
        public string EventWebsite { get; set; }

        //Ticket Price
        [Display(Name = "Ticket Price")]
        public double? EventTicketPrice { get; set; }

        //Ticket Shop Link/ Location
        [Display(Name = "Ticket Vendor")]
        public string EventTicketStore { get; set; }

        //Image File 
        public virtual ICollection<File> Files { get; set; }
    }

    public class Activities
    {
        //ID
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ActivityID { get; set; }

        //Name
        [Required(ErrorMessage = "You must enter a name")]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string ActivityName { get; set; }

        //Category
        [Required(ErrorMessage = "You must select a category")]
        public ActivityCategory ActivityCategory { get; set; }

        //Description
        [Required(ErrorMessage = "You must enter a description")]
        [DataType(DataType.Text)]
        [Display(Name = "Descrition")]
        public string ActivityDescription { get; set; }

        //Town
        [Required(ErrorMessage = "You must select a town from the list provided")]
        [Display(Name = "Town")]
        public Town ActivityTown { get; set; }

        //Address
        [Required(ErrorMessage = "You must enter an address")]
        [DataType(DataType.Text)]
        [Display(Name = "Address")]
        public string ActivityAddress { get; set; }

        //Website Link
        //regular expression
        public string ActivityWebsite { get; set; }

        //Facebook Link
        //regular expression
        public string ActivityFacebook { get; set; }

        //Price Information
        public double ActivityPrice { get; set; }

        //Telephone Number
        public string ActivityTelephoneNumber { get; set; }

        //Image File 
        public virtual ICollection<File> Files { get; set; }
    }

    //Image File class
    //http://www.mikesdotnetting.com/article/259/asp-net-mvc-5-with-ef-6-working-with-files
    public class File
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FileId { get; set; }

        [StringLength(255)]
        public string FileName { get; set; }

        [StringLength(100)]
        public string ContentType { get; set; }

        public byte[] Content { get; set; }

        public FileType FileType { get; set; }

        public int EventID { get; set; }

        public virtual Event Event { get; set; }
    }

    //Venue Files
    public class VenueFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VenueFileId { get; set; }

        [StringLength(255)]
        public string VenueFileName { get; set; }

        [StringLength(100)]
        public string VenueContentType { get; set; }

        public byte[] VenueContent { get; set; }

        public FileType VenueFileType { get; set; }

        public int VenueID { get; set; }

        public virtual Venue Venue { get; set; }
    }

    //Venue
    public class Venue
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int VenueID { get; set; }

        //Id for the owner of this venue
        public string OwnerId { get; set; }

        //List of events for this venue
        public List<Event> VenueEvents { get; set; }

        //Name
        [Required(ErrorMessage = "You must enter a name")]
        [DataType(DataType.Text)]
        [Display(Name = "Name")]
        public string VenueName { get; set; }

        //Type
        [DataType(DataType.Text)]
        [Display(Name = "Venue")]
        public VenueType VenueType { get; set; }

        //Town
        [Required(ErrorMessage = "You must select a town from the list provided")]
        [Display(Name = "Town")]
        public Town VenueTown { get; set; }

        //Address
        [Required(ErrorMessage = "You must enter a street")]
        [DataType(DataType.Text)]
        [Display(Name = "Street")]
        public string VenueAddress { get; set; }

        //Description
        [Required(ErrorMessage = "Give your venue a brief description")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "About Us")]
        public string VenueDescription { get; set; }

        //Contact Email
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "This is not a valid email address")]
        public string VenueEmail { get; set; }

        //Contact Number
        [Display(Name = "Telephone")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "This is not a valid phone number")]
        public string VenuePhoneNumber { get; set; }

        //Image File 
        public virtual ICollection<VenueFile> VenueFiles { get; set; }
    }

    //Band Class
    public class Band
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BandID { get; set; }

        //Name
        [Required(ErrorMessage = "You must enter a name")]
        [DataType(DataType.Text)]
        [Display(Name = "Venue")]
        public string BandName { get; set; }

        //Description
        [Required(ErrorMessage = "Give your venue a brief description")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string BandDescription { get; set; }

        //Contact Number
        [Display(Name = "Telephone")]
        [DataType(DataType.PhoneNumber, ErrorMessage = "This is not a valid phone number")]
        public int BandContactNumber { get; set; }

        //Contact Email
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress, ErrorMessage = "This is not a valid email address")]
        public string BandEmail { get; set; }

        //Facebook
        [Display(Name = "Facebook")]
        public string BandFacebook { get; set; }
    }

    //Admin settings class
    public class AdminSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdminSettingsID { get; set; }

        //Events
        [Display(Name = "Top Event")]
        public int TopFeaturedEvent { get; set; }

        [Display(Name = "Featured Event 1")]
        public int FeaturedEvent1 { get; set; }

        [Display(Name = "Featured Event 2")]
        public int FeaturedEvent2 { get; set; }

        [Display(Name = "Featured Event 3")]
        public int FeaturedEvent3 { get; set; }

        //Venues
        [Display(Name = "Top Venue")]
        public int TopFeaturedVenue { get; set; }

        [Display(Name = "Featured Venue 1")]
        public int FeaturedVenue1 { get; set; }

        [Display(Name = "Featured Venue 2")]
        public int FeaturedVenue2 { get; set; }

        [Display(Name = "Featured Venue 3")]
        public int FeaturedVenue3 { get; set; }
    }
}

//To Do
//set default image for event if none uploaded
//SoundCloud API
//Google Directions styling (pop up?, expand div?)
//Facebook (sign in/ up, share events, like) - https://www.asp.net/mvc/overview/security/create-an-aspnet-mvc-5-app-with-facebook-and-google-oauth2-and-openid-sign-on
//sendgrid
//YouTube/ SoundCloud icon on hover text
//Venue Model - add social links and website link
//Remove map from event details page and move this all to the venue details page
//Add link to the event details page linking the user to the venue for this event
//add enums to database (model classes)
//Get users current location - apply this location to the filters by default
//Add more filters to the index pages (Town, Date, Venue)
//Kick User of Admin page if they are not an Admin

//Style
//Image thumbnails - http://bootsnipp.com/snippets/featured/thumbnails-like-bootsnipp
//Styling (color wheel, human computer interaction)
//Themeforest
