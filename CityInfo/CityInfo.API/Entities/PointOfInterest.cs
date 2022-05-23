using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities
{
    public class PointOfInterest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Description { get; set; }

        /*
         * For navagiation property (City) EF will figure it out once you include City here
         * it will assume it's key is Id so will look for something called CityId, you don't need
         * to include that.
         * For convention we included it, if the City's key was something other than Id then we
         * need to tell EF core with the ForeignKey attribute, again we include it for convention
         * as it's easiler to read it.
         */

        [ForeignKey("CityId")]
        public City? City { get; set; }
        public int CityId { get; set; }
        
        public PointOfInterest(string name)
        {
            Name = name;
        }
    }
}